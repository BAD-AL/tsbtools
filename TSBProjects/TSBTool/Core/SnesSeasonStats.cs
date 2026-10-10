using System;

namespace TSBTool
{
    /// <summary>
    /// Reads the season stats of SNES TSB1 out of the game's battery save (.srm, 8 KB, as Mesen2 / snes9x
    /// write it). Same layout as Genesis (GenesisSeasonStats), one byte for each of Genesis' RAM words,
    /// 2-byte values little-endian, and a few byte pairs in the other order:
    ///  - Team k (SNES roster order, SNES_TecmoTool.GetTeamFromIndex) starts at 0x14E + k*0xE8:
    ///      +0x00  QB1, QB2 (10 bytes): att, pass TD*2 + att bit 8, comp, INT*4 + comp bits 8-9,
    ///             pass yds (2), rush att, rush TD, rush yds (2, signed)
    ///      +0x14  RB1-4, WR1-4, TE1-2 (8 bytes): rec, rec TD, rec yds (2), rush att, rush TD*2 + rush
    ///             att bit 8, rush yds (2, signed)
    ///      +0x64  RE..SS, DB1, DB2 (5 bytes): INTs, sacks, INT TDs, INT return yds (2)
    ///      +0xA5  K: XP att, XP made, FG att, FG made.   +0xA9  P: punts, punt yds (2)
    ///      +0xAC  the 3 return-team players (8 bytes, in the ROM's RETURN_TEAM order): KR, KR TD,
    ///             KR yds (2), PR, PR TD, PR yds (2)
    ///      +0xC4  W, L, T, PF (2), PA (2), pass yds allowed (2), rush yds allowed (2)
    /// Found with the SimTests SNES_05/SNES_06 seasons, checked against their result sheets and by how
    /// the numbers add up across the league (see TSBTool_Test\SimTests\stats_research\SNES).
    /// </summary>
    public class SnesSeasonStats : SeasonStats
    {
        private const int SramSize = 0x2000;
        private const int FirstTeam = 0x14E;
        private const int TeamSize = 0xE8;

        private byte[] mSram;

        private SnesSeasonStats(byte[] sram)
        {
            mSram = sram;
        }

        /// <summary>True when 'data' is the size of the SNES TSB battery save.</summary>
        public static bool IsSramSize(byte[] data)
        {
            return data != null && data.Length == SramSize;
        }

        /// <summary>
        /// Reads the stats from the bytes of a SNES TSB .srm file. Returns null (with the reason in
        /// 'error') when it isn't one or there are no season stats in it.
        /// </summary>
        public static SnesSeasonStats FromSram(byte[] sram, out string error)
        {
            error = null;
            if (!IsSramSize(sram))
            {
                error = "This isn't a SNES TSB battery save (.srm, 8 KB).";
                return null;
            }
            SnesSeasonStats ret = new SnesSeasonStats(sram);
            for (int k = 0; k < TeamCount; k++)
                ret.Teams.Add(ret.ReadTeam(k));

            if (!ret.LooksLikeSeasonStats())
            {
                error = "This battery save has no SNES TSB season stats in it. Play or simulate some games of a season first (the emulator writes the .srm when the game is closed).";
                return null;
            }
            return ret;
        }

        protected override string Title { get { return "SNES TSB SEASON STATS"; } }

        public override string RomKind { get { return "SNES"; } }

        public override bool IsRomForTheseStats(ITecmoContent rom)
        {
            return rom is SNES_TecmoTool;
        }

        protected override string GetRomPlayerName(ITecmoContent rom, string team, string position)
        {
            SNES_TecmoTool snes = rom as SNES_TecmoTool;
            return snes == null ? null : snes.GetName(team, position);
        }

        protected override string GetRomReturnTeam(ITecmoContent rom, string team)
        {
            SNES_TecmoTool snes = rom as SNES_TecmoTool;
            return snes == null ? null : snes.GetReturnTeam(team);
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

        private TeamSeasonStats ReadTeam(int teamIndex)
        {
            int q = FirstTeam + teamIndex * TeamSize;
            TeamSeasonStats t = new TeamSeasonStats();
            t.Team = SNES_TecmoTool.GetTeamFromIndex(teamIndex);

            for (int i = 0; i < QBs.Length; i++)
            {
                int a = q + i * 10;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = QBs[i];
                p.PassAttempts = ((Byte(a + 1) & 1) << 8) | Byte(a);
                p.PassTDs = Byte(a + 1) >> 1;
                p.Completions = ((Byte(a + 3) & 3) << 8) | Byte(a + 2);
                p.InterceptionsThrown = Byte(a + 3) >> 2;
                p.PassYards = SignedWord(a + 4);
                p.RushAttempts = Byte(a + 6);
                p.RushTDs = Byte(a + 7);
                p.RushYards = SignedWord(a + 8);
                t.Players.Add(p);
            }
            for (int i = 0; i < SkillPlayers.Length; i++)
            {
                int a = q + 0x14 + i * 8;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = SkillPlayers[i];
                p.Receptions = Byte(a);
                p.ReceivingTDs = Byte(a + 1);
                p.ReceivingYards = SignedWord(a + 2);
                p.RushAttempts = ((Byte(a + 5) & 1) << 8) | Byte(a + 4);
                p.RushTDs = Byte(a + 5) >> 1;
                p.RushYards = SignedWord(a + 6);
                t.Players.Add(p);
            }
            for (int i = 0; i < Defenders.Length; i++)
            {
                int a = q + 0x64 + i * 5;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = Defenders[i];
                p.Interceptions = Byte(a);
                p.Sacks = Byte(a + 1);
                p.InterceptionTDs = Byte(a + 2);
                p.InterceptionYards = SignedWord(a + 3);
                t.Players.Add(p);
            }
            PlayerSeasonStats k = new PlayerSeasonStats();
            k.Position = "K";
            k.ExtraPointAttempts = Byte(q + 0xA5);
            k.ExtraPointsMade = Byte(q + 0xA6);
            k.FieldGoalAttempts = Byte(q + 0xA7);
            k.FieldGoalsMade = Byte(q + 0xA8);
            t.Players.Add(k);

            PlayerSeasonStats punter = new PlayerSeasonStats();
            punter.Position = "P";
            punter.Punts = Byte(q + 0xA9);
            punter.PuntYards = Word(q + 0xAA);
            t.Players.Add(punter);

            for (int i = 0; i < 3; i++)
            {
                int a = q + 0xAC + i * 8;
                PlayerSeasonStats p = new PlayerSeasonStats();
                p.Position = "RET" + (i + 1);
                p.KickReturns = Byte(a);
                p.KickReturnTDs = Byte(a + 1);
                p.KickReturnYards = SignedWord(a + 2);
                p.PuntReturns = Byte(a + 4);
                p.PuntReturnTDs = Byte(a + 5);
                p.PuntReturnYards = SignedWord(a + 6);
                t.Returners.Add(p);
            }

            int r = q + 0xC4;
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
