using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool
{
    // Generic, per-platform roster name-table capacity checker for NES, SNES TSB1 and SNES TSB2/TSB3. Their
    // InsertPlayer used to have no overflow protection at all -- a write that doesn't fit shifted blindly into
    // whatever table comes next; since 2026-10 (NES/CXROM 10-03, SNES 10-10, TSB2/TSB3 10-25) it rejects such a
    // write, as Genesis_TSB1Tool.InsertPlayer does. This planner still runs first so names get shortened to fit
    // instead of rejected (the old name kept). Deliberately built as
    // new code operating only through each tool's existing public surface
    // (GetTeamPlayers/InsertPlayer/ProcessText; TSB2/TSB3 per season), duplicating just the two fixed addresses
    // that bound each TSB1 platform's name pool, rather than modifying TecmoTool.cs/SNES_TecmoTool.cs at all.
    public class PlayerNamesConfig
    {
        public readonly string PlatformName;
        public readonly string[] Teams;
        public readonly int PoolStartAddress;
        public readonly int CapacityBoundaryAddress;

        public readonly string[] Positions;

        // Mirrors each platform's own InsertPlayer auto-abbreviation exactly (TecmoTool.cs /
        // SNES_TecmoTool.cs, both around their own InsertPlayer): whenever fname+lname exceeds
        // MaxNameLength, fname is unconditionally collapsed to a single initial + "." -- lname is
        // *additionally* truncated to 12 characters only if it alone exceeds LnameTruncateThreshold.
        // Needed so ResolveFit's "new length" reflects what will actually land in the ROM, not the raw
        // text length -- otherwise a name well over the cap looks like it needs far more room than the
        // real (auto-abbreviated) write actually will.
        public readonly int MaxNameLength;
        public readonly int LnameTruncateThreshold;

        // TSB2/TSB3: the season whose name table this area is (only that SEASON block of the text counts).
        // 0 for TSB1, which has one roster and no SEASON lines.
        public readonly int Season;

        public PlayerNamesConfig(string platformName, string[] teams, string[] positions, int poolStartAddress, int capacityBoundaryAddress,
            int maxNameLength, int lnameTruncateThreshold)
            : this(platformName, teams, positions, poolStartAddress, capacityBoundaryAddress, maxNameLength, lnameTruncateThreshold, 0)
        {
        }

        public PlayerNamesConfig(string platformName, string[] teams, string[] positions, int poolStartAddress, int capacityBoundaryAddress,
            int maxNameLength, int lnameTruncateThreshold, int season)
        {
            Season = season;
            PlatformName = platformName;
            Teams = teams;
            Positions = positions;
            PoolStartAddress = poolStartAddress;
            CapacityBoundaryAddress = capacityBoundaryAddress;
            MaxNameLength = maxNameLength;
            LnameTruncateThreshold = lnameTruncateThreshold;
        }

        public int Capacity { get { return CapacityBoundaryAddress - PoolStartAddress; } }

        // Same 30-position list as TecmoTool.positionNames -- NES has no DB1/DB2 (those are SNES-only
        // additions on top of the shared 30). Pool start confirmed via
        // TecmoTool.GetDataPosition("bills","QB1") == 0x6DA; capacity boundary is ShiftDataAfter's
        // hardcoded scan origin (0x0300F) -- see TecmoTool.cs. "qb BILLS" (not a real player name) is the
        // real stock content at that address: a genuine NES-era name-licensing gap, matching the same
        // pattern already documented for Genesis's "qb CHIEFS"/"qb VIKINGS".
        public static readonly PlayerNamesConfig Nes = new PlayerNamesConfig(
            "NES",
            new[] { "bills", "colts", "dolphins", "patriots", "jets", "bengals", "browns", "oilers",
                     "steelers", "broncos", "chiefs", "raiders", "chargers", "seahawks", "redskins", "giants",
                     "eagles", "cardinals", "cowboys", "bears", "lions", "packers", "vikings", "buccaneers",
                     "49ers", "rams", "saints", "falcons" },
            new[] { "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1",
                     "TE2", "C", "LG", "RG", "LT", "RT",
                     "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS", "K", "P" },
            0x6DA, 0x0300F, 16, 14);

        // Same 32-position list as SNES_TecmoTool.positionNames -- SNES adds DB1/DB2 after K/P (32
        // total, matching Genesis's own 32-slot roster). Pool start confirmed via
        // SNES_TecmoTool.GetDataPosition("bills","QB1") == 0x17873A ("jim KELLY"); capacity boundary is
        // ShiftDataAfter's nameNumberSegmentEnd constant (0x17b7f0) -- see SNES_TecmoTool.cs.
        public static readonly PlayerNamesConfig Snes = new PlayerNamesConfig(
            "SNES",
            new[] { "bills", "colts", "dolphins", "patriots", "jets", "bengals", "browns", "oilers",
                     "steelers", "broncos", "chiefs", "raiders", "chargers", "seahawks", "cowboys", "giants",
                     "eagles", "cardinals", "redskins", "bears", "lions", "packers", "vikings", "buccaneers",
                     "falcons", "rams", "saints", "49ers" },
            new[] { "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1",
                     "TE2", "C", "LG", "RG", "LT", "RT",
                     "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS",
                     "K", "P", "DB1", "DB2" },
            0x17873A, 0x17b7f0, 17, 16);
    }

    public class PlayerNameEntry
    {
        public string Team;
        public string Position;
        public string FirstName;
        public string LastName;
        public byte Jersey;

        public int Length { get { return 1 + FirstName.Length + LastName.Length; } }
    }

    public class PlayerNamesSuggestion
    {
        public string Team;
        public string Position;
        public string ProposedName;
        public string SuggestedName;
        public int BytesSaved;
        // True when a first initial alone wasn't enough and the last name was cut to fit as well.
        public bool LastNameTruncated;
    }

    public class PlayerNamesFitReport
    {
        // The area's PlayerNamesConfig.Season: ApplySuggestions renames only inside that SEASON block.
        public int Season;
        public bool Fits;
        public int NeededSlack;
        public int AvailableSlack;

        // True iff applying the auto-trims below, then the roster text itself in file order, would
        // never ask the platform's own (unmodified, non-bounds-checked) InsertPlayer for more room than
        // exists at that point. False iff UnresolvedFailures is non-empty.
        public bool SafeToApplyInOrder;

        public List<PlayerNamesSuggestion> Suggestions = new List<PlayerNamesSuggestion>();
        public List<PlayerNamesSuggestion> UnresolvedFailures = new List<PlayerNamesSuggestion>();

        // Roster slots the given text never mentions, freely available to shrink to a short placeholder
        // ("qb1 BILLS") before ever suggesting an abbreviation on a name the text actually specified.
        // Only actually written to the ROM when ResolveFit was called with autoApply:true.
        public List<PlayerNameEntry> AutoTrimmedPlayers = new List<PlayerNameEntry>();
        public int SlackFreedByAutoTrim;

        // Names over the platform's length limit, shortened by the same rule InsertPlayer uses (which
        // would otherwise warn about each one after the apply). Independent of room.
        public List<PlayerNamesSuggestion> LengthCapped = new List<PlayerNamesSuggestion>();

        public bool NeedsAttention
        {
            get { return AutoTrimmedPlayers.Count > 0 || Suggestions.Count > 0 || UnresolvedFailures.Count > 0 || LengthCapped.Count > 0; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (LengthCapped.Count > 0)
            {
                sb.AppendFormat("The following {0} name(s) are over the length limit and will be shortened:\r\n", LengthCapped.Count);
                foreach (PlayerNamesSuggestion s in LengthCapped)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  ->  '{3}'\r\n", s.Team, s.Position, s.ProposedName, s.SuggestedName);
                sb.Append("\r\n");
            }
            if (!Fits)
                sb.AppendFormat("This roster needs {0} more byte(s) than the ROM currently has free.\r\n\r\n",
                    Math.Max(0, NeededSlack - AvailableSlack));

            if (AutoTrimmedPlayers.Count > 0)
            {
                sb.AppendFormat("The following {0} player(s), which the roster text being applied never mentions, will be renamed to a short placeholder to free up {1} byte(s):\r\n",
                    AutoTrimmedPlayers.Count, SlackFreedByAutoTrim);
                foreach (PlayerNameEntry p in AutoTrimmedPlayers)
                    sb.AppendFormat("  {0,-12} {1,-5} {2} {3}  ->  {4} {5}\r\n", p.Team, p.Position, p.FirstName, p.LastName, p.Position.ToLower(), p.Team.ToUpper());
                sb.Append("\r\n");
            }

            List<PlayerNamesSuggestion> initials = Suggestions.FindAll(s => !s.LastNameTruncated);
            List<PlayerNamesSuggestion> cut = Suggestions.FindAll(s => s.LastNameTruncated);
            if (initials.Count > 0)
            {
                sb.AppendFormat("The following {0} player(s) that WERE specified will have their first name shortened, since trimming unspecified players alone wasn't enough:\r\n", initials.Count);
                foreach (PlayerNamesSuggestion s in initials)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  ->  '{3}'  (saves {4})\r\n", s.Team, s.Position, s.ProposedName, s.SuggestedName, s.BytesSaved);
                sb.Append("\r\n");
            }

            if (cut.Count > 0)
            {
                sb.AppendFormat("The following {0} player(s) still didn't fit with just an initial, so their last name will be cut short too:\r\n", cut.Count);
                foreach (PlayerNamesSuggestion s in cut)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  ->  '{3}'  (saves {4})\r\n", s.Team, s.Position, s.ProposedName, s.SuggestedName, s.BytesSaved);
                sb.Append("\r\n");
            }

            if (UnresolvedFailures.Count > 0)
            {
                sb.AppendFormat("WARNING: the following {0} player(s) cannot be fixed automatically at all -- there's no room left even for a one-letter last name. Unless something else frees up more room, these will keep their OLD name and jersey number instead of the ones specified (the rest of the line is still applied):\r\n", UnresolvedFailures.Count);
                foreach (PlayerNamesSuggestion f in UnresolvedFailures)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  (best possible saving: {3}, not enough)\r\n", f.Team, f.Position, f.ProposedName, f.BytesSaved);
            }

            return sb.ToString();
        }
    }

    // Every name area's fit report for one apply (CXROM has two areas; NES/SNES one), so the user sees
    // one report and confirms once. Built by PlayerNames.PlanFit (a dry run -- nothing written).
    public class RosterNamePlan
    {
        public List<PlayerNamesConfig> Configs = new List<PlayerNamesConfig>();
        public List<PlayerNamesFitReport> Reports = new List<PlayerNamesFitReport>();

        public bool NeedsAttention
        {
            get { return Reports.Exists(r => r.NeedsAttention); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Reports.Count; i++)
            {
                if (!Reports[i].NeedsAttention)
                    continue;
                if (Reports.Count > 1)
                    sb.AppendFormat("=== {0} ===\r\n", Configs[i].PlatformName);
                sb.Append(Reports[i].ToString()).Append("\r\n");
            }
            return sb.ToString();
        }

        // Carries out the plan: trims unmentioned players in the ROM (each area's ResolveFit with
        // autoApply) and returns rosterText with the reported names shortened, ready for ProcessText.
        // The areas are independent, so this matches the dry run the report was built from.
        public string Apply(ITecmoContent tool, string rosterText)
        {
            for (int i = 0; i < Reports.Count; i++)
            {
                if (!Reports[i].NeedsAttention)
                    continue;
                PlayerNamesFitReport report = PlayerNames.ResolveFitFor(tool, Configs[i], rosterText, true);
                rosterText = PlayerNames.ApplySuggestions(rosterText, report);
            }
            return rosterText;
        }
    }

    public static class PlayerNames
    {
        // Single source of truth for "which ROM families get this check" -- shared by MainGUI and
        // MainClass so they can't drift out of sync on which ROM_TYPEs are NES-like vs SNES-like.
        // NES/SNES TSB1 (and NES's CXRom variants, which extend TecmoTool and share its exact roster
        // layout) here; SNES TSB2/TSB3 through PlanFit (BuildTsb2Configs, one area per season table).
        // Genesis has its own Genesis_TSB1Tool.ResolveRosterFit for this exact problem.
        public static PlayerNamesConfig ResolveConfigFor(ITecmoTool tool)
        {
            List<PlayerNamesConfig> configs = ResolveConfigsFor(tool);
            return configs.Count > 0 ? configs[0] : null;
        }

        // One config per separately-bounded name area on the ROM: NES and SNES have one; the 32-team
        // CXROM hacks have two -- the main area (their first 28 teams, which are NOT NES's 28: CXROM has
        // titans/jaguars/panthers/ravens/texans there, in its own order) and a separate expansion area
        // for the 49ers/rams/seahawks/cardinals. Each area must be checked on its own: growth in one
        // can't use the other's free space. Empty for ROM types without a check (TSB2/TSB3/Genesis).
        public static List<PlayerNamesConfig> ResolveConfigsFor(ITecmoTool tool)
        {
            List<PlayerNamesConfig> configs = new List<PlayerNamesConfig>();
            switch (tool.RomVersion)
            {
                case ROM_TYPE.NES_ORIGINAL_TSB:
                    configs.Add(PlayerNamesConfig.Nes);
                    break;
                case ROM_TYPE.CXROM_v105:
                case ROM_TYPE.CXROM_v111:
                case ROM_TYPE.CXROM_18WEEK:
                    configs.AddRange(BuildCxRomConfigs((TecmoTool)tool));
                    break;
                case ROM_TYPE.SNES_TSB1:
                    configs.Add(PlayerNamesConfig.Snes);
                    break;
            }
            return configs;
        }

        // Dry run of every name area's check for tool + rosterText, as one plan: one report to show
        // (MainGUI's single dialog, MainClass's single printout) and one Apply. Empty plan for ROM
        // types without a check (Genesis has its own ResolveRosterFit).
        public static RosterNamePlan PlanFit(ITecmoContent tool, string rosterText)
        {
            RosterNamePlan plan = new RosterNamePlan();
            foreach (PlayerNamesConfig config in ConfigsFor(tool))
            {
                plan.Configs.Add(config);
                plan.Reports.Add(ResolveFitFor(tool, config, rosterText, false));
            }
            return plan;
        }

        private static List<PlayerNamesConfig> ConfigsFor(ITecmoContent tool)
        {
            TSBTool2.TSB2Tool tsb2 = tool as TSBTool2.TSB2Tool;
            if (tsb2 != null)
                return BuildTsb2Configs(tsb2);
            ITecmoTool nesOrSnesTool = tool as ITecmoTool;
            return nesOrSnesTool != null ? ResolveConfigsFor(nesOrSnesTool) : new List<PlayerNamesConfig>();
        }

        // SNES TSB2 keeps a name table per season (3), TSB3 one; each is its own area, bounded by the tool
        // (TSB2Tool.GetNameTableArea: its first name to the end of NAME_TABLE_SIZE). TSB3's list includes its
        // freeAgents and allTime "teams". No length cap: TSB2/TSB3's InsertPlayerName has none.
        private static List<PlayerNamesConfig> BuildTsb2Configs(TSBTool2.TSB2Tool tool)
        {
            bool tsb3 = tool.RomVersion == ROM_TYPE.SNES_TSB3;
            List<PlayerNamesConfig> configs = new List<PlayerNamesConfig>();
            for (int season = 1; season <= (tsb3 ? 1 : 3); season++)
            {
                int start, end;
                tool.GetNameTableArea(season, out start, out end);
                configs.Add(new PlayerNamesConfig(tsb3 ? "TSB3" : "TSB2 season " + season,
                    TSBTool2.TSB2Tool.teams.ToArray(), TSBTool2.TSB2Tool.positionNames.ToArray(), start, end,
                    int.MaxValue, int.MaxValue, season));
            }
            return configs;
        }

        // What the planner needs from a tool for one name area: each team's roster dump, a name write, and
        // the name parsing its real apply uses (TSB1's InputParser or TSB2/TSB3's, which also knows LB5).
        // TSB1 tools through ITecmoTool; TSB2/TSB3 through one season's table.
        private abstract class NameArea
        {
            public abstract string GetTeamPlayers(string team);
            public abstract void InsertPlayer(string team, string position, string fname, string lname, byte jersey);
            public abstract string GetFirstName(string line);
            public abstract string GetLastName(string line);
        }

        private class Tsb1NameArea : NameArea
        {
            private readonly ITecmoTool tool;
            public Tsb1NameArea(ITecmoTool tool) { this.tool = tool; }
            public override string GetTeamPlayers(string team) { return tool.GetTeamPlayers(team); }
            public override void InsertPlayer(string team, string position, string fname, string lname, byte jersey)
            {
                tool.InsertPlayer(team, position, fname, lname, jersey);
            }
            public override string GetFirstName(string line) { return InputParser.GetFirstName(line); }
            public override string GetLastName(string line) { return InputParser.GetLastName(line); }
        }

        private class Tsb2NameArea : NameArea
        {
            private readonly TSBTool2.TSB2Tool tool;
            private readonly int season;
            public Tsb2NameArea(TSBTool2.TSB2Tool tool, int season) { this.tool = tool; this.season = season; }
            public override string GetTeamPlayers(string team) { return tool.GetTeamPlayers(season, team); }
            public override void InsertPlayer(string team, string position, string fname, string lname, byte jersey)
            {
                tool.InsertPlayerName(season, team, position, fname, lname, jersey);
            }
            public override string GetFirstName(string line) { return TSBTool2.InputParser.GetFirstName(line); }
            public override string GetLastName(string line) { return TSBTool2.InputParser.GetLastName(line); }
        }

        private static NameArea AreaFor(ITecmoContent tool, PlayerNamesConfig config)
        {
            TSBTool2.TSB2Tool tsb2 = tool as TSBTool2.TSB2Tool;
            if (tsb2 != null)
                return new Tsb2NameArea(tsb2, config.Season);
            return new Tsb1NameArea((ITecmoTool)tool);
        }

        internal static PlayerNamesFitReport ResolveFitFor(ITecmoContent tool, PlayerNamesConfig config, string rosterText, bool autoApply)
        {
            return ResolveFit(AreaFor(tool, config), config, rosterText, autoApply);
        }

        // Built from the ROM itself rather than as fixed statics: the team list comes from the
        // CXROM team order TecmoToolFactory installs (indices 0-27 main area, 28-29 the Pro Bowl
        // "AFC"/"NFC" entries, 30-33 expansion), and each area's start is its first player's data
        // position (never moves -- only later records shift), which differs between v105/v111/18-week.
        // Boundaries match what the tool's own InsertPlayer/ShiftDataAfter enforce: the main area
        // ends at 0x300F (TecmoTool.nameAreaEnd), the expansion area at m_ExpansionSegmentEnd-17
        // (0x3FFDF, see CXRomTSBTool.ShiftDataAfter).
        private static List<PlayerNamesConfig> BuildCxRomConfigs(TecmoTool tool)
        {
            List<string> all = TecmoTool.Teams;
            string[] mainTeams = all.GetRange(0, 28).ToArray();
            string[] expansionTeams = all.GetRange(30, 4).ToArray();
            string[] positions = PlayerNamesConfig.Nes.Positions;

            List<PlayerNamesConfig> configs = new List<PlayerNamesConfig>();
            configs.Add(new PlayerNamesConfig("CXROM main teams", mainTeams, positions,
                tool.GetDataPosition(mainTeams[0], positions[0]), 0x300F,
                PlayerNamesConfig.Nes.MaxNameLength, PlayerNamesConfig.Nes.LnameTruncateThreshold));
            configs.Add(new PlayerNamesConfig("CXROM " + string.Join("/", expansionTeams), expansionTeams, positions,
                tool.GetDataPosition(expansionTeams[0], positions[0]), 0x3FFDF,
                PlayerNamesConfig.Nes.MaxNameLength, PlayerNamesConfig.Nes.LnameTruncateThreshold));
            return configs;
        }

        private static readonly Regex teamLineRegex = new Regex("TEAM\\s*=\\s*([0-9a-zA-Z]+)"); // TSB3 has 'freeAgents'
        private static readonly Regex seasonLineRegex = new Regex("^\\s*SEASON\\s+([1-3])"); // as TSB2/TSB3's InputParser

        // A TSB2/TSB3 area (areaSeason > 0) only takes its own SEASON block; TSB1 (0) takes everything.
        private static bool InSeason(int areaSeason, int textSeason)
        {
            return areaSeason == 0 || areaSeason == textSeason;
        }
        private static readonly Regex posLineRegex = new Regex("^([A-Z]+[1-5]?)\\s*,\\s*([a-zA-Z \\.\\-]+),");
        private static readonly Regex jerseyRegex = new Regex("#([0-9a-fA-F]{1,2})");

        // Reads every current player for every team in config.Teams via the already-public
        // ITecmoTool.GetTeamPlayers dump (the same text ProcessText/InputParser round-trips), reusing
        // InputParser's own GetFirstName/GetLastName so this can't drift out of sync with a real apply.
        public static List<PlayerNameEntry> ExtractCurrentRoster(ITecmoTool tool, PlayerNamesConfig config)
        {
            return ExtractCurrentRoster(new Tsb1NameArea(tool), config);
        }

        private static List<PlayerNameEntry> ExtractCurrentRoster(NameArea tool, PlayerNamesConfig config)
        {
            List<PlayerNameEntry> result = new List<PlayerNameEntry>();
            foreach (string team in config.Teams)
            {
                string dump = tool.GetTeamPlayers(team);
                if (dump == null)
                    continue;

                foreach (string rawLine in dump.Replace("\r\n", "\n").Split('\n'))
                {
                    string line = rawLine.Trim();
                    Match posMatch = posLineRegex.Match(line);
                    if (!posMatch.Success)
                        continue;
                    string pos = posMatch.Groups[1].ToString();
                    if (Array.IndexOf(config.Positions, pos) < 0)
                        continue;

                    string fname = tool.GetFirstName(line);
                    string lname = tool.GetLastName(line);
                    if (string.IsNullOrEmpty(fname) || string.IsNullOrEmpty(lname))
                        continue;

                    Match jerseyMatch = jerseyRegex.Match(line);
                    byte jersey = jerseyMatch.Success ? Convert.ToByte(jerseyMatch.Groups[1].ToString(), 16) : (byte)0;

                    PlayerNameEntry entry = new PlayerNameEntry();
                    entry.Team = team;
                    entry.Position = pos;
                    entry.FirstName = fname;
                    entry.LastName = lname;
                    entry.Jersey = jersey;
                    result.Add(entry);
                }
            }
            return result;
        }

        private class OverlayEntry
        {
            public string Team;
            public string Position;
            public string FirstName;
            public string LastName;
            public string RawName;   // as written in the text, before the platform's length cap
        }

        // Parses a (possibly partial) roster text block -- same TEAM=/POS-line shape ProcessText itself
        // consumes -- into one entry per recognized line. No tool/rom access needed: only the *proposed*
        // name matters here, current length comes from ExtractCurrentRoster separately. Each parsed name
        // is immediately clamped to config's own platform cap (see PlayerNamesConfig.MaxNameLength) so
        // every entry already reflects what InsertPlayer will really write, not the raw text.
        private static List<OverlayEntry> ParseOverlayText(NameArea tool, string rosterText, PlayerNamesConfig config)
        {
            List<OverlayEntry> entries = new List<OverlayEntry>();
            if (rosterText == null)
                return entries;

            string currentTeam = null;
            int currentSeason = 1;
            foreach (string rawLine in rosterText.Replace("\r\n", "\n").Split('\n'))
            {
                string line = rawLine.Trim();
                Match seasonMatch = seasonLineRegex.Match(line);
                if (seasonMatch.Success)
                {
                    currentSeason = int.Parse(seasonMatch.Groups[1].Value);
                    continue;
                }
                Match teamMatch = teamLineRegex.Match(line);
                if (teamMatch.Success)
                {
                    currentTeam = teamMatch.Groups[1].ToString();
                    continue;
                }
                if (currentTeam == null || !InSeason(config.Season, currentSeason))
                    continue;

                Match posMatch = posLineRegex.Match(line);
                if (!posMatch.Success)
                    continue;
                string pos = posMatch.Groups[1].ToString();

                string fname = tool.GetFirstName(line);
                string lname = tool.GetLastName(line);
                if (string.IsNullOrEmpty(fname) || string.IsNullOrEmpty(lname))
                    continue;
                string rawName = fname + " " + lname;
                ApplyPlatformCap(config, ref fname, ref lname);

                OverlayEntry entry = new OverlayEntry();
                entry.RawName = rawName;
                entry.Team = currentTeam;
                entry.Position = pos;
                entry.FirstName = fname;
                entry.LastName = lname;
                entries.Add(entry);
            }
            return entries;
        }

        private static void ApplyPlatformCap(PlayerNamesConfig config, ref string fname, ref string lname)
        {
            if (fname.Length + lname.Length <= config.MaxNameLength)
                return;
            if (lname.Length > config.LnameTruncateThreshold)
                lname = lname.Substring(0, 12);
            fname = fname.Substring(0, 1) + ".";
        }

        // Checks whether rosterText can safely be applied to tool's current ROM, and (if autoApply)
        // renames unspecified roster slots to a short placeholder via the tool's own public
        // InsertPlayer to free up room first. Never applies rosterText's own specified names itself --
        // that's still the caller's separate, existing ProcessText call, exactly like
        // Genesis_TSB1Tool.ResolveRosterFit's own contract.
        //
        // Uses the same "max cumulative net growth over any prefix, in file order" technique as
        // Genesis_TSB1Tool.ResolveRosterFit to compute NeededSlack, since a real apply goes through
        // ProcessText's own top-to-bottom InsertPlayer calls, and NES/SNES's InsertPlayer rejects any
        // write that doesn't fit at that moment -- checking only the *total* would miss an intermediate
        // overflow (a rejected name) that a later shrink in the file can't retroactively rescue.
        public static PlayerNamesFitReport ResolveFit(ITecmoTool tool, PlayerNamesConfig config, string rosterText, bool autoApply)
        {
            return ResolveFit(new Tsb1NameArea(tool), config, rosterText, autoApply);
        }

        private static PlayerNamesFitReport ResolveFit(NameArea tool, PlayerNamesConfig config, string rosterText, bool autoApply)
        {
            PlayerNamesFitReport report = new PlayerNamesFitReport();
            report.Season = config.Season;

            List<PlayerNameEntry> current = ExtractCurrentRoster(tool, config);
            List<OverlayEntry> overlay = ParseOverlayText(tool, rosterText, config);

            Dictionary<string, PlayerNameEntry> currentByKey = new Dictionary<string, PlayerNameEntry>();
            int currentTotal = 0;
            foreach (PlayerNameEntry e in current)
            {
                currentByKey[e.Team + "|" + e.Position] = e;
                currentTotal += e.Length;
            }
            report.AvailableSlack = config.Capacity - currentTotal;

            Dictionary<string, OverlayEntry> overlayByKey = new Dictionary<string, OverlayEntry>();
            foreach (OverlayEntry o in overlay)
            {
                overlayByKey[o.Team + "|" + o.Position] = o;
                string capped = o.FirstName + " " + o.LastName;
                if (o.RawName != capped && currentByKey.ContainsKey(o.Team + "|" + o.Position))
                {
                    PlayerNamesSuggestion s = new PlayerNamesSuggestion();
                    s.Team = o.Team;
                    s.Position = o.Position;
                    s.ProposedName = o.RawName;
                    s.SuggestedName = capped;
                    s.BytesSaved = o.RawName.Length - capped.Length;
                    report.LengthCapped.Add(s);
                }
            }

            int cumulative = 0;
            int neededSlack = 0;
            foreach (OverlayEntry o in overlay)
            {
                PlayerNameEntry existing;
                if (!currentByKey.TryGetValue(o.Team + "|" + o.Position, out existing))
                    continue;
                int delta = (1 + o.FirstName.Length + o.LastName.Length) - existing.Length;
                cumulative += delta;
                if (cumulative > neededSlack)
                    neededSlack = cumulative;
            }
            report.NeededSlack = neededSlack;
            report.Fits = neededSlack <= report.AvailableSlack;

            if (!report.Fits)
            {
                List<PlayerNameEntry> candidates = new List<PlayerNameEntry>();
                foreach (PlayerNameEntry e in current)
                {
                    if (overlayByKey.ContainsKey(e.Team + "|" + e.Position))
                        continue;
                    candidates.Add(e);
                }
                candidates.Sort((a, b) => CandidateSavings(b).CompareTo(CandidateSavings(a)));

                int freed = 0;
                foreach (PlayerNameEntry candidate in candidates)
                {
                    if (report.AvailableSlack + freed >= neededSlack)
                        break;
                    int savings = CandidateSavings(candidate);
                    if (savings <= 0)
                        continue;

                    if (autoApply)
                        tool.InsertPlayer(candidate.Team, candidate.Position, candidate.Position.ToLower(), candidate.Team.ToUpper(), candidate.Jersey);

                    report.AutoTrimmedPlayers.Add(candidate);
                    freed += savings;
                }
                report.SlackFreedByAutoTrim = freed;
            }

            int runningSlack = report.AvailableSlack + report.SlackFreedByAutoTrim;
            foreach (OverlayEntry o in overlay)
            {
                PlayerNameEntry existing;
                if (!currentByKey.TryGetValue(o.Team + "|" + o.Position, out existing))
                    continue;

                int newLength = 1 + o.FirstName.Length + o.LastName.Length;
                int delta = newLength - existing.Length;
                if (delta <= runningSlack)
                {
                    runningSlack -= delta;
                    continue;
                }

                int maxSavings = Math.Max(0, o.FirstName.Length - 2); // "x." is 2 chars
                int abbreviatedDelta = delta - maxSavings;
                if (maxSavings > 0 && abbreviatedDelta <= runningSlack)
                {
                    PlayerNamesSuggestion suggestion = new PlayerNamesSuggestion();
                    suggestion.Team = o.Team;
                    suggestion.Position = o.Position;
                    suggestion.ProposedName = o.FirstName + " " + o.LastName;
                    suggestion.SuggestedName = o.FirstName.Substring(0, 1).ToLower() + ". " + o.LastName;
                    suggestion.BytesSaved = maxSavings;
                    report.Suggestions.Add(suggestion);
                    runningSlack -= abbreviatedDelta;
                    continue;
                }

                // An initial alone isn't enough: also cut the last name to whatever fits. Better a
                // clipped name than a rejected one -- a rejected name keeps the OLD player's name and
                // jersey while the rest of the line (face, ratings, sim data) still applies.
                string shortFirst = maxSavings > 0 ? o.FirstName.Substring(0, 1).ToLower() + "." : o.FirstName;
                int lastRoom = existing.Length + runningSlack - 1 - shortFirst.Length;
                if (lastRoom >= 1)
                {
                    PlayerNamesSuggestion suggestion = new PlayerNamesSuggestion();
                    suggestion.Team = o.Team;
                    suggestion.Position = o.Position;
                    suggestion.ProposedName = o.FirstName + " " + o.LastName;
                    suggestion.SuggestedName = shortFirst + " " + o.LastName.Substring(0, lastRoom);
                    suggestion.BytesSaved = (o.FirstName.Length - shortFirst.Length) + (o.LastName.Length - lastRoom);
                    suggestion.LastNameTruncated = true;
                    report.Suggestions.Add(suggestion);
                    runningSlack -= (1 + shortFirst.Length + lastRoom) - existing.Length;
                }
                else
                {
                    PlayerNamesSuggestion failure = new PlayerNamesSuggestion();
                    failure.Team = o.Team;
                    failure.Position = o.Position;
                    failure.ProposedName = o.FirstName + " " + o.LastName;
                    failure.SuggestedName = null;
                    failure.BytesSaved = maxSavings;
                    report.UnresolvedFailures.Add(failure);
                }
            }
            report.SafeToApplyInOrder = report.UnresolvedFailures.Count == 0;

            return report;
        }

        private static readonly Regex playerLineNameRegex = new Regex("^(\\s*[A-Z]+[1-5]?\\s*,\\s*)([^,]+)(,.*)$");

        // Rewrites the roster text so each of report.Suggestions' and report.LengthCapped's players is
        // given its shortened name -- what the report tells the user will happen. Matches players the same
        // way ResolveFit/ProcessText do (current "TEAM =" block + position); every other line is left
        // exactly as it was. Run before ProcessText, so the shortened names are what actually get
        // written, and the real apply follows the same space budget ResolveFit simulated.
        public static string ApplySuggestions(string rosterText, PlayerNamesFitReport report)
        {
            if (rosterText == null || report == null || (report.Suggestions.Count == 0 && report.LengthCapped.Count == 0))
                return rosterText;

            // Room suggestions are made from the already-capped name, so they win.
            Dictionary<string, string> suggested = new Dictionary<string, string>();
            foreach (PlayerNamesSuggestion s in report.LengthCapped)
                suggested[s.Team + "|" + s.Position] = s.SuggestedName;
            foreach (PlayerNamesSuggestion s in report.Suggestions)
                suggested[s.Team + "|" + s.Position] = s.SuggestedName;
            return RenamePlayersInText(rosterText, suggested, report.Season);
        }

        // Gives each "team|POSITION" player in namesByTeamPos its new name in rosterText; every other
        // line is left exactly as it was. Shared with Genesis_TSB1Tool.RosterFitReport.ApplySuggestions.
        public static string RenamePlayersInText(string rosterText, Dictionary<string, string> namesByTeamPos)
        {
            return RenamePlayersInText(rosterText, namesByTeamPos, 0);
        }

        // season > 0 (TSB2/TSB3): only players inside that SEASON block (text before any SEASON line is season 1).
        public static string RenamePlayersInText(string rosterText, Dictionary<string, string> namesByTeamPos, int season)
        {
            if (rosterText == null || namesByTeamPos.Count == 0)
                return rosterText;
            Dictionary<string, string> suggested = namesByTeamPos;
            string[] lines = rosterText.Split('\n');
            string currentTeam = null;
            int currentSeason = 1;
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                Match seasonMatch = seasonLineRegex.Match(trimmed);
                if (seasonMatch.Success)
                {
                    currentSeason = int.Parse(seasonMatch.Groups[1].Value);
                    continue;
                }
                Match teamMatch = teamLineRegex.Match(trimmed);
                if (teamMatch.Success)
                {
                    currentTeam = teamMatch.Groups[1].ToString();
                    continue;
                }
                if (currentTeam == null || !InSeason(season, currentSeason))
                    continue;
                Match posMatch = posLineRegex.Match(trimmed);
                if (!posMatch.Success)
                    continue;

                string newName;
                if (!suggested.TryGetValue(currentTeam + "|" + posMatch.Groups[1].ToString(), out newName))
                    continue;
                Match nameMatch = playerLineNameRegex.Match(lines[i].TrimEnd('\r'));
                if (nameMatch.Success)
                    lines[i] = nameMatch.Groups[1].Value + newName + nameMatch.Groups[3].Value + (lines[i].EndsWith("\r") ? "\r" : "");
            }
            return string.Join("\n", lines);
        }

        private static int CandidateSavings(PlayerNameEntry e)
        {
            int oldLength = e.Length;
            int newLength = 1 + e.Position.Length + e.Team.Length;
            return oldLength - newLength;
        }

        // Defense-in-depth verification for platforms whose own InsertPlayer has no bounds check at
        // all: replays applyEverything against a *clone* of originalRom, then confirms the windowSize
        // bytes starting at config.CapacityBoundaryAddress -- which belong to whichever table comes
        // right after the name pool -- are completely unchanged. A correct name-table edit should never
        // reach that boundary regardless of how the shift/pointer math inside the untouched,
        // unmodified InsertPlayer happens to work; if it moved, something in ResolveFit's arithmetic
        // didn't match the real ROM's behavior and the result must not be trusted. Never mutates
        // originalRom.
        public static bool VerifyRomSafety(byte[] originalRom, PlayerNamesConfig config, Action<ITecmoTool> applyEverything, int windowSize)
        {
            byte[] clone = (byte[])originalRom.Clone();
            byte[] before = new byte[windowSize];
            Array.Copy(clone, config.CapacityBoundaryAddress, before, 0, windowSize);

            ITecmoTool cloneTool = (ITecmoTool)TecmoToolFactory.GetToolForRom(clone);
            applyEverything(cloneTool);

            byte[] after = new byte[windowSize];
            Array.Copy(cloneTool.OutputRom, config.CapacityBoundaryAddress, after, 0, windowSize);

            for (int i = 0; i < windowSize; i++)
                if (before[i] != after[i])
                    return false;
            return true;
        }
    }
}
