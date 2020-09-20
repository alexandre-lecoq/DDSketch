using System;
using System.Security.Cryptography;

namespace DistributedDistributionSketch.Tests
{
    public static class TestUtils
    {
        private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

        private static double GetNextRandomDouble()
        {
            var b = new byte[4];
            RandomNumberGenerator.GetBytes(b);
            var i = BitConverter.ToUInt32(b, 0);
            var d = (double)i / 0xffffffff;

            return d;
        }

        /// <summary>
        /// Compute the exact q-quantile for a multiset.
        /// </summary>
        /// <param name="set">The multiset for which to compute the q-quantile.</param>
        /// <param name="quantile">The q-quantile to compute.</param>
        /// <returns>The exact q-quantile.</returns>
        public static double GetExactQuantile(this double[] set, double quantile)
        {
            var setCopy = new double[set.Length];
            Array.Copy(set, setCopy, setCopy.Length);
            Array.Sort(setCopy);
            var rank = (int)Math.Floor(1.0 + quantile * (setCopy.Length - 1));

            return setCopy[rank];
        }
        
        /// <summary>
        /// Gets a uniform random distribution.
        /// </summary>
        /// <param name="samples">The number of samples to get.</param>
        /// <param name="mu">The mean of the distribution.</param>
        /// <param name="sigma">The standard deviation of the distribution.</param>
        /// <returns>The random distribution.</returns>
        public static double[] GetUniformRandomDistribution(int samples, double mu = 0.0, double sigma = 1.0)
        {
            var result = new double[samples];

            for (var i = 0; i < samples; i++)
            {
                var x = GetNextRandomDouble() * 2.0 - 1.0;
                result[i] = mu + sigma * x;
            }

            return result;
        }

        /// <summary>
        /// Gets a standard normal random distribution.
        /// </summary>
        /// <param name="samples">Number of samples to get.</param>
        /// <param name="mu">The mean of the distribution.</param>
        /// <param name="sigma">The standard deviation of the distribution.</param>
        /// <returns>The random distribution.</returns>
        /// <remarks>
        /// This implements Marsaglia polar method.
        /// More information here : https://en.wikipedia.org/wiki/Marsaglia_polar_method
        /// </remarks>
        public static double[] GetStandardNormalRandomDistribution(int samples, double mu = 0.0, double sigma = 1.0)
        {
            var result = new double[samples];
            var hasSpare = false;
            var spareValue = 0.0;

            for (var i = 0; i < samples; i++)
            {
                if (hasSpare)
                {
                    hasSpare = false;
                    result[i] = mu + sigma * spareValue;
                    continue;
                }

                double x, y, s;

                do
                {
                    x = GetNextRandomDouble() * 2.0 - 1.0;
                    y = GetNextRandomDouble() * 2.0 - 1.0;
                    s = x * x + y * y;
                }
                while (s >= 1.0 || s == 0.0);

                s = Math.Sqrt((-2.0 * Math.Log(s)) / s);
                var firstValue = x * s;
                result[i] = mu + sigma * firstValue;
                spareValue = y * s;
                hasSpare = true;
            }

            return result;
        }

        /// <summary>
        /// Gets a log-normal random distribution.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="mu">The mean of the variable's natural logarithm, not the mean of the distribution.</param>
        /// <param name="sigma">The standard deviation of the variable's natural logarithm, not the standard deviation of the distribution</param>
        /// <returns>The random distribution.</returns>
        /// <remarks>
        /// As described in https://en.wikipedia.org/wiki/Log-normal_distribution
        /// </remarks>
        public static double[] GetLogNormalRandomDistribution(int samples, double mu = 0.0, double sigma = 1.0)
        {
            var result = GetStandardNormalRandomDistribution(samples);

            for (var i = 0; i < samples; i++)
            {
                result[i] = Math.Exp(mu + sigma * result[i]);
            }

            return result;
        }
    }
}
