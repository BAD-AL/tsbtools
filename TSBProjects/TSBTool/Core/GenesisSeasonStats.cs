using System;
using System.Text;

namespace TSBTool
{
    /// <summary>
    /// Reads the season stats of Genesis TSB out of an ares emulator save state (.bs1), saved at the
    /// 'Season Menu' screen. The game's battery save isn't usable (ares' .ram file stays blank), but the
    /// state holds the 68K RAM, and at the Season Menu the season stats are in it:
    ///  - RAM is at file offset 0x22AF, as 16-bit words with the bytes swapped; every stat value is one
    ///    byte per word (the low byte), 2-word values are big-endian.
    ///  - Team k (Genesis roster order, Genesis_TSB1Tool.GetTeamFromIndex) starts at RAM 0x726A + k*0x1D0:
    ///      +0x000  QB1, QB2 (10 words): pass TD*4 + att bits 8-9, att, INT*4 + comp bits 8-9, comp,
    ///              pass yds (2), rush TD, rush att, rush yds (2, signed)
    ///      +0x028  RB1-4, WR1-4, TE1-2 (8 words): rec TD, rec, rec yds (2), rush TD*2 + rush att bit 8,
    ///              rush att, rush yds (2, signed)
    ///      +0x0C8  RE..SS, DB1, DB2 (5 words): sacks, INTs, INT TDs, INT return yds (2)
    ///      +0x14A  K: XP made, XP att, FG made, FG att.   +0x152  P: punts, punt yds (2)
    ///      +0x158  the 3 return-team players (8 words, in the ROM's RETURN_TEAM order): KR TD, KR,
    ///              KR yds (2), PR TD, PR, PR yds (2)
    ///      +0x188  W, L, T, PF (2), PA (2), pass yds allowed (2), rush yds allowed (2)
    /// Found with the SimTests GEN_05b/GEN_06b seasons, checked against the game's stat screens.
    /// </summary>
    public class GenesisSeasonStats : SeasonStats
    {
        private const string AresMagic = "BST1";
        private const string KnownAresVersion = "v145";
        private const int RamStart = 0x22AF;
        private const int RamLength = 0x10000;
        private const int FirstTeam = 0x726A;
        private const int TeamSize = 0x1D0;

        private byte[] mState;

        private GenesisSeasonStats(byte[] state)
        {
            mState = state;
        }

        /// <summary>
        /// Reads the stats from the bytes of an ares .bs1 file. Returns null (with the reason in 'error')
        /// when it isn't an ares state or the state has no season stats in it.
        /// </summary>
        public static GenesisSeasonStats FromAresState(byte[] state, out string error)
        {
            error = null;
            if (!IsAresState(state))
            {
                error = "This isn't an ares emulator save state (.bs1) of the Genesis game.";
                return null;
            }
            GenesisSeasonStats ret = new GenesisSeasonStats(state);
            for (int k = 0; k < TeamCount; k++)
                ret.Teams.Add(ret.ReadTeam(k));

            if (!ret.LooksLikeSeasonStats())
            {
                string version = GetAresVersion(state);
                error = "This save state has no Genesis TSB season stats in it. Save the state at the 'Season Menu' screen, after some games have been played.";
                if (version != KnownAresVersion)
                    error += string.Format(" (This state is from ares save version '{0}'; only '{1}' is known to work.)", version, KnownAresVersion);
                return null;
            }
            return ret;
        }

        /// <summary>True when 'data' is an ares save state (big enough to hold the 68K RAM).</summary>
        public static bool IsAresState(byte[] data)
        {
            return data != null && data.Length >= RamStart + RamLength && Encoding.ASCII.GetString(data, 0, 4) == AresMagic;
        }

        protected override string Title { get { return "GENESIS TSB SEASON STATS"; } }

        public override string RomKind { get { return "Genesis"; } }

        public override bool IsRomForTheseStats(ITecmoTool rom)
        {
            return rom is Genesis_TSB1Tool;
        }

        protected override string GetRomPlayerName(ITecmoTool rom, string team, string position)
        {
            Genesis_TSB1Tool genesis = rom as Genesis_TSB1Tool;
            return genesis == null ? null : genesis.GetDisplayName(team, position);
        }

