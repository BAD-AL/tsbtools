using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool
{
    // Generic, per-platform roster name-table capacity checker for NES and SNES, whose own InsertPlayer
    // (unlike Genesis_TSB1Tool's, see Genesis_TSB1Tool.InsertPlayer) has no overflow protection at all --
    // a write that doesn't fit just shifts blindly into whatever table comes next. Deliberately built as
    // new code operating only through each tool's existing public ITecmoTool surface
    // (GetTeamPlayers/InsertPlayer/ProcessText), duplicating just the two fixed addresses that bound
    // each platform's name pool, rather than modifying TecmoTool.cs/SNES_TecmoTool.cs at all.
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

        public PlayerNamesConfig(string platformName, string[] teams, string[] positions, int poolStartAddress, int capacityBoundaryAddress,
            int maxNameLength, int lnameTruncateThreshold)
        {
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
    }

    public class PlayerNamesFitReport
    {
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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

            if (Suggestions.Count > 0)
            {
                sb.AppendFormat("The following {0} player(s) that WERE specified will have their first name shortened, since trimming unspecified players alone wasn't enough:\r\n", Suggestions.Count);
                foreach (PlayerNamesSuggestion s in Suggestions)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  ->  '{3}'  (saves {4})\r\n", s.Team, s.Position, s.ProposedName, s.SuggestedName, s.BytesSaved);
                sb.Append("\r\n");
            }

            if (UnresolvedFailures.Count > 0)
            {
                sb.AppendFormat("WARNING: the following {0} player(s) cannot be fixed automatically at all -- their name is too short to abbreviate enough. Unless something else frees up more room, these will keep their OLD name instead of the one specified:\r\n", UnresolvedFailures.Count);
                foreach (PlayerNamesSuggestion f in UnresolvedFailures)
                    sb.AppendFormat("  {0,-12} {1,-5} '{2}'  (best possible saving: {3}, not enough)\r\n", f.Team, f.Position, f.ProposedName, f.BytesSaved);
            }

            return sb.ToString();
        }
    }

    public static class PlayerNames
    {
        // Single source of truth for "which ROM families get this check" -- shared by MainGUI and
        // MainClass so they can't drift out of sync on which ROM_TYPEs are NES-like vs SNES-like.
        // Deliberately limited to NES/SNES TSB1 (and NES's CXRom variants, which extend TecmoTool and
        // share its exact roster layout) -- TSB2/TSB3/Genesis are excluded, matching the user's explicit
        // "NES and SNES are vital, TSB2/TSB3 less so" priority, and Genesis already has its own
        // Genesis_TSB1Tool.ResolveRosterFit for this exact problem.
        public static PlayerNamesConfig ResolveConfigFor(ITecmoTool tool)
        {
            switch (tool.RomVersion)
            {
                case ROM_TYPE.NES_ORIGINAL_TSB:
                case ROM_TYPE.CXROM_v105:
                case ROM_TYPE.CXROM_v111:
                case ROM_TYPE.CXROM_18WEEK:
                    return PlayerNamesConfig.Nes;
                case ROM_TYPE.SNES_TSB1:
                    return PlayerNamesConfig.Snes;
                default:
                    return null;
            }
        }

        private static readonly Regex teamLineRegex = new Regex("TEAM\\s*=\\s*([0-9a-z]+)");
        private static readonly Regex posLineRegex = new Regex("^([A-Z]+[1-4]?)\\s*,\\s*([a-zA-Z \\.\\-]+),");
        private static readonly Regex jerseyRegex = new Regex("#([0-9a-fA-F]{1,2})");

        // Reads every current player for every team in config.Teams via the already-public
        // ITecmoTool.GetTeamPlayers dump (the same text ProcessText/InputParser round-trips), reusing
        // InputParser's own GetFirstName/GetLastName so this can't drift out of sync with a real apply.
        public static List<PlayerNameEntry> ExtractCurrentRoster(ITecmoTool tool, PlayerNamesConfig config)
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

                    string fname = InputParser.GetFirstName(line);
                    string lname = InputParser.GetLastName(line);
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
        }

        // Parses a (possibly partial) roster text block -- same TEAM=/POS-line shape ProcessText itself
        // consumes -- into one entry per recognized line. No tool/rom access needed: only the *proposed*
        // name matters here, current length comes from ExtractCurrentRoster separately. Each parsed name
        // is immediately clamped to config's own platform cap (see PlayerNamesConfig.MaxNameLength) so
        // every entry already reflects what InsertPlayer will really write, not the raw text.
        private static List<OverlayEntry> ParseOverlayText(string rosterText, PlayerNamesConfig config)
        {
            List<OverlayEntry> entries = new List<OverlayEntry>();
            if (rosterText == null)
                return entries;

            string currentTeam = null;
            foreach (string rawLine in rosterText.Replace("\r\n", "\n").Split('\n'))
            {
                string line = rawLine.Trim();
                Match teamMatch = teamLineRegex.Match(line);
                if (teamMatch.Success)
                {
                    currentTeam = teamMatch.Groups[1].ToString();
                    continue;
                }
                if (currentTeam == null)
                    continue;

                Match posMatch = posLineRegex.Match(line);
                if (!posMatch.Success)
                    continue;
                string pos = posMatch.Groups[1].ToString();

                string fname = InputParser.GetFirstName(line);
                string lname = InputParser.GetLastName(line);
                if (string.IsNullOrEmpty(fname) || string.IsNullOrEmpty(lname))
                    continue;
                ApplyPlatformCap(config, ref fname, ref lname);

                OverlayEntry entry = new OverlayEntry();
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
        // ProcessText's own top-to-bottom InsertPlayer calls and NES/SNES's InsertPlayer has no
        // per-call bounds check at all -- checking only the *total* would miss an intermediate overflow
        // that a later shrink in the file can't retroactively rescue.
        public static PlayerNamesFitReport ResolveFit(ITecmoTool tool, PlayerNamesConfig config, string rosterText, bool autoApply)
        {
            PlayerNamesFitReport report = new PlayerNamesFitReport();

            List<PlayerNameEntry> current = ExtractCurrentRoster(tool, config);
            List<OverlayEntry> overlay = ParseOverlayText(rosterText, config);

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
                overlayByKey[o.Team + "|" + o.Position] = o;

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
