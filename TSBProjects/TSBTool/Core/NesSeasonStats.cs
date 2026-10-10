using System;

namespace TSBTool
{
    /// <summary>
    /// Reads the season stats of NES TSB out of the game's battery save (.sav, 8 KB = SRAM $6000-$7FFF,
    /// as Mesen2 / FCEUX / Nestopia write it). Layout from bruddog's disassembly of the game
    /// (github.com/bruddog/Tecmo_Super_Bowl_NES_Disassembly: sram_variables.asm, stat_indexes.asm,
    /// ADD_ALL_GAME_STATS_TO_SEASON_STATS):
    ///  - Each team has a 0xD0-byte block. Stock 28-team game: teams 0-8 (TecmoTool order) at file offset
    ///    0x7AE + k*0xD0, then 260 unused bytes, teams 9-27 at 0x1002. 32-team ROMs (CXROM order, AFC/NFC
    ///    pro bowl teams skipped): teams 0-27 at 0x7A0 + k*0xD0; the 49ers, rams, seahawks and cardinals
    ///    at 0x1E60, 0x1F30, 0x250, 0x320 (the hack's own table at ROM 0x3F652).
    ///  - Values are bit-packed (bN = byte N of the player's stats; "b0 att (bits 8-9 in b2 bits 0-1)":
    ///    b0 holds the low 8 bits of the pass attempts, bits 0-1 of b2 hold bits 8-9):
    ///      +0x00  QB1, QB2 (9 bytes): b0 att (bits 8-9 in b2 bits 0-1), b1 comp (bits 8-9 in b3 bits 0-1),
    ///             b2 bits 2-7 pass TD, b3 bits 2-7 INT, b4 pass yds (bits 8-12 in b7 bits 3-7),
    ///             b5 rush att, b6 rush yds (bits 8-10 in b7 bits 0-2), b8 bits 2-7 rush TD
    ///      +0x12  RB1-4, WR1-4, TE1-2 (12 bytes): b0 rec, b1 rec yds (bits 8-11 in b8 bits 0-3),
    ///             b2 bits 2-7 rec TD, b3 bits 1-7 KR, b4 KR yds (bits 8-10 in b5 bits 0-2), b5 bits 3-7
    ///             KR TD, b6 bits 2-7 PR, b7 PR yds (bits 8-9 in b2 bits 0-1), b8 bits 4-7 PR TD,
    ///             b9 rush att, b10 rush yds (bits 8-9 in b6 bits 0-1, bits 10-11 in b11 bits 0-1),
    ///             b11 bits 2-7 rush TD
    ///      +0x8A  RE..SS (3 bytes, no DB1/DB2): b0 bits 1-7 sacks, b1 bits 4-7 INT (bit 4 in b0 bit 0),
    ///             b1 bits 1-3 INT TD, b2 INT yds (bit 8 in b1 bit 0)
    ///      +0xAB  K: XP att, XP made, FG att, FG made.   +0xAF  P: punts, punt yds (2)
    ///      +0xB2  W, L, T, PF (2), PA (2), pass yds allowed (2), rush yds allowed (2)
    ///    2-byte values are little-endian. All values are unsigned: the game sets a season yards total
    ///    that goes below 0 to 0. Kick/punt returns are kept per RB/WR/TE (no return-team slots).
    /// Checked with full seasons of the stock game and two 32-team ROMs (TSBTool_Test\SimTests\
    /// stats_research\NES): the numbers balance across the league and every team's points add up.
    /// </summary>
    public class NesSeasonStats : SeasonStats
    {
        private const int SavSize = 0x2000;
        private const int TeamSize = 0xD0;

        // Team names in the game's team id order. Not TecmoTool.Teams: TecmoToolFactory switches that
        // list to the last loaded ROM's teams.
        private static readonly string[] OriginalTeams = {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "redskins","giants", "eagles",   "cardinals", "cowboys",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "49ers",   "rams",   "saints",   "falcons"
        };
        private static readonly string[] Teams32 = {
            "bills",     "dolphins", "patriots", "jets",
            "bengals",    "browns",  "ravens",   "steelers",
            "colts",      "texans",  "jaguars",  "titans",
            "broncos",    "chiefs",  "raiders",  "chargers",
            "redskins",   "giants",  "eagles",   "cowboys",
            "bears",      "lions",   "packers",  "vikings",
            "buccaneers", "saints",  "falcons",  "panthers",
            "49ers",   "rams", "seahawks",   "cardinals"
        };
        private static readonly int[] ExpansionTeamBlocks32 = { 0x1E60, 0x1F30, 0x250, 0x320 };

        private static readonly string[] NesDefenders = { "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS" };

        private byte[] mSav;
        private bool m32Team;

        private NesSeasonStats(byte[] sav, bool is32Team)
        {
            mSav = sav;
            m32Team = is32Team;
        }

        /// <summary>True when 'data' is the size of the NES TSB battery save.</summary>
        public static bool IsSavSize(byte[] data)
        {
            return data != null && data.Length == SavSize;
        }

        /// <summary>
        /// Reads the stats from the bytes of a NES TSB .sav file, stock 28-team game or 32-team ROM.
        /// Returns null (with the reason in 'error') when it isn't one or there are no season stats in it.
        /// </summary>
        public static NesSeasonStats FromSav(byte[] sav, out string error)
        {
            error = null;
            if (!IsSavSize(sav))
            {
                error = "This isn't a NES TSB battery save (.sav, 8 KB).";
                return null;
            }
            foreach (bool is32Team in new bool[] { false, true })
            {
                NesSeasonStats ret = new NesSeasonStats(sav, is32Team);
                string[] teams = is32Team ? Teams32 : OriginalTeams;
                for (int k = 0; k < teams.Length; k++)
                    ret.Teams.Add(ret.ReadTeam(teams[k], GetTeamBlock(k, is32Team)));
                if (ret.LooksLikeSeasonStats())
                    return ret;
            }
            error = "This battery save has no NES TSB season stats in it. Play or simulate some games of a season first (the emulator writes the .sav when the game is closed).";
            return null;
        }