        protected override string GetRomReturnTeam(ITecmoTool rom, string team)
        {
            Genesis_TSB1Tool genesis = rom as Genesis_TSB1Tool;
            return genesis == null ? null : genesis.GetReturnTeam(team);
        }

        private static string GetAresVersion(byte[] state)
        {
            int end = 5;
            while (end < 16 && state[end] != 0)
                end++;
            return Encoding.ASCII.GetString(state, 5, end - 5);
        }

        // The stat byte (low byte) of the RAM word at 'ramOffset'.
        private int Byte(int ramOffset)
        {
            return mState[RamStart + ramOffset];
        }

        private int Word(int ramOffset)
        {
            return (Byte(ramOffset) << 8) | Byte(ramOffset + 2);
        }

        private int SignedWord(int ramOffset)
        {
            int ret = Word(ramOffset);
            return ret >= 0x8000 ? ret - 0x10000 : ret;
        }

        private TeamSeasonStats ReadTeam(int teamIndex)
        {
            int q = FirstTeam + teamIndex * TeamSize;
            TeamSeasonStats t = new TeamSeasonStats();
            t.Team = Genesis_TSB1Tool.GetTeamFromIndex(teamIndex);

            for (int i = 0; i < QBs.Length; i++)
            {
                int a = q + i * 20;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = QBs[i];
                p.PassTDs = Byte(a) >> 2;
                p.PassAttempts = ((Byte(a) & 3) << 8) | Byte(a + 2);
                p.InterceptionsThrown = Byte(a + 4) >> 2;
                p.Completions = ((Byte(a + 4) & 3) << 8) | Byte(a + 6);
                p.PassYards = SignedWord(a + 8);
                p.RushTDs = Byte(a + 12);
                p.RushAttempts = Byte(a + 14);
                p.RushYards = SignedWord(a + 16);
                t.Players.Add(p);
            }
            for (int i = 0; i < SkillPlayers.Length; i++)
            {
                int a = q + 0x28 + i * 16;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = SkillPlayers[i];
                p.ReceivingTDs = Byte(a);
                p.Receptions = Byte(a + 2);
                p.ReceivingYards = SignedWord(a + 4);
                p.RushTDs = Byte(a + 8) >> 1;
                p.RushAttempts = ((Byte(a + 8) & 1) << 8) | Byte(a + 10);
                p.RushYards = SignedWord(a + 12);
                t.Players.Add(p);
            }
            for (int i = 0; i < Defenders.Length; i++)
            {
                int a = q + 0xC8 + i * 10;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = Defenders[i];
                p.Sacks = Byte(a);
                p.Interceptions = Byte(a + 2);
                p.InterceptionTDs = Byte(a + 4);
                p.InterceptionYards = SignedWord(a + 6);
                t.Players.Add(p);
            }
            PlayerSeasonStats k = new PlayerSeasonStats();
            k.Position = "K";
            k.ExtraPointsMade = Byte(q + 0x14A);
            k.ExtraPointAttempts = Byte(q + 0x14C);
            k.FieldGoalsMade = Byte(q + 0x14E);
            k.FieldGoalAttempts = Byte(q + 0x150);
            t.Players.Add(k);

            PlayerSeasonStats punter = new PlayerSeasonStats();
            punter.Position = "P";
            punter.Punts = Byte(q + 0x152);
            punter.PuntYards = Word(q + 0x154);
            t.Players.Add(punter);

            for (int i = 0; i < 3; i++)
            {
                int a = q + 0x158 + i * 16;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = "RET" + (i + 1);
                p.KickReturnTDs = Byte(a);
                p.KickReturns = Byte(a + 2);
                p.KickReturnYards = SignedWord(a + 4);
                p.PuntReturnTDs = Byte(a + 8);
                p.PuntReturns = Byte(a + 10);
                p.PuntReturnYards = SignedWord(a + 12);
                t.Returners.Add(p);
            }

            int r = q + 0x188;
            t.Wins = Byte(r);
            t.Losses = Byte(r + 2);
            t.Ties = Byte(r + 4);
            t.PointsFor = Word(r + 6);
            t.PointsAgainst = Word(r + 10);
            t.PassYardsAllowed = Word(r + 14);
            t.RushYardsAllowed = Word(r + 18);
            return t;
        }
    }
}
