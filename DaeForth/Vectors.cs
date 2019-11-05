using System;

namespace DaeForth {
	public struct Vec2 {
		public static readonly Vec2 Zero = new Vec2();
		public float X, Y;

		public float LengthSquared => X * X + Y * Y;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec2 Normalized {
			get {
				var len = LengthSquared;
				if(len == 0)
					return this;
				return this / MathF.Sqrt(len);
			}
		}

		public Vec2(float value) => X = Y = value;
		public Vec2(float x, float y) {
			X = x;
			Y = y;
		}

		public override string ToString() => $"({X}, {Y})";
		public override int GetHashCode() => HashCode.Combine(MathF.Round(X, 4).GetHashCode(),
			MathF.Round(Y, 4).GetHashCode());
		public override bool Equals(object obj) =>
			obj is Vec2 other && MathF.Abs(MathF.Round(X, 4) - MathF.Round(other.X, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(Y, 4) - MathF.Round(other.Y, 4)) < 0.001f;
		
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
		public Vec2 Apply(Func<float, float, float> func, Vec2 b) => new Vec2(func(X, b.X), func(Y, b.Y));
		
		public float x => X;
		public float y => Y;
		
		public Vec2 xx => new Vec2(X, X);
		public Vec2 xy => new Vec2(X, Y);
		public Vec2 yx => new Vec2(Y, X);
		public Vec2 yy => new Vec2(Y, Y);

		public Vec3 xxx => new Vec3(X, X, X);
		public Vec3 xxy => new Vec3(X, X, Y);
		public Vec3 xyx => new Vec3(X, Y, X);
		public Vec3 xyy => new Vec3(X, Y, Y);
		public Vec3 yxx => new Vec3(Y, X, X);
		public Vec3 yxy => new Vec3(Y, X, Y);
		public Vec3 yyx => new Vec3(Y, Y, X);
		public Vec3 yyy => new Vec3(Y, Y, Y);

		public Vec4 xxxx => new Vec4(X, X, X, X);
		public Vec4 xxxy => new Vec4(X, X, X, Y);
		public Vec4 xxyx => new Vec4(X, X, Y, X);
		public Vec4 xxyy => new Vec4(X, X, Y, Y);
		public Vec4 xyxx => new Vec4(X, Y, X, X);
		public Vec4 xyxy => new Vec4(X, Y, X, Y);
		public Vec4 xyyx => new Vec4(X, Y, Y, X);
		public Vec4 xyyy => new Vec4(X, Y, Y, Y);
		public Vec4 yxxx => new Vec4(Y, X, X, X);
		public Vec4 yxxy => new Vec4(Y, X, X, Y);
		public Vec4 yxyx => new Vec4(Y, X, Y, X);
		public Vec4 yxyy => new Vec4(Y, X, Y, Y);
		public Vec4 yyxx => new Vec4(Y, Y, X, X);
		public Vec4 yyxy => new Vec4(Y, Y, X, Y);
		public Vec4 yyyx => new Vec4(Y, Y, Y, X);
		public Vec4 yyyy => new Vec4(Y, Y, Y, Y);
	}
	
	public struct Vec3 {
		public static readonly Vec3 Zero = new Vec3();
		public float X, Y, Z;

		public float LengthSquared => X * X + Y * Y + Z * Z;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec3 Normalized {
			get {
				var len = LengthSquared;
				if(len == 0)
					return this;
				return this / MathF.Sqrt(len);
			}
		}

		public Vec3(float value) => X = Y = Z = value;
		public Vec3(float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}

		public Vec3(Vec2 v, float z) : this(v.X, v.Y, z) {}
		public Vec3(float x, Vec2 v) : this(x, v.X, v.Y) {}

		public override string ToString() => $"({X}, {Y}, {Z})";
		public override int GetHashCode() => HashCode.Combine(MathF.Round(X, 4).GetHashCode(),
			MathF.Round(Y, 4).GetHashCode(), MathF.Round(Z, 4).GetHashCode());

