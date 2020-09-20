using System;
using Xunit;

namespace DistributedDistributionSketch.Tests
{
    public class DDSketchTests
    {
        [Fact]
        public void DistributedDistributionSketchDefaultConstructorDoesNotThrow()
        {
            var sketch = new DistributedDistributionSketch();
            Assert.Equal(sketch.ErrorFactor, sketch.ErrorFactor);
        }

        [Fact]
        public void DistributedDistributionSketchConstructorZeroFiveDoesNotThrow()
        {
            var sketch = new DistributedDistributionSketch(0.5);
            Assert.Equal(0.5, sketch.ErrorFactor);
        }

        [Fact]
        public void DistributedDistributionSketchConstructorOneDoesNotThrow()
        {
            var sketch = new DistributedDistributionSketch(1.0);
            Assert.Equal(1.0, sketch.ErrorFactor);
        }

        [Fact]
        public void DistributedDistributionSketchConstructorNegativeErrorFactorThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DistributedDistributionSketch(-0.1));
        }

        [Fact]
        public void DistributedDistributionSketchConstructorZeroErrorFactorThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DistributedDistributionSketch(0.0));
        }

        [Fact]
        public void DistributedDistributionSketchConstructorOneOneErrorFactorThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DistributedDistributionSketch(1.1));
        }


        [Fact]
        public void EmptySketchHasZeroCount()
        {
            var sketch = new DistributedDistributionSketch();

            Assert.Equal(0, sketch.Count);
        }

        [Fact]
        public void SketchWithOneInsertHasOneCount()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);

            Assert.Equal(1, sketch.Count);
        }

        [Fact]
        public void SketchWithTwoInsertsHasTwoCount()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);
            sketch.Insert(5500);

            Assert.Equal(2, sketch.Count);
        }

        [Fact]
        public void SketchWithThreeInsertsHasThreeCount()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);
            sketch.Insert(5500);
            sketch.Insert(6500);

            Assert.Equal(3, sketch.Count);
        }

        [Fact]
        public void SketchWithOneInsertHasCorrectMinimumAndMaximum()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);

            Assert.Equal(4500, sketch.Minimum);
            Assert.Equal(4500, sketch.Maximum);
        }

        [Fact]
        public void SketchWithTwoInsertsHasCorrectMinimumAndMaximum()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);
            sketch.Insert(5500);

            Assert.Equal(4500, sketch.Minimum);
            Assert.Equal(5500, sketch.Maximum);
        }

        [Fact]
        public void SketchWithThreeInsertsHasCorrectMinimumAndMaximum()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);
            sketch.Insert(5500);
            sketch.Insert(6500);

            Assert.Equal(4500, sketch.Minimum);
            Assert.Equal(6500, sketch.Maximum);
        }

        [Fact]
        public void InsertSetInSketchYieldsCorrectState()
        {
            var sketch = new DistributedDistributionSketch();
            var set = new double[] {4500, 5500, 6500};
            sketch.Insert(set);

            Assert.Equal(3, sketch.Count);
            Assert.Equal(4500, sketch.Minimum);
            Assert.Equal(6500, sketch.Maximum);
        }

        [Fact]
        public void InsertRemarkableValuesInSketchYieldsCorrectState()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(double.MinValue);
            sketch.Insert(double.MaxValue);
            sketch.Insert(double.Epsilon);
            sketch.Insert(2.0);
            sketch.Insert(1.0);
            sketch.Insert(0.5);
            sketch.Insert(0.0);
            sketch.Insert(-0.5);
            sketch.Insert(-1.0);
            sketch.Insert(-2.0);

            Assert.Equal(10, sketch.Count);
            Assert.Equal(double.MinValue, sketch.Minimum);
            Assert.Equal(double.MaxValue, sketch.Maximum);
        }

        [Fact]
        public void GetQuantileYieldsCorrectEstimation()
        {
            var sketch = new DistributedDistributionSketch();
            var set = new double[]
            {
                100, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
                60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
                70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
                80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
                90, 91, 92, 93, 94, 95, 96, 97, 98, 99
            };
            sketch.Insert(set);

            Assert.Equal(100, sketch.Count);
            Assert.Equal(1, sketch.Minimum);
            Assert.Equal(100, sketch.Maximum);
            Assert.Equal(sketch.Minimum, sketch.GetQuantile(0.0));
            Assert.Equal(sketch.Maximum, sketch.GetQuantile(1.0));
            Assert.True(Math.Abs(50.0 - sketch.GetQuantile(0.5)) < sketch.ErrorFactor * 50.0);
        }

        [Fact]
        public void GetNegativeQuantileThrows()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);

            Assert.Throws<ArgumentOutOfRangeException>(() => sketch.GetQuantile(-0.1));
        }

        [Fact]
        public void GetGreaterThanOneQuantileThrows()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(4500);

            Assert.Throws<ArgumentOutOfRangeException>(() => sketch.GetQuantile(1.1));
        }

        [Fact]
        public void GetQuantileFromEmptySketchThrows()
        {
            var sketch = new DistributedDistributionSketch();

            Assert.Throws<InvalidOperationException>(() => sketch.GetQuantile(0.5));
        }

        [Fact]
        public void GetQuantileForZeroValueYieldsCorrectResult()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(0);

            Assert.Equal(0, sketch.GetQuantile(0.5));
        }

        [Fact]
        public void GetQuantileForOneBigValueYieldsCorrectResult()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(1000000000);

            Assert.Equal(999740604.37254822, sketch.GetQuantile(0.5));
        }

        [Fact]
        public void SketchWithTNegativeValuesYieldsCorrectQuantile()
        {
            var sketch = new DistributedDistributionSketch();
            sketch.Insert(-1000);
            sketch.Insert(-1000000);
            sketch.Insert(-1000000000);

            Assert.Equal(-999493.67526358145, sketch.GetQuantile(0.5));
        }
        
        [Fact]
        public void SketchQuantileForUniformDistributionLiesWithinExpectedError()
        {
            var distribution = TestUtils.GetUniformRandomDistribution(1000000, 500, 5000);

            var sketch = new DistributedDistributionSketch();
            sketch.Insert(distribution);

            var approximateQuantile = sketch.GetQuantile(0.9);

            var exactQuantile = TestUtils.GetExactQuantile(distribution, 0.9);

            Assert.True(Math.Abs(exactQuantile - approximateQuantile) <= sketch.ErrorFactor * exactQuantile);
        }

        [Fact]
        public void SketchQuantileForNormalDistributionLiesWithinExpectedError()
        {
            var distribution = TestUtils.GetStandardNormalRandomDistribution(1000000, 500, 5000);

            var sketch = new DistributedDistributionSketch();
            sketch.Insert(distribution);

            var approximateQuantile = sketch.GetQuantile(0.9);

            var exactQuantile = TestUtils.GetExactQuantile(distribution, 0.9);

            Assert.True(Math.Abs(exactQuantile - approximateQuantile) <= sketch.ErrorFactor * exactQuantile);
        }

        [Fact]
        public void SketchQuantileForLogNormalDistributionLiesWithinExpectedError()
        {
            var distribution = TestUtils.GetLogNormalRandomDistribution(1000000, 2.7, 3.7);

            var sketch = new DistributedDistributionSketch();
            sketch.Insert(distribution);

            var approximateQuantile = sketch.GetQuantile(0.9);

            var exactQuantile = distribution.GetExactQuantile(0.9);

            Assert.True(Math.Abs(exactQuantile - approximateQuantile) <= sketch.ErrorFactor * exactQuantile);
        }

        [Fact]
        public void MergingSketchesYieldsCorrectQuantiles()
        {
            var sketch1 = new DistributedDistributionSketch();
            sketch1.Insert(-5);
            sketch1.Insert(-3);
            sketch1.Insert(-1);
            sketch1.Insert(1);
            sketch1.Insert(3);
            sketch1.Insert(5);
            var q1 = sketch1.GetQuantile(0.84);

            var sketch2 = new DistributedDistributionSketch();
            sketch2.Insert(-4);
            sketch2.Insert(-2);
            sketch2.Insert(0);
            sketch2.Insert(2);
            sketch2.Insert(4);
            sketch2.Insert(6);
            var q2 = sketch2.GetQuantile(0.84);

            sketch1.Merge(sketch2);
            var mergedQ = sketch1.GetQuantile(0.84);

            Assert.Equal(3.0011629583492887, q1);
            Assert.Equal(4.0028234008341412, q2);
            Assert.Equal(4.0028234008341412, mergedQ);
        }

        [Fact]
        public void MergingSketchWithDifferentErrorFactorThrowException()
        {
            var sketch1 = new DistributedDistributionSketch(0.1);
            sketch1.Insert(-5);

            var sketch2 = new DistributedDistributionSketch(0.01);
            sketch2.Insert(-4);

            Assert.Throws<ArgumentException>(() => sketch1.Merge(sketch2));
        }
    }
}
