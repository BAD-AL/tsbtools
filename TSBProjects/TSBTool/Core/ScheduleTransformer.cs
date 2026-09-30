using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TSBTool
{
    // Pure text-in/text-out schedule transformer -- takes the same "WEEK N" / "team at team" line
    // format ApplySchedule/InputParser already use, and rewrites it to fit a *different* set of teams
    // and a fixed target games-per-team, without touching any ROM bytes at all. Transform itself is
    // deliberately ROM-agnostic (driven entirely by a ScheduleTransformerConfig); ForRom is the one
    // place that knows which config each ROM_TYPE needs. It's a preprocessing step a tool's
    // ApplySchedule runs (only when HasUnknownTeams says the schedule was written for a different
    // league) before handing TransformedLines to its own schedule helper, the same relationship
    // PlayerNames/ResolveRosterFit have to ProcessText.
    //
    // Never invents a new week; only ever removes/adds games within weeks already present in the
    // input, and never has a team play twice in one week. Every game not explicitly reported as
    // dropped/trimmed/displaced is guaranteed to survive into TransformedLines completely unchanged
    // -- same week, same "away at home" text, same relative order within that week.
    public class ScheduleTransformerConfig
    {
        // The target ROM's full team list. Any game (after TeamRenames) involving a team not in this
        // set is dropped.
        public string[] ValidTeams;

        // Franchise-identity substitutions applied BEFORE validity checking -- e.g. "titans"->"oilers"
        // for a team that's genuinely the same franchise under a later name, not just a drop
        // candidate. Both keys and values are expected lowercase, matching this project's team-name
        // convention throughout.
        public Dictionary<string, string> TeamRenames = new Dictionary<string, string>();

        // Real teams this ROM doesn't have (e.g. post-1993 expansion teams). Their games are dropped
        // and made up. A name that's neither a ValidTeams entry, a DroppableTeams entry, nor a
        // TeamRenames key is treated as a typo, and FitToRom refuses to adjust the schedule.
        public string[] DroppableTeams = new string[0];

        // The ROM's season length in weeks, or 0 to leave the input's week count alone. Extra weeks
        // are removed from the END of the season (games and all); missing weeks are added, empty,
        // at the end before makeup games are placed. Transform fits any difference; FitToRom only
        // lets it run when the input is off by at most one week.
        public int Weeks;

        // Target number of games each valid team should end up with.
        public int GamesPerTeam;

        // Hard per-week cap; a makeup game is never added to a week already at this count.
        public int MaxGamesPerWeek;

        // Optional per-week caps by position in the season (week 1 = index 0), overriding
        // MaxGamesPerWeek for the weeks listed -- for ROMs with a fixed slot count per week (TSB3).
        public int[] WeekCaps;
    }

    public class ScheduledGame
    {
        public int Week;
        public string Away;
        public string Home;

        public ScheduledGame(int week, string away, string home)
        {
            Week = week;
            Away = away;
            Home = home;
        }
    }

    public class ScheduleTransformResult
    {
        public List<string> TransformedLines = new List<string>();

        // Removed because a team (after renaming) wasn't in ValidTeams.
        public List<ScheduledGame> DroppedGames = new List<ScheduledGame>();

        // Removed purely to bring an over-GamesPerTeam team back down to target.
        public List<ScheduledGame> TrimmedGames = new List<ScheduledGame>();

        // Removed so a short team could get a makeup game without anyone playing twice in one week:
        // the short team plays a team that's free that week, and that team gives up one of its other
        // games (whose opponent then gets a makeup game of its own, and so on down the chain).
        public List<ScheduledGame> DisplacedGames = new List<ScheduledGame>();

        // Exactly-full seasons only (see FillExactlyFullWeeks): removed because a team had no week
        // left where it could take its bye.
        public List<ScheduledGame> ByeRoomGames = new List<ScheduledGame>();

        // Makeup games synthesized to bring an under-GamesPerTeam team up to target.
        public List<ScheduledGame> AddedGames = new List<ScheduledGame>();

        // The subset of AddedGames that had to reuse a pairing which already occurred elsewhere in
        // the schedule, because no fresh (never-played) partner was available -- an imperfect but
        // reported fallback, not a silent one.
        public List<ScheduledGame> RepeatMakeupGames = new List<ScheduledGame>();

        // Per-team game count in the final, transformed schedule. Any team below GamesPerTeam here
        // means AddMakeupGames genuinely ran out of legal moves (e.g. every week already full) --
        // worth surfacing to the caller rather than silently shipping an uneven schedule.
        public Dictionary<string, int> FinalGameCounts = new Dictionary<string, int>();

        // Weeks removed from the end of the season because the ROM holds fewer, and their games.
        public List<int> RemovedWeeks = new List<int>();
        public List<ScheduledGame> RemovedWeekGames = new List<ScheduledGame>();

        // Empty weeks added at the end because the ROM holds more.
        public List<int> AddedWeeks = new List<int>();

        // TeamRenames entries that actually matched a team in the input, as "from -> to".
        public List<string> RenamesApplied = new List<string>();
    }

    public static class ScheduleTransformer
    {
        private static readonly Regex weekLineRegex = new Regex("^WEEK\\s+([0-9]+)", RegexOptions.IgnoreCase);
        private static readonly Regex gameLineRegex = new Regex("^([0-9a-zA-Z]+)\\s+at\\s+([0-9a-zA-Z]+)", RegexOptions.IgnoreCase);

        // Team set shared by every 28-team TSB1 ROM (NES, SNES, Genesis -- their in-ROM team ORDER
        // differs, see TecmoToolFactory, but the set is identical, which is all a config needs).
        // ForRom_ValidTeamsMatchFactoryTeamList guards against this drifting from the factory's lists.
        private static readonly string[] twentyEightTeams =
        {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cowboys", "giants", "eagles",   "cardinals", "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "rams",   "saints",   "49ers"
        };

        // TSB3's 30 teams: the 28 plus 1995's expansion jaguars and panthers (TSB3Tool.scheduleTeamOrder).
        private static readonly string[] thirtyTeams =
        {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "jaguars",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cowboys", "giants", "eagles",   "cardinals", "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "panthers", "rams",   "saints",    "49ers"
        };

        // The CXROM hacks' 32 teams (TecmoToolFactory's list minus its Pro Bowl "AFC"/"NFC" entries).
        private static readonly string[] thirtyTwoTeams =
        {
            "bills",      "dolphins", "patriots", "jets",
            "bengals",    "browns",   "ravens",   "steelers",
            "colts",      "texans",   "jaguars",  "titans",
            "broncos",    "chiefs",   "raiders",  "chargers",
            "redskins",   "giants",   "eagles",   "cowboys",
            "bears",      "lions",    "packers",  "vikings",
            "buccaneers", "saints",   "falcons",  "panthers",
            "49ers",      "rams",     "seahawks", "cardinals"
        };

        // The config for fitting a schedule onto a given ROM type, or null when that ROM needs no
        // transform (CXROM_18WEEK already takes a full modern 32-team/18-week season). Returns a
        // fresh instance every call, so a caller tweaking its copy can't affect anyone else's.
        // TSB2Tool and TSB3Tool both call FitToRom; TSB3's exactly-full weeks go through
        // FillExactlyFullWeeks.
        public static ScheduleTransformerConfig ForRom(ROM_TYPE romType)
        {
            return ForRom(romType, 1);
        }

        // `season` only matters for TSB2, whose three stored seasons differ in length.
        public static ScheduleTransformerConfig ForRom(ROM_TYPE romType, int season)
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.GamesPerTeam = 16;
            switch (romType)
            {
                case ROM_TYPE.NES_ORIGINAL_TSB:
                case ROM_TYPE.SNES_TSB1:
                case ROM_TYPE.GENESIS_TSB1:
                case ROM_TYPE.SNES_TSB2:
                    config.ValidTeams = (string[])twentyEightTeams.Clone();
                    config.TeamRenames["titans"] = "oilers"; // same franchise, Houston->Tennessee, renamed
                    config.DroppableTeams = new[] { "jaguars", "panthers", "ravens", "texans" }; // post-1993 expansion
                    config.MaxGamesPerWeek = 14;
                    if (romType == ROM_TYPE.SNES_TSB2)
                        config.Weeks = season == 2 ? 18 : 17; // TSB2Tool: seasons 1 and 3 are 17 weeks, season 2 is 18
                    else
                        config.Weeks = romType == ROM_TYPE.NES_ORIGINAL_TSB ? 17 : 18;
                    return config;
                case ROM_TYPE.SNES_TSB3:
                    config.ValidTeams = (string[])thirtyTeams.Clone();
                    config.TeamRenames["titans"] = "oilers";
                    config.DroppableTeams = new[] { "ravens", "texans" }; // jaguars and panthers are in TSB3
                    config.MaxGamesPerWeek = 15;
                    config.Weeks = 17;
                    // TSB3Tool's fixed slots per week -- they add up to exactly 240 (30 * 16 / 2),
                    // so every week of a full season is exactly full.
                    config.WeekCaps = new[] { 15, 15, 15, 12, 13, 13, 13, 13, 13, 14, 14, 15, 15, 15, 15, 15, 15 };
                    return config;
                case ROM_TYPE.CXROM_v105:
                case ROM_TYPE.CXROM_v111:
                    config.ValidTeams = (string[])thirtyTwoTeams.Clone();
                    config.MaxGamesPerWeek = 16;
                    config.Weeks = 17;
                    return config;
                default:
                    return null;
            }
        }

        // Every game naming a team that's neither valid, droppable, nor a rename -- almost certainly
        // a misspelling. FitToRom refuses to adjust a schedule containing any of these, so a typo
        // never causes games to be made up; the schedule helper then rejects the bad lines as usual.
        public static List<ScheduledGame> FindUnrecognizedGames(List<string> lines, ScheduleTransformerConfig config)
        {
            List<ScheduledGame> games;
            List<int> weekOrder;
            ParseLines(lines, out games, out weekOrder);

            List<ScheduledGame> unrecognized = new List<ScheduledGame>();
            foreach (ScheduledGame g in games)
            {
                if (!IsRecognized(g.Away, config) || !IsRecognized(g.Home, config))
                    unrecognized.Add(g);
            }
            return unrecognized;
        }

        private static bool IsRecognized(string team, ScheduleTransformerConfig config)
        {
            string renamed = Rename(team, config);
            return Array.IndexOf(config.ValidTeams, renamed) >= 0 || Array.IndexOf(config.DroppableTeams, renamed) >= 0;
        }

        // The user-facing summary of what Transform changed: which teams' games were dropped (and
        // how many each), which renames were used, and the trim/add counts. Per-game detail goes to
        // the log instead (see FitToRom).
        public static string DescribeAdjustments(ScheduleTransformResult result, ScheduleTransformerConfig config)
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("Schedule adjusted to fit this ROM's {0} teams:", config.ValidTeams.Length));

            if (result.DroppedGames.Count > 0)
            {
                SortedDictionary<string, int> droppedByTeam = new SortedDictionary<string, int>(StringComparer.Ordinal);
                foreach (ScheduledGame g in result.DroppedGames)
                {
                    foreach (string team in new[] { g.Away, g.Home })
                    {
                        if (Array.IndexOf(config.ValidTeams, team) >= 0)
                            continue;
                        droppedByTeam[team] = (droppedByTeam.ContainsKey(team) ? droppedByTeam[team] : 0) + 1;
                    }
                }
                List<string> parts = new List<string>();
                foreach (KeyValuePair<string, int> kv in droppedByTeam)
                    parts.Add(string.Format("{0} ({1})", kv.Key, kv.Value));
                lines.Add(string.Format("  Teams not in this ROM (games dropped): {0} -- {1} games",
                    string.Join(", ", parts.ToArray()), result.DroppedGames.Count));
            }

            if (result.RenamesApplied.Count > 0)
                lines.Add("  Renamed: " + string.Join(", ", result.RenamesApplied.ToArray()));

            if (result.RemovedWeeks.Count > 0)
                lines.Add(string.Format("  Removed {0} (this ROM holds {1} weeks) -- {2} games",
                    WeekList(result.RemovedWeeks), config.Weeks, result.RemovedWeekGames.Count));
            if (result.AddedWeeks.Count > 0)
                lines.Add(string.Format("  Added empty {0} (this ROM holds {1} weeks)", WeekList(result.AddedWeeks), config.Weeks));

            lines.Add(string.Format("  {0} games trimmed, {1}{2}{3} makeup games added ({4} repeat matchups)",
                result.TrimmedGames.Count,
                result.DisplacedGames.Count == 0 ? "" : string.Format("{0} games displaced to avoid double-booking, ", result.DisplacedGames.Count),
                result.ByeRoomGames.Count == 0 ? "" : string.Format("{0} games removed to make room for byes, ", result.ByeRoomGames.Count),
                result.AddedGames.Count, result.RepeatMakeupGames.Count));

            return string.Join("\n", lines.ToArray());
        }

        // True if any game (after TeamRenames) names a team outside config.ValidTeams -- i.e. the
        // schedule was written for a different league (e.g. 32 teams) and needs Transform before
        // this ROM can take it. A schedule using only valid teams returns false and should be
        // written exactly as given, even if its per-team counts are uneven.
        public static bool HasUnknownTeams(List<string> lines, ScheduleTransformerConfig config)
        {
            List<ScheduledGame> games;
            List<int> weekOrder;
            ParseLines(lines, out games, out weekOrder);

            List<string> validTeams = new List<string>(config.ValidTeams);
            foreach (ScheduledGame g in games)
            {
                if (!validTeams.Contains(Rename(g.Away, config)) || !validTeams.Contains(Rename(g.Home, config)))
                    return true;
            }
            return false;
        }

        // What a tool's ApplySchedule calls before handing lines to its schedule helper. A schedule
        // naming real teams this ROM doesn't have (e.g. a 32-team NFL/CXROM schedule), or with one
        // more week than the ROM holds, is transformed, with a summary reported through
        // StaticUtils.AddError and every change logged. Anything else comes back exactly as given:
        // a schedule using only this ROM's teams and weeks (so hand-edited schedules are never
        // altered), a ROM type ForRom has no config for, a schedule containing a misspelled team,
        // and one whose week count is off by more than one -- the last two with a warning instead,
        // so questionable input never causes games to be made up.
        public static List<string> FitToRom(List<string> lines, ROM_TYPE romType)
        {
            return FitToRom(lines, romType, 1);
        }

        // `season` only matters for TSB2, whose three stored seasons differ in length.
        public static List<string> FitToRom(List<string> lines, ROM_TYPE romType, int season)
        {
            ScheduleTransformerConfig config = ForRom(romType, season);
            if (config == null)
                return lines;

            List<ScheduledGame> unrecognized = FindUnrecognizedGames(lines, config);
            if (unrecognized.Count > 0)
            {
                ReportUnrecognizedTeams(unrecognized, config);
                return lines;
            }

            // Also adjust a schedule with more weeks than the ROM holds -- one written for this ROM
            // can't have that (the helper would reject the extra weeks anyway).
            int weeks = CountWeeks(lines);
            bool tooManyWeeks = config.Weeks > 0 && weeks > config.Weeks;
            if (!HasUnknownTeams(lines, config) && !tooManyWeeks)
                return lines;

            // The season length is only fitted when it's off by ONE week -- the real-world mismatch
            // (17- vs 18-week NFL seasons). Further off looks like incomplete or doubled-up input
            // (a partial paste, two seasons in one), so it's written as given, with a warning, rather
            // than padded with made-up games or silently cut down.
            if (config.Weeks > 0 && Math.Abs(weeks - config.Weeks) > 1)
            {
                StaticUtils.AddError(string.Format(
                    "Schedule has {0} weeks; this ROM holds {1} -- the schedule was not adjusted to fit this ROM; check that the whole season was included.",
                    weeks, config.Weeks));
                return lines;
            }

            ScheduleTransformResult result = Transform(lines, config);
            StaticUtils.AddError(DescribeAdjustments(result, config));
            foreach (ScheduledGame g in result.RemovedWeekGames)
                StaticUtils.LogMessage(string.Format("Schedule adjustment: removed week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
            foreach (ScheduledGame g in result.TrimmedGames)
                StaticUtils.LogMessage(string.Format("Schedule adjustment: trimmed week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
            foreach (ScheduledGame g in result.DisplacedGames)
                StaticUtils.LogMessage(string.Format("Schedule adjustment: displaced week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
            foreach (ScheduledGame g in result.ByeRoomGames)
                StaticUtils.LogMessage(string.Format("Schedule adjustment: removed for bye room week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
            foreach (ScheduledGame g in result.AddedGames)
                StaticUtils.LogMessage(string.Format("Schedule adjustment: added week {0} '{1} at {2}'{3}", g.Week, g.Away, g.Home,
                    result.RepeatMakeupGames.Contains(g) ? " (repeat matchup)" : ""));
            return result.TransformedLines;
        }

        // One warning per distinct unrecognized name, pointing at its first game.
        private static void ReportUnrecognizedTeams(List<ScheduledGame> unrecognized, ScheduleTransformerConfig config)
        {
            List<string> names = new List<string>();
            Dictionary<string, List<ScheduledGame>> gamesByName = new Dictionary<string, List<ScheduledGame>>();
            foreach (ScheduledGame g in unrecognized)
            {
                foreach (string team in new[] { g.Away, g.Home })
                {
                    if (IsRecognized(team, config))
                        continue;
                    if (!gamesByName.ContainsKey(team))
                    {
                        names.Add(team);
                        gamesByName[team] = new List<ScheduledGame>();
                    }
                    gamesByName[team].Add(g);
                }
            }

            foreach (string name in names)
            {
                ScheduledGame first = gamesByName[name][0];
                int more = gamesByName[name].Count - 1;
                StaticUtils.AddError(string.Format(
                    "Unrecognized team '{0}' in week {1}: '{2} at {3}'{4} -- the schedule was not adjusted to fit this ROM; fix the team name and re-apply.",
                    name, first.Week, first.Away, first.Home,
                    more == 0 ? "" : string.Format(" (and {0} more game{1})", more, more == 1 ? "" : "s")));
            }
        }

        private static string ApplyRename(string team, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            if (!config.TeamRenames.ContainsKey(team))
                return team;
            string renamed = config.TeamRenames[team];
            string description = team + " -> " + renamed;
            if (!result.RenamesApplied.Contains(description))
                result.RenamesApplied.Add(description);
            return renamed;
        }

        private static string Rename(string team, ScheduleTransformerConfig config)
        {
            return config.TeamRenames.ContainsKey(team) ? config.TeamRenames[team] : team;
        }

        public static ScheduleTransformResult Transform(List<string> lines, ScheduleTransformerConfig config)
        {
            ScheduleTransformResult result = new ScheduleTransformResult();

            List<ScheduledGame> games;
            List<int> weekOrder;
            ParseLines(lines, out games, out weekOrder);

            foreach (ScheduledGame g in games)
            {
                g.Away = ApplyRename(g.Away, config, result);
                g.Home = ApplyRename(g.Home, config, result);
            }

            Dictionary<string, bool> validTeams = new Dictionary<string, bool>();
            foreach (string t in config.ValidTeams)
                validTeams[t] = true;

            List<ScheduledGame> surviving = new List<ScheduledGame>();
            foreach (ScheduledGame g in games)
            {
                if (validTeams.ContainsKey(g.Away) && validTeams.ContainsKey(g.Home))
                    surviving.Add(g);
                else
                    result.DroppedGames.Add(g);
            }

            FitWeekCount(surviving, weekOrder, config, result);

            Dictionary<string, int> counts = ComputeCounts(surviving, validTeams);
            TrimExcess(surviving, counts, config.GamesPerTeam, result.TrimmedGames);
            TrimWeekOverflow(surviving, weekOrder, config, result.TrimmedGames);

            if (!IsExactlyFull(weekOrder, config) || !FillExactlyFullWeeks(surviving, weekOrder, config, result))
                AddMakeupGames(surviving, weekOrder, config, result);

            BuildTransformedLines(surviving, weekOrder, result);

            result.FinalGameCounts = ComputeCounts(surviving, validTeams);
            return result;
        }

        private static void ParseLines(List<string> lines, out List<ScheduledGame> games, out List<int> weekOrder)
        {
            games = new List<ScheduledGame>();
            weekOrder = new List<int>();
            int currentWeek = -1;
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                Match weekMatch = weekLineRegex.Match(line);
                if (weekMatch.Success)
                {
                    currentWeek = int.Parse(weekMatch.Groups[1].Value);
                    if (!weekOrder.Contains(currentWeek))
                        weekOrder.Add(currentWeek);
                    continue;
                }
                if (currentWeek < 0)
                    continue;
                Match gameMatch = gameLineRegex.Match(line);
                if (!gameMatch.Success)
                    continue;
                string away = gameMatch.Groups[1].Value.ToLower();
                string home = gameMatch.Groups[2].Value.ToLower();
                games.Add(new ScheduledGame(currentWeek, away, home));
            }
        }

        private static Dictionary<string, int> ComputeCounts(List<ScheduledGame> games, Dictionary<string, bool> validTeams)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (string t in validTeams.Keys)
                counts[t] = 0;
            foreach (ScheduledGame g in games)
            {
                counts[g.Away]++;
                counts[g.Home]++;
            }
            return counts;
        }

        // Pass 1 prefers removing a game where BOTH teams are still over target -- fixes two teams
        // with one removal. Pass 2 mops up anything left by removing a game touching any still-over
        // team, whatever it's paired against (the other team's count drops too, which the later
        // AddMakeupGames call will naturally see and compensate for).
        // Brings the season to config.Weeks: extra weeks come off the END (their surviving games are
        // removed with them -- the teams that lose a game there get made up later); missing weeks
        // are appended empty, so makeup games have room.
        private static void FitWeekCount(List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            if (config.Weeks <= 0)
                return;

            while (weekOrder.Count > config.Weeks)
            {
                int week = weekOrder[weekOrder.Count - 1];
                weekOrder.RemoveAt(weekOrder.Count - 1);
                result.RemovedWeeks.Insert(0, week);
                for (int i = surviving.Count - 1; i >= 0; i--)
                {
                    if (surviving[i].Week == week)
                    {
                        result.RemovedWeekGames.Insert(0, surviving[i]);
                        surviving.RemoveAt(i);
                    }
                }
            }

            while (weekOrder.Count < config.Weeks)
            {
                int week = 1;
                foreach (int w in weekOrder)
                    week = Math.Max(week, w + 1);
                weekOrder.Add(week);
                result.AddedWeeks.Add(week);
            }
        }

        // The game limit for `week`: its WeekCaps entry (by position in the season) if there is one,
        // otherwise MaxGamesPerWeek.
        private static int WeekCap(int week, List<int> weekOrder, ScheduleTransformerConfig config)
        {
            int index = weekOrder.IndexOf(week);
            if (config.WeekCaps != null && index >= 0 && index < config.WeekCaps.Length)
                return config.WeekCaps[index];
            return config.MaxGamesPerWeek;
        }

        // A week already holding more games than its cap (e.g. a real NFL week of 16 games going
        // into a TSB3 week with 12 slots) loses its last games; the teams involved get made up in
        // other weeks. Reported as trimmed.
        private static void TrimWeekOverflow(List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformerConfig config, List<ScheduledGame> trimmedOut)
        {
            foreach (int week in weekOrder)
            {
                int cap = WeekCap(week, weekOrder, config);
                int count = 0;
                foreach (ScheduledGame g in surviving)
                    if (g.Week == week)
                        count++;
                for (int i = surviving.Count - 1; i >= 0 && count > cap; i--)
                {
                    if (surviving[i].Week != week)
                        continue;
                    trimmedOut.Add(surviving[i]);
                    surviving.RemoveAt(i);
                    count--;
                }
            }
        }

        private static int CountWeeks(List<string> lines)
        {
            List<ScheduledGame> games;
            List<int> weekOrder;
            ParseLines(lines, out games, out weekOrder);
            return weekOrder.Count;
        }

        private static string WeekList(List<int> weeks)
        {
            List<string> parts = new List<string>();
            foreach (int w in weeks)
                parts.Add(w.ToString());
            return (weeks.Count == 1 ? "week " : "weeks ") + string.Join(", ", parts.ToArray());
        }

        private static void TrimExcess(List<ScheduledGame> surviving, Dictionary<string, int> counts, int target, List<ScheduledGame> trimmedOut)
        {
            Dictionary<string, int> excess = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kv in counts)
                if (kv.Value > target)
                    excess[kv.Key] = kv.Value - target;

            if (excess.Count == 0)
                return;

            for (int i = surviving.Count - 1; i >= 0 && excess.Count > 0; i--)
            {
                ScheduledGame g = surviving[i];
                if (excess.ContainsKey(g.Away) && excess.ContainsKey(g.Home))
                {
                    surviving.RemoveAt(i);
                    trimmedOut.Add(g);
                    DecrementExcess(excess, g.Away);
                    DecrementExcess(excess, g.Home);
                }
            }

            for (int i = surviving.Count - 1; i >= 0 && excess.Count > 0; i--)
            {
                ScheduledGame g = surviving[i];
                string overTeam = null;
                if (excess.ContainsKey(g.Away))
                    overTeam = g.Away;
                else if (excess.ContainsKey(g.Home))
                    overTeam = g.Home;
                if (overTeam != null)
                {
                    surviving.RemoveAt(i);
                    trimmedOut.Add(g);
                    DecrementExcess(excess, overTeam);
                }
            }
        }

        private static void DecrementExcess(Dictionary<string, int> excess, string team)
        {
            if (!excess.ContainsKey(team))
                return;
            excess[team]--;
            if (excess[team] <= 0)
                excess.Remove(team);
        }

        // Brings every short team up to GamesPerTeam without ever having a team play twice in one
        // week. Each short team is fixed by the shortest "chain" found (breadth-first):
        //   - it plays a team that's free the same week; if that partner was also short, done;
        //   - otherwise the partner (already at target) gives up one of its other games, and that
        //     game's opponent becomes the next team needing a game -- until the chain reaches a team
        //     that was already short (or loops back to the starting team, if it needs 2+ games).
        // Fresh pairings are tried first; a repeat matchup only if no fresh chain exists. A team no
        // chain can fix is left short, and shows up in FinalGameCounts.
        private static void AddMakeupGames(List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            List<string> stuck = new List<string>();
            while (true)
            {
                ScheduleState state = new ScheduleState(surviving, config.ValidTeams);
                string start = null;
                foreach (string team in config.ValidTeams)
                {
                    if (state.Count(team) < config.GamesPerTeam && !stuck.Contains(team))
                    {
                        start = team;
                        break;
                    }
                }
                if (start == null)
                    break;

                if (!TryMakeupChain(start, true, state, surviving, weekOrder, config, result) &&
                    !TryMakeupChain(start, false, state, surviving, weekOrder, config, result))
                    stuck.Add(start);
            }
        }

        // True when the week caps add up to exactly one full season (every team at GamesPerTeam) --
        // e.g. TSB3 -- so every week must end up exactly full. Such a season has a fixed number of
        // byes per week (teams - 2 * cap), and every team gets exactly (weeks - GamesPerTeam) of them.
        private static bool IsExactlyFull(List<int> weekOrder, ScheduleTransformerConfig config)
        {
            if (config.WeekCaps == null || weekOrder.Count != config.WeekCaps.Length)
                return false;
            int total = 0;
            foreach (int cap in config.WeekCaps)
                total += cap;
            return total * 2 == config.ValidTeams.Length * config.GamesPerTeam;
        }

        // Exactly-full seasons, where one-team-at-a-time chains get stuck: decide every team's bye
        // week(s) first, then the games follow. Each team takes its byes in weeks it's already free,
        // with exactly the right number of byes per week (a bipartite matching, solved exactly by
        // augmenting paths). If some team can't get a bye, one of its games in a bye week is removed
        // to make room (the removal that lets the most teams get a bye) and the matching is redone.
        // Once every bye is placed, each week's remaining free teams are paired off -- any pairing
        // is legal, and the count is always even -- choosing the pairing with the fewest rematches.
        // Returns false (changing nothing) if no complete bye assignment can be reached, so the
        // caller can fall back to AddMakeupGames.
        private static bool FillExactlyFullWeeks(List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            int byesPerTeam = weekOrder.Count - config.GamesPerTeam;
            List<ScheduledGame> removed = new List<ScheduledGame>();
            List<ScheduledGame> working = new List<ScheduledGame>(surviving);
            Dictionary<string, List<int>> byes = AssignByes(working, weekOrder, config, byesPerTeam);

            for (int guard = 0; guard < 2 * config.ValidTeams.Length && Unassigned(byes, config, byesPerTeam) != null; guard++)
            {
                string team = Unassigned(byes, config, byesPerTeam);
                ScheduledGame best = null;
                int bestAssigned = -1;
                foreach (ScheduledGame g in working)
                {
                    if ((g.Away != team && g.Home != team) || ByesInWeek(g.Week, weekOrder, config) <= 0)
                        continue;
                    List<ScheduledGame> trial = new List<ScheduledGame>(working);
                    trial.Remove(g);
                    int assigned = CountAssigned(AssignByes(trial, weekOrder, config, byesPerTeam));
                    if (assigned > bestAssigned)
                    {
                        bestAssigned = assigned;
                        best = g;
                    }
                }
                if (best == null)
                    return false;
                working.Remove(best);
                removed.Add(best);
                byes = AssignByes(working, weekOrder, config, byesPerTeam);
            }
            if (Unassigned(byes, config, byesPerTeam) != null)
                return false;

            // Every bye is placed: commit the removals, then fill each week.
            foreach (ScheduledGame g in removed)
            {
                surviving.Remove(g);
                result.ByeRoomGames.Add(g);
            }
            foreach (int week in weekOrder)
            {
                ScheduleState state = new ScheduleState(surviving, config.ValidTeams);
                List<string> toPlay = new List<string>();
                foreach (string t in config.ValidTeams)
                    if (!state.Busy(t, week) && !byes[t].Contains(week))
                        toPlay.Add(t);
                foreach (ScheduledGame g in FewestRematchPairing(toPlay, week, state))
                {
                    bool isRepeat = state.Played(g.Away, g.Home);
                    surviving.Add(g);
                    result.AddedGames.Add(g);
                    if (isRepeat)
                        result.RepeatMakeupGames.Add(g);
                }
            }
            return true;
        }

        private static int ByesInWeek(int week, List<int> weekOrder, ScheduleTransformerConfig config)
        {
            return config.ValidTeams.Length - 2 * WeekCap(week, weekOrder, config);
        }

        private static string Unassigned(Dictionary<string, List<int>> byes, ScheduleTransformerConfig config, int byesPerTeam)
        {
            foreach (string t in config.ValidTeams)
                if (byes[t].Count < byesPerTeam)
                    return t;
            return null;
        }

        private static int CountAssigned(Dictionary<string, List<int>> byes)
        {
            int n = 0;
            foreach (List<int> weeks in byes.Values)
                n += weeks.Count;
            return n;
        }

        // Places as many byes as possible: each team up to byesPerTeam, only in weeks it's free,
        // each week up to its bye count. Augmenting paths make this a maximum assignment -- a team
        // left short means no assignment gives it a bye without changing the games.
        private static Dictionary<string, List<int>> AssignByes(List<ScheduledGame> games, List<int> weekOrder, ScheduleTransformerConfig config, int byesPerTeam)
        {
            ScheduleState state = new ScheduleState(games, config.ValidTeams);
            Dictionary<string, List<int>> byesOf = new Dictionary<string, List<int>>();
            Dictionary<int, List<string>> byesIn = new Dictionary<int, List<string>>();
            foreach (string t in config.ValidTeams)
                byesOf[t] = new List<int>();
            foreach (int w in weekOrder)
                byesIn[w] = new List<string>();
            for (int round = 0; round < byesPerTeam; round++)
                foreach (string t in config.ValidTeams)
                    if (byesOf[t].Count == round)
                        TryPlaceBye(t, state, weekOrder, config, byesOf, byesIn, new List<int>());
            return byesOf;
        }

        private static bool TryPlaceBye(string team, ScheduleState state, List<int> weekOrder, ScheduleTransformerConfig config,
            Dictionary<string, List<int>> byesOf, Dictionary<int, List<string>> byesIn, List<int> visited)
        {
            foreach (int week in weekOrder)
            {
                if (visited.Contains(week) || state.Busy(team, week) || byesOf[team].Contains(week) || ByesInWeek(week, weekOrder, config) <= 0)
                    continue;
                visited.Add(week);
                if (byesIn[week].Count < ByesInWeek(week, weekOrder, config))
                {
                    byesIn[week].Add(team);
                    byesOf[team].Add(week);
                    return true;
                }
                // Week full of byes: see if one of its bye teams can move its bye elsewhere.
                foreach (string other in new List<string>(byesIn[week]))
                {
                    byesOf[other].Remove(week);
                    if (TryPlaceBye(other, state, weekOrder, config, byesOf, byesIn, visited))
                    {
                        byesIn[week].Remove(other);
                        byesIn[week].Add(team);
                        byesOf[team].Add(week);
                        return true;
                    }
                    byesOf[other].Add(week);
                }
            }
            return false;
        }

        // Pairs off `teams` (an even number, all free in `week`) with as few rematches as possible.
        // The sets are small -- only teams freed by dropped/trimmed games -- so this searches every
        // pairing, stopping as soon as one with no rematches is found.
        private static List<ScheduledGame> FewestRematchPairing(List<string> teams, int week, ScheduleState state)
        {
            List<ScheduledGame> best = new List<ScheduledGame>();
            int bestRepeats = int.MaxValue;
            SearchPairings(teams, week, state, new List<ScheduledGame>(), 0, ref best, ref bestRepeats);
            return best;
        }

        private static void SearchPairings(List<string> left, int week, ScheduleState state, List<ScheduledGame> pairs, int repeats,
            ref List<ScheduledGame> best, ref int bestRepeats)
        {
            if (repeats >= bestRepeats)
                return;
            if (left.Count < 2)
            {
                best = new List<ScheduledGame>(pairs);
                bestRepeats = repeats;
                return;
            }
            string a = left[0];
            for (int i = 1; i < left.Count && bestRepeats > 0; i++)
            {
                List<string> rest = new List<string>(left);
                rest.RemoveAt(i);
                rest.RemoveAt(0);
                pairs.Add(new ScheduledGame(week, a, left[i]));
                SearchPairings(rest, week, state, pairs, repeats + (state.Played(a, left[i]) ? 1 : 0), ref best, ref bestRepeats);
                pairs.RemoveAt(pairs.Count - 1);
            }
        }

        // A snapshot of who plays when, for fast lookups while searching for a chain.
        private class ScheduleState
        {
            private readonly Dictionary<string, bool> busy = new Dictionary<string, bool>();
            private readonly Dictionary<string, int> counts = new Dictionary<string, int>();
            private readonly Dictionary<int, int> weekCounts = new Dictionary<int, int>();
            private readonly Dictionary<string, bool> playedPairs = new Dictionary<string, bool>();
            public readonly Dictionary<string, List<ScheduledGame>> GamesByTeam = new Dictionary<string, List<ScheduledGame>>();

            public ScheduleState(List<ScheduledGame> games, string[] teams)
            {
                foreach (string t in teams)
                {
                    counts[t] = 0;
                    GamesByTeam[t] = new List<ScheduledGame>();
                }
                foreach (ScheduledGame g in games)
                {
                    busy[g.Week + "|" + g.Away] = true;
                    busy[g.Week + "|" + g.Home] = true;
                    counts[g.Away]++;
                    counts[g.Home]++;
                    GamesByTeam[g.Away].Add(g);
                    GamesByTeam[g.Home].Add(g);
                    weekCounts[g.Week] = WeekCount(g.Week) + 1;
                    playedPairs[PairKey(g.Away, g.Home)] = true;
                }
            }

            public bool Busy(string team, int week) { return busy.ContainsKey(week + "|" + team); }
            public int Count(string team) { return counts[team]; }
            public int WeekCount(int week) { return weekCounts.ContainsKey(week) ? weekCounts[week] : 0; }
            public bool Played(string a, string b) { return playedPairs.ContainsKey(PairKey(a, b)); }
        }

        // One link in a chain: `prevTeam` plays `partner` in `week`, and `partner` gives up `givenUp`.
        private class ChainLink
        {
            public string PrevTeam;
            public int Week;
            public string Partner;
            public ScheduledGame GivenUp;
        }

        private static bool TryMakeupChain(string start, bool freshOnly, ScheduleState state, List<ScheduledGame> surviving,
            List<int> weekOrder, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            Dictionary<string, ChainLink> links = new Dictionary<string, ChainLink>();
            links[start] = null;
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                string team = queue.Dequeue();
                List<string> onChain = ChainTeams(links, team);
                // The week `team` just lost a game in (if it joined the chain that way): it's free
                // there now, and that week has a slot open again -- neither of which the snapshot shows.
                ScheduledGame vacated = links[team] == null ? null : links[team].GivenUp;
                foreach (int week in weekOrder)
                {
                    bool isVacatedWeek = vacated != null && vacated.Week == week;
                    if (state.Busy(team, week) && !isVacatedWeek)
                        continue;
                    int openSlots = WeekCap(week, weekOrder, config) - state.WeekCount(week) + (isVacatedWeek ? 1 : 0);
                    foreach (string partner in config.ValidTeams)
                    {
                        if (partner == team)
                            continue;
                        if (freshOnly && state.Played(team, partner))
                            continue;

                        if (state.Busy(partner, week))
                        {
                            // Swap: partner leaves its game this week to play `team` instead. The
                            // week's game count doesn't change, so this works even in a full week;
                            // the other team from partner's game is next in the chain.
                            if (partner == start || onChain.Contains(partner))
                                continue;
                            foreach (ScheduledGame givenUp in state.GamesByTeam[partner])
                            {
                                if (givenUp.Week != week || givenUp == vacated)
                                    continue;
                                string next = givenUp.Away == partner ? givenUp.Home : givenUp.Away;
                                if (next == start || next == team || onChain.Contains(next) || links.ContainsKey(next))
                                    continue;
                                ChainLink link = new ChainLink();
                                link.PrevTeam = team;
                                link.Week = week;
                                link.Partner = partner;
                                link.GivenUp = givenUp;
                                links[next] = link;
                                queue.Enqueue(next);
                            }
                            continue;
                        }

                        // Everything below adds a game to this week, so it needs an open slot.
                        if (openSlots <= 0)
                            continue;

                        // The start team, short by 2+, can close its own chain (in a different week
                        // than its first makeup game).
                        if (partner == start)
                        {
                            if (team != start && config.GamesPerTeam - state.Count(start) >= 2 && week != FirstChainWeek(links, team) &&
                                TryApplyChain(links, team, week, partner, surviving, weekOrder, config, result))
                                return true;
                            continue;
                        }
                        if (onChain.Contains(partner))
                            continue;

                        if (state.Count(partner) < config.GamesPerTeam)
                        {
                            if (TryApplyChain(links, team, week, partner, surviving, weekOrder, config, result))
                                return true;
                            continue;
                        }

                        // partner is at target: it gives up one of its other games, and that game's
                        // opponent becomes the next team needing a game.
                        foreach (ScheduledGame givenUp in state.GamesByTeam[partner])
                        {
                            if (givenUp.Week == week)
                                continue;
                            string next = givenUp.Away == partner ? givenUp.Home : givenUp.Away;
                            if (next == start || next == team || onChain.Contains(next) || links.ContainsKey(next))
                                continue;
                            ChainLink link = new ChainLink();
                            link.PrevTeam = team;
                            link.Week = week;
                            link.Partner = partner;
                            link.GivenUp = givenUp;
                            links[next] = link;
                            queue.Enqueue(next);
                        }
                    }
                }
            }
            return false;
        }

        // Every team on the chain ending at `team` (both the teams needing games and their partners).
        private static List<string> ChainTeams(Dictionary<string, ChainLink> links, string team)
        {
            List<string> teams = new List<string>();
            string current = team;
            while (current != null)
            {
                teams.Add(current);
                ChainLink link = links[current];
                if (link == null)
                    break;
                teams.Add(link.Partner);
                current = link.PrevTeam;
            }
            return teams;
        }

        // Week of the chain's first makeup game (the one involving the start team).
        private static int FirstChainWeek(Dictionary<string, ChainLink> links, string team)
        {
            int week = -1;
            string current = team;
            while (links[current] != null)
            {
                week = links[current].Week;
                current = links[current].PrevTeam;
            }
            return week;
        }

        // Applies the chain ending with `team` vs `partner` in `week`, but only if the result is
        // still legal (no team twice in a week, no week over MaxGamesPerWeek) -- two links of one
        // chain can land in the same week, which the per-link checks during the search can't see.
        private static bool TryApplyChain(Dictionary<string, ChainLink> links, string team, int week, string partner,
            List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformerConfig config, ScheduleTransformResult result)
        {
            List<ScheduledGame> adds = new List<ScheduledGame>();
            List<ScheduledGame> removes = new List<ScheduledGame>();
            adds.Add(new ScheduledGame(week, team, partner));
            string current = team;
            while (links[current] != null)
            {
                ChainLink link = links[current];
                removes.Add(link.GivenUp);
                adds.Add(new ScheduledGame(link.Week, link.PrevTeam, link.Partner));
                current = link.PrevTeam;
            }

            List<ScheduledGame> after = new List<ScheduledGame>(surviving);
            foreach (ScheduledGame g in removes)
                after.Remove(g);
            after.AddRange(adds);
            Dictionary<string, bool> busy = new Dictionary<string, bool>();
            Dictionary<int, int> weekCounts = new Dictionary<int, int>();
            foreach (ScheduledGame g in after)
            {
                if (busy.ContainsKey(g.Week + "|" + g.Away) || busy.ContainsKey(g.Week + "|" + g.Home))
                    return false;
                busy[g.Week + "|" + g.Away] = true;
                busy[g.Week + "|" + g.Home] = true;
                weekCounts[g.Week] = (weekCounts.ContainsKey(g.Week) ? weekCounts[g.Week] : 0) + 1;
                if (weekCounts[g.Week] > WeekCap(g.Week, weekOrder, config))
                    return false;
            }

            foreach (ScheduledGame g in removes)
            {
                surviving.Remove(g);
                // A makeup game from an earlier chain being given up again was never in the input,
                // so it simply stops being "added" rather than counting as displaced.
                if (result.AddedGames.Remove(g))
                    result.RepeatMakeupGames.Remove(g);
                else
                    result.DisplacedGames.Add(g);
            }
            foreach (ScheduledGame g in adds)
            {
                bool isRepeat = false;
                foreach (ScheduledGame existing in surviving)
                {
                    if (PairKey(existing.Away, existing.Home) == PairKey(g.Away, g.Home))
                    {
                        isRepeat = true;
                        break;
                    }
                }
                surviving.Add(g);
                result.AddedGames.Add(g);
                if (isRepeat)
                    result.RepeatMakeupGames.Add(g);
            }
            return true;
        }

        private static string PairKey(string a, string b)
        {
            return (string.CompareOrdinal(a, b) < 0) ? (a + "|" + b) : (b + "|" + a);
        }

        private static void BuildTransformedLines(List<ScheduledGame> surviving, List<int> weekOrder, ScheduleTransformResult result)
        {
            Dictionary<int, List<ScheduledGame>> byWeek = new Dictionary<int, List<ScheduledGame>>();
            foreach (int w in weekOrder)
                byWeek[w] = new List<ScheduledGame>();
            foreach (ScheduledGame g in surviving)
            {
                if (!byWeek.ContainsKey(g.Week))
                {
                    byWeek[g.Week] = new List<ScheduledGame>();
                    weekOrder.Add(g.Week);
                }
                byWeek[g.Week].Add(g);
            }

            foreach (int w in weekOrder)
            {
                result.TransformedLines.Add("WEEK " + w);
                foreach (ScheduledGame g in byWeek[w])
                    result.TransformedLines.Add(g.Away + " at " + g.Home);
            }
        }
    }
}
