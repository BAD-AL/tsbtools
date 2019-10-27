 using System;
 namespace UserFunctions
 {
     public class SimFunctions
     {
        /// <summary>
        /// Should calculate the team's sim defense based on the defensive player attributes.
        /// </summary>
        /// <returns>a 2-digit hex string like "2f"; should be a representation of 
        ///  the team's ability to play defense (sim)</returns>
         public static string GetTeamSimDefense(int[] re, int[] nt, int[] le,  int[] lolb,  
                int[] lilb, int[] rilb, int[] rolb, int[] rcb, int[] lcb, int[] fs, int[] ss)
         {
            // defensive attribute indexes TSB1
            int rs = 0; int rp = 1; int ms = 2; int hp = 3; int pi = 4; int qu = 5;
            int rush_def = 0; int pass_def = 0; // ****** set these two below ******
            // \/ \/ \/ \/  Calculation code goes below here \/ \/ \/ \/ 
            int team_hp = 0; int team_pi = 0; int team_ms = 0;
            
            if (re[ms] > 49)   team_ms++; if (re[hp] > 49)   team_hp++; if (re[pi] > 49)   team_pi++;
            if (le[ms] > 49)   team_ms++; if (le[hp] > 49)   team_hp++; if (le[pi] > 49)   team_pi++;
            if (nt[ms] > 49)   team_ms++; if (nt[hp] > 49)   team_hp++; if (nt[pi] > 49)   team_pi++;
            if (lolb[ms] > 49) team_ms++; if (lolb[hp] > 49) team_hp++; if (lolb[pi] > 49) team_pi++;
            if (lilb[ms] > 49) team_ms++; if (lilb[hp] > 49) team_hp++; if (lilb[pi] > 49) team_pi++;
            if (rilb[ms] > 49) team_ms++; if (rilb[hp] > 49) team_hp++; if (rilb[pi] > 49) team_pi++;
            if (rolb[ms] > 49) team_ms++; if (rolb[hp] > 49) team_hp++; if (rolb[pi] > 49) team_pi++;
            if (rcb[ms] > 49)  team_ms++; if (rcb[hp] > 49)  team_hp++; if (rcb[pi] > 49)  team_pi++;
            if (lcb[ms] > 49)  team_ms++; if (lcb[hp] > 49)  team_hp++; if (lcb[pi] > 49)  team_pi++;
            if (fs[ms] > 49)   team_ms++; if (fs[hp] > 49)   team_hp++; if (fs[pi] > 49)   team_pi++;
            if (ss[ms] > 49)   team_ms++; if (ss[hp] > 49)   team_hp++; if (ss[pi] > 49)   team_pi++;
            
            rush_def = team_ms + team_hp;
            pass_def = team_ms + team_pi;
            
            // /\ /\ /\ /\  Calculation code goes above here /\ /\ /\ /\ 
            if (rush_def > 0xf) rush_def = 0xf; // make sure that rush_def is no more than 0xf 
            if (pass_def > 0xf) pass_def = 0xf; // make sure that pass_def is no more than 0xf 
            string retVal = string.Format("{0:x}{1:x}",rush_def, pass_def ); // format the result as a hex string
            Console.WriteLine("My Value is " + retVal);
            return retVal;
         }
     }
 }

