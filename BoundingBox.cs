using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeX2img {
    struct BoundingBox {
        private decimal left, right, bottom, top;
        public decimal Left { get { return left; } }
        public decimal Right { get { return right; } }
        public decimal Bottom { get { return bottom; } }
        public decimal Top { get { return top; } }
        public decimal Width { get { return right - left; } }
        public decimal Height { get { return top - bottom; } }
        public BoundingBox(decimal l, decimal b, decimal r, decimal t) {
            left = l; right = r; bottom = b; top = t;
        }
        public bool IsEmpty { get { return Width <= 0 || Height <= 0; } }
        public BoundingBox HiresBBToBB() {
            int ileft = (int)left, iright = (int)right, ibottom = (int)bottom, itop = (int)top;
            if ((decimal)itop != top) ++itop;
            if ((decimal)iright != right) ++iright;
            return new BoundingBox(ileft, ibottom, iright, itop);
        }
        public void Translate(decimal x,decimal y) {
            left -= x;right -= x;
            top -= y;bottom -= y;
        }
        public override string ToString() {
            return "%%BoundingBox: " + Left + " " + Bottom + " " + Right + " " + Top;
        }
    };

    class BoundingBoxPair {
        public BoundingBox bb, hiresbb;
        public BoundingBoxPair() { }
        public BoundingBoxPair(BoundingBox b, BoundingBox h) {
            bb = b; hiresbb = h;
        }
    }
}
