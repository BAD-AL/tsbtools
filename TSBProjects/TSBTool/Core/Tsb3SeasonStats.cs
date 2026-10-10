using System;
using System.Collections.Generic;

namespace TSBTool
{
    /// <summary>
    /// Reads the season stats of SNES TSB3 out of the game's battery save (.srm, 32 KB, as Mesen2 / snes9x
    /// write it). 2-byte values are little-endian; yards are signed.
    ///  - Team chunk, team k (TSB3 roster order) at 0x457 + k*101:
    ///      +0    the roster: 37 player IDs (2 bytes), QB1 .. P. ID &amp; 0x0FFF = original team * 37 + slot
    ///            (original team 30 = the free agents); trades and signings change who's where.
    ///      +76   W, L, T, PF (2), PA (2), pass yds allowed (2), rush yds allowed (2)
    ///  - Player stats, by ORIGINAL team t (0-29, 30 = free agents) at 0x11C1 + t*485, at the player's
    ///    original slot: the same layout as TSB2 (Tsb2SeasonStats). A player's stats are found through the
    ///    roster ID of the slot he plays now.
    /// Found with one simmed season of the stock ROM, checked against the game's screens and by how the
    /// numbers add up for every team (TSBTool_Test\SimTests\TSB3_study\TSB3_SRAM_Stats.txt). Rushing yards
    /// can be a yard or two apart from the yards allowed (the game's own team totals are too), so they
    /// aren't required to balance.
    /// </summary>
    public class Tsb3SeasonStats : SeasonStats
    {
        private const int SramSize = 0x8000;
        private const int FirstTeamChunk = 0x457;
        private const int TeamChunkSize = 101;
        private const int RecordOffset = 76;
        private const int FirstStatsBlock = 0x11C1;
        private const int StatsBlockSize = 485;
        private const int PlayersPerTeam = 37;
        private const int Tsb3TeamCount = 30;
        private const int FreeAgentTeam = 30;

        // Team names in the game's order. Not TSB2Tool.teams: TSB3Tool replaces that list.
        private static readonly string[] Tsb3Teams = {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "jaguars",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cardinals", "cowboys", "giants", "eagles",   "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "panthers", "saints", "rams",      "49ers",
            "freeAgents"
        };

        // TSB2/TSB3 roster slots, in the game's order
        private static readonly string[] Positions = {
            "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2",
            "C", "LG", "RG", "LT", "RT",
            "RE", "NT", "LE", "RE2", "NT2", "LE2", "ROLB", "RILB", "LILB", "LOLB", "LB5",
            "RCB", "LCB", "DB1", "DB2", "FS", "SS", "DB3", "K", "P"
        };
        private const int FirstSkill = 2, FirstLineman = 12, FirstDefender = 17, FirstDefensiveBack = 28, Kicker = 35, Punter = 36;

        private byte[] mSram;

        // "team/position" -> the original team and slot of the player there now (for his name)
        private Dictionary<string, KeyValuePair<string, string>> mOriginal = new Dictionary<string, KeyValuePair<string, string>>();

        private Tsb3SeasonStats(byte[] sram)
        {
            mSram = sram;
        }

        /// <summary>True when 'data' is the size of the SNES TSB3 battery save.</summary>
        public static bool IsSramSize(byte[] data)
        {
            return data != null && data.Length == SramSize;
        }

        /// <summary>
        /// Reads the stats from the bytes of a SNES TSB3 .srm file. Returns null (with the reason in
        /// 'error') when it isn't one or there are no season stats in it.
        /// </summary>
        public static Tsb3SeasonStats FromSram(byte[] sram, out string error)
        {
            error = null;
            if (!IsSramSize(sram))
            {
                error = "This isn't a SNES TSB3 battery save (.srm, 32 KB).";
                return null;
            }
            Tsb3SeasonStats ret = new Tsb3SeasonStats(sram);
            for (int k = 0; k < Tsb3TeamCount; k++)
            {
                TeamSeasonStats t = ret.ReadTeam(k);
                if (t == null)
                {
                    error = "This battery save has no SNES TSB3 season stats in it.";
                    return null;
                }
                ret.Teams.Add(t);
            }
            if (!ret.LooksLikeSeasonStats())
            {
                error = "This battery save has no SNES TSB3 season stats in it. Play or simulate some games of a season first (the emulator writes the .srm when the game is closed).";
                return null;
            }
            return ret;
        }

        protected override string Title { get { return "SNES TSB3 SEASON STATS"; } }

        public override string RomKind { get { return "SNES TSB3"; } }

        protected override bool RushYardsBalance { get { return false; } }

        public override bool IsRomForTheseStats(ITecmoContent rom)
        {
            return rom is TSBTool2.TSB3Tool;
        }

