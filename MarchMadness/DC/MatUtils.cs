using System;
using DaeForth;

namespace MarchMadness.DC {
    public static class MatUtils {
        public static double fnorm(DMat3 a) {
            return Math.Sqrt((a.m00 * a.m00) + (a.m01 * a.m01) + (a.m02 * a.m02)
                             + (a.m10 * a.m10) + (a.m11 * a.m11) + (a.m12 * a.m12)
                             + (a.m20 * a.m20) + (a.m21 * a.m21) + (a.m22 * a.m22));
        }

        public static double fnorm(SMat3 a) {
            return Math.Sqrt((a.m00 * a.m00) + (a.m01 * a.m01) + (a.m02 * a.m02)
                              + (a.m01 * a.m01) + (a.m11 * a.m11) + (a.m12 * a.m12)
                              + (a.m02 * a.m02) + (a.m12 * a.m12) + (a.m22 * a.m22));
        }

        public static double off(DMat3 a) {
            return Math.Sqrt((a.m01 * a.m01) + (a.m02 * a.m02) + (a.m10 * a.m10) + (a.m12 * a.m12) + (a.m20 * a.m20) +
                              (a.m21 * a.m21));
        }

        public static double off(SMat3 a) {
            return Math.Sqrt(2 * ((a.m01 * a.m01) + (a.m02 * a.m02) + (a.m12 * a.m12)));
        }

        public static void mmul(out DMat3 Out, DMat3 a, DMat3 b) {
            Out = new DMat3();
            Out.set(a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20,
                a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21,
                a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22,
                a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20,
                a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21,
                a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22,
                a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20,
                a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21,
                a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22);
        }

        public static void mmul_ata(out SMat3 Out, DMat3 a) {
            Out = new SMat3();

            Out.setSymmetric(a.m00 * a.m00 + a.m10 * a.m10 + a.m20 * a.m20,
                a.m00 * a.m01 + a.m10 * a.m11 + a.m20 * a.m21,
                a.m00 * a.m02 + a.m10 * a.m12 + a.m20 * a.m22,
                a.m01 * a.m01 + a.m11 * a.m11 + a.m21 * a.m21,
                a.m01 * a.m02 + a.m11 * a.m12 + a.m21 * a.m22,
                a.m02 * a.m02 + a.m12 * a.m12 + a.m22 * a.m22);
        }

        public static void transpose(out DMat3 Out, DMat3 a) {
            Out = new DMat3();

            Out.set(a.m00, a.m10, a.m20, a.m01, a.m11, a.m21, a.m02, a.m12, a.m22);
        }

        public static void vmul(out Vec3 Out, DMat3 a, Vec3 v) {
            Out = new Vec3(
                (float) ((a.m00 * v.X) + (a.m01 * v.Y) + (a.m02 * v.Z)),
                (float) ((a.m10 * v.X) + (a.m11 * v.Y) + (a.m12 * v.Z)),
                (float) ((a.m20 * v.X) + (a.m21 * v.Y) + (a.m22 * v.Z)));
        }

        public static void vmul_symmetric(out Vec3 Out, SMat3 a, Vec3 v) {
            Out = new Vec3(
                (float) ((a.m00 * v.X) + (a.m01 * v.Y) + (a.m02 * v.Z)),
                (float) ((a.m01 * v.X) + (a.m11 * v.Y) + (a.m12 * v.Z)),
                (float) ((a.m02 * v.X) + (a.m12 * v.Y) + (a.m22 * v.Z)));
        }
    }
}