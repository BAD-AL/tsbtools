using System;

namespace TSBTool
{
	/// <summary>
	/// Summary description for SimStuff.
	/// </summary>
	public class SimStuff
	{

		public SimStuff()
		{
		}

		/// <summary>
		/// Returns the SimPocket value when passed the QB's
		/// MS.
		/// </summary>
		/// <param name="MS"></param>
		/// <returns></returns>
		public int SimPocket(int MS)
		{
			int ret = 0;
			switch( MS )
			{
				case 100: case 94: case 88: case 81:
				case 75:  case 69: case 63: case 56: 
				case 50:
					ret = 0;
					break;
				case 44: case 38:
					ret = 1;
					break;
				case 31: case 25:
					ret = 2;
					break;
				default:
					ret = 3;
					break;
			}
			return ret;
		}

		public int SimPass( int PC, int APB, int PS )
		{
			int ret = 0;

			if( PC > 75 )
				ret = 13;
			else if( PC > 44 )
				ret = ( PS + PC + APB ) / 17;
			else
				ret = (PC + APB ) / 14;
			if( ret > 15)
				ret = 15;
			return ret;
		}

		public int QbSimRun( int MS )
		{
			int ret = MS / 5;
			if( ret > 15)
				ret = 15;
			return ret;
		}

		/// <summary>
		/// Sim yards per catch (a skill player's 3rd sim value): position base (WR 4, TE 4, RB 3)
		/// + (MS + REC) / 25. Matches the stock ROM's averages per position (see TSB_Simulation.md).
		/// </summary>
		public int SimYPC(string position, int MS, int REC)
		{
			int positionBase = position.StartsWith("RB") ? 3 : 4;
			int ret = (int)Math.Round(positionBase + (MS + REC) / 25.0, MidpointRounding.AwayFromZero);
			if( ret > 15)
				ret = 15;
			return ret;
		}

		/// <summary>
		/// Sim targets (a skill player's 4th sim value): how often the player is thrown to, by depth
		/// slot. Backups get 1, not 0 -- 0 means never thrown to.
		/// </summary>
		public static int SimTargets(string position)
		{
			switch( position )
			{
				case "WR1": return 9;
				case "WR2": return 8;
				case "TE1": return 5;
				case "RB1": case "RB2": return 4;
				case "WR3": case "WR4": return 2;
				case "TE2": case "RB3": case "RB4": return 1;
				default: return 0;
			}
		}

		public int RbSimCatch(int REC )
		{
			int ret = 0;
			if( REC  > 44 )
				ret = REC / 6;
			else 
				ret = REC / 10;

			if( ret > 15)
				ret = 15;
			return ret;
		}

		public int RbSimRush( int MS, int HP, int BC, int RS)
		{
			int ret = 0;
			if( HP  < 50 )
				ret = ( ( MS + BC ) / 11) - 2;
			else
				ret = (RS + HP) / 15;
			if( ret > 15)
				ret = 15;
			return ret;
		}

		public int WrTeSimCatch(int REC )
		{
			int ret = REC /6;
			if( ret > 15)
				ret = 15;
			return ret;
		}

		public int WrTeSimRush( )
		{
			return 2;
		}

		public int PKSimKick(int KA, int AKB )
		{
			int ret = ( KA +(AKB/2)) / 11;
			if( ret > 15)
				ret = 15;
			return ret;
		}