        /// <summary>
        /// The name ("dana HALL") of the player at 'team' / 'position' in this season's rosters, from 'rom';
        /// null when 'rom' isn't a TSB3 ROM.
        /// </summary>
        public string GetPlayerName(ITecmoContent rom, string team, string position)
        {
            return GetRomPlayerName(rom, team, position);
        }

        // The player may have come from another team (or the free agents): his name is at his original slot.
        protected override string GetRomPlayerName(ITecmoContent rom, string team, string position)
        {
            if (!IsRomForTheseStats(rom))
                return null;
            KeyValuePair<string, string> original;
            if (!mOriginal.TryGetValue(team + "/" + position, out original))
                original = new KeyValuePair<string, string>(team, position);
            byte jerseyNumber;
            return ((TSBTool2.TSB3Tool)rom).GetPlayerName(1, original.Key, original.Value, out jerseyNumber);
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

        /// <summary>Where the stats of the player in 'slot' of an original team's block start.</summary>
        private static int StatsOffset(int team, int slot)
        {
            int q = FirstStatsBlock + team * StatsBlockSize;
            if (slot < FirstSkill)
                return q + slot * 13;
            if (slot < FirstLineman)
                return q + 26 + (slot - FirstSkill) * 22;
            q += 246;
            for (int i = FirstDefender; i < slot && i < Kicker; i++)
                q += i < FirstDefensiveBack ? 9 : 19;
            return slot == Punter ? q + 4 : q;
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

        /// <summary>Team k from its roster in the save, or null when the roster isn't TSB3's.</summary>
        private TeamSeasonStats ReadTeam(int teamIndex)
        {
            TeamSeasonStats t = new TeamSeasonStats();
            t.Team = Tsb3Teams[teamIndex];
            int chunk = FirstTeamChunk + teamIndex * TeamChunkSize;
            for (int slot = 0; slot < Positions.Length; slot++)
            {
                int id = Word(chunk + slot * 2) & 0x0FFF;
                int team = id / PlayersPerTeam, original = id % PlayersPerTeam;
                if (team > FreeAgentTeam)
                    return null; // not a TSB3 roster (a TSB2 save has other data here)
                string pos = Positions[slot];
                if (team != teamIndex || original != slot)
                    mOriginal[t.Team + "/" + pos] = new KeyValuePair<string, string>(Tsb3Teams[team], Positions[original]);
                ReadPlayer(t, pos, StatsOffset(team, original), original);
            }
            int r = chunk + RecordOffset;
            t.Wins = Byte(r);
            t.Losses = Byte(r + 1);
            t.Ties = Byte(r + 2);
            t.PointsFor = Word(r + 3);
            t.PointsAgainst = Word(r + 5);
            t.PassYardsAllowed = Word(r + 7);
            t.RushYardsAllowed = Word(r + 9);
            return t;
        }

        /// <summary>
        /// The stats of the player now at 'pos', from 'a' (his original slot's record; its kind decides
        /// the layout).
        /// </summary>
        private void ReadPlayer(TeamSeasonStats t, string pos, int a, int originalSlot)
        {
            int slot = originalSlot;
            PlayerSeasonStats p = new PlayerSeasonStats();
            p.Position = pos;
            if (slot < FirstSkill)
            {
                p.PassTDs = Byte(a);
                p.PassAttempts = Word(a + 1);
                p.InterceptionsThrown = Byte(a + 3);
                p.Completions = Word(a + 4);
                p.PassYards = SignedWord(a + 6);
                p.RushTDs = Byte(a + 8);
                p.RushAttempts = Word(a + 9);
                p.RushYards = SignedWord(a + 11);
            }
            else if (slot < FirstLineman)
            {
                p.ReceivingTDs = Byte(a);
                p.Receptions = Word(a + 1);
                p.ReceivingYards = SignedWord(a + 3);
                p.RushTDs = Byte(a + 5);
                p.RushAttempts = Word(a + 6);
                p.RushYards = SignedWord(a + 8);
                t.Returners.Add(ReadReturns(pos, a + 12));
            }
            else if (slot < FirstDefender)
            {
                return; // OL: no stats
            }
            else if (slot < Kicker)
            {
                p.Sacks = Byte(a);
                p.Interceptions = Byte(a + 2);
                p.InterceptionTDs = Byte(a + 3);
                p.InterceptionYards = SignedWord(a + 4);
                if (slot >= FirstDefensiveBack)
                    t.Returners.Add(ReadReturns(pos, a + 9));
            }
            else if (slot == Kicker)
            {
                p.ExtraPointsMade = Byte(a);
                p.ExtraPointAttempts = Byte(a + 1);
                p.FieldGoalsMade = Byte(a + 2);
                p.FieldGoalAttempts = Byte(a + 3);
            }
            else
            {
                p.Punts = Byte(a);
                p.PuntYards = Word(a + 1);
            }
            t.Players.Add(p);
        }
    }
}
