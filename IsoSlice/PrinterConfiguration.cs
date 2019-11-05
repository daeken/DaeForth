using DaeForth;

namespace IsoSlice {
	public class PrinterConfiguration {
		public bool HasHeatedBed, HasHeatedBuildVolume;
		public Vec3 BuildVolume;
		public Vec3 BuildOrigin = Vec3.Zero;

		public float NozzleSize = 0.4f;
		public float FilamentDiameter = 1.75f;
		public Vec2 NozzleOffset = Vec2.Zero;
	}
}