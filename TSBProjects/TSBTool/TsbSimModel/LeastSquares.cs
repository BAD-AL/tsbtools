using System;

namespace TsbSimModel
{
    /// <summary>
    /// Ordinary least squares: the coefficients b that minimize |X b - y|, by Householder QR (stable,
    /// no normal equations). The designs here are small (at most ~1500 rows x ~25 columns) and full rank.
    /// </summary>
    public static class LeastSquares
    {
        public static double[] Solve(double[][] x, double[] y)
        {
            int m = x.Length, n = x[0].Length;
            if (m < n)
                throw new ArgumentException("Least squares needs at least as many rows as columns.");
            double[,] a = new double[m, n];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    a[i, j] = x[i][j];
            double[] b = (double[])y.Clone();

            // Householder: for each column, reflect it onto the diagonal, applying the same to b
            for (int k = 0; k < n; k++)
            {
                double norm = 0;
                for (int i = k; i < m; i++)
                    norm += a[i, k] * a[i, k];
                norm = Math.Sqrt(norm);
                if (norm == 0)
                    throw new InvalidOperationException("Least squares: the design matrix isn't full rank (column " + k + ").");
                double alpha = a[k, k] > 0 ? -norm : norm;
                double[] v = new double[m];
                for (int i = k; i < m; i++)
                    v[i] = a[i, k];
                v[k] -= alpha;
                double vv = 0;
                for (int i = k; i < m; i++)
                    vv += v[i] * v[i];
                for (int j = k; j < n; j++)
                {
                    double dot = 0;
                    for (int i = k; i < m; i++)
                        dot += v[i] * a[i, j];
                    double f = 2 * dot / vv;
                    for (int i = k; i < m; i++)
                        a[i, j] -= f * v[i];
                }
                double db = 0;
                for (int i = k; i < m; i++)
                    db += v[i] * b[i];
                double fb = 2 * db / vv;
                for (int i = k; i < m; i++)
                    b[i] -= fb * v[i];
            }
            // back substitution with the upper triangle R
            double[] coef = new double[n];
            for (int j = n - 1; j >= 0; j--)
            {
                double s = b[j];
                for (int k = j + 1; k < n; k++)
                    s -= a[j, k] * coef[k];
                coef[j] = s / a[j, j];
            }
            return coef;
        }
    }
}
