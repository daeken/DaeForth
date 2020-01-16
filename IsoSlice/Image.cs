using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Force.Crc32;
using Ionic.Zlib;

namespace IsoSlice {
	public enum ColorMode {
		Greyscale,
		Rgb, 
		Rgba
	}
	
	public class Image {
		public readonly ColorMode ColorMode;
		public readonly (int Width, int Height) Size;
		public readonly byte[] Data;
		public readonly int Stride, PixelBytes;
		public readonly string Name;
		
		public Image(ColorMode colorMode, (int Width, int Height) size, byte[] data, string name = null) {
			ColorMode = colorMode;
			Size = size;
			Data = data;
			PixelBytes = PixelSize(ColorMode);
			Stride = Size.Width * PixelBytes;
			Debug.Assert(Data.Length == PixelBytes * Size.Width * Size.Height);
			Name = name;
		}

		public Image(ColorMode colorMode, (int Width, int Height) size, uint[] data, string name = null) {
			ColorMode = colorMode;
			Size = size;
			Debug.Assert(data.Length == Size.Width * Size.Height);
			Data = new byte[size.Width * size.Height * PixelSize(colorMode)];
			Buffer.BlockCopy(data, 0, Data, 0, 4 * size.Width * size.Height);
			Name = name;
		}

		public static int PixelSize(ColorMode mode) {
			switch(mode) {
				case ColorMode.Greyscale: return 1;
				case ColorMode.Rgb: return 3;
				case ColorMode.Rgba: return 4;
				default: throw new NotImplementedException();
			}
		}
	}

	public static class Png {
		public static void Encode(Image image, Stream stream) {
			var bw = new BinaryWriter(stream, Encoding.Default, leaveOpen: true);
			bw.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

			void WriteChunk(string type, byte[] data) {
				Debug.Assert(type.Length == 4);
				bw.WriteBe(data.Length);
				var td = Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
				bw.Write(td);
				bw.WriteBe(Crc32Algorithm.Compute(td));
			}

			void Chunk(string type, Action<BinaryWriter> func) {
				using(var ms = new MemoryStream())
					using(var sbw = new BinaryWriter(ms)) {
						func(sbw);
						sbw.Flush();
						WriteChunk(type, ms.ToArray());
					}
			}
			
			Chunk("IHDR", w => {
				w.WriteBe(image.Size.Width);
				w.WriteBe(image.Size.Height);
				w.Write((byte) 8);
				switch(image.ColorMode) {
					case ColorMode.Greyscale: w.Write((byte) 0); break;
					case ColorMode.Rgb: w.Write((byte) 2); break;
					case ColorMode.Rgba: w.Write((byte) 6); break;
					default: throw new NotImplementedException();
				}
				w.Write((byte) 0); // Compression mode
				w.Write((byte) 0); // Filter
				w.Write((byte) 0); // Interlace
			});

			var ps = Image.PixelSize(image.ColorMode);
			var stride = image.Size.Width * ps;
			var imem = new byte[image.Size.Height + image.Size.Width * image.Size.Height * ps]; // One byte per scanline for filter (0)
			for(var y = 0; y < image.Size.Height; ++y)
				Array.Copy(image.Data, y * stride, imem, y * stride + y + 1, stride);
			using(var ms = new MemoryStream()) {
				using(var ds = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.Default, leaveOpen: true)) {
					ds.Write(imem, 0, imem.Length);
					ds.Flush();
				}
				ms.Flush();
				WriteChunk("IDAT", ms.ToArray());
			}

			Chunk("IEND", w => { });
			bw.Flush();
		}
	}
}