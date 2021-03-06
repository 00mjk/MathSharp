﻿using OpenTK;
using System.Collections.Generic;
using Xunit;

namespace MathSharp.UnitTests.VectorTests.VectorDouble.BasicMathsTests
{
    public class Remainder
    {
        public static IEnumerable<object[]> Data =>
            new[]
            {
                new object[] { new Vector4d(10.4f), new Vector4d(2f) },
                new object[] { new Vector4d(-10.4f), new Vector4d(2f) },
                new object[] { new Vector4d(float.NaN, float.MinValue, float.MaxValue, float.PositiveInfinity), new Vector4d(3f) },
                new object[] { new Vector4d(float.NaN, float.MinValue, float.MaxValue, float.PositiveInfinity), new Vector4d(2.4f) },
                new object[] { new Vector4d(23.45f), new Vector4d(219f) },
                new object[] { new Vector4d(12), new Vector4d(float.NaN) },
                new object[] { new Vector4d(-10000000f), new Vector4d(2.2f) },
                new object[] { new Vector4d(10000000f), new Vector4d(2.2f) },
                new object[] { new Vector4d(float.NegativeInfinity), new Vector4d(3f) }
            };

        [Theory]
        [MemberData(nameof(Data))]
        public unsafe void Remainder_Theory(Vector4d left, Vector4d right)
        {
            var l = Vector.FromVector4D(&left.X);
            var r = Vector.FromVector4D(&right.X);

            var remainder = Vector.Remainder(l, r);
            var expected = new Vector4d(left.X % right.X, left.Y % right.Y, left.Z % right.Z, left.W % right.W);

            TestHelpers.AreEqual(expected, remainder);
        }
    }
}