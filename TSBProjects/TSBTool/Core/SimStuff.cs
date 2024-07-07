using System;

namespace TSBTool
{
	/// <summary>
	/// Summary description for SimStuff.
	/// </summary>
	public class SimStuff
	{
		public const int FRONT_7_SIM_POINT_POOL    = 200;
		public const int FRONT_7_MIN_SIM_PASS_RUSH = 13;

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

		public int SimKickRet(int MS )
		{
			int ret = MS / 4;
			if( ret > 15)
				ret = 15;
			return ret;
		}

		public int SimPuntRet(int MS )
		{
			int ret = MS / 4;
			if( ret > 15)
				ret = 15;
			return ret;
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


		/// <summary>
		/// Use PI 
		/// </summary>
		/// <param name="rolbInts"></param>
		/// <param name="rilbInts"></param>
		/// <param name="lilbInts"></param>
		/// <param name="lolbInts"></param>
		/// <param name="rcbInts"></param>
		/// <param name="lcbInts"></param>
		/// <param name="fsInts"></param>
		/// <param name="ssInts"></param>
		/// <returns></returns>
		public int[] GetSimPassDefense(
			int rolbInts, int rilbInts, int lilbInts, int lolbInts,
			int rcbInts,  int lcbInts,  int fsInts,   int ssInts   )
		{
			double totalInts = rolbInts + rilbInts + lilbInts + lolbInts + rcbInts + lcbInts + fsInts +ssInts;
			double totalSimPoints = 254; 
			int rolbPoints, rilbPoints, lilbPoints, lolbPoints, rcbPoints, lcbPoints, fsPoints, ssPoints;

			rolbPoints   = (int)((rolbInts / totalInts )* totalSimPoints);
			rilbPoints   = (int)((rilbInts / totalInts )* totalSimPoints);
			//lilbPoints = (int)((lilbInts / totalInts )* totalSimPoints);
			lolbPoints   = (int)((lolbInts / totalInts )* totalSimPoints);
			rcbPoints    = (int)((rcbInts  / totalInts )* totalSimPoints);
			lcbPoints    = (int)((lcbInts  / totalInts )* totalSimPoints);
			fsPoints     = (int)((fsInts   / totalInts )* totalSimPoints);
			ssPoints     = (int)((ssInts   / totalInts )* totalSimPoints);

			lilbPoints = 1 + (int)(totalSimPoints 
				- 
				(rcbPoints + lcbPoints + fsPoints + rolbPoints + ssPoints +
				rilbPoints +  lolbPoints));

			int [] ret = new int[8];
			ret[0] = rolbPoints;
			ret[1] = rilbPoints;
			ret[2] = lilbPoints;
			ret[3] = lolbPoints;
			ret[4] = rcbPoints;
			ret[5] = lcbPoints;
			ret[6] = fsPoints;
			ret[7] = ssPoints;
			
			return ret;
		}


		/// <summary>
		/// use HP instead of sacks
		/// </summary>
		/// <param name="reSacks"></param>
		/// <param name="ntSacks"></param>
		/// <param name="leSacks"></param>
		/// <param name="rolbSacks"></param>
		/// <param name="rilbSacks"></param>
		/// <param name="lilbSacks"></param>
		/// <param name="lolbSacks"></param>
		/// <param name="playerData"></param>
		/// <returns></returns>
 
		public int[] GetSimPassRush( 
			double reSacks,   double ntSacks,   double leSacks,  double rolbSacks,
			double rilbSacks, double lilbSacks, double lolbSacks )
		{
			double totalSacks = reSacks + ntSacks + leSacks + rolbSacks + rilbSacks + lilbSacks + lolbSacks;
			
			int totalSimPoints = FRONT_7_SIM_POINT_POOL;
			int minPr          = FRONT_7_MIN_SIM_PASS_RUSH;

			int rePoints, ntPoints, lePoints, rolbPoints, rilbPoints, lilbPoints, lolbPoints, ssPoints;
			int dbPoints = 0;
			int cbPoints = 0;
			int front7Points = 0;

			if( totalSacks == 0 )
			{
				rePoints= ntPoints= lePoints= rolbPoints= rilbPoints= lilbPoints= lolbPoints= ssPoints=31;
				rePoints += 4;
			}
			else
			{
				rePoints   = Math.Max( (int)((reSacks   / totalSacks )* totalSimPoints), minPr);
				lePoints   = Math.Max( (int)((leSacks   / totalSacks )* totalSimPoints), minPr);
				ntPoints   = Math.Max( (int)((ntSacks   / totalSacks )* totalSimPoints), minPr);
				rolbPoints = Math.Max( (int)((rolbSacks / totalSacks )* totalSimPoints), minPr);
				rilbPoints = Math.Max( (int)((rilbSacks / totalSacks )* totalSimPoints), minPr);
				lilbPoints = Math.Max( (int)((lilbSacks / totalSacks )* totalSimPoints), minPr);
				lolbPoints = Math.Max( (int)((lolbSacks / totalSacks )* totalSimPoints), minPr);

				front7Points = rePoints + lePoints + ntPoints + rolbPoints + 
					rilbPoints + lilbPoints + lolbPoints;

				dbPoints = (int)(255 - front7Points);
				
				cbPoints = dbPoints / 4 ;
				ssPoints = (int) (255 - ((3*cbPoints) + front7Points)  );
			}
			int[] ret = new int[8];
			
			ret[0] = rePoints;
			ret[1] = ntPoints;
			ret[2] = lePoints;
			ret[3] = rolbPoints;
			ret[4] = rilbPoints;
			ret[5] = lilbPoints;
			ret[6] = lolbPoints;
			ret[7] = ssPoints;

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
