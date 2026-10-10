using System;
using System.Collections.Generic;

namespace TsbSimModel
{
    /// <summary>
    /// What differs between TSB2 and TSB3 for the model: the ratings on a player line, which seasons/teams
    /// the stock data has and how it's split for the held-out check, and the game's own lookup tables
    /// (used only by the report and the no-dice check).
    /// Both games run the same sim engine (SimTests\TSB2_study\TSB2_Sim_Engine.txt); TSB3 has its own
    /// tables (scramble reversed, other rush/YAC/pass values) and AG among its ratings.
    /// </summary>
    public class Game
    {
        public string Name;                 // "TSB2" / "TSB3"
        public string[] QBRatings, SkillRatings, DefenseRatings, KickerRatings, PunterRatings, LinemanRatings;
        public int[] Seasons;               // the ROM seasons with stock data
        public string[] NotTeams;           // 'teams' that aren't teams (TSB3: free agents, all-time team)
        public int RushTableOffset;         // rush digit -> value table ($BF:FA00 TSB2, $BD:EE00 TSB3)
        public int ScrambleTableOffset;     // scramble digit -> chance out of 256

        /// <summary>
        /// TSB2's stock data is three seasons: the held-out check fits on two and checks on the third.
        /// TSB3's is one season of 30 teams: 3 folds of 10 teams (team 0, 3, 6 .. / 1, 4, 7 .. / 2, 5, 8 ..).
        /// </summary>
        public bool FoldsByTeam;

        /// <summary>
        /// The game's sim reads the team SimData byte as rush defense / pass defense (TSB2), so the model works it
        /// out too. TSB3's team SimData is a defense bonus the game adds to what the defenders give (TSB3Tool):
        /// not a team defense the model can fit.
        /// </summary>
        public bool HasTeamDefense;

        /// <summary>TSB3: the free agent value (Face's last digit), which its team defense adds up, is modelled too.</summary>
        public bool HasFreeAgentValue;

        public static readonly Game Tsb2 = new Game
        {
            Name = "TSB2",
            QBRatings = new[] { "RS", "RP", "MS", "HP", "BB", "PS", "PC", "PA", "AR", "CO" },
            SkillRatings = new[] { "RS", "RP", "MS", "HP", "BB", "BC", "RC" },
            DefenseRatings = new[] { "RS", "RP", "MS", "HP", "BB", "PI", "QU" },
            KickerRatings = new[] { "RS", "RP", "MS", "HP", "BB", "KP", "KA", "AB" },
            PunterRatings = new[] { "RS", "RP", "MS", "HP", "BB", "KP", "AB" },
            LinemanRatings = new[] { "RS", "RP", "MS", "HP", "BB" },
            Seasons = new[] { 1, 2, 3 },
            NotTeams = new string[0],
            RushTableOffset = 0x1FFA00,
            ScrambleTableOffset = 0x1FFA70,
            FoldsByTeam = false,
            HasTeamDefense = true,
            HasFreeAgentValue = false,
        };

        public static readonly Game Tsb3 = new Game
        {
            Name = "TSB3",
            QBRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG", "PS", "PC", "PA", "AR", "CO" },
            SkillRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG", "BC", "RC" },
            DefenseRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" },
            KickerRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "KA", "AB" },
            PunterRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "AB" },
            LinemanRatings = new[] { "RS", "RP", "MS", "HP", "BB", "AG" },
            Seasons = new[] { 1 },
            NotTeams = new[] { "freeAgents", "allTime" },
            RushTableOffset = 0x1EEE00,
            ScrambleTableOffset = 0x1EEE70,
            FoldsByTeam = true,
            HasTeamDefense = false,
            HasFreeAgentValue = true,
        };

        public static Game Parse(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "TSB2": return Tsb2;
                case "TSB3": return Tsb3;
            }
            throw new ArgumentException("Game must be TSB2 or TSB3, not '" + name + "'");
        }

        /// <summary>The ratings on a player line of this position, in order; null for linemen (no sim data).</summary>
        public string[] RatingsFor(string pos)
        {
            if (pos == "QB1" || pos == "QB2") return QBRatings;
            if (Array.IndexOf(Positions.Skill, pos) > -1) return SkillRatings;
            if (Array.IndexOf(Positions.Defense, pos) > -1) return DefenseRatings;
            if (pos == "K") return KickerRatings;
            if (pos == "P") return PunterRatings;
            return null;
        }
    }

    /// <summary>Roster slots, in the games' order.</summary>
    public static class Positions
    {
        public static readonly string[] Skill = { "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        public static readonly string[] Defense = { "RE", "NT", "LE", "RE2", "NT2", "LE2", "ROLB", "RILB", "LILB", "LOLB", "LB5",
                                                    "RCB", "LCB", "DB1", "DB2", "FS", "SS", "DB3" };
        /// <summary>The run-play carrier pick walks QB1, QB2 and the 10; QB2's share is kept 0 (bug G3), so QB1 + 10.</summary>
        public static readonly string[] CarryPool = { "QB1", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };

        /// <summary>QB / RB / WR / TE / DL / LB / DB, or the position itself (K, P).</summary>
        public static string GroupOf(string pos)
        {
            if (pos.StartsWith("QB")) return "QB";
            if (Array.IndexOf(Skill, pos) > -1) return pos.Substring(0, 2);
            int d = Array.IndexOf(Defense, pos);
            if (d > -1) return d < 6 ? "DL" : d < 11 ? "LB" : "DB";
            return pos;
        }
    }
}
