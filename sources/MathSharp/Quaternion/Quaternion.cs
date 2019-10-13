﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MathSharp.Utils;
using Microsoft.VisualBasic;

namespace MathSharp.Quaternion
{
    public static partial class Quaternion
    {
        public static Vector128<float> LengthSquared(Vector128<float> quaternion)
            => Vector.LengthSquared4D(quaternion);

        public static Vector128<float> Length(Vector128<float> quaternion)
            => Vector.Length4D(quaternion);
        public static Vector128<float> Normalize(Vector128<float> quaternion)
            => Vector.Normalize4D(quaternion);

        public static Vector128<float> Conjugate(Vector128<float> quaternion)
            => Vector.Xor(quaternion, SingleConstants.SignMaskXYZ);

        public static Vector128<float> Inverse(Vector128<float> quaternion)
        {
            var lengthSquared = LengthSquared(quaternion);
            var inv = Vector.Reciprocal(lengthSquared);

            return Vector.Multiply(Conjugate(quaternion), inv);
        }

        public static Vector128<float> DotProduct(Vector128<float> left, Vector128<float> right)
            => Vector.DotProduct4D(left, right);

        public static Vector128<float> Add(Vector128<float> left, Vector128<float> right)
            => Vector.Add(left, right);

        public static Vector128<float> Subtract(Vector128<float> left, Vector128<float> right)
            => Vector.Subtract(left, right);

        public static Vector128<float> CompareEqual(Vector128<float> left, Vector128<float> right)
            => Vector.CompareEqual(left, right);

        public static Vector128<float> CompareNotEqual(Vector128<float> left, Vector128<float> right)
            => Vector.CompareNotEqual(left, right);

        public static Vector128<float> Negate(Vector128<float> quaternion)
            => Vector.Negate(quaternion);

        public static Vector128<float> Lerp(Vector128<float> left, Vector128<float> right, float amount)
        {
            var vAmount = Vector128.Create(amount);
            var mAmount = Vector128.Create(1 - amount);

            var dot = DotProduct(left, right);

            var lhs = Vector.Multiply(mAmount, left);
            var rhs = Vector.Multiply(vAmount, right);

            rhs = Vector.CopySign(dot, rhs);

            var result = Add(lhs, rhs);

            return Normalize(result);
        }

        public static Vector128<float> Slerp(Vector128<float> left, Vector128<float> right, Vector128<float> amount)
        {
            var cosOmega = DotProduct(left, right);

            var control = Vector.CompareLessThan(cosOmega, Vector128<float>.Zero);
            var sign = Vector.Select(Vector.SingleConstants.One, Vector.SingleConstants.NegativeOne, control);

            cosOmega = Vector.Multiply(cosOmega, sign);

            control = Vector.CompareLessThan(cosOmega, SingleConstants.OneMinusSlerpEpsilon);

            var sinOmega = Vector.Square(cosOmega);
            sinOmega = Subtract(Vector.SingleConstants.One, sinOmega);
            sinOmega = Vector.Sqrt(sinOmega);

            Vector128<float> omega = Vector.ATan2(sinOmega, cosOmega);

            var v01 = Vector.Permute(amount, ShuffleValues._1_0_3_2);
            v01 = Vector.And(v01, Vector.SingleConstants.MaskZW);
            v01 = Vector.Xor(v01, SingleConstants.SignMaskX);
            v01 = Vector.Add(v01, SingleConstants.MatrixIdentityRow0);

            var s0 = Vector.Multiply(v01, omega);
            s0 = Vector.Sin(s0);
            s0 = Vector.Divide(s0, sinOmega);

            s0 = Vector.Select(v01, s0, control);

            var s1 = Vector.PermuteWithY(s0);
            s0 = Vector.PermuteWithX(s0);
            s1 = Vector.Multiply(s1, sign);

            var result = Vector.Multiply(left, s0);
            s1 = Vector.Multiply(s1, right);

            return Vector.Add(result, s1);
        }

        public static Vector128<float> Concatenate(Vector128<float> left, Vector128<float> right)
            => Multiply(right, left); // order reversed

        // ReSharper disable InconsistentNaming, IdentifierTypo
        private static readonly Vector128<float> ControlWZYX = Vector128.Create(1.0f, -1.0f, 1.0f, -1.0f);
        private static readonly Vector128<float> ControlZWXY = Vector128.Create(1.0f, 1.0f, -1.0f, -1.0f);
        private static readonly Vector128<float> ControlYXWZ = Vector128.Create(-1.0f, 1.0f, 1.0f, -1.0f);
        // ReSharper restore InconsistentNaming, IdentifierTypo

        public static Vector128<float> Multiply(in Vector128<float> right, in Vector128<float> left)
        {
            var q2X = right;
            var q2Y = right;
            var q2Z = right;

            var result = right;

            result = Vector.PermuteWithW(result);
            q2X = Vector.PermuteWithX(q2X);
            q2Y = Vector.PermuteWithY(q2Y);
            q2Z = Vector.PermuteWithZ(q2Z);

            result = Vector.Multiply(result, left);
            var q1Shuffle = left;

            q1Shuffle = Vector.Permute(q1Shuffle, ShuffleValues._3_2_1_0);

            q2X = Vector.Multiply(q2X, q1Shuffle);
            q1Shuffle = Vector.Permute(q1Shuffle, ShuffleValues._1_0_3_2);

            q2X = Vector.Multiply(q2X, ControlWZYX);

            q2Y = Vector.Multiply(q2Y, q1Shuffle);
            q1Shuffle = Vector.Permute(q1Shuffle, ShuffleValues._3_2_1_0);

            q2Y = Vector.Multiply(q2Y, ControlZWXY);
            q2Z = Vector.Multiply(q2Z, q1Shuffle);
            result = Vector.Add(result, q2X);

            q2Z = Vector.Multiply(q2Z, ControlYXWZ);
            q2Y = Vector.Add(q2Y, q2Z);

            result = Vector.Add(result, q2Y);
            return result;
        }
    }
}