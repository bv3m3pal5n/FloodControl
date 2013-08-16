using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FloodControl
{
    class FadingPiece:GamePiece
    {
        public float AlphaLevel = 1.0f;
        public static float AlphaChangeRate = 0.02f;

        public FadingPiece(string pieceType, string suffix)
            : base(pieceType, suffix)
        {
        }

        public void UpdatePiece()
        {
            AlphaLevel = MathHelper.Max(0, AlphaLevel - AlphaChangeRate);
        }


    }
}
