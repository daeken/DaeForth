using System.IO;

namespace IsoSlice {
	public static class Extensions {
		public static void WriteBe(this BinaryWriter bw, int _value) {
			unchecked {
				var value = (uint) _value;
				bw.Write((byte) (value >> 24));
				bw.Write((byte) (value >> 16));
				bw.Write((byte) (value >> 8));
				bw.Write((byte) value);
			}
		}
		
		public static void WriteBe(this BinaryWriter bw, uint value) {
			bw.Write((byte) (value >> 24));
			bw.Write((byte) (value >> 16));
			bw.Write((byte) (value >> 8));
			bw.Write((byte) value);
		}
	}
}