		// Who gets credit for a sack or an INT (TSB_Simulation.md, Step 5): the game rolls 0-255 and
		// subtracts the shares of RE, NT, LE, ROLB, RILB, LILB, LOLB, RCB, LCB, FS in turn; whatever is left
		// goes to the SS. So the first 10 shares should add up to nearly 256, or the SS gets the rest (the
		// old pool of 200 gave him ~23% of all sacks). Weights fitted to Tecmo's stock SNES/NES shares.
		private const int PassRushPool = 246; // front 7 (stock teams: 230-255)
		private const int SmallShare   = 2;   // CB/FS sacks and DL INTs, as in the stock ROMs
		private static readonly double[] PassRushPositionWeights = { 1.0, 0.9, 1.0, 0.9, 0.3, 0.3, 0.9 };     // RE NT LE ROLB RILB LILB LOLB
		private static readonly double[] CoveragePositionWeights = { 0.5, 0.5, 0.5, 0.5, 0.8, 0.8, 1.0, 0.8 }; // ROLB RILB LILB LOLB RCB LCB FS SS

		/// <summary>
		/// Sim pass rush (sack share) for RE, NT, LE, ROLB, RILB, LILB, LOLB, RCB, LCB, FS, SS, from each
		/// front-7 player's MS + HP: position weight * (MS + HP)^3 shares 246, the CBs and FS get 2, and the
		/// SS value is what's left of 256 (his real share).
		/// </summary>
		public static int[] GetSimPassRush(int re, int nt, int le, int rolb, int rilb, int lilb, int lolb)
		{
			int[] front7 = Share(new int[] { re, nt, le, rolb, rilb, lilb, lolb }, PassRushPositionWeights, 3, PassRushPool);
			int[] ret = new int[11];
			int sum = 0;
			for (int i = 0; i < 7; i++)
			{
				ret[i] = front7[i];
				sum += front7[i];
			}
			for (int i = 7; i < 10; i++)
			{
				ret[i] = SmallShare;
				sum += SmallShare;
			}
			ret[10] = Math.Max(0, 256 - sum);
			return ret;
		}

		/// <summary>
		/// Sim coverage (INT share) for RE, NT, LE, ROLB, RILB, LILB, LOLB, RCB, LCB, FS, SS, from the PI of
		/// the 4 LBs and 4 DBs: the DL get 2, position weight * PI^2 shares the rest, and the SS value is
		/// what's left of 256 (his real share).
		/// </summary>
		public static int[] GetSimPassDefense(int rolb, int rilb, int lilb, int lolb, int rcb, int lcb, int fs, int ss)
		{
			int[] shares = Share(new int[] { rolb, rilb, lilb, lolb, rcb, lcb, fs, ss }, CoveragePositionWeights, 2, 256 - 3 * SmallShare);
			int[] ret = new int[11];
			int sum = 0;
			for (int i = 0; i < 3; i++)
			{
				ret[i] = SmallShare;
				sum += SmallShare;
			}
			for (int i = 0; i < 7; i++)
			{
				ret[3 + i] = shares[i];
				sum += shares[i];
			}
			ret[10] = Math.Max(0, 256 - sum);
			return ret;
		}

		/// <summary>pool * weight * rating^power / (sum over the group), rounded down, at least 1.</summary>
		private static int[] Share(int[] ratings, double[] positionWeights, int power, int pool)
		{
			double[] weights = new double[ratings.Length];
			double total = 0;
			for (int i = 0; i < ratings.Length; i++)
			{
				weights[i] = positionWeights[i] * Math.Pow(Math.Max(ratings[i], 1), power);
				total += weights[i];
			}
			int[] ret = new int[ratings.Length];
			for (int i = 0; i < ratings.Length; i++)
				ret[i] = Math.Max(1, (int)(pool * weights[i] / total));
			return ret;
		}

		public int GetSimOffense(int QB1SimPass,
			int RB1SimRush,  int RB2SimRush, 
			int WR1SimCatch, int WR2SimCatch )
		{
			int f1, f2;
			if( RB1SimRush > RB2SimRush )
				f1 = RB1SimRush;
			else
				f1 = RB2SimRush;
			if( WR1SimCatch > WR2SimCatch )
				f2 = WR1SimCatch;
			else
				f2 = WR2SimCatch;

			int ret = (QB1SimPass+ f1 + f2) / 3;
			return ret;
		}


	}
}
