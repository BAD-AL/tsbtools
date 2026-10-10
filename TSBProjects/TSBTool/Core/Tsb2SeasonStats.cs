using System;

namespace TSBTool
{
    /// <summary>
    /// Reads the season stats of SNES TSB2 out of the game's battery save (.srm, 32 KB, as Mesen2 / snes9x
    /// write it). 2-byte values are little-endian; yards are signed.
    ///  - 0xD6 (bits 0-1): the season that was played, 0/1/2 = 1992/1993/1994 = TSBTool seasons 1/2/3.
    ///  - Team records, team k (TSB2 roster order) at 0x3A2 + k*163: W, L, T, PF (2), PA (2),
    ///    pass yds allowed (2), rush yds allowed (2).
    ///  - Players, team k at 0x152A + k*485:
    ///      +0     QB1, QB2 (13 bytes): pass TD, att (2), INT, comp (2), pass yds (2), rush TD, rush att (2),
    ///             rush yds (2)
    ///      +26    RB1-4, WR1-4, TE1-2 (22 bytes): rec TD, rec (2), rec yds (2), rush TD, rush att (2),
    ///             rush yds (2), rec long, rush long, then the return block (10 bytes): KR TD, KR,
    ///             KR yds (2), PR TD, PR, PR yds (2), KR long, PR long
    ///      +246   the 18 defenders in TSB2Tool's order (RE .. LB5: 9 bytes, RCB .. DB3: 19 bytes): sacks, 0,
    ///             INT, INT TD, INT yds (2), tackles (2), forced fumbles, and for the 7 DBs the return block
    ///      +478   K: XP made, XP att, FG made, FG att.   +482  P: punts, punt yds (2)
    ///    Kick/punt returns are kept per player (RB/WR/TE and DBs), not per return-team slot.
    /// Found with one simmed season of the stock ROM, checked against the game's screens and by how the
    /// numbers add up across the league (TSBTool_Test\SimTests\TSB2_study\TSB2_SRAM_Stats.txt).
    /// The game doesn't credit about 1 in 20 interceptions to a defender (TSB2_Sim_Engine.txt, bug G1),
    /// so INTs thrown and INTs made don't balance; nor, by a few yards, do rushing yards (bug G3).
    /// </summary>
    public class Tsb2SeasonStats : SeasonStats
    {
        private const int SramSize = 0x8000;
        private const int SeasonByte = 0xD6;
        private const int FirstTeamRecord = 0x3A2;
        private const int TeamRecordSize = 163;
        private const int FirstTeam = 0x152A;
        private const int TeamSize = 485;

        // Team names in the game's order. Not TSB2Tool.teams: TSB3Tool replaces that list.
        private static readonly string[] Tsb2Teams = {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cowboys", "giants", "eagles",   "cardinals", "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "rams",   "saints",   "49ers"
        };

        private static readonly string[] Tsb2Defenders = {
            "RE", "NT", "LE", "RE2", "NT2", "LE2", "ROLB", "RILB", "LILB", "LOLB", "LB5",
            "RCB", "LCB", "DB1", "DB2", "FS", "SS", "DB3"
        };
        private const int FirstDefensiveBack = 11; // RCB: the DBs also have a return block

        private static readonly string[] Years = { "1992", "1993", "1994" };

        private byte[] mSram;

        private Tsb2SeasonStats(byte[] sram)
        {
            mSram = sram;
        }

        /// <summary>True when 'data' is the size of the SNES TSB2 battery save.</summary>
        public static bool IsSramSize(byte[] data)
        {
            return data != null && data.Length == SramSize;
        }

        /// <summary>
        /// Reads the stats from the bytes of a SNES TSB2 .srm file. Returns null (with the reason in
        /// 'error') when it isn't one or there are no season stats in it.
        /// </summary>
        public static Tsb2SeasonStats FromSram(byte[] sram, out string error)
        {
            error = null;
            if (!IsSramSize(sram))
            {
                error = "This isn't a SNES TSB2 battery save (.srm, 32 KB).";
                return null;
            }
            Tsb2SeasonStats ret = new Tsb2SeasonStats(sram);
            for (int k = 0; k < TeamCount; k++)
                ret.Teams.Add(ret.ReadTeam(k));

            if (ret.Season > Years.Length || !ret.LooksLikeSeasonStats())
            {
                error = "This battery save has no SNES TSB2 season stats in it. Play or simulate some games of a season first (the emulator writes the .srm when the game is closed).";
                return null;
            }
            return ret;
        }

        /// <summary>The season that was played: 1-3 (1992/1993/1994), as TSBTool numbers TSB2's seasons.</summary>
        public int Season
        {
            get { return (mSram[SeasonByte] & 3) + 1; }
        }

