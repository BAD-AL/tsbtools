using System;
using System.Collections.Generic;

namespace TsbSimModel
{
    /// <summary>
    /// What the sim engine does with the share bytes (SimTests\TSB2_study\TSB2_Sim_Engine.txt): for a carry,
    /// target, sack or INT it rolls R = 0..255 and walks the pool, subtracting each player's share; the
    /// player R falls under gets it. R left over past everyone goes to a fallback. This turns a pool's
    /// shares into each player's chance -- what the model is fitted and checked on.
    /// </summary>
    public class ShareKind
    {
        public string Name;          // carry / target / sack / int
        public string[] Pool;        // positions, in the order the engine walks them (INT: reversed, see Chances)
        public int SimByte;          // which sim byte holds the share
        public int[] Leftover;       // roster slots (Pool order) a roll past everyone goes to, at random
        public bool Reverse;         // the INT walk goes DB3 first

        public static readonly ShareKind Carry = new ShareKind { Name = "carry", Pool = Positions.CarryPool, SimByte = 0, Leftover = new[] { 1 } };              // -> RB1
        public static readonly ShareKind Target = new ShareKind { Name = "target", Pool = Positions.Skill, SimByte = 2, Leftover = Range(2, 10) };            // RB3 .. TE2
        public static readonly ShareKind Sack = new ShareKind { Name = "sack", Pool = Positions.Defense, SimByte = 0, Leftover = Range(0, 8) };               // RE .. RILB
        // the INT walk goes DB3 first, but its fallback is roster slots 4-11 (NT2 .. RCB), not the walk's 4th-11th
        // (until 2026-10-10 it was the walk's: ROLB .. DB1 -- LCB and DB1 instead of NT2 and LE2)
        public static readonly ShareKind Int = new ShareKind { Name = "int", Pool = Positions.Defense, SimByte = 1, Leftover = Range(4, 12), Reverse = true }; // NT2 .. RCB
        public static readonly ShareKind[] All = { Carry, Target, Sack, Int };

        private static int[] Range(int from, int to)
        {
            int[] r = new int[to - from];
            for (int i = 0; i < r.Length; i++)
                r[i] = from + i;
            return r;
        }

        /// <summary>
        /// Each pool player's chance (0..1, roster order) when their shares are 'shareOf(position)': the
        /// engine's walk, with the leftover spread over the fallback slots.
        /// </summary>
        public double[] Chances(Func<string, double> shareOf)
        {
            int n = Pool.Length;
            double[] shares = new double[n];
            for (int i = 0; i < n; i++)
                shares[i] = shareOf(Pool[Reverse ? n - 1 - i : i]);
            int[] leftover = Reverse ? Array.ConvertAll(Leftover, slot => n - 1 - slot) : Leftover; // in the walk's order
            double[] p = Walk(shares, leftover);
            if (Reverse)
                Array.Reverse(p);
            return p;
        }

        private static double[] Walk(double[] shares, int[] leftover)
        {
            double[] p = new double[shares.Length];
            double cum = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                double take = Math.Max(0, Math.Min(shares[i], 256 - cum));
                p[i] += take / 256.0;
                cum += take;
            }
            double rest = Math.Max(0, 256 - cum) / 256.0;
            foreach (int i in leftover)
                p[i] += rest / leftover.Length;
            return p;
        }

        /// <summary>The share of plays that go to a different player: half the summed difference (0 = the same).</summary>
        public static double Differs(double[] p, double[] q)
        {
            double s = 0;
            for (int i = 0; i < p.Length; i++)
                s += Math.Abs(p[i] - q[i]);
            return 0.5 * s;
        }

        /// <summary>Whole shares in proportion to 'weights' adding up to 'total', none over 254 (FF = absent player).</summary>
        public static int[] ScaleTo(double[] weights, int total)
        {
            // the same as TSBTool's TsbSimAutoUpdater.ScaleTo, so the fitted shares are what TSBTool writes
            return TSBTool2.TsbSimAutoUpdater.ScaleTo(weights, total);
        }
    }
}
