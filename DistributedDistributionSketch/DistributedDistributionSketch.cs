using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DistributedDistributionSketch
{
    /// <summary>
    /// Implements DDSketch algorithm.
    /// </summary>
    /// <remarks>
    /// References :
    /// 1) "Approximate quantiles from a stream"
    ///     Wikipedia,
    ///     2020,
    ///     https://en.wikipedia.org/wiki/Quantile#Approximate_quantiles_from_a_stream
    /// 2) "DDSketch: A Fast and Fully-Mergeable Quantile Sketch with Relative-Error Guarantees"
    ///     Charles Masson, Jee E. Rim, Homin K. Lee,
    ///     2019,
    ///     Proceedings of the VLDB Endowment, 2019
    ///     https://arxiv.org/pdf/1908.10693.pdf
    /// </remarks>
    public class DistributedDistributionSketch
    {
        private readonly double _gamma;
        private readonly double _logGamma;
        private readonly Dictionary<int, int> _negativeBuckets;
        private int _zeroCount;
        private readonly Dictionary<int, int> _positiveBuckets;

        /// <summary>
        /// Initializes a distributed distribution sketch.
        /// </summary>
        /// <param name="errorFactor">Maximum error factor between 0.0 and 1.0.</param>
        /// <remarks>
        /// The error factor guarantees that : Abs(quantile - estimatedQuantile) &lt;= errorFactor * quantile.
        /// </remarks>
        public DistributedDistributionSketch(double errorFactor = 0.001)
        {
            if (errorFactor <= 0.0)
                throw new ArgumentOutOfRangeException("Error factor cannot be less or equal than 0.0.", nameof(errorFactor));

            if (errorFactor > 1.0)
                throw new ArgumentOutOfRangeException("Error factor cannot be greater than 1.0.", nameof(errorFactor));

            ErrorFactor = errorFactor;
            _gamma = (1 + ErrorFactor) / (1 - ErrorFactor);
            _logGamma = Math.Log(_gamma);
            _positiveBuckets = new Dictionary<int, int>(71); // XXX
            _negativeBuckets = new Dictionary<int, int>(71); // XXX
            _zeroCount = 0;
            Count = 0;
            Minimum = double.MaxValue;
            Maximum = double.MinValue;
        }

        /// <summary>
        /// The error factor.
        /// </summary>
        public double ErrorFactor { get; }

        /// <summary>
        /// The number of inserted values.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The minimum value.
        /// </summary>
        public double Minimum { get; private set; }

        /// <summary>
        /// The maximum value.
        /// </summary>
        public double Maximum { get; private set; }

        /// <summary>
        /// Add several values to the sketch.
        /// </summary>
        /// <param name="values">The values to add.</param>
        public void Insert(IEnumerable<double> values)
        {
            foreach (var v in values)
            {
                Insert(v);
            }
        }

        /// <summary>
        /// Add a value to the sketch.
        /// </summary>
        /// <param name="x">The value to add.</param>
        public void Insert(double x)
        {
            if (x >= 1.0)
            {
                IncrementCountAtIndex(_positiveBuckets, GetIndex(x));
            }
            else if (x <= -1.0)
            {
                IncrementCountAtIndex(_negativeBuckets, GetIndex(-x));
            }
            else
            {
                _zeroCount++;
            }

            Count++;

            if (x < Minimum)
                Minimum = x;

            if (x > Maximum)
                Maximum = x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int GetIndex(double x)
        {
            var i = (int)Math.Ceiling(Math.Log(x) / _logGamma);

            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void IncrementCountAtIndex(IDictionary<int, int> buckets, int index)
        {
            if (!buckets.ContainsKey(index))
                buckets.Add(index, 1);
            else
                buckets[index]++;
        }

        /// <summary>
        /// Gets the q-quantile estimation.
        /// </summary>
        /// <param name="quantile">The q-quantile to estimate.</param>
        /// <returns>The estimate of the q-quantile.</returns>
        public double GetQuantile(double quantile)
        {
            if (quantile < 0.0)
                throw new ArgumentOutOfRangeException(nameof(quantile), "Quantile cannot be less than 0.0.");

            if (quantile > 1.0)
                throw new ArgumentOutOfRangeException(nameof(quantile), "Quantile cannot be greater than 1.0.");

            if (Count == 0)
                throw new InvalidOperationException("Cannot get quantile for an empty sketch.");

            switch (quantile)
            {
                case 0.0:
                    return Minimum;
                case 1.0:
                    return Maximum;
            }

            var count = 0;
            var rank = quantile * (Count - 1);

            var negativeSortedDictionary = new SortedDictionary<int, int>(_negativeBuckets);

            foreach (var (index, value) in negativeSortedDictionary)
            {
                count += value;

                if (count > rank)
                    return -GetValue(index);
            }

            count += _zeroCount;

            if (count > rank)
                return 0;

            var positiveSortedDictionary = new SortedDictionary<int, int>(_positiveBuckets);

            foreach (var (index, value) in positiveSortedDictionary)
            {
                count += value;

                if (count > rank)
                    return GetValue(index);
            }

            return Maximum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private double GetValue(int index)
        {
            return 2.0 * Math.Pow(_gamma, index) / (_gamma + 1);
        }

        /// <summary>
        /// Merge a sketch into the current sketch.
        /// </summary>
        /// <param name="other">The sketch to merge.</param>
        public void Merge(DistributedDistributionSketch other)
        {
            if (Math.Abs(other.ErrorFactor - this.ErrorFactor) > 0.0)
                throw new ArgumentException("Sketches must have exactly the same error factor.", nameof(other));

            foreach (var (key, value) in other._positiveBuckets)
            {
                if (!_positiveBuckets.ContainsKey(key))
                    _positiveBuckets.Add(key, value);
                else
                    _positiveBuckets[key] += value;
            }

            _zeroCount += other._zeroCount;

            foreach (var (key, value) in other._negativeBuckets)
            {
                if (!_negativeBuckets.ContainsKey(key))
                    _negativeBuckets.Add(key, value);
                else
                    _negativeBuckets[key] += value;
            }

            Count += other.Count;

            if (other.Minimum < Minimum)
                Minimum = other.Minimum;

            if (other.Maximum > Maximum)
                Maximum = other.Maximum;
        }
    }
}
