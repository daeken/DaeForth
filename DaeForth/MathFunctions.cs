using System;

namespace DaeForth {
	public static class MathFunctions {
		public static Vec4 gl_FragColor; // HACK

		public static float radians(float degrees) => degrees * MathF.PI / 180f;
		public static float degrees(float radians) => radians * 180f /  MathF.PI;

		public static Vec2 normalize(Vec2 v) => v.Normalized;
		public static Vec3 normalize(Vec3 v) => v.Normalized;
		public static Vec4 normalize(Vec4 v) => v.Normalized;

		public static float length(Vec2 v) => v.Length;
		public static float length(Vec3 v) => v.Length;
		public static float length(Vec4 v) => v.Length;

		public static Vec3 cross(Vec3 a, Vec3 b) => a.Cross(b);

		public static float dot(Vec2 a, Vec2 b) => a.Dot(b);
		public static float dot(Vec3 a, Vec3 b) => a.Dot(b);
		public static float dot(Vec4 a, Vec4 b) => a.Dot(b);

		public static float tan(float v) => MathF.Tan(v);
		
		public static float sin(float v) => MathF.Sin(v);
		
		public static float cos(float v) => MathF.Cos(v);

		public static float abs(float v) => MathF.Abs(v);
		public static Vec2 abs(Vec2 v) => v.Apply(MathF.Abs);
		public static Vec3 abs(Vec3 v) => v.Apply(MathF.Abs);
		public static Vec4 abs(Vec4 v) => v.Apply(MathF.Abs);

		public static float floor(float v) => MathF.Floor(v);
		public static Vec2 floor(Vec2 v) => v.Apply(MathF.Floor);
		public static Vec3 floor(Vec3 v) => v.Apply(MathF.Floor);
		public static Vec4 floor(Vec4 v) => v.Apply(MathF.Floor);

		public static float ceil(float v) => MathF.Ceiling(v);
		public static Vec2 ceil(Vec2 v) => v.Apply(MathF.Ceiling);
		public static Vec3 ceil(Vec3 v) => v.Apply(MathF.Ceiling);
		public static Vec4 ceil(Vec4 v) => v.Apply(MathF.Ceiling);

		public static float min(float a, float b) => MathF.Min(a, b);
		public static Vec2 min(Vec2 a, Vec2 b) => a.Apply(MathF.Min, b);
		public static Vec3 min(Vec3 a, Vec3 b) => a.Apply(MathF.Min, b);
		public static Vec4 min(Vec4 a, Vec4 b) => a.Apply(MathF.Min, b);
		public static Vec2 min(Vec2 a, float b) => a.Apply(x => MathF.Min(x, b));
		public static Vec3 min(Vec3 a, float b) => a.Apply(x => MathF.Min(x, b));
		public static Vec4 min(Vec4 a, float b) => a.Apply(x => MathF.Min(x, b));
		public static Vec2 min(float a, Vec2 b) => b.Apply(x => MathF.Min(a, x));
		public static Vec3 min(float a, Vec3 b) => b.Apply(x => MathF.Min(a, x));
		public static Vec4 min(float a, Vec4 b) => b.Apply(x => MathF.Min(a, x));

		public static float max(float a, float b) => MathF.Max(a, b);
		public static Vec2 max(Vec2 a, Vec2 b) => a.Apply(MathF.Max, b);
		public static Vec3 max(Vec3 a, Vec3 b) => a.Apply(MathF.Max, b);
		public static Vec4 max(Vec4 a, Vec4 b) => a.Apply(MathF.Max, b);
		public static Vec2 max(Vec2 a, float b) => a.Apply(x => MathF.Max(x, b));
		public static Vec3 max(Vec3 a, float b) => a.Apply(x => MathF.Max(x, b));
		public static Vec4 max(Vec4 a, float b) => a.Apply(x => MathF.Max(x, b));
		public static Vec2 max(float a, Vec2 b) => b.Apply(x => MathF.Max(a, x));
		public static Vec3 max(float a, Vec3 b) => b.Apply(x => MathF.Max(a, x));
		public static Vec4 max(float a, Vec4 b) => b.Apply(x => MathF.Max(a, x));

		public static float clamp(float x, float minVal, float maxVal) => max(min(maxVal, x), minVal);
		public static Vec2 clamp(Vec2 x, float minVal, float maxVal) => max(min(maxVal, x), minVal);
		public static Vec3 clamp(Vec3 x, float minVal, float maxVal) => max(min(maxVal, x), minVal);
		public static Vec4 clamp(Vec4 x, float minVal, float maxVal) => max(min(maxVal, x), minVal);

		public static float mix(float x, float y, float a) => x * (1 - a) + y * a;
		public static Vec2 mix(Vec2 x, Vec2 y, float a) => x * (1 - a) + y * a;
		public static Vec3 mix(Vec3 x, Vec3 y, float a) => x * (1 - a) + y * a;
		public static Vec4 mix(Vec4 x, Vec4 y, float a) => x * (1 - a) + y * a;
		public static Vec2 mix(Vec2 x, Vec2 y, Vec2 a) => x * (1 - a) + y * a;
		public static Vec3 mix(Vec3 x, Vec3 y, Vec3 a) => x * (1 - a) + y * a;
		public static Vec4 mix(Vec4 x, Vec4 y, Vec4 a) => x * (1 - a) + y * a;

		public static float mod(float a, float b) => a - b * floor(a / b);
		public static Vec2 mod(Vec2 a, Vec2 b) => a - b * floor(a / b);
		public static Vec3 mod(Vec3 a, Vec3 b) => a - b * floor(a / b);
		public static Vec4 mod(Vec4 a, Vec4 b) => a - b * floor(a / b);
		public static Vec2 mod(Vec2 a, float b) => a - b * floor(a / b);
		public static Vec3 mod(Vec3 a, float b) => a - b * floor(a / b);
		public static Vec4 mod(Vec4 a, float b) => a - b * floor(a / b);
		
		public static Matrix4x4 mat4(Vec4 a, Vec4 b, Vec4 c, Vec4 d) => new Matrix4x4(a, b, c, d);
	}
}