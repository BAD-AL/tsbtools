using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TSBTool
{
    /// <summary>
    /// Genesis TSB1's schedule -- one season only (per the project owner's decision to match SNES
    /// TSB1's single-season shape rather than TSB2Tool's season-parameterized one, see
    /// compressed-riding-rossum.md), specifically the ROM's *third* physical season block. This ROM
    /// carries 3 complete, independent 224-game schedules back-to-back (season 1, season 2, season 3) 
    /// and only the third is the real 1993 schedule: it's the
    /// only one of the three with an 18-week/bye-week structure (games-per-week
    /// `14 14 10 10 11 11 10 10 12 12 13 13 14 14 14 14 14 14`) matching the NFL's actual 1993 format,
    /// confirmed directly by decoding its week 1 -- BUF at NE, CLE at CIN, IND at MIA, etc. -- against
    /// the real 1993 opening week. Seasons 1 and 2 are flat 17-week blocks (no bye-week dip) and are
    /// 1991/1992.  Structurally, this class is 
    /// modeled on Core\SchedulerHelper2.cs's algorithm (running week/game counters, a games-per-week
    /// sentinel array, and a week-pointer table kept in sync as a write-time side effect) -- but NOT a
    /// subclass of it, since ScheduleHelper2's pointer read/write is a private, non-virtual 2-byte-stride
    /// method and Genesis needs a 4-byte, reversed byte order ("hh ll 00 00", confirmed directly against
    /// real ROM bytes) stride instead.
    ///
    /// Team lookups go through the shared TecmoTool.GetTeamIndex/GetTeamFromIndex statics, matching
    /// every other schedule helper in this codebase (SNES_ScheduleHelper included) -- TecmoToolFactory
    /// sets TecmoTool.Teams to Genesis's team order before constructing Genesis_TSB1Tool, so this
    /// resolves correctly (same mechanism Phase 1's team-strings work depends on).
    ///
    /// Season 3 always has exactly 18 weeks / 224 games (confirmed against
    /// TSBTool_Test\Genesis_TSB1_ROM_Findings.md and the real ROM)
    /// </summary>
    public class Genesis_ScheduleHelper
    {
        // Global week-pointer table base is 0x5210 (55 x 4-byte entries covering all 3 seasons); season
        // 3's 18 week-pointers are entries 34-51, so this is 0x5210 + 4*34 -- confirmed directly: entry
        // 34's stock value is 0x56A2, exactly weekOneStartLoc below.
        private const int weekPointersStartLoc = 0x5298; // 4-byte entries, format "hh ll 00 00", week N at index N
        private const int gamesPerWeekStartLoc = 0x530E; // 1 byte/week, 18 values, fixed location (never derived from the pointer table)
        private const int weekOneStartLoc = 0x56A2;       // (away,home) team-index byte pairs, 2 bytes/game
        private const int endScheduleSection = 0x5862;    // exclusive -- end of season 3's schedule block (start of the roster pointer table)
        private const int totalWeeks = 18;
        private const int gamePerWeekLimit = 14;
        private const int totalGameLimit = 224;
        private const int gamesPerTeamExpected = 16;

        private int week = -1;
        private int week_game_count = 0;
        private int total_game_count = 0;
        private int[] teamGames;

        private List<string> messages;
        private byte[] outputRom;
        private Regex gameRegex;

        public Genesis_ScheduleHelper(byte[] outputRom)
        {
            this.outputRom = outputRom;
            gameRegex = new Regex("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
        }

        public void ApplySchedule(List<string> lines)
        {
            week = -1;
            week_game_count = 0;
            total_game_count = 0;
            messages = new List<string>(50);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].ToString().Trim().ToLower();
                try
                {
                    if (line.StartsWith("#") || line.Length < 3)
                    {
                        // do nothing.
                    }
                    else if (line.StartsWith("week"))
                    {
                        if (week > totalWeeks - 1)
                        {
                            AddMessage(string.Format("Error! You can have only {0} weeks in a season.", totalWeeks));
                            break;
                        }
                        SetupWeek();
                    }
                    else
                    {
                        ScheduleGame(line);
                    }
                }
                catch (Exception e)
                {
                    StaticUtils.WriteError(string.Format("Exception! with line '{0}' {1}\n{2}", line, e.Message, e.StackTrace));
                    AddMessage(string.Format("Error on line '{0}'", line));
                }
            }
            ClosePrevWeek(); // close off the last week.
            if (week < totalWeeks - 1)
            {
                AddMessage(string.Format("Warning! You didn't schedule all {0} weeks. The schedule could be messed up.", totalWeeks));
            }
            if (teamGames != null)
            {
                for (int i = 0; i < teamGames.Length; i++)
                {
                    if (teamGames[i] != gamesPerTeamExpected)
                    {
                        AddMessage(string.Format(
                            "Warning! The {0} have {1} games scheduled.",
                            TecmoTool.GetTeamFromIndex(i), teamGames[i]));
                    }
                }
            }
        }

        private void SetupWeek()
        {
            ClosePrevWeek();
            week++;
            total_game_count += week_game_count;
            week_game_count = 0;
            SetupPointerForCurrentWeek();
        }

        private void ClosePrevWeek()
        {
            if (week > -1 && week < totalWeeks)
            {
                outputRom[gamesPerWeekStartLoc + week] = (byte)week_game_count;
                if (week_game_count == 0)
                {
                    AddMessage(string.Format("ERROR! Week {0}. You need at least 1 game in each week.", week + 1));
                }
            }
        }

        // Week 0's pointer is fixed (always weekOneStartLoc) and never written, matching
        // ScheduleHelper2's own "if (week == 0) return" convention.
        private void SetupPointerForCurrentWeek()
        {
            if (week == 0)
                return;
            int val = weekOneStartLoc + (2 * total_game_count);
            int location = weekPointersStartLoc + (week * 4);
            if (week < totalWeeks)
            {
                // "hh ll 00 00" -- only the first 2 bytes are a real pointer value; the trailing 2
                // bytes are left untouched (confirmed against real ROM bytes to already be 0x00 0x00
                // for every week-pointer entry actually written here).
                outputRom[location] = (byte)((val >> 8) & 0xFF);
                outputRom[location + 1] = (byte)(val & 0xFF);
            }
            else
            {
                AddMessage(string.Format("ERROR! Too many weeks {0}", week + 1));
            }
        }

        private bool ScheduleGame(string awayTeam, string homeTeam)
        {
            int awayIndex = TecmoTool.GetTeamIndex(awayTeam);
            int homeIndex = TecmoTool.GetTeamIndex(homeTeam);

            if (awayIndex == -1 || homeIndex == -1)
            {
                AddMessage(string.Format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, week + 1));
                return false;
            }
            if (awayIndex == homeIndex)
            {
                AddMessage(string.Format(
                    "Warning! Week {0}: The {1} are scheduled to play against themselves.", week + 1, awayTeam));
            }

            int location = weekOneStartLoc + ((week_game_count + total_game_count) * 2);
            if (location >= weekOneStartLoc && location < endScheduleSection)
            {
                outputRom[location] = (byte)awayIndex;
                outputRom[location + 1] = (byte)homeIndex;
                IncrementTeamGames(awayIndex);
                IncrementTeamGames(homeIndex);
                return true;
            }
            return false;
        }

        private void ScheduleGame(string line)
        {
            Match m = gameRegex.Match(line);
            if (m != Match.Empty)
            {
                string awayTeam = m.Groups[1].ToString();
                string homeTeam = m.Groups[2].ToString();
                if (week_game_count > gamePerWeekLimit - 1)
                {
                    AddMessage(string.Format(
                        "Error! Week {0}: You can have no more than {1} games in a week.", week + 1, gamePerWeekLimit));
                }
                else if (ScheduleGame(awayTeam, homeTeam))
                {
                    week_game_count++;
                }
            }
            if (total_game_count + week_game_count > totalGameLimit)
            {
                AddMessage(string.Format(
                    "Warning! Week {0}: There are more than {1} games scheduled.", week + 1, totalGameLimit));
            }
        }

        public string GetSchedule()
        {
            StringBuilder sb = new StringBuilder(totalWeeks * 28 * 12);
            for (int i = 0; i < totalWeeks; i++)
            {
                sb.Append(string.Format("WEEK {0}\n", i + 1));
                sb.Append(GetWeek(i) + "\n");
            }
            return sb.ToString();
        }

        public string GetWeek(int week)
        {
            if (week < 0 || week > totalWeeks - 1)
            {
                AddMessage(string.Format("Programming Error! 'GetWeek' Week must be in the range 0-{0}.", totalWeeks - 1));
                return null;
            }

            StringBuilder sb = new StringBuilder(14 * 12);
            int gamesInWeek = GetGamesInWeek(week);
            int prevGames = 0;
            for (int i = 0; i < week; i++)
                prevGames += GetGamesInWeek(i);
            int gameLocation = weekOneStartLoc + (2 * prevGames);
            for (int i = 0; i < gamesInWeek; i++)
                sb.Append(GetGame(gameLocation + (2 * i)));
            return sb.ToString();
        }

        public string GetGame(int romLocation)
        {
            int away = outputRom[romLocation];
            int home = outputRom[romLocation + 1];
            string awayTeam = TecmoTool.GetTeamFromIndex(away);
            string homeTeam = TecmoTool.GetTeamFromIndex(home);
            return string.Format("{0} at {1}\n", awayTeam, homeTeam);
        }

        public int GetGamesInWeek(int week)
        {
            if (week < 0 || week > totalWeeks - 1)
            {
                AddMessage(string.Format("Programming Error! GetGamesInWeek Week {0} is invalid. Week range = 0-{1}.", week, totalWeeks - 1));
                return -1;
            }
            return outputRom[gamesPerWeekStartLoc + week];
        }

        private void IncrementTeamGames(int teamIndex)
        {
            if (teamGames == null)
                teamGames = new int[TecmoTool.Teams.Count];
            teamGames[teamIndex]++;
        }

        private void AddMessage(string message)
        {
            if (message != null && message.Length > 0 && messages != null)
                messages.Add(message);
        }

        public List<string> GetErrorMessages()
        {
            return messages;
        }
    }
}