        protected override string Title { get { return "SNES TSB2 SEASON STATS (" + Years[Season - 1] + ")"; } }

        public override string RomKind { get { return "SNES TSB2"; } }

        // A run can lose its credit (game bug G3: the carrier pick lands on the QB who isn't playing),
        // so the players' rushing yards can come up short of the rushing yards allowed.
        protected override bool RushYardsBalance { get { return false; } }

        public override bool IsRomForTheseStats(ITecmoContent rom)
        {
            return rom is TSBTool2.TSB2Tool && !(rom is TSBTool2.TSB3Tool);
        }

        // Each season has its own roster, so the names come from the season that was played.
        protected override string GetRomPlayerName(ITecmoContent rom, string team, string position)
        {
            if (!IsRomForTheseStats(rom))
                return null;
            byte jerseyNumber;
            return ((TSBTool2.TSB2Tool)rom).GetPlayerName(Season, team, position, out jerseyNumber);
        }

        // Returns are per player (no return-team slots).
        protected override string GetRomReturnTeam(ITecmoContent rom, string team)
        {
            return null;
        }

        private int Byte(int offset)
        {
            return mSram[offset];
        }

        private int Word(int offset)
        {
            return mSram[offset] | (mSram[offset + 1] << 8);
        }

        private int SignedWord(int offset)
        {
            int ret = Word(offset);
            return ret >= 0x8000 ? ret - 0x10000 : ret;
        }

        private PlayerSeasonStats ReadReturns(string position, int a)
        {
            PlayerSeasonStats r = new PlayerSeasonStats();
            r.Position = position;
            r.KickReturnTDs = Byte(a);
            r.KickReturns = Byte(a + 1);
            r.KickReturnYards = SignedWord(a + 2);
            r.PuntReturnTDs = Byte(a + 4);
            r.PuntReturns = Byte(a + 5);
            r.PuntReturnYards = SignedWord(a + 6);
            return r;
        }

        private TeamSeasonStats ReadTeam(int teamIndex)
        {
            TeamSeasonStats t = new TeamSeasonStats();
            t.Team = Tsb2Teams[teamIndex];

            int q = FirstTeam + teamIndex * TeamSize;
            for (int i = 0; i < QBs.Length; i++)
            {
                int a = q + i * 13;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = QBs[i];
                p.PassTDs = Byte(a);
                p.PassAttempts = Word(a + 1);
                p.InterceptionsThrown = Byte(a + 3);
                p.Completions = Word(a + 4);
                p.PassYards = SignedWord(a + 6);
                p.RushTDs = Byte(a + 8);
                p.RushAttempts = Word(a + 9);
                p.RushYards = SignedWord(a + 11);
                t.Players.Add(p);
            }
            for (int i = 0; i < SkillPlayers.Length; i++)
            {
                int a = q + 26 + i * 22;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = SkillPlayers[i];
                p.ReceivingTDs = Byte(a);
                p.Receptions = Word(a + 1);
                p.ReceivingYards = SignedWord(a + 3);
                p.RushTDs = Byte(a + 5);
                p.RushAttempts = Word(a + 6);
                p.RushYards = SignedWord(a + 8);
                t.Players.Add(p);
                t.Returners.Add(ReadReturns(p.Position, a + 12));
            }
            int d = q + 246;
            for (int i = 0; i < Tsb2Defenders.Length; i++)
            {
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = Tsb2Defenders[i];
                p.Sacks = Byte(d);
                p.Interceptions = Byte(d + 2);
                p.InterceptionTDs = Byte(d + 3);
                p.InterceptionYards = SignedWord(d + 4);
                t.Players.Add(p);
                if (i >= FirstDefensiveBack)
                {
                    t.Returners.Add(ReadReturns(p.Position, d + 9));
                    d += 19;
                }
                else
                    d += 9;
            }
            PlayerSeasonStats k = new PlayerSeasonStats();
            k.Position = "K";
            k.ExtraPointsMade = Byte(q + 478);
            k.ExtraPointAttempts = Byte(q + 479);
            k.FieldGoalsMade = Byte(q + 480);
            k.FieldGoalAttempts = Byte(q + 481);
            t.Players.Add(k);

            PlayerSeasonStats punter = new PlayerSeasonStats();
            punter.Position = "P";
            punter.Punts = Byte(q + 482);
            punter.PuntYards = Word(q + 483);
            t.Players.Add(punter);

            int r = FirstTeamRecord + teamIndex * TeamRecordSize;
            t.Wins = Byte(r);
            t.Losses = Byte(r + 1);
            t.Ties = Byte(r + 2);
            t.PointsFor = Word(r + 3);
            t.PointsAgainst = Word(r + 5);
            t.PassYardsAllowed = Word(r + 7);
            t.RushYardsAllowed = Word(r + 9);
            return t;
        }
    }
}
