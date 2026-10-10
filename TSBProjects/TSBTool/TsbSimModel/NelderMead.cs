using System;
using System.Collections.Generic;

namespace TsbSimModel
{
    /// <summary>
    /// Nelder-Mead simplex minimizer (no derivatives): used for the few calibration settings per share kind,
    /// whose loss (built on rounded shares and the engine's walk) isn't smooth. Starts from x0 with a simplex of
    /// side 'step'; stops when the best and worst points' losses are within 'tol' or after 'iterations'.
    /// </summary>
    public static class NelderMead
    {
        public static double[] Minimize(Func<double[], double> f, double[] x0, double step = 0.3, int iterations = 300, double tol = 1e-6)
        {
            int n = x0.Length;
            List<double[]> pts = new List<double[]> { (double[])x0.Clone() };
            for (int i = 0; i < n; i++)
            {
                double[] p = (double[])x0.Clone();
                p[i] += step;
                pts.Add(p);
            }
            List<double> vals = new List<double>();
            foreach (double[] p in pts)
                vals.Add(f(p));

            for (int it = 0; it < iterations; it++)
            {
                // order best .. worst (stable for ties)
                List<double[]> sp = new List<double[]>();
                List<double> sv = new List<double>();
                foreach (int i in StableOrder(vals.ToArray()))
                {
                    sp.Add(pts[i]);
                    sv.Add(vals[i]);
                }
                pts = sp;
                vals = sv;
                if (Math.Abs(vals[vals.Count - 1] - vals[0]) < tol)
                    break;

                double[] c = Centroid(pts, n);           // of all but the worst
                double[] worst = pts[n];
                double[] r = Add(c, Sub(c, worst), 1.0);  // reflection
                double fr = f(r);
                if (fr < vals[0])
                {
                    double[] e = Add(c, Sub(c, worst), 2.0); // expansion
                    double fe = f(e);
                    if (fe < fr) { pts[n] = e; vals[n] = fe; }
                    else { pts[n] = r; vals[n] = fr; }
                }
                else if (fr < vals[n - 1])
                {
                    pts[n] = r; vals[n] = fr;
                }
                else
                {
                    double[] ct = Add(c, Sub(worst, c), 0.5); // contraction toward the worst
                    double fc = f(ct);
                    if (fc < vals[n])
                    {
                        pts[n] = ct; vals[n] = fc;
                    }
                    else
                    {
                        for (int i = 1; i < pts.Count; i++) // shrink toward the best
                        {
                            pts[i] = Add(pts[0], Sub(pts[i], pts[0]), 0.5);
                            vals[i] = f(pts[i]);
                        }
                    }
                }
            }
            int best = 0;
            for (int i = 1; i < vals.Count; i++)
                if (vals[i] < vals[best]) best = i;
            return pts[best];
        }

        private static int[] StableOrder(double[] keys)
        {
            int[] idx = new int[keys.Length];
            for (int i = 0; i < idx.Length; i++) idx[i] = i;
            // insertion sort: stable, and the simplex is tiny
            for (int i = 1; i < idx.Length; i++)
            {
                int cur = idx[i], j = i - 1;
                while (j >= 0 && keys[idx[j]] > keys[cur]) { idx[j + 1] = idx[j]; j--; }
                idx[j + 1] = cur;
            }
            return idx;
        }

        private static double[] Centroid(List<double[]> pts, int count)
        {
            double[] c = new double[pts[0].Length];
            for (int i = 0; i < count; i++)
                for (int j = 0; j < c.Length; j++)
                    c[j] += pts[i][j];
            for (int j = 0; j < c.Length; j++)
                c[j] /= count;
            return c;
        }

        private static double[] Sub(double[] a, double[] b)
        {
            double[] r = new double[a.Length];
            for (int i = 0; i < a.Length; i++) r[i] = a[i] - b[i];
            return r;
        }

        private static double[] Add(double[] a, double[] d, double scale)
        {
            double[] r = new double[a.Length];
            for (int i = 0; i < a.Length; i++) r[i] = a[i] + scale * d[i];
            return r;
        }
    }
}
