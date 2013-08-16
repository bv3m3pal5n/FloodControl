using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FloodControl
{
    class FallingPiece:GamePiece
    {
        public int VerticalOffset;
        public static int FallRate = 5;

        public FallingPiece(string pieceType, int verticalOffset):base(pieceType)
        {
            VerticalOffset = verticalOffset;
        }

        public void UpdatePiece()
        {
            VerticalOffset = (int)MathHelper.Max(0, VerticalOffset - FallRate);
        }

    }
}
