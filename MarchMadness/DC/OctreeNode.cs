using DaeForth;

namespace MarchMadness.DC {
    public class OctreeNode {
        public OctreeNodeType Type;
        public Vec3 min, max;
        public double radius;
        public int size;
        public OctreeNode[] children;
        public OctreeDrawInfo drawInfo;

        public OctreeNode() {
            Type = OctreeNodeType.Node_None;
            min = Vec3.Zero;
            size = 0;
            drawInfo = new OctreeDrawInfo();

            children = new OctreeNode[8];
            for(var i = 0; i < 8; i++) {
                children[i] = null;
            }
        }

        public OctreeNode(OctreeNodeType _type) {
            Type = _type;
            min = Vec3.Zero;
            size = 0;
            drawInfo = new OctreeDrawInfo();

            children = new OctreeNode[8];
            for(var i = 0; i < 8; i++) {
                children[i] = null;
            }
        }
    }
}