        private static int GetTeamBlock(int teamIndex, bool is32Team)
        {
            if (is32Team)
                return teamIndex < 28 ? 0x7A0 + teamIndex * TeamSize : ExpansionTeamBlocks32[teamIndex - 28];
            return teamIndex < 9 ? 0x7AE + teamIndex * TeamSize : 0x1002 + (teamIndex - 9) * TeamSize;
        }

        /// <summary>True for a season of a 32-team ROM (CXROM based), false for the stock 28-team game.</summary>
        public bool Is32Team { get { return m32Team; } }

        protected override string Title { get { return "NES TSB SEASON STATS"; } }

        public override string RomKind { get { return m32Team ? "32-team NES" : "NES"; } }

        public override bool IsRomForTheseStats(ITecmoContent rom)
        {
            return m32Team ? rom is CXRomTSBTool : rom is TecmoTool && !(rom is CXRomTSBTool);
        }

        // NES players' rushing yards and the teams' rush yards allowed end up a few yards apart.
        protected override bool RushYardsBalance { get { return false; } }

        protected override string GetRomPlayerName(ITecmoContent rom, string team, string position)
        {
            return IsRomForTheseStats(rom) ? ((TecmoTool)rom).GetName(team, position) : null;
        }

        // Returns are per player on NES (no return-team slots).
        protected override string GetRomReturnTeam(ITecmoContent rom, string team)
        {
            return null;
        }

        private int Byte(int offset)
        {
            return mSav[offset];
        }

        private int Word(int offset)
        {
            return mSav[offset] | (mSav[offset + 1] << 8);
        }

        private TeamSeasonStats ReadTeam(string team, int q)
        {
            TeamSeasonStats t = new TeamSeasonStats();
            t.Team = team;

            for (int i = 0; i < QBs.Length; i++)
            {
                int a = q + i * 9;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = QBs[i];
                p.PassAttempts = Byte(a) | ((Byte(a + 2) & 3) << 8);
                p.Completions = Byte(a + 1) | ((Byte(a + 3) & 3) << 8);
                p.PassTDs = Byte(a + 2) >> 2;
                p.InterceptionsThrown = Byte(a + 3) >> 2;
                p.PassYards = Byte(a + 4) | ((Byte(a + 7) >> 3) << 8);
                p.RushAttempts = Byte(a + 5);
                p.RushYards = Byte(a + 6) | ((Byte(a + 7) & 7) << 8);
                p.RushTDs = Byte(a + 8) >> 2;
                t.Players.Add(p);
            }
            for (int i = 0; i < SkillPlayers.Length; i++)
            {
                int a = q + 0x12 + i * 12;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = SkillPlayers[i];
                p.Receptions = Byte(a);
                p.ReceivingYards = Byte(a + 1) | ((Byte(a + 8) & 0xF) << 8);
                p.ReceivingTDs = Byte(a + 2) >> 2;
                p.RushAttempts = Byte(a + 9);
                p.RushYards = Byte(a + 10) | ((Byte(a + 6) & 3) << 8) | ((Byte(a + 11) & 3) << 10);
                p.RushTDs = Byte(a + 11) >> 2;
                t.Players.Add(p);

                PlayerSeasonStats r = new PlayerSeasonStats();
                r.Position = SkillPlayers[i];
                r.KickReturns = Byte(a + 3) >> 1;
                r.KickReturnYards = Byte(a + 4) | ((Byte(a + 5) & 7) << 8);
                r.KickReturnTDs = Byte(a + 5) >> 3;
                r.PuntReturns = Byte(a + 6) >> 2;
                r.PuntReturnYards = Byte(a + 7) | ((Byte(a + 2) & 3) << 8);
                r.PuntReturnTDs = Byte(a + 8) >> 4;
                t.Returners.Add(r);
            }
            for (int i = 0; i < NesDefenders.Length; i++)
            {
                int a = q + 0x8A + i * 3;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = NesDefenders[i];
                p.Sacks = Byte(a) >> 1;
                p.Interceptions = (Byte(a + 1) >> 4) | ((Byte(a) & 1) << 4);
                p.InterceptionTDs = (Byte(a + 1) >> 1) & 7;
                p.InterceptionYards = Byte(a + 2) | ((Byte(a + 1) & 1) << 8);
                t.Players.Add(p);
            }
            PlayerSeasonStats k = new PlayerSeasonStats();
            k.Position = "K";
            k.ExtraPointAttempts = Byte(q + 0xAB);
            k.ExtraPointsMade = Byte(q + 0xAC);
            k.FieldGoalAttempts = Byte(q + 0xAD);
            k.FieldGoalsMade = Byte(q + 0xAE);
            t.Players.Add(k);

            PlayerSeasonStats punter = new PlayerSeasonStats();
            punter.Position = "P";
            punter.Punts = Byte(q + 0xAF);
            punter.PuntYards = Word(q + 0xB0);
            t.Players.Add(punter);

            int w = q + 0xB2;
            t.Wins = Byte(w);
            t.Losses = Byte(w + 1);
            t.Ties = Byte(w + 2);
            t.PointsFor = Word(w + 3);
            t.PointsAgainst = Word(w + 5);
            t.PassYardsAllowed = Word(w + 7);
            t.RushYardsAllowed = Word(w + 9);
            return t;
        }
    }
}
