namespace DaeForth {
	public struct Matrix4x4 {
		public float M11, M12, M13, M14;
		public float M21, M22, M23, M24;
		public float M31, M32, M33, M34;
		public float M41, M42, M43, M44;

		public Matrix4x4(Vec4 a, Vec4 b, Vec4 c, Vec4 d) {
			M11 = a.X;
			M21 = a.Y;
			M31 = a.Z;
			M41 = a.W;
			
			M12 = b.X;
			M22 = b.Y;
			M32 = b.Z;
			M42 = b.W;
			
			M13 = c.X;
			M23 = c.Y;
			M33 = c.Z;
			M43 = c.W;
			
			M14 = d.X;
			M24 = d.Y;
			M34 = d.Z;
			M44 = d.W;
		}
		
		public static Vec4 operator *(Matrix4x4 matrix, Vec4 vector) =>
			new Vec4(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41,
				vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42,
				vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43,
				vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44);
	}
}