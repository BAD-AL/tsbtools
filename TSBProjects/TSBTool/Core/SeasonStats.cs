using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TSBTool
{
    /// <summary>One player's season stats (a roster slot; only the fields for that slot's kind are used).</summary>
    public class PlayerSeasonStats
    {
        public string Position;
        public int PassAttempts, Completions, PassYards, PassTDs, InterceptionsThrown;
        public int RushAttempts, RushYards, RushTDs;
        public int Receptions, ReceivingYards, ReceivingTDs;
        public int Sacks, Interceptions, InterceptionYards, InterceptionTDs;
        public int ExtraPointsMade, ExtraPointAttempts, FieldGoalsMade, FieldGoalAttempts;
        public int Punts, PuntYards;
        public int KickReturns, KickReturnYards, KickReturnTDs, PuntReturns, PuntReturnYards, PuntReturnTDs;
    }

    /// <summary>One team's season record plus its players' stats (keyed by roster position).</summary>
    public class TeamSeasonStats
    {
        public string Team;
        public int Wins, Losses, Ties, PointsFor, PointsAgainst, PassYardsAllowed, RushYardsAllowed;
        public List<PlayerSeasonStats> Players = new List<PlayerSeasonStats>();

        /// <summary>
        /// Kick/punt returns. Genesis/SNES: the 3 return-team slots ('RET1'-'RET3', the ROM's RETURN_TEAM
        /// order). NES keeps returns per player instead: one entry for each RB/WR/TE, by its position.
        /// </summary>
        public List<PlayerSeasonStats> Returners = new List<PlayerSeasonStats>();

        public PlayerSeasonStats GetPlayer(string position)
        {
            return Players.Find(p => p.Position == position);
        }

        /// <summary>Return-team slot 1-3 (Genesis/SNES).</summary>
        public PlayerSeasonStats GetReturner(int slot)
        {
            return Returners[slot - 1];
        }

        private delegate int StatField(PlayerSeasonStats p);

        private int Sum(StatField field)
        {
            int ret = 0;
            foreach (PlayerSeasonStats p in Players)
                ret += field(p);
            return ret;
        }

        public int RushAttempts { get { return Sum(p => p.RushAttempts); } }
        public int RushYards { get { return Sum(p => p.RushYards); } }
        public int PassAttempts { get { return Sum(p => p.PassAttempts); } }
        public int Completions { get { return Sum(p => p.Completions); } }
        public int PassYards { get { return Sum(p => p.PassYards); } }
        public int Sacks { get { return Sum(p => p.Sacks); } }
        public int InterceptionsMade { get { return Sum(p => p.Interceptions); } }
    }

    /// <summary>
    /// The season stats of a TSB1 season (GenesisSeasonStats, SnesSeasonStats, NesSeasonStats) or a SNES
    /// TSB2 season (Tsb2SeasonStats): they only differ in where the stats are read from. And their text /
    /// CSV output.
    /// </summary>
    public abstract class SeasonStats
    {
        protected static readonly string[] QBs = { "QB1", "QB2" };
        protected static readonly string[] SkillPlayers = { "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        protected static readonly string[] Defenders = { "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS", "DB1", "DB2" };
        protected const int TeamCount = 28;

        public List<TeamSeasonStats> Teams = new List<TeamSeasonStats>();

        /// <summary>
        /// Reads the stats from a Genesis ares save state (.bs1), a SNES TSB1 (.srm) or NES (.sav) battery
        /// save, or a SNES TSB2 battery save (.srm, 32 KB), going by the file's contents (SNES TSB1 and NES
        /// battery saves are both 8 KB, so each layout is tried and the one whose numbers balance wins).
        /// Returns null (with the reason in 'error') when it's none of those, or has no season stats in it.
        /// </summary>
        public static SeasonStats FromFile(byte[] data, out string error)
        {
            if (GenesisSeasonStats.IsAresState(data))
                return GenesisSeasonStats.FromAresState(data, out error);
            if (Tsb2SeasonStats.IsSramSize(data) || Tsb3SeasonStats.IsSramSize(data))
            {
                // both 32 KB: each layout is tried, the one whose numbers balance wins
                SeasonStats ret = Tsb2SeasonStats.FromSram(data, out error);
                if (ret == null)
                    ret = Tsb3SeasonStats.FromSram(data, out error);
                if (ret == null)
                    error = "This battery save has no SNES TSB2 or TSB3 season stats in it. Play or simulate some games of a season first (the emulator writes the .srm when the game is closed).";
                return ret;
            }
            if (SnesSeasonStats.IsSramSize(data) || NesSeasonStats.IsSavSize(data))
            {
                SeasonStats ret = SnesSeasonStats.FromSram(data, out error);
                if (ret == null)
                    ret = NesSeasonStats.FromSav(data, out error);
                if (ret == null)
                    error = "This battery save has no SNES or NES TSB season stats in it. Play or simulate some games of a season first (the emulator writes the battery save when the game is closed).";
                return ret;
            }
            error = "Season stats are read from a Genesis TSB ares save state (.bs1), a SNES TSB, TSB2 or TSB3 (.srm) or NES TSB (.sav) battery save; this file is none of those.";
            return null;
        }

        /// <summary>The first line of ToText, e.g. "GENESIS TSB SEASON STATS".</summary>
        protected abstract string Title { get; }

        /// <summary>The kind of ROM the season was played on, for messages: "Genesis", "SNES", "NES", "32-team NES".</summary>
        public abstract string RomKind { get; }

        /// <summary>True when 'rom' is the kind of ROM the season was played on (RomKind), so it has the players' names.</summary>
        public abstract bool IsRomForTheseStats(ITecmoContent rom);

        /// <summary>
        /// False when the players' rushing yards don't have to add up to the rush yards allowed across the
        /// league (NES: they're a few yards apart), so LooksLikeSeasonStats doesn't check them.
        /// </summary>
        protected virtual bool RushYardsBalance { get { return true; } }

        /// <summary>The player's name ("jim KELLY") from 'rom', or null when 'rom' isn't this game's kind of ROM.</summary>
        protected abstract string GetRomPlayerName(ITecmoContent rom, string team, string position);

        /// <summary>'rom's "RETURN_TEAM RB3, RB4, RCB" line for 'team', or null when 'rom' isn't this game's kind of ROM.</summary>
        protected abstract string GetRomReturnTeam(ITecmoContent rom, string team);

        public TeamSeasonStats GetTeam(string team)
        {
            return Teams.Find(t => t.Team == team);
        }

        // Wins and losses, points for and against, and the players' yards vs. the yards allowed all
        // have to balance across the league; data from anything else won't.
        protected bool LooksLikeSeasonStats()
        {
            int wins = 0, losses = 0, pointsFor = 0, pointsAgainst = 0;
            int rushYards = 0, rushAllowed = 0, passYards = 0, passAllowed = 0;
            foreach (TeamSeasonStats t in Teams)
            {
                if (t.Wins + t.Losses + t.Ties > 20)
                    return false;
                wins += t.Wins; losses += t.Losses;
                pointsFor += t.PointsFor; pointsAgainst += t.PointsAgainst;
                rushYards += t.RushYards; rushAllowed += t.RushYardsAllowed;
                passYards += t.PassYards; passAllowed += t.PassYardsAllowed;
            }
            return wins + losses > 0 && wins == losses && pointsFor == pointsAgainst &&
                   (rushYards == rushAllowed || !RushYardsBalance) && passYards == passAllowed;
        }

        // ----------------------------------------------------------------------------------------
        // Text output

        protected static string Capitalize(string team)
        {
            return team.Substring(0, 1).ToUpper() + team.Substring(1);
        }

        private static string Average(int total, int count)
        {
            return count == 0 ? "-" : ((double)total / count).ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static bool HasReturns(PlayerSeasonStats r)
        {
            return r.KickReturns > 0 || r.PuntReturns > 0;
        }

        /// <summary>
        /// The roster position of each of t.Returners: the return-team slots' positions from 'rom'
        /// (RETURN_TEAM), else each returner's own Position (NES returners are the RB/WR/TE themselves;
        /// Genesis/SNES slots without a ROM stay 'RET1'-'RET3').
        /// </summary>
        private string[] GetReturnerPositions(TeamSeasonStats t, ITecmoContent rom)
        {
            string[] ret = GetReturnTeamPositions(t, rom);
            if (ret == null || ret.Length != t.Returners.Count)
            {
                ret = new string[t.Returners.Count];
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = t.Returners[i].Position;
            }
            return ret;
        }

        /// <summary>
        /// The roster position of each return-team slot, from 'rom' (RETURN_TEAM), or null when there's
        /// no ROM (or it can't be read).
        /// </summary>
        private string[] GetReturnTeamPositions(TeamSeasonStats t, ITecmoContent rom)
        {
            if (rom == null)
                return null;
            string returnTeam = GetRomReturnTeam(rom, t.Team); // "RETURN_TEAM RB3, RB4, RCB"
            if (returnTeam == null || !returnTeam.StartsWith("RETURN_TEAM "))
                return null;
            string[] ret = returnTeam.Substring("RETURN_TEAM ".Length).Split(',');
            if (ret.Length != 3)
                return null;
            for (int i = 0; i < ret.Length; i++)
                ret[i] = ret[i].Trim();
            return ret;
        }

        /// <summary>
        /// The stats as readable text, team by team. Player names come from 'rom' (the ROM the season
        /// was played with); when it's null (or another game's ROM), players get generic names like
        /// 'QB1 Bills'.
        /// </summary>
        public string ToText(ITecmoContent rom)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Title + "\n");
            if (rom == null)
                sb.Append("(No ROM loaded: players are shown by position. Load the ROM the season was played with to see names.)\n");
            sb.Append("\n");

            sb.Append("STANDINGS                W   L   T    PF    PA   Rush yds allowed   Pass yds allowed\n");
            foreach (TeamSeasonStats t in Teams)
                sb.Append(string.Format("  {0,-20} {1,3} {2,3} {3,3} {4,5} {5,5} {6,18} {7,18}\n",
                    Capitalize(t.Team), t.Wins, t.Losses, t.Ties, t.PointsFor, t.PointsAgainst, t.RushYardsAllowed, t.PassYardsAllowed));
            sb.Append("\n");

            sb.Append("TEAM OFFENSE / DEFENSE   Rush att  Rush yds  Pass att  Comp  Pass yds   Sacks  INTs\n");
            foreach (TeamSeasonStats t in Teams)
                sb.Append(string.Format("  {0,-20} {1,9} {2,9} {3,9} {4,5} {5,9} {6,7} {7,5}\n",
                    Capitalize(t.Team), t.RushAttempts, t.RushYards, t.PassAttempts, t.Completions, t.PassYards, t.Sacks, t.InterceptionsMade));

            foreach (TeamSeasonStats t in Teams)
                AppendTeam(sb, t, rom);
            return sb.ToString();
        }

        // ----------------------------------------------------------------------------------------
        // CSV output

        private const string CsvHeader =
            "Team,Position,Group,Name,PassAtt,Comp,Comp%,PassYds,PassAvg,PassTD,INTThrown," +
            "RushAtt,RushYds,RushAvg,RushTD,Rec,RecYds,RecAvg,RecTD,Sacks,INT,INTYds,INTTD," +
            "XP,XPA,FG,FGA,Punts,PuntYds,PuntAvg,KR,KRYds,KRAvg,KRTD,PR,PRYds,PRAvg,PRTD";

        private static string GetGroup(string position)
        {
            switch (position)
            {
                case "RE": case "NT": case "LE":
                case "RE2": case "NT2": case "LE2": // TSB2
                    return "DL";
                case "ROLB": case "RILB": case "LILB": case "LOLB":
                case "LB5": // TSB2
                    return "LB";
                case "RCB": case "LCB": case "FS": case "SS": case "DB1": case "DB2":
                case "DB3": // TSB2
                    return "DB";
                case "C": case "LG": case "RG": case "LT": case "RT":
                    return "OL";
                case "K": case "P":
                    return position;
                case "RET1": case "RET2": case "RET3":
                    return "RET";
                default:
                    return position.Substring(0, 2); // QB, RB, WR, TE
            }
        }

        private static string CsvAverage(int total, int count)
        {
            return count == 0 ? "" : Average(total, count);
        }

        private static string Num(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The stats as one CSV table: one row per player who has stats (Team, Position, Group, Name,
        /// then every stat column; columns that don't apply to the player are blank), so it can be
        /// sorted/filtered in a spreadsheet. Group is QB, RB, WR, TE, DL, LB, DB, K or P. Names as in
        /// ToText (from 'rom', or 'QB1 Bills' style when it's null). Kick/punt returns go on the
        /// returner's own row (NES; Genesis/SNES when 'rom' says who the return-team players are);
        /// Genesis/SNES returns without a ROM get their own rows ('RET1'-'RET3', Group RET).
        /// </summary>
        public string ToCsv(ITecmoContent rom)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CsvHeader + "\n");
            foreach (TeamSeasonStats t in Teams)
            {
                string[] returnPositions = GetReturnerPositions(t, rom);
                bool[] returnerShown = new bool[t.Returners.Count];
                foreach (PlayerSeasonStats p in t.Players)
                {
                    PlayerSeasonStats returner = null;
                    for (int i = 0; i < returnPositions.Length; i++)
                    {
                        if (returnPositions[i] == p.Position && HasReturns(t.Returners[i]))
                        {
                            returner = t.Returners[i];
                            returnerShown[i] = true;
                        }
                    }
                    AppendCsvRow(sb, t, p, returner, rom);
                }
                // returners not on a player row above: no ROM, or a return-team player without a stats slot (OL)
                for (int i = 0; i < t.Returners.Count; i++)
                {
                    if (returnerShown[i] || !HasReturns(t.Returners[i]))
                        continue;
                    PlayerSeasonStats who = new PlayerSeasonStats();
                    who.Position = returnPositions[i];
                    AppendCsvRow(sb, t, who, t.Returners[i], rom);
                }
            }
            return sb.ToString();
        }

        private void AppendCsvRow(StringBuilder sb, TeamSeasonStats t, PlayerSeasonStats p,
            PlayerSeasonStats returner, ITecmoContent rom)
        {
            string group = GetGroup(p.Position);
            string[] pass = { "", "", "", "", "", "", "" };
            string[] rush = { "", "", "", "" };
            string[] rec = { "", "", "", "" };
            string[] def = { "", "", "", "" };
            string[] kick = { "", "", "", "" };
            string[] punt = { "", "", "" };
            string[] returns = { "", "", "", "", "", "", "", "" };
            bool hasStats = false;
            if (group == "QB")
            {
                hasStats = p.PassAttempts > 0 || p.RushAttempts > 0;
                pass = new string[] { Num(p.PassAttempts), Num(p.Completions), CsvAverage(p.Completions * 100, p.PassAttempts),
                    Num(p.PassYards), CsvAverage(p.PassYards, p.PassAttempts), Num(p.PassTDs), Num(p.InterceptionsThrown) };
                rush = new string[] { Num(p.RushAttempts), Num(p.RushYards), CsvAverage(p.RushYards, p.RushAttempts), Num(p.RushTDs) };
            }
            else if (group == "RB" || group == "WR" || group == "TE")
            {
                hasStats = p.RushAttempts > 0 || p.Receptions > 0;
                rush = new string[] { Num(p.RushAttempts), Num(p.RushYards), CsvAverage(p.RushYards, p.RushAttempts), Num(p.RushTDs) };
                rec = new string[] { Num(p.Receptions), Num(p.ReceivingYards), CsvAverage(p.ReceivingYards, p.Receptions), Num(p.ReceivingTDs) };
            }
            else if (group == "K")
            {
                hasStats = p.ExtraPointAttempts > 0 || p.FieldGoalAttempts > 0;
                kick = new string[] { Num(p.ExtraPointsMade), Num(p.ExtraPointAttempts), Num(p.FieldGoalsMade), Num(p.FieldGoalAttempts) };
            }
            else if (group == "P")
            {
                hasStats = p.Punts > 0;
                punt = new string[] { Num(p.Punts), Num(p.PuntYards), CsvAverage(p.PuntYards, p.Punts) };
            }
            else if (group == "DL" || group == "LB" || group == "DB")
            {
                hasStats = p.Sacks > 0 || p.Interceptions > 0;
                def = new string[] { Num(p.Sacks), Num(p.Interceptions), Num(p.InterceptionYards), Num(p.InterceptionTDs) };
            }
            if (returner != null)
            {
                hasStats = true;
                returns = new string[] {
                    Num(returner.KickReturns), Num(returner.KickReturnYards), CsvAverage(returner.KickReturnYards, returner.KickReturns), Num(returner.KickReturnTDs),
                    Num(returner.PuntReturns), Num(returner.PuntReturnYards), CsvAverage(returner.PuntReturnYards, returner.PuntReturns), Num(returner.PuntReturnTDs) };
            }
            if (!hasStats)
                return;

            List<string> cells = new List<string>();
            cells.Add(t.Team);
            cells.Add(p.Position);
            cells.Add(group);
            cells.Add("\"" + PlayerName(t, p, rom).Replace("\"", "\"\"") + "\"");
            cells.AddRange(pass);
            cells.AddRange(rush);
            cells.AddRange(rec);
            cells.AddRange(def);
            cells.AddRange(kick);
            cells.AddRange(punt);
            cells.AddRange(returns);
            sb.Append(string.Join(",", cells.ToArray()) + "\n");
        }

        private string PlayerName(TeamSeasonStats t, PlayerSeasonStats p, ITecmoContent rom)
        {
            string name = null;
            if (rom != null)
                name = GetRomPlayerName(rom, t.Team, p.Position);
            if (string.IsNullOrEmpty(name))
                name = p.Position + " " + Capitalize(t.Team);
            return name;
        }

        private void AppendTeam(StringBuilder sb, TeamSeasonStats t, ITecmoContent rom)
        {
            sb.Append(string.Format("\n==== {0}  ({1}-{2}-{3}, PF {4}, PA {5}) ====\n",
                t.Team.ToUpper(), t.Wins, t.Losses, t.Ties, t.PointsFor, t.PointsAgainst));

            const string row = "  {0,-5} {1,-22}";
            List<PlayerSeasonStats> list = t.Players.FindAll(p => p.PassAttempts > 0);
            if (list.Count > 0)
            {
                sb.Append("PASSING                        Att  Comp  Comp%   Yds   Avg  TD  INT\n");
                foreach (PlayerSeasonStats p in list)
                    sb.Append(string.Format(row + " {2,5} {3,5} {4,6} {5,5} {6,5} {7,3} {8,4}\n",
                        p.Position, PlayerName(t, p, rom), p.PassAttempts, p.Completions,
                        Average(p.Completions * 100, p.PassAttempts), p.PassYards, Average(p.PassYards, p.PassAttempts),
                        p.PassTDs, p.InterceptionsThrown));
            }
            list = t.Players.FindAll(p => p.RushAttempts > 0);
            if (list.Count > 0)
            {
                sb.Append("RUSHING                        Att   Yds   Avg  TD\n");
                foreach (PlayerSeasonStats p in list)
                    sb.Append(string.Format(row + " {2,5} {3,5} {4,5} {5,3}\n",
                        p.Position, PlayerName(t, p, rom), p.RushAttempts, p.RushYards, Average(p.RushYards, p.RushAttempts), p.RushTDs));
            }
            list = t.Players.FindAll(p => p.Receptions > 0);
            if (list.Count > 0)
            {
                sb.Append("RECEIVING                      Rec   Yds   Avg  TD\n");
                foreach (PlayerSeasonStats p in list)
                    sb.Append(string.Format(row + " {2,5} {3,5} {4,5} {5,3}\n",
                        p.Position, PlayerName(t, p, rom), p.Receptions, p.ReceivingYards, Average(p.ReceivingYards, p.Receptions), p.ReceivingTDs));
            }
            list = t.Players.FindAll(p => p.Sacks > 0 || p.Interceptions > 0);
            if (list.Count > 0)
            {
                sb.Append("DEFENSE                      Sacks  INTs  INT yds  INT TD\n");
                foreach (PlayerSeasonStats p in list)
                    sb.Append(string.Format(row + " {2,5} {3,5} {4,8} {5,7}\n",
                        p.Position, PlayerName(t, p, rom), p.Sacks, p.Interceptions, p.InterceptionYards, p.InterceptionTDs));
            }
            PlayerSeasonStats k = t.GetPlayer("K");
            PlayerSeasonStats punter = t.GetPlayer("P");
            sb.Append("KICKING                         XP   XPA    FG   FGA\n");
            sb.Append(string.Format(row + " {2,5} {3,5} {4,5} {5,5}\n",
                k.Position, PlayerName(t, k, rom), k.ExtraPointsMade, k.ExtraPointAttempts, k.FieldGoalsMade, k.FieldGoalAttempts));
            sb.Append("PUNTING                       Punts   Yds   Avg\n");
            sb.Append(string.Format(row + " {2,5} {3,5} {4,5}\n",
                punter.Position, PlayerName(t, punter, rom), punter.Punts, punter.PuntYards, Average(punter.PuntYards, punter.Punts)));

            string[] returnPositions = GetReturnerPositions(t, rom);
            bool header = false;
            for (int i = 0; i < t.Returners.Count; i++)
            {
                PlayerSeasonStats r = t.Returners[i];
                if (!HasReturns(r))
                    continue;
                if (!header)
                {
                    sb.Append("RETURNS                         KR   Yds   Avg  TD    PR   Yds   Avg  TD\n");
                    header = true;
                }
                PlayerSeasonStats who = new PlayerSeasonStats();
                who.Position = returnPositions[i];
                sb.Append(string.Format(row + " {2,5} {3,5} {4,5} {5,3} {6,5} {7,5} {8,5} {9,3}\n",
                    who.Position, PlayerName(t, who, rom),
                    r.KickReturns, r.KickReturnYards, Average(r.KickReturnYards, r.KickReturns), r.KickReturnTDs,
                    r.PuntReturns, r.PuntReturnYards, Average(r.PuntReturnYards, r.PuntReturns), r.PuntReturnTDs));
            }
        }
    }
}
