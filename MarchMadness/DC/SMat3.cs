namespace MarchMadness.DC {
    public class SMat3 {
        public double m00, m01, m02, m11, m12, m22;

        public SMat3() {
            clear();
        }

        public SMat3(double m00, double m01, double m02, double m11, double m12, double m22) {
            setSymmetric(m00, m01, m02, m11, m12, m22);
        }

        public void clear() {
            setSymmetric(0, 0, 0, 0, 0, 0);
        }

        public void setSymmetric(double a00, double a01, double a02, double a11, double a12, double a22) {
            m00 = a00;
            m01 = a01;
            m02 = a02;
            m11 = a11;
            m12 = a12;
            m22 = a22;
        }

        public void setSymmetric(SMat3 rhs) {
            setSymmetric(rhs.m00, rhs.m01, rhs.m02, rhs.m11, rhs.m12, rhs.m22);
        }

        SMat3(SMat3 rhs) {
            setSymmetric(rhs);
        }
    }
}