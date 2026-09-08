using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool
{
    /// <summary>
    /// Genesis (Sega Mega Drive) Tecmo Super Bowl 1993. Modeled directly on SNES_TecmoTool (single
    /// season, own local team list/pointer-table helpers, not shared with TecmoTool/StaticUtils) rather
    /// than the season-parameterized TSB2Tool/TSB3Tool pattern -- Genesis and SNES TSB1 were released
    /// around the same time and are otherwise structurally similar. This ROM has 3 baked-in
    /// season schedules. Only one is read/written -- specifically season 3, the real 1993 schedule
    /// (confirmed by its unique 18-week/bye-week structure and by decoding its actual week 1 games; see
    /// Genesis_ScheduleHelper's own comment). Seasons 1 and 2 (presumably 1991/1992) exist in the ROM
    /// but are out of scope.
    ///
    /// </summary>
    public class Genesis_TSB1Tool : ITecmoTool, ITecmoContent
    {
        private byte[] outputRom;
        private bool mShowOffPref = true;

        private const int ROM_LENGTH = TecmoToolFactory.GENESIS_TSB1_LEN;

        // Team order/indexing matches every table in this ROM (0=Bills...27=49ers) -- confirmed
        // against TSBTool_Test\Genesis_TSB1_ROM_Findings.md, and identical to SNES_TecmoTool's own team order.
        private static string[] teams =
        {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cowboys", "giants", "eagles",   "cardinals", "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "rams",   "saints",   "49ers"
        };

        // Roster slot order -- confirmed against TSBTool_Test\Genesis_TSB1_ROM_Findings.md's
        // "Roster slot order -- solved" section: all 32 slots use exactly SNES TSB1's own
        // positionNames order (190/192 name+jersey matches across 6 trusted teams). Used to index the
        // roster (name/jersey), attribute, and offensive sim-data tables alike -- all three share the
        // same positionIndex.
        private static string[] positionNames =
        {
            "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2",
            "C", "LG", "RG", "LT", "RT",
            "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS",
            "K", "P", "DB1", "DB2"
        };

        private static int GetPositionIndex(string position)
        {
            return Array.IndexOf(positionNames, position);
        }

        public Genesis_TSB1Tool(byte[] rom)
        {
            outputRom = rom;
        }

        public virtual ROM_TYPE RomVersion { get { return ROM_TYPE.GENESIS_TSB1; } }

        public byte[] OutputRom
        {
            get { return outputRom; }
            set { outputRom = value; }
        }

        public bool ShowOffPref
        {
            get { return mShowOffPref; }
            set { mShowOffPref = value; }
        }

        // use this instead of directly setting data in OutputRom
        public void SetByte(int location, byte b)
        {
            outputRom[location] = b;
        }

        public void SaveRom(string filename)
        {
            StaticUtils.SaveRom(filename, outputRom);
        }

        public string GetKey()
        {
            return String.Format(
@"# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/
# Editing: Tecmo Super Bowl I (Genesis) [{0}]
# 
# Double click on a team name (or playbook) to bring up the edit Team GUI.
# Double click on a player to bring up the edit player GUI (Click 'Sim Data'
#   button to find out more on Sim Data). 
# Key
# -- Quarterbacks:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PS, PC, PA, APB, [Sim rush, Sim pass, Sim Pocket].
# -- Offensive Skill players (non-QB):
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, BC, REC, [Sim rush, Sim catch, Sim punt Ret, Sim kick ret].
# -- Offensive Linemen:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP
# -- Defensive Players:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PI, QU, [Sim pass rush, Sim coverage].
# -- Punters and Kickers:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, KA, AKB,[ Sim kicking ability].
# TEAM:\n
#  name, SimData  0x<offense><defense><offense preference>
#  Offensive pref values 0-3. 
#     0 = Little more rushing, 1 = Heavy Rushing,
#     2 = little more passing, 3 = Heavy Passing.
", this.RomVersion);
        }

        public static int GetTeamIndex(string team)
        {
            return Array.IndexOf(teams, team);
        }

        public static string GetTeamFromIndex(int index)
        {
            if (index < 0 || index >= teams.Length)
                return null;
            return teams[index];
        }

        public bool IsValidTeam(string team)
        {
            return GetTeamIndex(team) > -1;
        }

        public bool IsValidPosition(string position)
        {
            return GetPositionIndex(position) > -1;
        }

        public void ProcessText(string text)
        {
            InputParser parser = new InputParser(this);
            text = text.Replace("\r\n", "\n");
            string[] lines = text.Split("\n".ToCharArray());
            parser.ProcessLines(lines);
        }

        public void ApplySet(string line)
        {
            StaticUtils.ApplySimpleSet(line, this);
        }

        // ---- ITecmoContent season-shaped members: this is a single-season tool (season 1 only), so
        // these ignore the season parameter and delegate to the single-season methods below, exactly
        // like SNES_TecmoTool.GetAll(int)/GetSchedule(int)/GetProBowlPlayers(int) do. ----

        public string GetAll(int season) { return GetAll(); }
        public string GetSchedule(int season) { return GetSchedule(); }
        public string GetProBowlPlayers(int season) { return GetProBowlPlayers(); }

        // ---- Not yet implemented. Each of these lands in a specific phase of
        // compressed-riding-rossum.md; see TSBTool_Test\Genesis_TSB1_ROM_Findings.md for the underlying ROM
        // research. Left as explicit NotImplementedException (rather than a silent no-op) so a caller
        // finds out immediately rather than getting corrupted/empty data. ----

        private static Exception NotYetMapped(string member)
        {
            return new NotImplementedException(string.Format(
                "Genesis_TSB1Tool.{0} is not implemented yet -- see TSBTool_Test\\Genesis_TSB1_ROM_Findings.md.", member));
        }

        // Mirrors SNES_TecmoTool.GetAll/GetTeamPlayers exactly in shape and section order. One section
        // SNES includes is deliberately skipped here regardless of the shared TecmoTool.Show* flags:
        // team offensive formation (TecmoTool.ShowTeamFormation) -- that would throw, since formations
        // aren't mapped for Genesis yet (see Genesis_TSB1_ROM_Findings.md). The COLORS line omits champColors
        // (Div/Conf Champ colors, also unmapped) but otherwise mirrors SNES exactly -- GetUniformUsage
        // is a real (no-op) method now, not unmapped, so it's safe to call unconditionally.
        public string GetAll()
        {
            StringBuilder all = new StringBuilder(30 * 41 * positionNames.Length);
            all.Append(string.Format("YEAR={0}\n", GetYear()));
            for (int i = 0; i < teams.Length; i++)
                all.Append(GetTeamPlayers(teams[i]));
            return all.ToString();
        }

        public string GetTeamPlayers(string team)
        {
            StringBuilder result = new StringBuilder();

            byte teamSimData = GetTeamSimData(team);
            string data = teamSimData < 0xf ? string.Format("0{0:x}", teamSimData) : string.Format("{0:x}", teamSimData);
            if (ShowOffPref)
                data += GetTeamSimOffensePref(team);
            result.Append(string.Format("TEAM = {0} SimData=0x{1}", team, data));
            result.Append("\n");

            if (TecmoTool.ShowPlaybook)
                result.Append(string.Format("{0}\n", GetPlaybook(team)));

            if (TecmoTool.ShowColors)
            {
                StringBuilder colorsLine = new StringBuilder("COLORS ");
                colorsLine.Append(GetGameUniform(team));
                string uniformUsage = GetUniformUsage(team);
                if (!string.IsNullOrEmpty(uniformUsage))
                    colorsLine.Append(", " + uniformUsage);
                result.Append(colorsLine.ToString() + "\n");
            }

            if (TecmoTool.ShowTeamStrings)
            {
                int teamIndex = GetTeamIndex(team);
                result.Append(string.Format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n",
                    GetTeamAbbreviation(teamIndex), GetTeamCity(teamIndex), GetTeamName(teamIndex)));
            }

            for (int i = 0; i < positionNames.Length; i++)
                result.Append(string.Format("{0}\n", GetPlayerData(team, positionNames[i], true, true, true, true, true)));

            result.Append(string.Format("{0}\n", GetReturnTeam(team)));
            result.Append(string.Format("KR, {0}\nPR, {1}\n", GetKickReturner(team), GetPuntReturner(team)));
            result.Append("\n");

            return result.ToString();
        }

        #region Team string table (abbreviation/city/nickname) -- Phase 1

        // Pointer array: file 0x45C4-0x47C8, 129 x 4-byte entries, format "00 00 hh ll" (value in the
        // low 2 bytes, zero-padded high -- confirmed NOT reversed, unlike the schedule table's pointer
        // format). Each pointer value is a direct absolute file offset (no base-address adjustment
        // needed, unlike SNES's "pointerVal - 0x8000"). String data: file 0x47C8-0x4ABF, packed
        // back-to-back with implicit length via adjacent-pointer subtraction. Confirmed directly
        // against the real ROM: there is NO delimiter byte between entries (not even a null
        // terminator) -- so unlike SNES's team string table, no '*'-for-null-byte substitution
        // convention is needed here.
        //
        // Unlike SNES (which packs abbreviation+city+nickname into one combined string per team, using
        // '*' as an internal delimiter/terminator hack), Genesis stores these as three genuinely
        // separate index ranges within the one shared pointer table -- so each field is independently
        // addressable and this implementation is simpler than SNES's equivalent.
        private const int teamStringTablePointerStart = 0x45C4;
        private const int teamStringTableCount = 129;
        private const int teamStringTableDataEnd = 0x4ABF; // exclusive

        private const int abbreviationIndexBase = 0;  // indices 0-27
        private const int cityIndexBase = 32;          // indices 32-59
        private const int nicknameIndexBase = 64;       // indices 64-91

        public int NumberOfStringsInTeamStringTable { get { return teamStringTableCount; } }

        private int GetTeamStringTableLocation(int stringIndex, out int length)
        {
            int pointerLoc = teamStringTablePointerStart + 4 * stringIndex;
            int thisPtr = (outputRom[pointerLoc + 2] << 8) + outputRom[pointerLoc + 3];
            int nextPtr = (outputRom[pointerLoc + 6] << 8) + outputRom[pointerLoc + 7];
            length = nextPtr - thisPtr;
            return thisPtr;
        }

        public string GetTeamStringTableString(int stringIndex)
        {
            int length;
            int loc = GetTeamStringTableLocation(stringIndex, out length);
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)outputRom[loc + i];
            return new string(chars);
        }

        public void SetTeamStringTableString(int stringIndex, string newValue)
        {
            string oldValue = GetTeamStringTableString(stringIndex);
            if (oldValue == newValue)
                return;

            int shiftAmount = newValue.Length - oldValue.Length;
            if (shiftAmount != 0)
            {
                int currentPointerLocation = teamStringTablePointerStart + 4 * stringIndex;
                int lastPointerLocation = teamStringTablePointerStart + 4 * (teamStringTableCount - 1);

                // Read the OLD (pre-shift) location of the *next* string's data before adjusting any
                // pointers, so the byte-shift range below reflects where the bytes physically are right
                // now, not where the (about to be updated) pointer will claim they are.
                int junk;
                int oldNextStringLocation = GetTeamStringTableLocation(stringIndex + 1, out junk);

                AdjustTeamStringTablePointers(currentPointerLocation, shiftAmount, lastPointerLocation);

                if (shiftAmount < 0)
                    ShiftBytesUp(oldNextStringLocation, teamStringTableDataEnd - 1, shiftAmount);
                else
                    ShiftBytesDown(oldNextStringLocation, teamStringTableDataEnd - 1, shiftAmount);
            }

            int writeLength;
            int writeLoc = GetTeamStringTableLocation(stringIndex, out writeLength);
            for (int i = 0; i < newValue.Length; i++)
                outputRom[writeLoc + i] = (byte)newValue[i];
        }

        // Adds `change` to every pointer strictly after currentPointerLocation, through and including
        // lastPointerLocation (inclusive -- lastPointerLocation is meant as the address of the LAST
        // valid pointer slot, not one-past-the-end; SNES_TecmoTool.SetTeamStringTableString has a
        // documented comment about the historical off-by-one bug from getting this wrong).
        private void AdjustTeamStringTablePointers(int currentPointerLocation, int change, int lastPointerLocation)
        {
            int end = lastPointerLocation + 4;
            for (int loc = currentPointerLocation + 4; loc < end; loc += 4)
            {
                int val = (outputRom[loc + 2] << 8) + outputRom[loc + 3];
                val += change;
                outputRom[loc + 2] = (byte)((val >> 8) & 0xFF);
                outputRom[loc + 3] = (byte)(val & 0xFF);
            }
        }

        // shiftAmount is negative here (string got shorter) -- moves data toward lower addresses to
        // close the gap. startPos/endPos are inclusive, matching StaticUtils.ShiftDataUp's contract.
        private void ShiftBytesUp(int startPos, int endPos, int shiftAmount)
        {
            for (int i = startPos; i <= endPos; i++)
                outputRom[i + shiftAmount] = outputRom[i];
        }

        // shiftAmount is positive here (string got longer) -- moves data toward higher addresses to
        // make room, iterating backward so a byte isn't overwritten before it's read.
        private void ShiftBytesDown(int startPos, int endPos, int shiftAmount)
        {
            for (int i = endPos; i >= startPos; i--)
                outputRom[i + shiftAmount] = outputRom[i];
        }

        public string GetTeamAbbreviation(int teamIndex)
        {
            return GetTeamStringTableString(abbreviationIndexBase + teamIndex);
        }

        public void SetTeamAbbreviation(int teamIndex, string abb)
        {
            // Real ROM abbreviations are fixed 4 bytes, underscore-padded (e.g. "BUF_", "N_E_") --
            // confirmed against actual ROM bytes. Matches SNES_TecmoTool's own exact-4-char contract.
            if (abb == null || abb.Length != 4)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.SetTeamAbbreviation: expected exactly 4 characters (underscore-padded, e.g. 'BUF_'), got '{0}'.", abb));
                return;
            }
            SetTeamStringTableString(abbreviationIndexBase + teamIndex, abb);
        }

        public string GetTeamCity(int teamIndex)
        {
            return GetTeamStringTableString(cityIndexBase + teamIndex);
        }

        public void SetTeamCity(int teamIndex, string city)
        {
            SetTeamStringTableString(cityIndexBase + teamIndex, city);
        }

        public string GetTeamName(int teamIndex)
        {
            return GetTeamStringTableString(nicknameIndexBase + teamIndex);
        }

        public void SetTeamName(int teamIndex, string name)
        {
            SetTeamStringTableString(nicknameIndexBase + teamIndex, name);
        }

        #endregion

        // Mirrors SNES_TecmoTool.GetPlayerStuff exactly: a simpler alternate dump (just a "TEAM=x"
        // header line per team, no SimData/playbook/colors/return-team sections) built entirely from
        // the already-implemented GetPlayerData -- no new ROM research needed.
        public string GetPlayerStuff(bool jerseyNumbers, bool names, bool faces, bool abilities, bool simData)
        {
            StringBuilder sb = new StringBuilder(16 * 28 * 30 * 3);
            for (int i = 0; i < teams.Length; i++)
            {
                string team = teams[i];
                sb.Append(string.Format("TEAM={0}\n", team));
                for (int j = 0; j < positionNames.Length; j++)
                    sb.Append(GetPlayerData(team, positionNames[j], jerseyNumbers, names, faces, abilities, simData) + "\n");
            }
            return sb.ToString();
        }

        // Phase 4: season 3 (the real 1993 schedule) only -- see Genesis_ScheduleHelper.cs for why, and
        // for the reversed 4-byte pointer-table mechanics. Delegates to a standalone helper, exactly
        // like SNES_TecmoTool.GetSchedule/ApplySchedule delegate to SNES_ScheduleHelper.
        public string GetSchedule()
        {
            Genesis_ScheduleHelper helper = new Genesis_ScheduleHelper(outputRom);
            return helper.GetSchedule();
        }

        public void ApplySchedule(List<string> scheduleList)
        {
            if (scheduleList != null && outputRom != null)
            {
                Genesis_ScheduleHelper helper = new Genesis_ScheduleHelper(outputRom);
                helper.ApplySchedule(scheduleList);
            }
        }

        // Phase 2: flat 4-bytes-per-team array, byte0=(R1<<4)|R2, byte1=(R3<<4)|R4, byte2=(P1<<4)|P2,
        // byte3=(P3<<4)|P4 -- play slots 0-7 displayed as 1-8. Same nibble-packing scheme as
        // SNES_TecmoTool.GetPlaybook/SetPlaybook, just a different base address and no season
        // dimension at all (this table isn't season-multiplied in the ROM).
        private const int playbookStartLoc = 0x4CC4;

        private static Regex playbookRunRegex = new Regex("R([1-8])([1-8])([1-8])([1-8])");
        private static Regex playbookPassRegex = new Regex("P([1-8])([1-8])([1-8])([1-8])");

        public string GetPlaybook(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return "";

            int pbLocation = playbookStartLoc + (teamIndex * 4);
            int rSlot1 = outputRom[pbLocation] >> 4;
            int rSlot2 = outputRom[pbLocation] & 0x0f;
            int rSlot3 = outputRom[pbLocation + 1] >> 4;
            int rSlot4 = outputRom[pbLocation + 1] & 0x0f;
            int pSlot1 = outputRom[pbLocation + 2] >> 4;
            int pSlot2 = outputRom[pbLocation + 2] & 0x0f;
            int pSlot3 = outputRom[pbLocation + 3] >> 4;
            int pSlot4 = outputRom[pbLocation + 3] & 0x0f;

            return string.Format(
                "PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7} ",
                rSlot1 + 1, rSlot2 + 1, rSlot3 + 1, rSlot4 + 1,
                pSlot1 + 1, pSlot2 + 1, pSlot3 + 1, pSlot4 + 1);
        }

        public void SetPlaybook(string team, string runPlays, string passPlays)
        {
            Match runs = playbookRunRegex.Match(runPlays);
            Match pass = playbookPassRegex.Match(passPlays);

            int teamIndex = GetTeamIndex(team);
            if (teamIndex > -1 && runs != Match.Empty && pass != Match.Empty)
            {
                int pbLocation = playbookStartLoc + (teamIndex * 4);
                int r1 = Int32.Parse(runs.Groups[1].ToString()) - 1;
                int r2 = Int32.Parse(runs.Groups[2].ToString()) - 1;
                int r3 = Int32.Parse(runs.Groups[3].ToString()) - 1;
                int r4 = Int32.Parse(runs.Groups[4].ToString()) - 1;

                int p1 = Int32.Parse(pass.Groups[1].ToString()) - 1;
                int p2 = Int32.Parse(pass.Groups[2].ToString()) - 1;
                int p3 = Int32.Parse(pass.Groups[3].ToString()) - 1;
                int p4 = Int32.Parse(pass.Groups[4].ToString()) - 1;

                r1 = (r1 << 4) + r2;
                r3 = (r3 << 4) + r4;
                p1 = (p1 << 4) + p2;
                p3 = (p3 << 4) + p4;
                outputRom[pbLocation] = (byte)r1;
                outputRom[pbLocation + 1] = (byte)r3;
                outputRom[pbLocation + 2] = (byte)p1;
                outputRom[pbLocation + 3] = (byte)p3;
            }
            else
            {
                if (teamIndex < 0)
                    StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetPlaybook: Team {0} is invalid.", team));
                if (runs == Match.Empty)
                    StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetPlaybook: Run play definition '{0}' is invalid.", runPlays));
                if (pass == Match.Empty)
                    StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetPlaybook: Pass play definition '{0}' is invalid.", passPlays));
            }
        }

        // SNES writes a fixed 5-byte-per-week pattern from a hardcoded lookup table
        // (JUICE_LOCATION + week*5). No Genesis location was researched -- unlike uniform usage/
        // offensive formation, this wasn't even attempted (the edited season's 18-week/bye-week
        // structure doesn't cleanly match SNES's assumed layout, and there's no real-world ground
        // truth to verify a candidate against the way jersey colors could be screenshot-checked). No-op; returns true
        // (rather than false) so InputParser's "JUICE(week, amt)" line doesn't surface a spurious
        // "ERROR! Line = ..." for what's really just an unresearched, silently-ignored field -- matches
        // how every other no-op in this class stays silent rather than signaling failure.
        public bool ApplyJuice(int week, int amount) { return true; }

        // Resequenced ahead of the rest of Phase 3 (player-level sim data, still stubbed below): every
        // InputParser command is preceded by a "TEAM = x SimData=0xNN" line, which unconditionally
        // calls SetTeamSimData (and SetTeamSimOffensePref, when a second sim value is present) --
        // Core\InputParser.cs:309-318. Leaving these as NotImplementedException would make every
        // ProcessText-based command for every other field throw, not just team-level sim data. The
        // team-level byte layout (0x2F summary nibble, 0xB58E tendency byte) was already flagged in
        // compressed-riding-rossum.md as "confidently implementable now", so implementing it for real
        // here (rather than as a temporary no-op) isn't a guess -- see open question #3 for the one
        // still-unconfirmed detail (whether SimData=0xNN's single byte is meant to land on 0x2F,
        // 0xB58E, or both; both are written for now).
        private const int teamSimBlockStart = 0xDEA86;
        private const int teamSimBlockSize = 0x30;
        private const int teamSimSummaryOffset = 0x2F; // team-level offense(hi nibble)/defense(lo nibble) summary
        private const int teamSimOffensivePrefStart = 0xB58E; // 1 byte/team, values 0-3

        public byte GetTeamSimData(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return 0x00;
            return outputRom[teamSimBlockStart + teamIndex * teamSimBlockSize + teamSimSummaryOffset];
        }

        public void SetTeamSimData(string team, byte data)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetTeamSimData: team '{0}' is invalid.", team));
                return;
            }
            outputRom[teamSimBlockStart + teamIndex * teamSimBlockSize + teamSimSummaryOffset] = data;
        }

        public int GetTeamSimOffensePref(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("Team '{0}' is invalid.", team));
                return -1;
            }
            return outputRom[teamSimOffensivePrefStart + teamIndex];
        }

        public bool SetTeamSimOffensePref(string team, int val)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("Team '{0}' is invalid.", team));
                return false;
            }
            if (val < 0 || val > 3)
            {
                StaticUtils.AddError(string.Format("Can't set offensive pref to '{0}', valid values are 0-3.", val));
                return false;
            }
            outputRom[teamSimOffensivePrefStart + teamIndex] = (byte)val;
            return true;
        }
        // SNES's per-team offensive formation is a single flat byte (0-2) at 0xedf3+teamIndex -- looked
        // for a Genesis equivalent using the same trusted-team byte-matching technique that found
        // playbooks/sim-data/Pro Bowl, but never found it: an exact 28-byte match against SNES's values
        // found nothing, a looser match anchored only on the 7 trusted teams turned up 58 candidates
        // (all landing on garbage values outside the valid 0-2 range -- noise, not data), and a tight
        // match anchored on just the 6 real non-zero SNES values found exactly one "hit" that turned out
        // to be 68k code, not a data table (only 6/28 bytes matched; the rest were clearly opcode/operand
        // bytes). No-op, matching SNES_TecmoTool's own SetDivChampColors/SetConfChampColors precedent,
        // rather than throwing. Locating this would likely need an emulator debugger (RAM search +
        // write-breakpoint) rather than more static byte-scanning -- see Genesis_TSB1_ROM_Findings.md.
        public void SetTeamOffensiveFormation(string team, string formation) { }

        // Unlike NES/SNES (a single season -> one "current year" concept patched everywhere), this ROM
        // has 3 baked-in season schedules (see Genesis_ScheduleHelper.cs) and its menus show all three
        // years side by side -- season-select, "view schedule", etc. all list "1993....1992....1991"
        // (or similar orderings) as literal on-screen text, not one substitutable value. Found by
        // scanning the ROM for every occurrence of ASCII "1993" (10 hits) and checking context on each.
        // Per the project owner's explicit direction, this deliberately does NOT try to keep the 3-year
        // menus semantically consistent (i.e. 1991/1992 are left alone, even though every location here
        // is really "the season-3/1993 slot" of a 3-year list) -- it's a blanket find-and-replace of
        // every real on-screen "1993" text with the given year, nothing more.
        //
        // Excludes the ROM header's own "(C)T-36 1993.OCT" copyright-date field at 0x118 -- that's
        // cartridge metadata some tools use for ROM identification, not rendered game text, and
        // changing it risks confusing an emulator/loader for no visible benefit. The other 9 are all
        // real on-screen text: title screen, two copyright/legal lines, and four season-select menu
        // occurrences (0x1D08E/0x1D1FA/0x1D212/0x1D47E are compact "....YYYY...." list entries;
        // 0x1D5FD is the start of a standalone "1993 SEASON" menu label).
        private static readonly int[] yearTextLocations =
        {
            0x1CD0F, 0x1CD77, 0x1D08E, 0x1D1FA, 0x1D212, 0x1D47E, 0x1D5FD, 0x4B6AA, 0x4C164
        };

        public string GetYear()
        {
            int location = yearTextLocations[0];
            string ret = "";
            for (int i = location; i < location + 4; i++)
                ret += (char)outputRom[i];
            return ret;
        }

        public void SetYear(string year)
        {
            if (year == null || year.Length != 4)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetYear: '{0}' is not a valid year.", year));
                return;
            }
            foreach (int location in yearTextLocations)
            {
                outputRom[location] = (byte)year[0];
                outputRom[location + 1] = (byte)year[1];
                outputRom[location + 2] = (byte)year[2];
                outputRom[location + 3] = (byte)year[3];
            }
        }

        // "Face" isn't a real per-player portrait index on Genesis, any more than it is on SNES (only
        // NES has actual face art -- see Core\TecmoTool.cs's own SetFace, which writes the raw
        // 0x00-0xD4 value into a dedicated portrait table). SNES_TecmoTool.SetFace collapses that same
        // wide range down to a skin-tone flag written into attributeRecordStart+2 -- the very byte
        // this project's own Set*PlayerAbilities methods deliberately leave untouched -- and the
        // threshold (0x53) was chosen so that porting an NES roster's face values onto SNES still
        // lands on roughly the right skin tone even though SNES renders no per-player portrait.
        // Genesis inherited the same skin-tone-only scheme: confirmed by cross-checking Genesis's
        // stock +2 byte against SNES's for the same real players (13/13 exact match for the Bills).
        // SNES also flips a second, independent skin bit in a small cutscene bitmask table
        // (SetCutSceneRace) -- no Genesis equivalent has been looked for, so that part isn't mirrored
        // here. See Genesis_TSB1_ROM_Findings.md's attributes section.
        public void SetFace(string team, string position, int face)
        {
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), position);
            if (playerIndex < 0 || face < 0x00 || face > 0xD4)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetFace: invalid team '{0}'/position '{1}', or face {2} outside 0x00-0xD4.", team, position, face));
                return;
            }
            byte skin = (byte)(face < 0x53 ? 0x00 : 0x80);
            outputRom[attributeRecordStart + playerIndex * attributeRecordSize + 2] = skin;
        }

        public int GetFace(string team, string position)
        {
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), position);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.GetFace: invalid team '{0}' or position '{1}'.", team, position));
                return -1;
            }
            return outputRom[attributeRecordStart + playerIndex * attributeRecordSize + 2];
        }

        #region Roster/player names (Phase 5, QB1 only)

        // Pointer table: file 0x5862-0x5F62, 896 x 2-byte entries, tightly packed (NO padding --
        // confirmed directly against real ROM bytes; a third distinct pointer format in this ROM,
        // different from both the team string table's 4-byte and the schedule's 4-byte reversed
        // formats). Pointer values are direct absolute file offsets. flatPlayerIndex =
        // teamIndex*32 + slotIndex; only slot 0 (QB1) has a confirmed position mapping so far -- see
        // open question #2 in compressed-riding-rossum.md.
        //
        // Record: byte0 = jersey number, then lowercase first name, then UPPERCASE last name, implicit
        // length via adjacent-pointer subtraction (same scheme as the team string table). The jersey
        // byte is nominally "BCD-encoded" (0x80 = jersey #80), but no explicit decimal->BCD conversion
        // is needed here: Core\InputParser.cs's GetJerseyNumber already parses the "#NN" text token via
        // ParseIntFromHexString (its own comment: "will be in hex, not base 10"), and for any 2-digit
        // decimal jersey number, hex-parsing its digits produces exactly the same byte as BCD-packing
        // them (e.g. "#12" -> 0x12 either way) -- confirmed against the real ROM (Bills QB Jim Kelly,
        // jersey 0x12 = #12). So the byte this method receives is already correct to write as-is,
        // exactly like SNES_TecmoTool.InsertPlayer's plain `bytes[0] = number` (no BCD math needed on
        // this end, despite what an earlier draft of the plan assumed).
        private const int rosterPointerStart = 0x5862;
        private const int rosterPointerCount = 896;
        private const int rosterDataEnd = 0x89FC; // exclusive

        private const int rosterSlotsPerTeam = 32;

        private static int GetRosterPlayerIndex(int teamIndex, string pos)
        {
            int positionIndex = GetPositionIndex(pos);
            if (teamIndex < 0 || positionIndex < 0)
                return -1;
            return teamIndex * rosterSlotsPerTeam + positionIndex;
        }

        // The 896-entry roster/name pointer table has a 897th "sentinel" pointer immediately after it,
        // at rosterPointerStart + 2*rosterPointerCount (0x5F62) -- confirmed directly against the stock
        // ROM: its value is 0x89FC, exactly rosterDataEnd, and exactly the same "N+1 sentinel" pattern
        // this file's own team-string table already uses correctly (teamStringTableCount = 129 = 128
        // real strings + 1 sentinel, maintained by AdjustTeamStringTablePointers). A prior pass at this
        // roster table missed that sentinel entirely, mistaking the 2 bytes between the pointer table
        // and the name data for padding, and instead invented a separate, synthetic "0x00 terminator"
        // convention purely for this tool's own bookkeeping (mRosterRegionSlack /
        // ComputeInitialRosterRegionSlack / WriteRosterSlackTerminator, now removed) to recover the last
        // entry's real length after a reload. That made this tool's own reads self-consistent, but the
        // real Genesis game almost certainly determines 49ers DB2's length the normal way -- from this
        // *real* sentinel pointer -- and this tool never updated it, so it stayed frozen at the stock
        // value forever. Confirmed in practice: a real user's edited ROM had a perfectly clean, short
        // DB2 name by this tool's own (old, terminator-based) reckoning, with 58 bytes of leftover
        // garbage sitting between that short name and rosterDataEnd -- invisible to this tool, but
        // exactly what a renderer trusting the never-updated sentinel would read as part of DB2's name.
        //
        // Treating the sentinel as real pointer table data (not a fixed constant) removes the need for
        // any of that separate bookkeeping: GetRosterEntryLocation now reads playerIndex 895's "next"
        // pointer exactly like every other entry's, which lands on the sentinel automatically, and
        // InsertPlayer includes it in AdjustRosterPointers' update range exactly like every other
        // downstream pointer. rosterDataEnd remains the one true fixed constant -- the attribute table's
        // real, physical start, which the roster region's content must never be allowed to reach or pass.
        private const int rosterSentinelPointerLoc = rosterPointerStart + 2 * rosterPointerCount; // 0x5F62

        private int GetRosterSentinelValue()
        {
            return (outputRom[rosterSentinelPointerLoc] << 8) + outputRom[rosterSentinelPointerLoc + 1];
        }

        // How many bytes remain between the roster/name table's real current content (as marked by the
        // live sentinel pointer above) and the attribute table's fixed start. Always read live off the
        // ROM's own bytes -- there is no separate field to keep in sync, and so no way for this to go
        // stale across a save/reload the way the old synthetic terminator scheme could.
        public int RosterRegionSlack { get { return rosterDataEnd - GetRosterSentinelValue(); } }

        private int GetRosterEntryLocation(int playerIndex, out int length)
        {
            int pointerLoc = rosterPointerStart + 2 * playerIndex;
            int thisPtr = (outputRom[pointerLoc] << 8) + outputRom[pointerLoc + 1];
            int nextPtr = (outputRom[pointerLoc + 2] << 8) + outputRom[pointerLoc + 3];
            length = nextPtr - thisPtr;
            return thisPtr;
        }

        public string GetPlayerName(string team, string pos)
        {
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.GetPlayerName: team '{0}' or position '{1}' is invalid.", team, pos));
                return null;
            }
            int length;
            int loc = GetRosterEntryLocation(playerIndex, out length);
            char[] chars = new char[length - 1]; // skip the jersey number byte
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)outputRom[loc + 1 + i];
            return new string(chars);
        }

        // GetPlayerName returns the raw concatenated ROM form ("jimKELLY", no separator -- that's how
        // it's actually stored, no delimiter between first/last name). The text-dump/InputParser
        // protocol needs a space between them (matching "QB1, jim KELLY, ..." -- posNameFaceRegex
        // splits first/last name on the last space), so this inserts one at the lowercase->uppercase
        // transition (first name is always lowercase, last name always UPPERCASE) for GetPlayerData's
        // use only -- doesn't change GetPlayerName's own contract.
        private string GetDisplayName(string team, string position)
        {
            string raw = GetPlayerName(team, position);
            if (string.IsNullOrEmpty(raw))
                return raw;
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] >= 'A' && raw[i] <= 'Z')
                    return raw.Substring(0, i) + " " + raw.Substring(i);
            }
            return raw;
        }

        public string GetPlayerData(string team, string position, bool jerseyNumber_b, bool name_b, bool face_b, bool ability_b, bool simData_b)
        {
            if (!IsValidTeam(team) || !IsValidPosition(position))
                return "";

            StringBuilder result = new StringBuilder();
            result.Append(string.Format("{0}, ", position));
            if (name_b)
                result.Append(string.Format("{0}, ", GetDisplayName(team, position)));
            if (face_b)
                result.Append(string.Format("Face=0x{0:x}, ", GetFace(team, position)));
            if (jerseyNumber_b)
            {
                int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), position);
                int junk;
                int loc = GetRosterEntryLocation(playerIndex, out junk);
                result.Append(string.Format("#{0:x}, ", outputRom[loc]));
            }
            if (ability_b)
                result.Append(GetAbilityString(team, position));
            int[] simData = GetPlayerSimData(team, position);
            if (simData != null && simData_b)
                result.Append(string.Format(",[{0}]", StringifyArray(simData)));
            return result.ToString();
        }

        public void InsertPlayer(string currentTeam, string pos, string fname, string lname, byte jerseyNumber)
        {
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(currentTeam), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.InsertPlayer: team '{0}' or position '{1}' is invalid.", currentTeam, pos));
                return;
            }
            if (fname == null || lname == null || fname.Length < 1 || lname.Length < 1)
            {
                StaticUtils.AddError("ERROR! Genesis_TSB1Tool.InsertPlayer: player name is invalid.");
                return;
            }

            fname = fname.ToLower();
            lname = lname.ToUpper();

            int oldLength;
            int thisPtr = GetRosterEntryLocation(playerIndex, out oldLength);
            int newLength = 1 + fname.Length + lname.Length;
            int shiftAmount = newLength - oldLength;

            // The roster/name table for all 896 players (28 teams x 32 slots) is one shared, contiguous,
            // pointer-indexed region that sits immediately before the attribute table with zero slack
            // space in the stock ROM (confirmed: 49ers DB2, the very last entry, ends at exactly
            // rosterDataEnd-1). Any net growth in a name's length shifts everything after it further
            // into the file; if that ever pushes the table's true end past rosterDataEnd, the overflow
            // silently overwrites the attribute table's leading bytes (Bills' attribute records, since
            // Bills is playerIndex 0) with raw name text -- a real corruption this project hit in
            // practice applying a real-world roster whose names are collectively longer than the
            // original. Reject the whole write instead of silently corrupting neighboring data. Checked
            // against the live sentinel-derived RosterRegionSlack (not a fresh rosterDataEnd-minus-pointer
            // computation) since that assumption only holds before any edit has ever created real slack.
            int availableSlack = RosterRegionSlack;
            if (shiftAmount > availableSlack)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.InsertPlayer: setting {0}'s {1} to '{2} {3}' would overflow the shared roster name table into the attribute table by {4} byte(s) -- write rejected. Shorten this (or another) player's name to fit.",
                    currentTeam, pos, fname, lname, shiftAmount - availableSlack));
                return;
            }

            if (shiftAmount != 0)
            {
                int currentPointerLocation = rosterPointerStart + 2 * playerIndex;
                // Includes the sentinel pointer at rosterSentinelPointerLoc (rosterPointerStart +
                // 2*rosterPointerCount) in the update range -- see the sentinel's own comment above
                // GetRosterEntryLocation.
                int lastPointerLocation = rosterSentinelPointerLoc;

                int oldNextLocation = thisPtr + oldLength;

                AdjustRosterPointers(currentPointerLocation, shiftAmount, lastPointerLocation);
                if (shiftAmount < 0)
                    ShiftBytesUp(oldNextLocation, rosterDataEnd - 1, shiftAmount);
                else
                    ShiftBytesDown(oldNextLocation, rosterDataEnd - 1, shiftAmount);
            }

            int writeLength;
            int writeLoc = GetRosterEntryLocation(playerIndex, out writeLength);
            outputRom[writeLoc] = jerseyNumber;
            for (int i = 0; i < fname.Length; i++)
                outputRom[writeLoc + 1 + i] = (byte)fname[i];
            for (int i = 0; i < lname.Length; i++)
                outputRom[writeLoc + 1 + fname.Length + i] = (byte)lname[i];
        }

        // The roster/name table has zero slack against the attribute table in the stock ROM (see
        // InsertPlayer's own comment), and 49ers -- being team index 27, the LAST team in storage order
        // -- is always the one whose data physically abuts that boundary, no matter which real-world
        // roster gets applied. A full roster swap can need dozens of individual name trims scattered
        // across many teams just to avoid that boundary (see CheckRosterFit), since InsertPlayer only
        // sees slack that's already been created by whatever's been applied so far.
        //
        // This sidesteps that class of problem entirely: replace all 32 of the 49ers' roster names with
        // a short, fixed, predictable placeholder (position as first name, "49ers" as last name --
        // e.g. Steve Young's QB1 slot becomes "qb1 49ERS") *before* applying a real roster. Since 49ers
        // sits at the very end of the shared table, doing this first creates a large, known slack pool
        // immediately, available to every other team's edits that follow -- at the cost of the 49ers'
        // own player names, which become generic placeholders rather than real people. A pragmatic
        // trade against the one team that's always going to be the fragile one, rather than chasing
        // trims across the other 27 teams one at a time.
        public void Compress49ersNames()
        {
            foreach (string pos in positionNames)
            {
                int playerIndex = GetRosterPlayerIndex(GetTeamIndex("49ers"), pos);
                int unusedLength;
                int loc = GetRosterEntryLocation(playerIndex, out unusedLength);
                byte jerseyNumber = outputRom[loc];
                InsertPlayer("49ers", pos, pos, "49ers", jerseyNumber);
            }
        }

        // Same "adjust every later pointer by `change`" shape as AdjustTeamStringTablePointers, just a
        // 2-byte (not 4-byte) stride and no padding bytes to skip.
        private void AdjustRosterPointers(int currentPointerLocation, int change, int lastPointerLocation)
        {
            int end = lastPointerLocation + 2;
            for (int loc = currentPointerLocation + 2; loc < end; loc += 2)
            {
                int val = (outputRom[loc] << 8) + outputRom[loc + 1];
                val += change;
                outputRom[loc] = (byte)((val >> 8) & 0xFF);
                outputRom[loc + 1] = (byte)(val & 0xFF);
            }
        }

        public class RosterFitSuggestion
        {
            public string Team;
            public string Position;
            public string ProposedName; // the too-long name as given in the roster text being checked
            public string SuggestedName;
            public int BytesSaved;
        }

        // One roster slot's current state, as read directly off the ROM -- see GetAllPlayers.
        public class RosterPlayer
        {
            public string Team;
            public string Position;
            public string FirstName;
            public string LastName;
            public byte Jersey;

            // Set only by ResolveRosterFit's merge step (parsing a given roster text); GetAllPlayers
            // itself never sets this to true, since it has no text to compare against.
            public bool IsSpecifiedInText;
        }

        // Reads the current state of all 896 roster slots (28 teams x 32 positions) off the ROM. General
        // purpose -- not tied to the roster-fit-checking use case below.
        public List<RosterPlayer> GetAllPlayers()
        {
            List<RosterPlayer> players = new List<RosterPlayer>();
            for (int t = 0; t < teams.Length; t++)
            {
                string team = teams[t];
                for (int p = 0; p < positionNames.Length; p++)
                {
                    string pos = positionNames[p];
                    string name = GetPlayerName(team, pos);
                    if (name == null)
                        continue;

                    int playerIndex = GetRosterPlayerIndex(t, pos);
                    int unusedLength;
                    int loc = GetRosterEntryLocation(playerIndex, out unusedLength);

                    RosterPlayer player = new RosterPlayer();
                    player.Team = team;
                    player.Position = pos;
                    player.Jersey = outputRom[loc];
                    // Same lowercase/UPPERCASE split GetDisplayName uses, applied to GetPlayerName's raw
                    // concatenated form ("jimKELLY") -- first name is always lowercase, last name always
                    // UPPERCASE, so the lowercase->uppercase transition is the split point.
                    int splitIndex = name.Length;
                    for (int i = 0; i < name.Length; i++)
                    {
                        if (name[i] >= 'A' && name[i] <= 'Z')
                        {
                            splitIndex = i;
                            break;
                        }
                    }
                    player.FirstName = name.Substring(0, splitIndex);
                    player.LastName = name.Substring(splitIndex);
                    players.Add(player);
                }
            }
            return players;
        }

        public class RosterFitReport
        {
            // Simple total-vs-available-slack summary -- a quick headline, but NOT a guarantee that a
            // real top-to-bottom apply succeeds: a shrink late in the file can't retroactively rescue a
            // growth that already got rejected earlier, since InsertPlayer's bounds check only sees
            // slack that has actually been created *so far* at the point each line is processed. Use
            // SafeToApplyInOrder for the real answer.
            public bool Fits;
            public int TotalShiftNeeded;
            public int AvailableSlack;
            public int OverflowBytes;

            // True if applying this exact text top-to-bottom -- using the abbreviations listed in
            // Suggestions -- would have every single line succeed. False iff UnresolvedFailures is
            // non-empty.
            public bool SafeToApplyInOrder;

            // Players that would be rejected as originally written, at the point they're reached in the
            // file, but succeed once their first name is abbreviated to a single initial. Listed in file
            // order (the order a real apply would hit them), not by savings size.
            public List<RosterFitSuggestion> Suggestions = new List<RosterFitSuggestion>();

            // Players that would still be rejected even after fully abbreviating their own first name --
            // the shortfall is bigger than a first-name trim alone can cover. Needs a different fix (a
            // last-name trim too, or freeing more slack from an earlier player in the file).
            public List<RosterFitSuggestion> UnresolvedFailures = new List<RosterFitSuggestion>();

            // Only populated by ResolveRosterFit, never by CheckRosterFit: roster slots the given text
            // never mentioned, and so were freely available to shrink to a short placeholder ("db1
            // BILLS") before touching any name the text actually specified. Whether these were really
            // applied to the ROM depends on the autoApply argument passed to ResolveRosterFit.
            public List<RosterPlayer> AutoTrimmedPlayers = new List<RosterPlayer>();
            public int SlackFreedByAutoTrim;

            // Shared plain-text rendering, used by both MainGUI (RichTextDisplay.ShowMessage) and
            // MainClass (Console.WriteLine) so the wording can't drift between the two -- callers append
            // their own final instruction line (a GUI prompt, a CLI note about -noAutoFit, etc.) after
            // this, since that part is genuinely different per caller.
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("This roster needs {0} more byte(s) than the ROM currently has free.\r\n\r\n", TotalShiftNeeded - AvailableSlack);

                if (AutoTrimmedPlayers.Count > 0)
                {
                    sb.AppendFormat("The following {0} player(s), which the roster text being applied never mentions, will be renamed to a short placeholder to free up {1} byte(s):\r\n",
                        AutoTrimmedPlayers.Count, SlackFreedByAutoTrim);
                    foreach (RosterPlayer p in AutoTrimmedPlayers)
                        sb.AppendFormat("  {0,-12} {1,-5} {2} {3}  ->  {4} {5}\r\n", p.Team, p.Position, p.FirstName, p.LastName, p.Position.ToLower(), p.Team.ToUpper());
                    sb.Append("\r\n");
                }

                if (Suggestions.Count > 0)
                {
                    sb.AppendFormat("The following {0} player(s) that WERE specified will have their first name shortened, since trimming unspecified players alone wasn't enough:\r\n", Suggestions.Count);
                    foreach (RosterFitSuggestion s in Suggestions)
                        sb.AppendFormat("  {0,-12} {1,-5} '{2}'  ->  '{3}'  (saves {4})\r\n", s.Team, s.Position, s.ProposedName, s.SuggestedName, s.BytesSaved);
                    sb.Append("\r\n");
                }

                if (UnresolvedFailures.Count > 0)
                {
                    sb.AppendFormat("WARNING: the following {0} player(s) cannot be fixed automatically at all -- their name is too short to abbreviate enough. Unless something else frees up more room, these will keep their OLD name instead of the one specified:\r\n", UnresolvedFailures.Count);
                    foreach (RosterFitSuggestion f in UnresolvedFailures)
                        sb.AppendFormat("  {0,-12} {1,-5} '{2}'  (best possible saving: {3}, not enough)\r\n", f.Team, f.Position, f.ProposedName, f.BytesSaved);
                }

                return sb.ToString();
            }
        }

        // Pre-flight check for a whole roster text block (as fed to ProcessText), so a caller (MainGUI,
        // MainClass CLI) can find out *before* applying whether it would overflow the shared roster
        // table into the attribute table -- see InsertPlayer's own comment for why that's possible and
        // how the real write already rejects it. This is read-only: it never mutates the given rom
        // array (an internal clone backs the throwaway tool instance used to read current name
        // lengths), and it re-parses the same text format ProcessText/InputParser accept, reusing
        // InputParser's own GetFirstName/GetLastName so this can't drift out of sync with what the real
        // apply actually does.
        //
        // Checking only the *total* shift against available slack isn't enough to know whether a real
        // apply will succeed: InsertPlayer processes lines in file order, and its bounds check only sees
        // slack that has been created *so far*, not slack a later line in the file will eventually free
        // up. So this simulates the real sequential apply line by line, tracking running slack exactly
        // like InsertPlayer does, and for any line that would be rejected at that point, checks whether
        // abbreviating that line's own first name (to a single initial) is enough to let it through --
        // exactly mirroring what a user manually pre-trimming that one name and re-applying would get.
        // Report only; nothing is auto-applied.
        //
        // Available slack is read live off the given rom's own bytes (via the real sentinel pointer --
        // see GetRosterEntryLocation's own comment), so this correctly reflects slack the caller already
        // created (e.g. via Compress49ersNames) with no need to pass it in separately, and can't go stale
        // across a save/reload the way an earlier, synthetic-terminator-based scheme could.
        public static RosterFitReport CheckRosterFit(byte[] rom, string rosterText)
        {
            RosterFitReport report = new RosterFitReport();
            if (rom == null || rosterText == null)
                return report;

            Genesis_TSB1Tool readOnlyTool = new Genesis_TSB1Tool((byte[])rom.Clone());
            List<RosterFitEntry> entries = ParseRosterEntries(readOnlyTool, rosterText);

            int totalShift = 0;
            foreach (RosterFitEntry e in entries)
                totalShift += e.NewLength - e.OldLength;

            report.TotalShiftNeeded = totalShift;
            report.AvailableSlack = readOnlyTool.RosterRegionSlack;
            report.OverflowBytes = Math.Max(0, totalShift - report.AvailableSlack);
            report.Fits = report.OverflowBytes == 0;

            SimulateSequentialApply(entries, report.AvailableSlack, report);
            return report;
        }

        // Reused by both CheckRosterFit and ResolveRosterFit: walks entries in file order, tracking
        // running slack exactly like InsertPlayer does, and for any entry that would be rejected at
        // that point, checks whether abbreviating its own first name (to a single initial) is enough to
        // let it through. Fills in report.Suggestions/UnresolvedFailures/SafeToApplyInOrder.
        private static void SimulateSequentialApply(List<RosterFitEntry> entries, int startingSlack, RosterFitReport report)
        {
            int runningSlack = startingSlack;
            foreach (RosterFitEntry e in entries)
            {
                int delta = e.NewLength - e.OldLength;
                if (delta <= runningSlack)
                {
                    runningSlack -= delta;
                    continue;
                }

                int maxSavings = Math.Max(0, e.FirstName.Length - 2); // "x." is 2 chars
                int abbreviatedDelta = delta - maxSavings;
                if (maxSavings > 0 && abbreviatedDelta <= runningSlack)
                {
                    string suggestedFname = e.FirstName.Substring(0, 1).ToLower() + ".";
                    RosterFitSuggestion suggestion = new RosterFitSuggestion();
                    suggestion.Team = e.Team;
                    suggestion.Position = e.Position;
                    suggestion.ProposedName = e.FirstName + " " + e.LastName;
                    suggestion.SuggestedName = suggestedFname + " " + e.LastName;
                    suggestion.BytesSaved = maxSavings;
                    report.Suggestions.Add(suggestion);
                    runningSlack -= abbreviatedDelta;
                }
                else
                {
                    // Even a full first-name abbreviation isn't enough -- the write would still be
                    // rejected by a real InsertPlayer call, which leaves state unchanged, so
                    // runningSlack is deliberately left as-is here too.
                    RosterFitSuggestion failure = new RosterFitSuggestion();
                    failure.Team = e.Team;
                    failure.Position = e.Position;
                    failure.ProposedName = e.FirstName + " " + e.LastName;
                    failure.SuggestedName = null;
                    failure.BytesSaved = maxSavings;
                    report.UnresolvedFailures.Add(failure);
                }
            }
            report.SafeToApplyInOrder = report.UnresolvedFailures.Count == 0;
        }

        // Shared by CheckRosterFit and ResolveRosterFit: parses a roster text block into one
        // RosterFitEntry per recognized roster line, reusing InputParser.GetFirstName/GetLastName so
        // this can't drift out of sync with what a real apply does. readOnlyTool supplies each entry's
        // current (pre-edit) name length.
        private static List<RosterFitEntry> ParseRosterEntries(Genesis_TSB1Tool readOnlyTool, string rosterText)
        {
            Regex teamLineRegex = new Regex("TEAM\\s*=\\s*([0-9a-z]+)");
            Regex posLineRegex = new Regex("^([A-Z]+[1-4]?)\\s*,\\s*([a-zA-Z \\.\\-]+),");

            List<RosterFitEntry> entries = new List<RosterFitEntry>();
            string currentTeam = null;
            string[] lines = rosterText.Replace("\r\n", "\n").Split('\n');
            foreach (string rawLine in lines)
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
                if (GetPositionIndex(pos) < 0)
                    continue; // not a roster line (e.g. RETURN_TEAM/KR/PR)

                string fname = InputParser.GetFirstName(line);
                string lname = InputParser.GetLastName(line);
                if (string.IsNullOrEmpty(fname) || string.IsNullOrEmpty(lname))
                    continue;

                string currentName = readOnlyTool.GetPlayerName(currentTeam, pos);
                if (currentName == null)
                    continue;

                entries.Add(new RosterFitEntry(currentTeam, pos, fname, lname, 1 + currentName.Length));
            }
            return entries;
        }

        private class RosterFitEntry
        {
            public readonly string Team;
            public readonly string Position;
            public readonly string FirstName;
            public readonly string LastName;
            public readonly int OldLength;
            public readonly int NewLength;

            public RosterFitEntry(string team, string position, string fname, string lname, int oldLength)
            {
                Team = team;
                Position = position;
                FirstName = fname;
                LastName = lname;
                OldLength = oldLength;
                NewLength = 1 + fname.Length + lname.Length;
            }
        }

        // Combines CheckRosterFit's own analysis with an extra preference: before ever suggesting an
        // abbreviation on a name the roster text actually specified, first look for slack among roster
        // slots the text never mentioned at all (e.g. a real-world roster export that skips DB1/DB2) --
        // trimming those to a short placeholder ("db1 BILLS") costs nothing the caller asked to keep,
        // unlike trimming a name they explicitly gave.
        //
        // if autoApply is true, the selected auto-trims are actually written to this instance's ROM
        // (via InsertPlayer, same as Compress49ersNames); if false, this is a pure dry run -- nothing is
        // mutated, and the report just describes what auto-trimming would do. Either way, the specified
        // roster text itself is never applied here -- that's still a separate ProcessText call, done by
        // the caller afterward (ideally benefiting from whatever slack this method already freed up).
        public RosterFitReport ResolveRosterFit(string rosterText, bool autoApply)
        {
            RosterFitReport report = new RosterFitReport();
            if (rosterText == null)
                return report;

            Genesis_TSB1Tool readOnlyTool = autoApply ? this : new Genesis_TSB1Tool((byte[])outputRom.Clone());
            List<RosterFitEntry> entries = ParseRosterEntries(readOnlyTool, rosterText);

            int totalShift = 0;
            foreach (RosterFitEntry e in entries)
                totalShift += e.NewLength - e.OldLength;
            report.TotalShiftNeeded = totalShift;
            report.AvailableSlack = RosterRegionSlack;
            report.OverflowBytes = Math.Max(0, totalShift - report.AvailableSlack);
            report.Fits = report.OverflowBytes == 0;

            // Minimum starting slack that would let every specified entry succeed with zero
            // abbreviation at all: the largest cumulative net growth seen at any prefix, in file order.
            int cumulativeNet = 0;
            int neededSlack = 0;
            foreach (RosterFitEntry e in entries)
            {
                cumulativeNet += e.NewLength - e.OldLength;
                if (cumulativeNet > neededSlack)
                    neededSlack = cumulativeNet;
            }

            List<string> specifiedKeys = new List<string>();
            foreach (RosterFitEntry e in entries)
                specifiedKeys.Add(e.Team + "|" + e.Position);

            List<RosterPlayer> candidates = new List<RosterPlayer>();
            foreach (RosterPlayer player in readOnlyTool.GetAllPlayers())
            {
                if (specifiedKeys.Contains(player.Team + "|" + player.Position))
                    continue;
                candidates.Add(player);
            }
            candidates.Sort(CompareCandidatesBySavingsDescending);

            int freedSlack = 0;
            foreach (RosterPlayer candidate in candidates)
            {
                if (freedSlack >= neededSlack)
                    break;
                int savings = CandidateSavings(candidate);
                if (savings <= 0)
                    continue;

                if (autoApply)
                    InsertPlayer(candidate.Team, candidate.Position, candidate.Position, candidate.Team, candidate.Jersey);

                report.AutoTrimmedPlayers.Add(candidate);
                freedSlack += savings;
            }
            report.SlackFreedByAutoTrim = freedSlack;

            SimulateSequentialApply(entries, report.AvailableSlack + freedSlack, report);
            return report;
        }

        private static int CandidateSavings(RosterPlayer candidate)
        {
            int oldLength = 1 + candidate.FirstName.Length + candidate.LastName.Length;
            int newLength = 1 + candidate.Position.Length + candidate.Team.Length;
            return oldLength - newLength;
        }

        private static int CompareCandidatesBySavingsDescending(RosterPlayer a, RosterPlayer b)
        {
            return CandidateSavings(b).CompareTo(CandidateSavings(a));
        }

        #endregion

        #region Player attributes (Phase 6, QB1 only)

        // Flat 5-bytes-per-player record at 0x89FC + flatPlayerIndex*5 (same flatPlayerIndex as the
        // roster table -- teamIndex*32 + slotIndex). No pointers at all, the simplest of the solved
        // pieces. +0=(RushingPower<<4)|RunningSpeed, +1=(MaxSpeed<<4)|HittingPower, +2=unresolved
        // flag/tag byte (left untouched -- open question #4, don't guess), +3=(PassSpeed<<4)|PassControl,
        // +4=(PassAccuracy<<4)|Arm. Same 16-level nibble scale as
        // SNES_TecmoTool.GetAbility/MapAbality -- duplicated locally rather than shared, matching how
        // SNES_TecmoTool itself doesn't share it with TecmoTool's copy.
        //
        // ITecmoTool.SetQBAbilities's parameter names (accuracy/avoidPassBlock) are NES/SNES's own
        // labels for the interface's last two ability slots; Genesis's confirmed byte+4 layout calls
        // the same two nibble positions PassAccuracy/Arm. Positionally they're the same two interface
        // parameters landing in the same high/low nibble of the same last byte -- see
        // SNES_TecmoTool.SetQBAbilities for the identical byte-packing shape this mirrors.
        private const int attributeRecordStart = 0x89FC;
        private const int attributeRecordSize = 5;

        private static readonly int[] abilityScale = { 6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100 };

        private static bool IsValidAbilityScore(int score)
        {
            return Array.IndexOf(abilityScale, score) >= 0;
        }

        private static byte GetAbilityIndex(int score)
        {
            return (byte)Array.IndexOf(abilityScale, score);
        }

        private static int MapAbilityScore(int nibble)
        {
            if (nibble < 0 || nibble >= abilityScale.Length)
                return 0;
            return abilityScale[nibble];
        }

        // Read-back counterpart to SetQBAbilities/SetSkillPlayerAbilities/SetOLPlayerAbilities/
        // SetDefensivePlayerAbilities/SetKickPlayerAbilities -- decodes the same 5-byte record back
        // into ability scores. Return-array shape mirrors SNES_TecmoTool.GetAbilities exactly: 4
        // values for OL (no 3rd stat pair), 8 for QB (the 3rd pair plus the QB-only 4th pair), 6 for
        // everyone else (skill/defensive/K/P).
        public int[] GetAbilities(string team, string position)
        {
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), position);
            if (playerIndex < 0)
                return null;

            int loc = attributeRecordStart + playerIndex * attributeRecordSize;
            byte b1 = outputRom[loc];
            byte b2 = outputRom[loc + 1];
            byte b3 = outputRom[loc + 3];
            byte b4 = outputRom[loc + 4];

            int runningSpeed = MapAbilityScore(b1 & 0x0F);
            int rushingPower = MapAbilityScore((b1 & 0xF0) >> 4);
            int maxSpeed = MapAbilityScore((b2 & 0xF0) >> 4);
            int hittingPower = MapAbilityScore(b2 & 0x0F);
            int wild1 = MapAbilityScore((b3 & 0xF0) >> 4);
            int wild2 = MapAbilityScore(b3 & 0x0F);

            int[] ret;
            if (Array.IndexOf(olPositions, position) >= 0)
            {
                ret = new int[4];
            }
            else if (position == "QB1" || position == "QB2")
            {
                int accuracy = MapAbilityScore((b4 & 0xF0) >> 4);
                int avoidPassBlock = MapAbilityScore(b4 & 0x0F);
                ret = new int[8];
                ret[4] = wild1;
                ret[5] = wild2;
                ret[6] = accuracy;
                ret[7] = avoidPassBlock;
            }
            else
            {
                ret = new int[6];
                ret[4] = wild1;
                ret[5] = wild2;
            }
            ret[0] = runningSpeed;
            ret[1] = rushingPower;
            ret[2] = maxSpeed;
            ret[3] = hittingPower;
            return ret;
        }

        public string GetAbilityString(string team, string position)
        {
            int[] abilities = GetAbilities(team, position);
            return abilities == null ? "" : StringifyArray(abilities);
        }

        private static string StringifyArray(int[] arr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                sb.Append(arr[i]);
                if (i < arr.Length - 1)
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        public void SetQBAbilities(string team, string qb, int runningSpeed, int rushingPower, int maxSpeed,
            int hittingPower, int passingSpeed, int passControl, int accuracy, int avoidPassBlock)
        {
            int playerIndex = (qb == "QB1" || qb == "QB2") ? GetRosterPlayerIndex(GetTeamIndex(team), qb) : -1;
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.SetQBAbilities: team '{0}' or position '{1}' is invalid (must be QB1 or QB2).", team, qb));
                return;
            }
            if (!IsValidAbilityScore(runningSpeed) || !IsValidAbilityScore(rushingPower) ||
                !IsValidAbilityScore(maxSpeed) || !IsValidAbilityScore(hittingPower) ||
                !IsValidAbilityScore(passingSpeed) || !IsValidAbilityScore(passControl) ||
                !IsValidAbilityScore(accuracy) || !IsValidAbilityScore(avoidPassBlock))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetQBAbilities: abilities for {0} on {1} were not set (each value must be one of the 16 valid ability scores).", qb, team));
                return;
            }

            byte rs = GetAbilityIndex(runningSpeed);
            byte rp = GetAbilityIndex(rushingPower);
            byte ms = GetAbilityIndex(maxSpeed);
            byte hp = GetAbilityIndex(hittingPower);
            byte ps = GetAbilityIndex(passingSpeed);
            byte pc = GetAbilityIndex(passControl);
            byte pa = GetAbilityIndex(accuracy);
            byte ar = GetAbilityIndex(avoidPassBlock);

            int loc = attributeRecordStart + playerIndex * attributeRecordSize;
            outputRom[loc] = (byte)((rp << 4) | rs);
            outputRom[loc + 1] = (byte)((ms << 4) | hp);
            outputRom[loc + 3] = (byte)((ps << 4) | pc);
            outputRom[loc + 4] = (byte)((pa << 4) | ar);
        }

        #endregion

        // Player-level sim/CPU data: offensive positions (QB1,QB2,RB1-4,WR1-4,TE1-2, positionIndex
        // 0-11) live at 0xDEA86 + teamIndex*0x30 + positionIndex*2 -- confirmed identical formula to
        // SNES_TecmoTool.GetQBSimData/GetSkillSimData, and (unlike SNES) Genesis uses the plain main
        // team order throughout, no mSimTeams-style reordering (confirmed via the SNES-comparison
        // investigation, see Genesis_TSB1_ROM_Findings.md).
        //
        // QB (2 bytes -> 3 values) and skill (2 bytes -> 4 values) interpret those 2 bytes
        // differently -- this is a real SNES behavior, not a Genesis-specific quirk: QB's 2nd byte is
        // a single whole value (0-255), never nibble-split; skill's 2nd byte IS nibble-split. Mirrors
        // SNES_TecmoTool.SetQBSimData/SetSkillSimData exactly, including that neither validates data
        // values against the ability scale (these are raw sim-data numbers, not "ability scores").
        public void SetQBSimData(string team, string pos, int[] data)
        {
            if (pos != "QB1" && pos != "QB2")
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetQBSimData: '{0}' is not QB1 or QB2.", pos));
                return;
            }
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0 || data == null || data.Length < 3)
            {
                StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                return;
            }
            int loc = OffensiveSimLoc(teamIndex, pos == "QB2" ? 1 : 0);
            outputRom[loc] = (byte)(((byte)data[0] << 4) + (byte)data[1]);
            outputRom[loc + 1] = (byte)data[2];
        }

        private static int OffensiveSimLoc(int teamIndex, int positionIndex)
        {
            return teamSimBlockStart + teamIndex * teamSimBlockSize + positionIndex * 2;
        }

        // Position groups for the Set*PlayerAbilities methods below -- same valid-position lists
        // SNES_TecmoTool.cs uses for its own SetSkillPlayerAbilities/SetOLPlayerAbilities/
        // SetDefensivePlayerAbilities/SetKickPlayerAbilities.
        private static readonly string[] skillPositions = { "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        private static readonly string[] olPositions = { "C", "LG", "RG", "LT", "RT" };
        private static readonly string[] defensivePositions = { "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS", "DB1", "DB2" };
        private static readonly string[] kickPositions = { "K", "P" };

        private static bool AreValidAbilityScores(int a, int b, int c, int d)
        {
            return IsValidAbilityScore(a) && IsValidAbilityScore(b) && IsValidAbilityScore(c) && IsValidAbilityScore(d);
        }

        // Writes byte0=(RP<<4)|RS and byte1=(MS<<4)|HP unconditionally (every position), and
        // optionally byte3=(x<<4)|y (skill/defensive/K-P positions only -- OL has no 3rd stat pair,
        // confirmed 0x00/unwritten on the real ROM). byte2 and byte4 are never touched here.
        private void WriteAbilityBytes(int playerIndex, int runningSpeed, int rushingPower, int maxSpeed,
            int hittingPower, int x, int y, bool writeThirdPair)
        {
            int loc = attributeRecordStart + playerIndex * attributeRecordSize;
            outputRom[loc] = (byte)((GetAbilityIndex(rushingPower) << 4) | GetAbilityIndex(runningSpeed));
            outputRom[loc + 1] = (byte)((GetAbilityIndex(maxSpeed) << 4) | GetAbilityIndex(hittingPower));
            if (writeThirdPair)
                outputRom[loc + 3] = (byte)((GetAbilityIndex(x) << 4) | GetAbilityIndex(y));
        }

        public void SetSkillPlayerAbilities(string team, string pos, int runningSpeed, int rushingPower,
            int maxSpeed, int hittingPower, int ballControl, int receptions)
        {
            if (Array.IndexOf(skillPositions, pos) < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetSkillPlayerAbilities: '{0}' is not a skill position (RB1-4/WR1-4/TE1-2).", pos));
                return;
            }
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetSkillPlayerAbilities: team '{0}' is invalid.", team));
                return;
            }
            if (!AreValidAbilityScores(runningSpeed, rushingPower, maxSpeed, hittingPower) ||
                !IsValidAbilityScore(ballControl) || !IsValidAbilityScore(receptions))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetSkillPlayerAbilities: abilities for {0} on {1} were not set (invalid ability score).", pos, team));
                return;
            }
            WriteAbilityBytes(playerIndex, runningSpeed, rushingPower, maxSpeed, hittingPower, ballControl, receptions, true);
        }

        public void SetSkillSimData(string team, string pos, int[] data)
        {
            if (Array.IndexOf(skillPositions, pos) < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetSkillSimData: '{0}' is not a skill position.", pos));
                return;
            }
            int teamIndex = GetTeamIndex(team);
            int positionIndex = GetPositionIndex(pos);
            if (teamIndex < 0 || data == null || data.Length < 4)
            {
                StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                return;
            }
            int loc = OffensiveSimLoc(teamIndex, positionIndex);
            outputRom[loc] = (byte)(((byte)data[0] << 4) + (byte)data[1]);
            outputRom[loc + 1] = (byte)(((byte)data[2] << 4) + (byte)data[3]);
        }

        public void SetOLPlayerAbilities(string team, string pos, int runningSpeed, int rushingPower,
            int maxSpeed, int hittingPower)
        {
            if (Array.IndexOf(olPositions, pos) < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetOLPlayerAbilities: '{0}' is not an OL position (C/LG/RG/LT/RT).", pos));
                return;
            }
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetOLPlayerAbilities: team '{0}' is invalid.", team));
                return;
            }
            if (!AreValidAbilityScores(runningSpeed, rushingPower, maxSpeed, hittingPower))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetOLPlayerAbilities: abilities for {0} on {1} were not set (invalid ability score).", pos, team));
                return;
            }
            WriteAbilityBytes(playerIndex, runningSpeed, rushingPower, maxSpeed, hittingPower, 0, 0, false);
        }

        public void SetDefensivePlayerAbilities(string team, string pos, int runningSpeed, int rushingPower,
            int maxSpeed, int hittingPower, int passRush, int interceptions)
        {
            if (Array.IndexOf(defensivePositions, pos) < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetDefensivePlayerAbilities: '{0}' is not a defensive position.", pos));
                return;
            }
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetDefensivePlayerAbilities: team '{0}' is invalid.", team));
                return;
            }
            if (!AreValidAbilityScores(runningSpeed, rushingPower, maxSpeed, hittingPower) ||
                !IsValidAbilityScore(passRush) || !IsValidAbilityScore(interceptions))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetDefensivePlayerAbilities: abilities for {0} on {1} were not set (invalid ability score).", pos, team));
                return;
            }
            WriteAbilityBytes(playerIndex, runningSpeed, rushingPower, maxSpeed, hittingPower, passRush, interceptions, true);
        }

        // Defensive positions (RE..SS, defIndex 0-10, same order as the attribute table): pass-rush
        // byte at 0x18+defIndex, interception byte at 0x23+defIndex (i.e. +0xB past pass-rush) --
        // confirmed identical to SNES_TecmoTool.SetDefensiveSimData, and independently re-verified
        // 154/154 exact byte matches against SNES for 7 trusted teams (see Genesis_TSB1_ROM_Findings.md).
        private const int defensiveSimPassRushOffset = 0x18;
        private const int defensiveSimInterceptionStride = 0xB;

        public void SetDefensiveSimData(string team, string pos, int[] data)
        {
            int defIndex = GetPositionIndex(pos) - 17; // RE=17 in positionNames -> defIndex 0
            if (Array.IndexOf(defensivePositions, pos) < 0 || defIndex < 0 || defIndex > 10)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetDefensiveSimData: '{0}' is not a defensive position.", pos));
                return;
            }
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0 || data == null || data.Length < 2)
            {
                StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.", team, pos));
                return;
            }
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + defensiveSimPassRushOffset + defIndex;
            outputRom[loc] = (byte)data[0];
            outputRom[loc + defensiveSimInterceptionStride] = (byte)data[1];
        }

        public void SetKickPlayerAbilities(string team, string pos, int runningSpeed, int rushingPower,
            int maxSpeed, int hittingPower, int kickingAbility, int avoidKickBlock)
        {
            if (Array.IndexOf(kickPositions, pos) < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetKickPlayerAbilities: '{0}' is not a kicking position (K/P).", pos));
                return;
            }
            int playerIndex = GetRosterPlayerIndex(GetTeamIndex(team), pos);
            if (playerIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetKickPlayerAbilities: team '{0}' is invalid.", team));
                return;
            }
            if (!AreValidAbilityScores(runningSpeed, rushingPower, maxSpeed, hittingPower) ||
                !IsValidAbilityScore(kickingAbility) || !IsValidAbilityScore(avoidKickBlock))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetKickPlayerAbilities: abilities for {0} on {1} were not set (invalid ability score).", pos, team));
                return;
            }
            WriteAbilityBytes(playerIndex, runningSpeed, rushingPower, maxSpeed, hittingPower, kickingAbility, avoidKickBlock, true);
        }

        // Kicking (hi nibble) / punting (lo nibble) share one byte at 0x2E -- confirmed identical to
        // SNES_TecmoTool.SetKickingSimData/SetPuntingSimData, just retargeted to Genesis's base and
        // plain main team order.
        private const int kickPuntSimOffset = 0x2E;

        public void SetPuntingSimData(string team, int data)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetPuntingSimData: team '{0}' is invalid.", team));
                return;
            }
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + kickPuntSimOffset;
            int d = outputRom[loc] & 0xF0;
            d += data;
            outputRom[loc] = (byte)d;
        }

        public void SetKickingSimData(string team, int data)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetKickingSimData: team '{0}' is invalid.", team));
                return;
            }
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + kickPuntSimOffset;
            int g = outputRom[loc] & 0x0F;
            g += data << 4;
            outputRom[loc] = (byte)g;
        }

        // Read-back counterparts to the sim-data setters above, mirroring SNES_TecmoTool's private
        // GetQBSimData/GetSkillSimData/GetDefensiveSimData/GetKickingSimData/GetPuntingSimData shapes
        // exactly (including QB's 2nd value being a whole byte, not nibble-split). Used internally by
        // GetPlayerSimData/GetPlayerData for the full-ROM text dump.
        private int[] GetQBSimData(string team, string pos)
        {
            if (pos != "QB1" && pos != "QB2")
                return null;
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return null;
            int loc = OffensiveSimLoc(teamIndex, pos == "QB2" ? 1 : 0);
            return new int[] { outputRom[loc] >> 4, outputRom[loc] & 0x0F, outputRom[loc + 1] };
        }

        private int[] GetSkillSimData(string team, string pos)
        {
            if (Array.IndexOf(skillPositions, pos) < 0)
                return null;
            int teamIndex = GetTeamIndex(team);
            int positionIndex = GetPositionIndex(pos);
            if (teamIndex < 0)
                return null;
            int loc = OffensiveSimLoc(teamIndex, positionIndex);
            return new int[] { outputRom[loc] >> 4, outputRom[loc] & 0x0F, outputRom[loc + 1] >> 4, outputRom[loc + 1] & 0x0F };
        }

        private int[] GetDefensiveSimData(string team, string pos)
        {
            int defIndex = GetPositionIndex(pos) - 17;
            if (Array.IndexOf(defensivePositions, pos) < 0 || defIndex < 0 || defIndex > 10)
                return null;
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return null;
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + defensiveSimPassRushOffset + defIndex;
            return new int[] { outputRom[loc], outputRom[loc + defensiveSimInterceptionStride] };
        }

        private int[] GetKickingSimData(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return null;
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + kickPuntSimOffset;
            return new int[] { outputRom[loc] >> 4 };
        }

        private int[] GetPuntingSimData(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return null;
            int loc = teamSimBlockStart + teamIndex * teamSimBlockSize + kickPuntSimOffset;
            return new int[] { outputRom[loc] & 0x0F };
        }

        public int[] GetPlayerSimData(string team, string pos)
        {
            if (!IsValidTeam(team) || !IsValidPosition(pos))
                return null;
            if (pos == "QB1" || pos == "QB2")
                return GetQBSimData(team, pos);
            if (Array.IndexOf(skillPositions, pos) >= 0)
                return GetSkillSimData(team, pos);
            if (pos == "K")
                return GetKickingSimData(team);
            if (pos == "P")
                return GetPuntingSimData(team);
            if (Array.IndexOf(defensivePositions, pos) >= 0)
                return GetDefensiveSimData(team, pos); // returns null itself for DB1/DB2 (no sim slot)
            return null; // OL has no sim-data slot at all
        }

        // Kick returner / punt returner. Selector: 1 byte/team at krPrSelectorStart, hi nibble = kick
        // returner's index (0-2) into the team's 3-man return roster, lo nibble = punt returner's
        // index. Return-team roster: returnTeamStride bytes/team at returnTeamStart -- 3, not SNES's
        // 4 (Genesis has no spare 4th byte). Mirrors SNES_TecmoTool.SetReturnTeam/GetReturnTeam/
        // SetKickReturner/SetPuntReturner/GetKickReturner/GetPuntReturner/IsGuyOnReturnTeam exactly,
        // just retargeted addresses and stride -- see Genesis_TSB1_ROM_Findings.md.
        private const int krPrSelectorStart = 0x4BC0;
        private const int returnTeamStart = 0x4BDC;
        private const int returnTeamStride = 3;

        public void SetReturnTeam(string team, string pos0, string pos1, string pos2)
        {
            if (GetPositionIndex(pos0) > -1 && GetPositionIndex(pos1) > -1 && GetPositionIndex(pos2) > -1)
            {
                InsertGuyOnReturnTeam(pos0, team, 0);
                InsertGuyOnReturnTeam(pos1, team, 1);
                InsertGuyOnReturnTeam(pos2, team, 2);
            }
            else
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Invalid position on RETURN_TEAM {0} {1} {2}", pos0, pos1, pos2));
            }
        }

        public string GetReturnTeam(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.GetReturnTeam: invalid team {0}", team));
                return null;
            }
            int teamLocation = returnTeamStart + returnTeamStride * teamIndex;
            int pos0 = outputRom[teamLocation];
            int pos1 = outputRom[teamLocation + 1];
            int pos2 = outputRom[teamLocation + 2];
            if (pos0 < positionNames.Length && pos1 < positionNames.Length && pos2 < positionNames.Length)
                return string.Format("RETURN_TEAM {0}, {1}, {2}", positionNames[pos0], positionNames[pos1], positionNames[pos2]);
            StaticUtils.AddError("ERROR! Return Team Messed up in ROM.");
            return null;
        }

        public void SetPuntReturner(string team, string position)
        {
            if (!IsValidTeam(team) || !IsValidPosition(position))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetPuntReturner: invalid team {0} or position {1}", team, position));
                return;
            }
            int index = IsGuyOnReturnTeam(position, team);
            if (index < 0)
            {
                index = 1;
                InsertGuyOnReturnTeam(position, team, index);
            }
            int loc = krPrSelectorStart + GetTeamIndex(team);
            int krPr = outputRom[loc] & 0xF0;
            krPr += index;
            outputRom[loc] = (byte)krPr;
        }

        public void SetKickReturner(string team, string position)
        {
            if (!IsValidTeam(team) || !IsValidPosition(position))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetKickReturner: invalid team {0} or position {1}", team, position));
                return;
            }
            int index = IsGuyOnReturnTeam(position, team);
            if (index < 0)
            {
                index = 0;
                InsertGuyOnReturnTeam(position, team, index);
            }
            int loc = krPrSelectorStart + GetTeamIndex(team);
            int krPr = outputRom[loc] & 0x0F;
            krPr += index << 4;
            outputRom[loc] = (byte)krPr;
        }

        public string GetKickReturner(string team)
        {
            if (!IsValidTeam(team))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.GetKickReturner: invalid team {0}", team));
                return null;
            }
            int teamIndex = GetTeamIndex(team);
            int returnTeamIndex = outputRom[krPrSelectorStart + teamIndex] >> 4;
            int teamLocation = returnTeamStart + returnTeamStride * teamIndex;
            int positionIndex = outputRom[returnTeamIndex + teamLocation];
            return positionIndex < positionNames.Length ? positionNames[positionIndex] : "";
        }

        public string GetPuntReturner(string team)
        {
            if (!IsValidTeam(team))
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.GetPuntReturner: invalid team {0}", team));
                return null;
            }
            int teamIndex = GetTeamIndex(team);
            int returnTeamIndex = outputRom[krPrSelectorStart + teamIndex] & 0x0F;
            int teamLocation = returnTeamStart + returnTeamStride * teamIndex;
            int positionIndex = outputRom[returnTeamIndex + teamLocation];
            return positionIndex < positionNames.Length ? positionNames[positionIndex] : "";
        }

        private int IsGuyOnReturnTeam(string guy, string team)
        {
            int teamIndex = GetTeamIndex(team);
            int posIndex = GetPositionIndex(guy);
            int teamLocation = returnTeamStart + returnTeamStride * teamIndex;
            if (outputRom[teamLocation] == posIndex)
                return 0;
            if (outputRom[teamLocation + 1] == posIndex)
                return 1;
            if (outputRom[teamLocation + 2] == posIndex)
                return 2;
            return -1;
        }

        private void InsertGuyOnReturnTeam(string position, string team, int index)
        {
            int teamIndex = GetTeamIndex(team);
            int posIndex = GetPositionIndex(position);
            if (index < 0 || index > 2 || teamIndex < 0 || posIndex < 0)
            {
                StaticUtils.AddError(string.Format(
                    "InsertGuyOnReturnTeam: invalid arguments {0}, {1}, {2}", position, team, index));
                return;
            }
            int teamLocation = returnTeamStart + returnTeamStride * teamIndex;
            outputRom[teamLocation + index] = (byte)posIndex;
        }

        // Genesis's equivalent of SNES's per-opponent dark/light jersey-choice table (SNES:
        // SetUniformUsage/GetUniformUsage, 4 bytes/team at 0x1752, 1 bit/opponent) was searched for
        // extensively -- exact SNES values, every plausible byte/bit-order transform of a
        // corrected-for-one-real-observed-difference value, and a compact "light opponent list"
        // encoding -- and never found. Real in-game testing (project owner checked all 27 Bills
        // matchups) showed Genesis's actual choices are nearly identical to SNES's (26/27 match,
        // only Colts differs), so the *decisions* are real and roughly known for at least one team,
        // but the ROM bytes remain unlocated. No-op, matching SNES_TecmoTool's own precedent for
        // SetDivChampColors/SetConfChampColors (also empty stubs) rather than throwing, so a full
        // COLORS line can still round-trip. See Genesis_TSB1_ROM_Findings.md's uniform-colors section for the
        // full search history if this gets picked up again (e.g. with emulator RAM-watching).
        public void SetUniformUsage(string team, string usage) { }
        public string GetUniformUsage(string team) { return string.Empty; }
        // Division/Conference Championship screen colors were never researched for Genesis -- not even
        // attempted, since SNES_TecmoTool's own versions of these five methods are already empty no-ops
        // (never implemented there either, per that class's own comments). Matching that precedent costs
        // nothing and needs no research: no-op setters, empty-string getters.
        public void SetDivChampColors(string team, string colorString) { }
        public void SetConfChampColors(string team, string colorString) { }
        public string GetDivChampColors(string team) { return string.Empty; }
        public string GetConfChampColors(string team) { return string.Empty; }
        public string GetChampColors(string team) { return string.Empty; }

        // Pro Bowl roster: 0x4C30 (AFC) / +0x46 (NFC), 35 slots x 2 bytes/conference (32 roster
        // positions, positionNames order, + RET1/RET2/RET3 at slots 32/33/34). Byte order is
        // REVERSED from SNES: Genesis stores [teamIndex][positionIndex]; SNES stores
        // [positionIndex][teamIndex] -- confirmed against real ROM bytes (66/70 slots exact match
        // against SNES, every decoded name a real 1992 Pro Bowl selection). See
        // Genesis_TSB1_ROM_Findings.md's "Pro Bowl roster" section.
        private const int proBowlStart = 0x4C30;
        private const int proBowlNfcOffset = 0x46;

        private static int GetProBowlSlotIndex(string proBowlPos)
        {
            switch (proBowlPos)
            {
                case "RET1": return positionNames.Length;
                case "RET2": return positionNames.Length + 1;
                case "RET3": return positionNames.Length + 2;
                default: return GetPositionIndex(proBowlPos);
            }
        }

        public void SetProBowlPlayer(Conference conf, String proBowlPos, String fromTeam, TSBPlayer fromTeamPos)
        {
            int offset = (conf == Conference.NFC) ? proBowlNfcOffset : 0;
            int slotIndex = GetProBowlSlotIndex(proBowlPos);
            int teamIndex = GetTeamIndex(fromTeam);
            if (slotIndex < 0 || teamIndex < 0)
            {
                StaticUtils.AddError(string.Format(
                    "ERROR! Genesis_TSB1Tool.SetProBowlPlayer: invalid proBowlPos '{0}' or fromTeam '{1}'.", proBowlPos, fromTeam));
                return;
            }
            int loc = proBowlStart + offset + 2 * slotIndex;
            outputRom[loc] = (byte)teamIndex;
            outputRom[loc + 1] = (byte)fromTeamPos;
        }

        public String GetProBowlPlayer(Conference conf, String proBowlPos)
        {
            int offset = (conf == Conference.NFC) ? proBowlNfcOffset : 0;
            int slotIndex = GetProBowlSlotIndex(proBowlPos);
            if (slotIndex < 0)
                return "";
            int loc = proBowlStart + offset + 2 * slotIndex;
            int teamIndex = outputRom[loc];
            int pos = outputRom[loc + 1];
            string team = teamIndex < teams.Length ? teams[teamIndex] : "";
            return string.Format("{0},{1},{2},{3}", conf, proBowlPos, team, ((TSBPlayer)pos));
        }

        public String GetConferenceProBowlPlayers(Conference conf)
        {
            StringBuilder builder = new StringBuilder(500);
            for (int i = 0; i < positionNames.Length; i++)
            {
                builder.Append(GetProBowlPlayer(conf, positionNames[i]));
                builder.Append("\r\n");
            }
            builder.Append(GetProBowlPlayer(conf, "RET1"));
            builder.Append("\r\n");
            builder.Append(GetProBowlPlayer(conf, "RET2"));
            builder.Append("\r\n");
            builder.Append(GetProBowlPlayer(conf, "RET3"));
            builder.Append("\r\n");
            return builder.ToString();
        }

        // Uniform colors: a 16-color (32-byte) block per team at uniformColorStart + teamIndex *
        // uniformTeamStride, with a second, structurally identical block uniformAwayOffset further in.
        // Confirmed against two real screenshots (Bills: red helmet/blue jersey/white pants; Jets:
        // green helmet/green jersey) that all 16 colors decode to real, plausible team colors -- see
        // Genesis_TSB1_ROM_Findings.md's uniform-colors section for the full per-team breakdown. Within a
        // block: [0-2]=jersey ramp, [3-5]=pants ramp, [6-7]=a skin-tone-like pair that genuinely
        // differs between the two 0x20-apart blocks (meaning not understood -- left untouched),
        // [8-9]=shared gray/white trim (byte-identical across every team checked), [10-11]=shared black
        // (byte-identical across every team checked), [12-13]=helmet shell ramp, [14-15]=helmet
        // facemask/accent ramp (also left untouched, to keep the wire format below identical in shape
        // to SNES's).
        //
        // Jersey/pants stay byte-identical between the two 0x20-apart blocks for every team checked,
        // while helmet genuinely differs -- matching SNES_TecmoTool's own confirmed finding that helmet
        // color is independent between Home and Away while jersey/pants are not (see that class's
        // comment above mHomeLightSkinUniformLoc). Treated here as Home (offset 0) / Away (offset
        // uniformAwayOffset) on that basis; NOT yet confirmed against an actual away-game screenshot,
        // since only one matchup (Bills @ Jets, per the screenshots checked so far) has been verified.
        //
        // Wire format matches SNES_TecmoTool.SetHomeUniform/SetAwayUniform exactly (28 hex digits:
        // pants1,jersey2,pants2,jersey3,pants3,helmetDark,helmetMedium) even though the underlying ROM
        // table is completely different -- this keeps the "COLORS Uniform1=0x.../Uniform2=0x..." text
        // line identical in shape across platforms and needs no InputParser.cs changes (it already
        // accepts a 28-hex-digit Uniform1/Uniform2 value, shared unmodified with SNES). Jersey's
        // darkest shade (index 0) is deliberately excluded, mirroring SNES's own reasoning -- no direct
        // evidence yet either way for Genesis, kept consistent rather than guessed at.
        private const int uniformColorStart = 0x58974;
        private const int uniformTeamStride = 0x40;
        private const int uniformAwayOffset = 0x20;
        private static readonly int[] uniformJerseyByteOffsets = { 0x00, 0x02, 0x04 };
        private static readonly int[] uniformPantsByteOffsets = { 0x06, 0x08, 0x0A };
        private static readonly int[] uniformHelmetByteOffsets = { 0x18, 0x1A };

        private void WriteUniformColor(int loc, string hex4)
        {
            outputRom[loc] = Convert.ToByte(hex4.Substring(0, 2), 16);
            outputRom[loc + 1] = Convert.ToByte(hex4.Substring(2, 2), 16);
        }

        private string ReadUniformColor(int loc)
        {
            return string.Format("{0:x2}{1:x2}", outputRom[loc], outputRom[loc + 1]);
        }

        private void WriteUniformBlock(int blockBase, string colorString)
        {
            WriteUniformColor(blockBase + uniformPantsByteOffsets[0], colorString.Substring(0, 4));
            WriteUniformColor(blockBase + uniformJerseyByteOffsets[1], colorString.Substring(4, 4));
            WriteUniformColor(blockBase + uniformPantsByteOffsets[1], colorString.Substring(8, 4));
            WriteUniformColor(blockBase + uniformJerseyByteOffsets[2], colorString.Substring(12, 4));
            WriteUniformColor(blockBase + uniformPantsByteOffsets[2], colorString.Substring(16, 4));
            WriteUniformColor(blockBase + uniformHelmetByteOffsets[0], colorString.Substring(20, 4));
            WriteUniformColor(blockBase + uniformHelmetByteOffsets[1], colorString.Substring(24, 4));
        }

        private string ReadUniformBlock(int blockBase)
        {
            StringBuilder sb = new StringBuilder(28);
            sb.Append(ReadUniformColor(blockBase + uniformPantsByteOffsets[0]));
            sb.Append(ReadUniformColor(blockBase + uniformJerseyByteOffsets[1]));
            sb.Append(ReadUniformColor(blockBase + uniformPantsByteOffsets[1]));
            sb.Append(ReadUniformColor(blockBase + uniformJerseyByteOffsets[2]));
            sb.Append(ReadUniformColor(blockBase + uniformPantsByteOffsets[2]));
            sb.Append(ReadUniformColor(blockBase + uniformHelmetByteOffsets[0]));
            sb.Append(ReadUniformColor(blockBase + uniformHelmetByteOffsets[1]));
            return sb.ToString();
        }

        public void SetHomeUniform(string team, string colorString)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetHomeUniform: team {0} is invalid.", team));
                return;
            }
            if (colorString == null || colorString.Length != 28)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetHomeUniform: expected 28 hex digits (pants1,jersey2,pants2,jersey3,pants3,helmetDark,helmetMedium), got '{0}'.", colorString));
                return;
            }
            WriteUniformBlock(uniformColorStart + teamIndex * uniformTeamStride, colorString);
        }

        public void SetAwayUniform(string team, string colorString)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetAwayUniform: team {0} is invalid.", team));
                return;
            }
            if (colorString == null || colorString.Length != 28)
            {
                StaticUtils.AddError(string.Format("ERROR! Genesis_TSB1Tool.SetAwayUniform: expected 28 hex digits (pants1,jersey2,pants2,jersey3,pants3,helmetDark,helmetMedium), got '{0}'.", colorString));
                return;
            }
            WriteUniformBlock(uniformColorStart + teamIndex * uniformTeamStride + uniformAwayOffset, colorString);
        }

        public string GetHomeUniform(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return string.Empty;
            return string.Format("Uniform1=0x{0}", ReadUniformBlock(uniformColorStart + teamIndex * uniformTeamStride));
        }

        public string GetAwayUniform(string team)
        {
            int teamIndex = GetTeamIndex(team);
            if (teamIndex < 0)
                return string.Empty;
            return string.Format("Uniform2=0x{0}", ReadUniformBlock(uniformColorStart + teamIndex * uniformTeamStride + uniformAwayOffset));
        }

        public string GetGameUniform(string team)
        {
            return string.Format("{0}, {1}", GetHomeUniform(team), GetAwayUniform(team));
        }

        public String GetProBowlPlayers()
        {
            StringBuilder builder = new StringBuilder(1000);
            builder.Append("# AFC ProBowl players\r\n");
            builder.Append(GetConferenceProBowlPlayers(Conference.AFC));
            builder.Append("\r\n");

            builder.Append("# NFC ProBowl players\r\n");
            builder.Append(GetConferenceProBowlPlayers(Conference.NFC));
            builder.Append("\r\n");
            return builder.ToString();
        }
    }
}
