using System;

namespace DaeForth {
	public struct Vec2 {
		public float X, Y;

		public float LengthSquared => X * X + Y * Y;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec2(float value) => X = Y = value;
		public Vec2(float x, float y) {
			X = x;
			Y = y;
		}
		
		public static Vec2 operator +(Vec2 left, Vec2 right) => new Vec2(left.X + right.X, left.Y + right.Y);
		public static Vec2 operator +(Vec2 left, float right) => new Vec2(left.X + right, left.Y + right);
		public static Vec2 operator +(float left, Vec2 right) => new Vec2(left + right.X, left + right.Y);

		public static Vec2 operator -(Vec2 left, Vec2 right) => new Vec2(left.X - right.X, left.Y - right.Y);
		public static Vec2 operator -(Vec2 left, float right) => new Vec2(left.X - right, left.Y - right);
		public static Vec2 operator -(float left, Vec2 right) => new Vec2(left - right.X, left - right.Y);

		public static Vec2 operator *(Vec2 left, Vec2 right) => new Vec2(left.X * right.X, left.Y * right.Y);
		public static Vec2 operator *(Vec2 left, float right) => new Vec2(left.X * right, left.Y * right);
		public static Vec2 operator *(float left, Vec2 right) => new Vec2(left * right.X, left * right.Y);

		public static Vec2 operator /(Vec2 left, Vec2 right) => new Vec2(left.X / right.X, left.Y / right.Y);
		public static Vec2 operator /(Vec2 left, float right) => new Vec2(left.X / right, left.Y / right);
		public static Vec2 operator /(float left, Vec2 right) => new Vec2(left / right.X, left / right.Y);

		public float Dot(Vec2 right) => X * right.X + Y * right.Y;
		
		public Vec2 Apply(Func<float, float> func) => new Vec2(func(X), func(Y));
	}
	
	public struct Vec3 {
		public float X, Y, Z;

		public float LengthSquared => X * X + Y * Y + Z * Z;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec3(float value) => X = Y = Z = value;
		public Vec3(float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}
		
		public static Vec3 operator +(Vec3 left, Vec3 right) => new Vec3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		public static Vec3 operator +(Vec3 left, float right) => new Vec3(left.X + right, left.Y + right, left.Z + right);
		public static Vec3 operator +(float left, Vec3 right) => new Vec3(left + right.X, left + right.Y, left + right.Z);

		public static Vec3 operator -(Vec3 left, Vec3 right) => new Vec3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		public static Vec3 operator -(Vec3 left, float right) => new Vec3(left.X - right, left.Y - right, left.Z - right);
		public static Vec3 operator -(float left, Vec3 right) => new Vec3(left - right.X, left - right.Y, left - right.Z);

		public static Vec3 operator *(Vec3 left, Vec3 right) => new Vec3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		public static Vec3 operator *(Vec3 left, float right) => new Vec3(left.X * right, left.Y * right, left.Z * right);
		public static Vec3 operator *(float left, Vec3 right) => new Vec3(left * right.X, left * right.Y, left * right.Z);

		public static Vec3 operator /(Vec3 left, Vec3 right) => new Vec3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		public static Vec3 operator /(Vec3 left, float right) => new Vec3(left.X / right, left.Y / right, left.Z / right);
		public static Vec3 operator /(float left, Vec3 right) => new Vec3(left / right.X, left / right.Y, left / right.Z);

		public float Dot(Vec3 right) => X * right.X + Y * right.Y + Z * right.Z;
		
		public Vec3 Apply(Func<float, float> func) => new Vec3(func(X), func(Y), func(Z));
	}
	
	public struct Vec4 {
		public float X, Y, Z, W;

		public float LengthSquared => X * X + Y * Y + Z * Z + W * W;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec4(float value) => X = Y = Z = W = value;
		public Vec4(float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		
		public static Vec4 operator +(Vec4 left, Vec4 right) => new Vec4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		public static Vec4 operator +(Vec4 left, float right) => new Vec4(left.X + right, left.Y + right, left.Z + right, left.W + right);
		public static Vec4 operator +(float left, Vec4 right) => new Vec4(left + right.X, left + right.Y, left + right.Z, left + right.W);

		public static Vec4 operator -(Vec4 left, Vec4 right) => new Vec4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		public static Vec4 operator -(Vec4 left, float right) => new Vec4(left.X - right, left.Y - right, left.Z - right, left.W - right);
		public static Vec4 operator -(float left, Vec4 right) => new Vec4(left - right.X, left - right.Y, left - right.Z, left - right.W);

		public static Vec4 operator *(Vec4 left, Vec4 right) => new Vec4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		public static Vec4 operator *(Vec4 left, float right) => new Vec4(left.X * right, left.Y * right, left.Z * right, left.W * right);
		public static Vec4 operator *(float left, Vec4 right) => new Vec4(left * right.X, left * right.Y, left * right.Z, left * right.W);

		public static Vec4 operator /(Vec4 left, Vec4 right) => new Vec4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		public static Vec4 operator /(Vec4 left, float right) => new Vec4(left.X / right, left.Y / right, left.Z / right, left.W / right);
		public static Vec4 operator /(float left, Vec4 right) => new Vec4(left / right.X, left / right.Y, left / right.Z, left / right.W);

		public float Dot(Vec4 right) => X * right.X + Y * right.Y + Z * right.Z + W * right.W;
		
		public Vec4 Apply(Func<float, float> func) => new Vec4(func(X), func(Y), func(Z), func(W));
	}
}