		public override bool Equals(object obj) =>
			obj is Vec3 other && MathF.Abs(MathF.Round(X, 4) - MathF.Round(other.X, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(Y, 4) - MathF.Round(other.Y, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(Z, 4) - MathF.Round(other.Z, 4)) < 0.001f;

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

		public Vec3 Cross(Vec3 right) => new Vec3(
			Y * right.Z - Z * right.Y,
			X * right.Z - Z * right.X,
			X * right.Y - Y * right.X
		);
		
		public Vec3 Apply(Func<float, float> func) => new Vec3(func(X), func(Y), func(Z));
		public Vec3 Apply(Func<float, float, float> func, Vec3 b) => new Vec3(func(X, b.X), func(Y, b.Y), func(Z, b.Z));

		public float x => X;
		public float y => Y;
		public float z => Z;
		
		public Vec2 xx => new Vec2(X, X);
		public Vec2 xy => new Vec2(X, Y);
		public Vec2 xz => new Vec2(X, Z);
		public Vec2 yx => new Vec2(Y, X);
		public Vec2 yy => new Vec2(Y, Y);
		public Vec2 yz => new Vec2(Y, Z);
		public Vec2 zx => new Vec2(Z, X);
		public Vec2 zy => new Vec2(Z, Y);
		public Vec2 zz => new Vec2(Z, Z);

		public Vec3 xxx => new Vec3(X, X, X);
		public Vec3 xxy => new Vec3(X, X, Y);
		public Vec3 xxz => new Vec3(X, X, Z);
		public Vec3 xyx => new Vec3(X, Y, X);
		public Vec3 xyy => new Vec3(X, Y, Y);
		public Vec3 xyz => new Vec3(X, Y, Z);
		public Vec3 xzx => new Vec3(X, Z, X);
		public Vec3 xzy => new Vec3(X, Z, Y);
		public Vec3 xzz => new Vec3(X, Z, Z);
		public Vec3 yxx => new Vec3(Y, X, X);
		public Vec3 yxy => new Vec3(Y, X, Y);
		public Vec3 yxz => new Vec3(Y, X, Z);
		public Vec3 yyx => new Vec3(Y, Y, X);
		public Vec3 yyy => new Vec3(Y, Y, Y);
		public Vec3 yyz => new Vec3(Y, Y, Z);
		public Vec3 yzx => new Vec3(Y, Z, X);
		public Vec3 yzy => new Vec3(Y, Z, Y);
		public Vec3 yzz => new Vec3(Y, Z, Z);
		public Vec3 zxx => new Vec3(Z, X, X);
		public Vec3 zxy => new Vec3(Z, X, Y);
		public Vec3 zxz => new Vec3(Z, X, Z);
		public Vec3 zyx => new Vec3(Z, Y, X);
		public Vec3 zyy => new Vec3(Z, Y, Y);
		public Vec3 zyz => new Vec3(Z, Y, Z);
		public Vec3 zzx => new Vec3(Z, Z, X);
		public Vec3 zzy => new Vec3(Z, Z, Y);
		public Vec3 zzz => new Vec3(Z, Z, Z);

		public Vec4 xxxx => new Vec4(X, X, X, X);
		public Vec4 xxxy => new Vec4(X, X, X, Y);
		public Vec4 xxxz => new Vec4(X, X, X, Z);
		public Vec4 xxyx => new Vec4(X, X, Y, X);
		public Vec4 xxyy => new Vec4(X, X, Y, Y);
		public Vec4 xxyz => new Vec4(X, X, Y, Z);
		public Vec4 xxzx => new Vec4(X, X, Z, X);
		public Vec4 xxzy => new Vec4(X, X, Z, Y);
		public Vec4 xxzz => new Vec4(X, X, Z, Z);
		public Vec4 xyxx => new Vec4(X, Y, X, X);
		public Vec4 xyxy => new Vec4(X, Y, X, Y);
		public Vec4 xyxz => new Vec4(X, Y, X, Z);
		public Vec4 xyyx => new Vec4(X, Y, Y, X);
		public Vec4 xyyy => new Vec4(X, Y, Y, Y);
		public Vec4 xyyz => new Vec4(X, Y, Y, Z);
		public Vec4 xyzx => new Vec4(X, Y, Z, X);
		public Vec4 xyzy => new Vec4(X, Y, Z, Y);
		public Vec4 xyzz => new Vec4(X, Y, Z, Z);
		public Vec4 xzxx => new Vec4(X, Z, X, X);
		public Vec4 xzxy => new Vec4(X, Z, X, Y);
		public Vec4 xzxz => new Vec4(X, Z, X, Z);
		public Vec4 xzyx => new Vec4(X, Z, Y, X);
		public Vec4 xzyy => new Vec4(X, Z, Y, Y);
		public Vec4 xzyz => new Vec4(X, Z, Y, Z);
		public Vec4 xzzx => new Vec4(X, Z, Z, X);
		public Vec4 xzzy => new Vec4(X, Z, Z, Y);
		public Vec4 xzzz => new Vec4(X, Z, Z, Z);
		public Vec4 yxxx => new Vec4(Y, X, X, X);
		public Vec4 yxxy => new Vec4(Y, X, X, Y);
		public Vec4 yxxz => new Vec4(Y, X, X, Z);
		public Vec4 yxyx => new Vec4(Y, X, Y, X);
		public Vec4 yxyy => new Vec4(Y, X, Y, Y);
		public Vec4 yxyz => new Vec4(Y, X, Y, Z);
		public Vec4 yxzx => new Vec4(Y, X, Z, X);
		public Vec4 yxzy => new Vec4(Y, X, Z, Y);
		public Vec4 yxzz => new Vec4(Y, X, Z, Z);
		public Vec4 yyxx => new Vec4(Y, Y, X, X);
		public Vec4 yyxy => new Vec4(Y, Y, X, Y);
		public Vec4 yyxz => new Vec4(Y, Y, X, Z);
		public Vec4 yyyx => new Vec4(Y, Y, Y, X);
		public Vec4 yyyy => new Vec4(Y, Y, Y, Y);
		public Vec4 yyyz => new Vec4(Y, Y, Y, Z);
		public Vec4 yyzx => new Vec4(Y, Y, Z, X);
		public Vec4 yyzy => new Vec4(Y, Y, Z, Y);
		public Vec4 yyzz => new Vec4(Y, Y, Z, Z);
		public Vec4 yzxx => new Vec4(Y, Z, X, X);
		public Vec4 yzxy => new Vec4(Y, Z, X, Y);
		public Vec4 yzxz => new Vec4(Y, Z, X, Z);
		public Vec4 yzyx => new Vec4(Y, Z, Y, X);
		public Vec4 yzyy => new Vec4(Y, Z, Y, Y);
		public Vec4 yzyz => new Vec4(Y, Z, Y, Z);
		public Vec4 yzzx => new Vec4(Y, Z, Z, X);
		public Vec4 yzzy => new Vec4(Y, Z, Z, Y);
		public Vec4 yzzz => new Vec4(Y, Z, Z, Z);
		public Vec4 zxxx => new Vec4(Z, X, X, X);
		public Vec4 zxxy => new Vec4(Z, X, X, Y);
		public Vec4 zxxz => new Vec4(Z, X, X, Z);
		public Vec4 zxyx => new Vec4(Z, X, Y, X);
		public Vec4 zxyy => new Vec4(Z, X, Y, Y);
		public Vec4 zxyz => new Vec4(Z, X, Y, Z);
		public Vec4 zxzx => new Vec4(Z, X, Z, X);
		public Vec4 zxzy => new Vec4(Z, X, Z, Y);
		public Vec4 zxzz => new Vec4(Z, X, Z, Z);
		public Vec4 zyxx => new Vec4(Z, Y, X, X);
		public Vec4 zyxy => new Vec4(Z, Y, X, Y);
		public Vec4 zyxz => new Vec4(Z, Y, X, Z);
		public Vec4 zyyx => new Vec4(Z, Y, Y, X);
		public Vec4 zyyy => new Vec4(Z, Y, Y, Y);
		public Vec4 zyyz => new Vec4(Z, Y, Y, Z);
		public Vec4 zyzx => new Vec4(Z, Y, Z, X);
		public Vec4 zyzy => new Vec4(Z, Y, Z, Y);
		public Vec4 zyzz => new Vec4(Z, Y, Z, Z);
		public Vec4 zzxx => new Vec4(Z, Z, X, X);
		public Vec4 zzxy => new Vec4(Z, Z, X, Y);
		public Vec4 zzxz => new Vec4(Z, Z, X, Z);
		public Vec4 zzyx => new Vec4(Z, Z, Y, X);
		public Vec4 zzyy => new Vec4(Z, Z, Y, Y);
		public Vec4 zzyz => new Vec4(Z, Z, Y, Z);
		public Vec4 zzzx => new Vec4(Z, Z, Z, X);
		public Vec4 zzzy => new Vec4(Z, Z, Z, Y);
		public Vec4 zzzz => new Vec4(Z, Z, Z, Z);
	}
	
	public struct Vec4 {
		public static readonly Vec4 Zero = new Vec4();
		public float X, Y, Z, W;

		public float LengthSquared => X * X + Y * Y + Z * Z + W * W;
		public float Length => MathF.Sqrt(LengthSquared);

		public Vec4 Normalized {
			get {
				var len = LengthSquared;
				if(len == 0)
					return this;
				return this / MathF.Sqrt(len);
			}
		}

		public Vec4(float value) => X = Y = Z = W = value;
		public Vec4(float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		public Vec4(Vec2 v, float z, float w) : this(v.X, v.Y, z, w) {}
		public Vec4(float x, Vec2 v, float w) : this(x, v.X, v.Y, w) {}
		public Vec4(float x, float y, Vec2 v) : this(x, y, v.X, v.Y) {}
		
		public Vec4(Vec3 v, float w) : this(v.X, v.Y, v.Z, w) {}
		public Vec4(float x, Vec3 v) : this(x, v.X, v.Y, v.Z) {}

		public override string ToString() => $"({X}, {Y}, {Z}, {W})";
		public override int GetHashCode() => HashCode.Combine(MathF.Round(X, 4).GetHashCode(),
			MathF.Round(Y, 4).GetHashCode(), MathF.Round(Z, 4).GetHashCode(), MathF.Round(W, 4).GetHashCode());
		public override bool Equals(object obj) =>
			obj is Vec4 other && MathF.Abs(MathF.Round(X, 4) - MathF.Round(other.X, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(Y, 4) - MathF.Round(other.Y, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(Z, 4) - MathF.Round(other.Z, 4)) < 0.001f &&
			MathF.Abs(MathF.Round(W, 4) - MathF.Round(other.W, 4)) < 0.001f;
		
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
		public Vec4 Apply(Func<float, float, float> func, Vec4 b) => new Vec4(func(X, b.X), func(Y, b.Y), func(Z, b.Z), func(W, b.W));
		
		public float x => X;
		public float y => Y;
		public float z => Z;
		public float w => W;

		public Vec2 xx => new Vec2(X, X);
		public Vec2 xy => new Vec2(X, Y);
		public Vec2 xz => new Vec2(X, Z);
		public Vec2 xw => new Vec2(X, W);
		public Vec2 yx => new Vec2(Y, X);
		public Vec2 yy => new Vec2(Y, Y);
		public Vec2 yz => new Vec2(Y, Z);
		public Vec2 yw => new Vec2(Y, W);
		public Vec2 zx => new Vec2(Z, X);
		public Vec2 zy => new Vec2(Z, Y);
		public Vec2 zz => new Vec2(Z, Z);
		public Vec2 zw => new Vec2(Z, W);
		public Vec2 wx => new Vec2(W, X);
		public Vec2 wy => new Vec2(W, Y);
		public Vec2 wz => new Vec2(W, Z);
		public Vec2 ww => new Vec2(W, W);

		public Vec3 xxx => new Vec3(X, X, X);
		public Vec3 xxy => new Vec3(X, X, Y);
		public Vec3 xxz => new Vec3(X, X, Z);
		public Vec3 xxw => new Vec3(X, X, W);
		public Vec3 xyx => new Vec3(X, Y, X);
		public Vec3 xyy => new Vec3(X, Y, Y);
		public Vec3 xyz => new Vec3(X, Y, Z);
		public Vec3 xyw => new Vec3(X, Y, W);
		public Vec3 xzx => new Vec3(X, Z, X);
		public Vec3 xzy => new Vec3(X, Z, Y);
		public Vec3 xzz => new Vec3(X, Z, Z);
		public Vec3 xzw => new Vec3(X, Z, W);
		public Vec3 xwx => new Vec3(X, W, X);
		public Vec3 xwy => new Vec3(X, W, Y);
		public Vec3 xwz => new Vec3(X, W, Z);
		public Vec3 xww => new Vec3(X, W, W);
		public Vec3 yxx => new Vec3(Y, X, X);
		public Vec3 yxy => new Vec3(Y, X, Y);
		public Vec3 yxz => new Vec3(Y, X, Z);
		public Vec3 yxw => new Vec3(Y, X, W);
		public Vec3 yyx => new Vec3(Y, Y, X);
		public Vec3 yyy => new Vec3(Y, Y, Y);
		public Vec3 yyz => new Vec3(Y, Y, Z);
		public Vec3 yyw => new Vec3(Y, Y, W);
		public Vec3 yzx => new Vec3(Y, Z, X);
		public Vec3 yzy => new Vec3(Y, Z, Y);
		public Vec3 yzz => new Vec3(Y, Z, Z);
		public Vec3 yzw => new Vec3(Y, Z, W);
		public Vec3 ywx => new Vec3(Y, W, X);
		public Vec3 ywy => new Vec3(Y, W, Y);
		public Vec3 ywz => new Vec3(Y, W, Z);
		public Vec3 yww => new Vec3(Y, W, W);
		public Vec3 zxx => new Vec3(Z, X, X);
		public Vec3 zxy => new Vec3(Z, X, Y);
		public Vec3 zxz => new Vec3(Z, X, Z);
		public Vec3 zxw => new Vec3(Z, X, W);
		public Vec3 zyx => new Vec3(Z, Y, X);
		public Vec3 zyy => new Vec3(Z, Y, Y);
		public Vec3 zyz => new Vec3(Z, Y, Z);
		public Vec3 zyw => new Vec3(Z, Y, W);
		public Vec3 zzx => new Vec3(Z, Z, X);
		public Vec3 zzy => new Vec3(Z, Z, Y);
		public Vec3 zzz => new Vec3(Z, Z, Z);
		public Vec3 zzw => new Vec3(Z, Z, W);
		public Vec3 zwx => new Vec3(Z, W, X);
		public Vec3 zwy => new Vec3(Z, W, Y);
		public Vec3 zwz => new Vec3(Z, W, Z);
		public Vec3 zww => new Vec3(Z, W, W);
		public Vec3 wxx => new Vec3(W, X, X);
		public Vec3 wxy => new Vec3(W, X, Y);
		public Vec3 wxz => new Vec3(W, X, Z);
		public Vec3 wxw => new Vec3(W, X, W);
		public Vec3 wyx => new Vec3(W, Y, X);
		public Vec3 wyy => new Vec3(W, Y, Y);
		public Vec3 wyz => new Vec3(W, Y, Z);
		public Vec3 wyw => new Vec3(W, Y, W);
		public Vec3 wzx => new Vec3(W, Z, X);
		public Vec3 wzy => new Vec3(W, Z, Y);
		public Vec3 wzz => new Vec3(W, Z, Z);
		public Vec3 wzw => new Vec3(W, Z, W);
		public Vec3 wwx => new Vec3(W, W, X);
		public Vec3 wwy => new Vec3(W, W, Y);
		public Vec3 wwz => new Vec3(W, W, Z);
		public Vec3 www => new Vec3(W, W, W);

		public Vec4 xxxx => new Vec4(X, X, X, X);
		public Vec4 xxxy => new Vec4(X, X, X, Y);
		public Vec4 xxxz => new Vec4(X, X, X, Z);
		public Vec4 xxxw => new Vec4(X, X, X, W);
		public Vec4 xxyx => new Vec4(X, X, Y, X);
		public Vec4 xxyy => new Vec4(X, X, Y, Y);
		public Vec4 xxyz => new Vec4(X, X, Y, Z);
		public Vec4 xxyw => new Vec4(X, X, Y, W);
		public Vec4 xxzx => new Vec4(X, X, Z, X);
		public Vec4 xxzy => new Vec4(X, X, Z, Y);
		public Vec4 xxzz => new Vec4(X, X, Z, Z);
		public Vec4 xxzw => new Vec4(X, X, Z, W);
		public Vec4 xxwx => new Vec4(X, X, W, X);
		public Vec4 xxwy => new Vec4(X, X, W, Y);
		public Vec4 xxwz => new Vec4(X, X, W, Z);
		public Vec4 xxww => new Vec4(X, X, W, W);
		public Vec4 xyxx => new Vec4(X, Y, X, X);
		public Vec4 xyxy => new Vec4(X, Y, X, Y);
		public Vec4 xyxz => new Vec4(X, Y, X, Z);
		public Vec4 xyxw => new Vec4(X, Y, X, W);
		public Vec4 xyyx => new Vec4(X, Y, Y, X);
		public Vec4 xyyy => new Vec4(X, Y, Y, Y);
		public Vec4 xyyz => new Vec4(X, Y, Y, Z);
		public Vec4 xyyw => new Vec4(X, Y, Y, W);
		public Vec4 xyzx => new Vec4(X, Y, Z, X);
		public Vec4 xyzy => new Vec4(X, Y, Z, Y);
		public Vec4 xyzz => new Vec4(X, Y, Z, Z);
		public Vec4 xyzw => new Vec4(X, Y, Z, W);
		public Vec4 xywx => new Vec4(X, Y, W, X);
		public Vec4 xywy => new Vec4(X, Y, W, Y);
		public Vec4 xywz => new Vec4(X, Y, W, Z);
		public Vec4 xyww => new Vec4(X, Y, W, W);
		public Vec4 xzxx => new Vec4(X, Z, X, X);
		public Vec4 xzxy => new Vec4(X, Z, X, Y);
		public Vec4 xzxz => new Vec4(X, Z, X, Z);
		public Vec4 xzxw => new Vec4(X, Z, X, W);
		public Vec4 xzyx => new Vec4(X, Z, Y, X);
		public Vec4 xzyy => new Vec4(X, Z, Y, Y);
		public Vec4 xzyz => new Vec4(X, Z, Y, Z);
		public Vec4 xzyw => new Vec4(X, Z, Y, W);
		public Vec4 xzzx => new Vec4(X, Z, Z, X);
		public Vec4 xzzy => new Vec4(X, Z, Z, Y);
		public Vec4 xzzz => new Vec4(X, Z, Z, Z);
		public Vec4 xzzw => new Vec4(X, Z, Z, W);
		public Vec4 xzwx => new Vec4(X, Z, W, X);
		public Vec4 xzwy => new Vec4(X, Z, W, Y);
		public Vec4 xzwz => new Vec4(X, Z, W, Z);
		public Vec4 xzww => new Vec4(X, Z, W, W);
		public Vec4 xwxx => new Vec4(X, W, X, X);
		public Vec4 xwxy => new Vec4(X, W, X, Y);
		public Vec4 xwxz => new Vec4(X, W, X, Z);
		public Vec4 xwxw => new Vec4(X, W, X, W);
		public Vec4 xwyx => new Vec4(X, W, Y, X);
		public Vec4 xwyy => new Vec4(X, W, Y, Y);
		public Vec4 xwyz => new Vec4(X, W, Y, Z);
		public Vec4 xwyw => new Vec4(X, W, Y, W);
		public Vec4 xwzx => new Vec4(X, W, Z, X);
		public Vec4 xwzy => new Vec4(X, W, Z, Y);
		public Vec4 xwzz => new Vec4(X, W, Z, Z);
		public Vec4 xwzw => new Vec4(X, W, Z, W);
		public Vec4 xwwx => new Vec4(X, W, W, X);
		public Vec4 xwwy => new Vec4(X, W, W, Y);
		public Vec4 xwwz => new Vec4(X, W, W, Z);
		public Vec4 xwww => new Vec4(X, W, W, W);
		public Vec4 yxxx => new Vec4(Y, X, X, X);
		public Vec4 yxxy => new Vec4(Y, X, X, Y);
		public Vec4 yxxz => new Vec4(Y, X, X, Z);
		public Vec4 yxxw => new Vec4(Y, X, X, W);
		public Vec4 yxyx => new Vec4(Y, X, Y, X);
		public Vec4 yxyy => new Vec4(Y, X, Y, Y);
		public Vec4 yxyz => new Vec4(Y, X, Y, Z);
		public Vec4 yxyw => new Vec4(Y, X, Y, W);
		public Vec4 yxzx => new Vec4(Y, X, Z, X);
		public Vec4 yxzy => new Vec4(Y, X, Z, Y);
		public Vec4 yxzz => new Vec4(Y, X, Z, Z);
		public Vec4 yxzw => new Vec4(Y, X, Z, W);
		public Vec4 yxwx => new Vec4(Y, X, W, X);
		public Vec4 yxwy => new Vec4(Y, X, W, Y);
		public Vec4 yxwz => new Vec4(Y, X, W, Z);
		public Vec4 yxww => new Vec4(Y, X, W, W);
		public Vec4 yyxx => new Vec4(Y, Y, X, X);
		public Vec4 yyxy => new Vec4(Y, Y, X, Y);
		public Vec4 yyxz => new Vec4(Y, Y, X, Z);
		public Vec4 yyxw => new Vec4(Y, Y, X, W);
		public Vec4 yyyx => new Vec4(Y, Y, Y, X);
		public Vec4 yyyy => new Vec4(Y, Y, Y, Y);
		public Vec4 yyyz => new Vec4(Y, Y, Y, Z);
		public Vec4 yyyw => new Vec4(Y, Y, Y, W);
		public Vec4 yyzx => new Vec4(Y, Y, Z, X);
		public Vec4 yyzy => new Vec4(Y, Y, Z, Y);
		public Vec4 yyzz => new Vec4(Y, Y, Z, Z);
		public Vec4 yyzw => new Vec4(Y, Y, Z, W);
		public Vec4 yywx => new Vec4(Y, Y, W, X);
		public Vec4 yywy => new Vec4(Y, Y, W, Y);
		public Vec4 yywz => new Vec4(Y, Y, W, Z);
		public Vec4 yyww => new Vec4(Y, Y, W, W);
		public Vec4 yzxx => new Vec4(Y, Z, X, X);
		public Vec4 yzxy => new Vec4(Y, Z, X, Y);
		public Vec4 yzxz => new Vec4(Y, Z, X, Z);
		public Vec4 yzxw => new Vec4(Y, Z, X, W);
		public Vec4 yzyx => new Vec4(Y, Z, Y, X);
		public Vec4 yzyy => new Vec4(Y, Z, Y, Y);
		public Vec4 yzyz => new Vec4(Y, Z, Y, Z);
		public Vec4 yzyw => new Vec4(Y, Z, Y, W);
		public Vec4 yzzx => new Vec4(Y, Z, Z, X);
		public Vec4 yzzy => new Vec4(Y, Z, Z, Y);
		public Vec4 yzzz => new Vec4(Y, Z, Z, Z);
		public Vec4 yzzw => new Vec4(Y, Z, Z, W);
		public Vec4 yzwx => new Vec4(Y, Z, W, X);
		public Vec4 yzwy => new Vec4(Y, Z, W, Y);
		public Vec4 yzwz => new Vec4(Y, Z, W, Z);
		public Vec4 yzww => new Vec4(Y, Z, W, W);
		public Vec4 ywxx => new Vec4(Y, W, X, X);
		public Vec4 ywxy => new Vec4(Y, W, X, Y);
		public Vec4 ywxz => new Vec4(Y, W, X, Z);
		public Vec4 ywxw => new Vec4(Y, W, X, W);
		public Vec4 ywyx => new Vec4(Y, W, Y, X);
		public Vec4 ywyy => new Vec4(Y, W, Y, Y);
		public Vec4 ywyz => new Vec4(Y, W, Y, Z);
		public Vec4 ywyw => new Vec4(Y, W, Y, W);
		public Vec4 ywzx => new Vec4(Y, W, Z, X);
		public Vec4 ywzy => new Vec4(Y, W, Z, Y);
		public Vec4 ywzz => new Vec4(Y, W, Z, Z);
		public Vec4 ywzw => new Vec4(Y, W, Z, W);
		public Vec4 ywwx => new Vec4(Y, W, W, X);
		public Vec4 ywwy => new Vec4(Y, W, W, Y);
		public Vec4 ywwz => new Vec4(Y, W, W, Z);
		public Vec4 ywww => new Vec4(Y, W, W, W);
		public Vec4 zxxx => new Vec4(Z, X, X, X);
		public Vec4 zxxy => new Vec4(Z, X, X, Y);
		public Vec4 zxxz => new Vec4(Z, X, X, Z);
		public Vec4 zxxw => new Vec4(Z, X, X, W);
		public Vec4 zxyx => new Vec4(Z, X, Y, X);
		public Vec4 zxyy => new Vec4(Z, X, Y, Y);
		public Vec4 zxyz => new Vec4(Z, X, Y, Z);
		public Vec4 zxyw => new Vec4(Z, X, Y, W);
		public Vec4 zxzx => new Vec4(Z, X, Z, X);
		public Vec4 zxzy => new Vec4(Z, X, Z, Y);
		public Vec4 zxzz => new Vec4(Z, X, Z, Z);
		public Vec4 zxzw => new Vec4(Z, X, Z, W);
		public Vec4 zxwx => new Vec4(Z, X, W, X);
		public Vec4 zxwy => new Vec4(Z, X, W, Y);
		public Vec4 zxwz => new Vec4(Z, X, W, Z);
		public Vec4 zxww => new Vec4(Z, X, W, W);
		public Vec4 zyxx => new Vec4(Z, Y, X, X);
		public Vec4 zyxy => new Vec4(Z, Y, X, Y);
		public Vec4 zyxz => new Vec4(Z, Y, X, Z);
		public Vec4 zyxw => new Vec4(Z, Y, X, W);
		public Vec4 zyyx => new Vec4(Z, Y, Y, X);
		public Vec4 zyyy => new Vec4(Z, Y, Y, Y);
		public Vec4 zyyz => new Vec4(Z, Y, Y, Z);
		public Vec4 zyyw => new Vec4(Z, Y, Y, W);
		public Vec4 zyzx => new Vec4(Z, Y, Z, X);
		public Vec4 zyzy => new Vec4(Z, Y, Z, Y);
		public Vec4 zyzz => new Vec4(Z, Y, Z, Z);
		public Vec4 zyzw => new Vec4(Z, Y, Z, W);
		public Vec4 zywx => new Vec4(Z, Y, W, X);
		public Vec4 zywy => new Vec4(Z, Y, W, Y);
		public Vec4 zywz => new Vec4(Z, Y, W, Z);
		public Vec4 zyww => new Vec4(Z, Y, W, W);
		public Vec4 zzxx => new Vec4(Z, Z, X, X);
		public Vec4 zzxy => new Vec4(Z, Z, X, Y);
		public Vec4 zzxz => new Vec4(Z, Z, X, Z);
		public Vec4 zzxw => new Vec4(Z, Z, X, W);
		public Vec4 zzyx => new Vec4(Z, Z, Y, X);
		public Vec4 zzyy => new Vec4(Z, Z, Y, Y);
		public Vec4 zzyz => new Vec4(Z, Z, Y, Z);
		public Vec4 zzyw => new Vec4(Z, Z, Y, W);
		public Vec4 zzzx => new Vec4(Z, Z, Z, X);
		public Vec4 zzzy => new Vec4(Z, Z, Z, Y);
		public Vec4 zzzz => new Vec4(Z, Z, Z, Z);
		public Vec4 zzzw => new Vec4(Z, Z, Z, W);
		public Vec4 zzwx => new Vec4(Z, Z, W, X);
		public Vec4 zzwy => new Vec4(Z, Z, W, Y);
		public Vec4 zzwz => new Vec4(Z, Z, W, Z);
		public Vec4 zzww => new Vec4(Z, Z, W, W);
		public Vec4 zwxx => new Vec4(Z, W, X, X);
		public Vec4 zwxy => new Vec4(Z, W, X, Y);
		public Vec4 zwxz => new Vec4(Z, W, X, Z);
		public Vec4 zwxw => new Vec4(Z, W, X, W);
		public Vec4 zwyx => new Vec4(Z, W, Y, X);
		public Vec4 zwyy => new Vec4(Z, W, Y, Y);
		public Vec4 zwyz => new Vec4(Z, W, Y, Z);
		public Vec4 zwyw => new Vec4(Z, W, Y, W);
		public Vec4 zwzx => new Vec4(Z, W, Z, X);
		public Vec4 zwzy => new Vec4(Z, W, Z, Y);
		public Vec4 zwzz => new Vec4(Z, W, Z, Z);
		public Vec4 zwzw => new Vec4(Z, W, Z, W);
		public Vec4 zwwx => new Vec4(Z, W, W, X);
		public Vec4 zwwy => new Vec4(Z, W, W, Y);
		public Vec4 zwwz => new Vec4(Z, W, W, Z);
		public Vec4 zwww => new Vec4(Z, W, W, W);
		public Vec4 wxxx => new Vec4(W, X, X, X);
		public Vec4 wxxy => new Vec4(W, X, X, Y);
		public Vec4 wxxz => new Vec4(W, X, X, Z);
		public Vec4 wxxw => new Vec4(W, X, X, W);
		public Vec4 wxyx => new Vec4(W, X, Y, X);
		public Vec4 wxyy => new Vec4(W, X, Y, Y);
		public Vec4 wxyz => new Vec4(W, X, Y, Z);
		public Vec4 wxyw => new Vec4(W, X, Y, W);
		public Vec4 wxzx => new Vec4(W, X, Z, X);
		public Vec4 wxzy => new Vec4(W, X, Z, Y);
		public Vec4 wxzz => new Vec4(W, X, Z, Z);
		public Vec4 wxzw => new Vec4(W, X, Z, W);
		public Vec4 wxwx => new Vec4(W, X, W, X);
		public Vec4 wxwy => new Vec4(W, X, W, Y);
		public Vec4 wxwz => new Vec4(W, X, W, Z);
		public Vec4 wxww => new Vec4(W, X, W, W);
		public Vec4 wyxx => new Vec4(W, Y, X, X);
		public Vec4 wyxy => new Vec4(W, Y, X, Y);
		public Vec4 wyxz => new Vec4(W, Y, X, Z);
		public Vec4 wyxw => new Vec4(W, Y, X, W);
		public Vec4 wyyx => new Vec4(W, Y, Y, X);
		public Vec4 wyyy => new Vec4(W, Y, Y, Y);
		public Vec4 wyyz => new Vec4(W, Y, Y, Z);
		public Vec4 wyyw => new Vec4(W, Y, Y, W);
		public Vec4 wyzx => new Vec4(W, Y, Z, X);
		public Vec4 wyzy => new Vec4(W, Y, Z, Y);
		public Vec4 wyzz => new Vec4(W, Y, Z, Z);
		public Vec4 wyzw => new Vec4(W, Y, Z, W);
		public Vec4 wywx => new Vec4(W, Y, W, X);
		public Vec4 wywy => new Vec4(W, Y, W, Y);
		public Vec4 wywz => new Vec4(W, Y, W, Z);
		public Vec4 wyww => new Vec4(W, Y, W, W);
		public Vec4 wzxx => new Vec4(W, Z, X, X);
		public Vec4 wzxy => new Vec4(W, Z, X, Y);
		public Vec4 wzxz => new Vec4(W, Z, X, Z);
		public Vec4 wzxw => new Vec4(W, Z, X, W);
		public Vec4 wzyx => new Vec4(W, Z, Y, X);
		public Vec4 wzyy => new Vec4(W, Z, Y, Y);
		public Vec4 wzyz => new Vec4(W, Z, Y, Z);
		public Vec4 wzyw => new Vec4(W, Z, Y, W);
		public Vec4 wzzx => new Vec4(W, Z, Z, X);
		public Vec4 wzzy => new Vec4(W, Z, Z, Y);
		public Vec4 wzzz => new Vec4(W, Z, Z, Z);
		public Vec4 wzzw => new Vec4(W, Z, Z, W);
		public Vec4 wzwx => new Vec4(W, Z, W, X);
		public Vec4 wzwy => new Vec4(W, Z, W, Y);
		public Vec4 wzwz => new Vec4(W, Z, W, Z);
		public Vec4 wzww => new Vec4(W, Z, W, W);
		public Vec4 wwxx => new Vec4(W, W, X, X);
		public Vec4 wwxy => new Vec4(W, W, X, Y);
		public Vec4 wwxz => new Vec4(W, W, X, Z);
		public Vec4 wwxw => new Vec4(W, W, X, W);
		public Vec4 wwyx => new Vec4(W, W, Y, X);
		public Vec4 wwyy => new Vec4(W, W, Y, Y);
		public Vec4 wwyz => new Vec4(W, W, Y, Z);
		public Vec4 wwyw => new Vec4(W, W, Y, W);
		public Vec4 wwzx => new Vec4(W, W, Z, X);
		public Vec4 wwzy => new Vec4(W, W, Z, Y);
		public Vec4 wwzz => new Vec4(W, W, Z, Z);
		public Vec4 wwzw => new Vec4(W, W, Z, W);
		public Vec4 wwwx => new Vec4(W, W, W, X);
		public Vec4 wwwy => new Vec4(W, W, W, Y);
		public Vec4 wwwz => new Vec4(W, W, W, Z);
		public Vec4 wwww => new Vec4(W, W, W, W);
	}
}