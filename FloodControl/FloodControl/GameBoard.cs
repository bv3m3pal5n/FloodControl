using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FloodControl
{
    class GameBoard
    {
        public Dictionary<string, FallingPiece> FallingPieces = new Dictionary<string, FallingPiece>();
        public Dictionary<string, RotatingPiece> RotatingPieces = new Dictionary<string, RotatingPiece>();
        public Dictionary<string, FadingPiece> FadingPieces = new Dictionary<string, FadingPiece>();


        Random rand = new Random();

        public const int GameBoardWidth = 8;
        public const int GameBoardHeight = 10;

        private GamePiece[,] boardSquares = new GamePiece[GameBoardWidth, GameBoardHeight];

        private List<Vector2> WaterTracker = new List<Vector2>();

        public GameBoard()
        {
            ClearBoard();
        }

        public void ClearBoard()
        {
            for (int x = 0; x < GameBoardWidth; x++)
                for (int y = 0; y < GameBoardHeight; y++)
                    boardSquares[x, y] = new GamePiece("Empty");
        }

        public void RotatePiece(int x, int y, bool clockwise)
        {
            boardSquares[x, y].RotatePiece(clockwise);
        }

        public Rectangle GetSourceRect(int x, int y)
        {
            return boardSquares[x, y].GetSourceRect();
        }

        public string GetSquare(int x, int y)
        {
            return boardSquares[x, y].PieceType;
        }

        public void SetSquare(int x, int y, string pieceName)
        {
            boardSquares[x, y].SetPiece(pieceName);
        }

        public bool HasConnector(int x, int y, string direction)
        {
            return boardSquares[x, y].HasConnector(direction);
        }

        public void RandomPiece(int x, int y)
        {
            boardSquares[x, y].SetPiece(GamePiece.PieceTypes[rand.Next(0, GamePiece.MaxPlayablePieceIndex + 1)]);
        }

        public void FillFromAbove(int x, int y)
        {
            int rowLookup = y - 1;
            while (rowLookup >= 0)
            {
                if (GetSquare(x, rowLookup) != "Empty")
                {
                    SetSquare(x, y, GetSquare(x, rowLookup));
                    SetSquare(x, rowLookup, "Empty");
                    AddFallingPiece(x, y, GetSquare(x, y), GamePiece.PieceHeight * (y - rowLookup));
                    rowLookup = -1;
                }
                rowLookup--;
            }
               
        }

        public void GenerateNewPieces(bool dropSquares)
        {
            if (dropSquares)
            {
                for (int x = 0; x < GameBoard.GameBoardWidth; x++)
                {
                    for (int y = GameBoard.GameBoardHeight - 1; y >= 0; y--)
                    {
                        if (GetSquare(x, y) == "Empty")
                        {
                            FillFromAbove(x, y);
                        }
                    }
                }

            }

            for (int y=0; y<GameBoard.GameBoardHeight; y++)
                for (int x = 0; x < GameBoard.GameBoardWidth; x++)
                {
                    if (GetSquare(x, y) == "Empty")
                    {
                        RandomPiece(x, y);
                        AddFallingPiece(x, y, GetSquare(x, y), GamePiece.PieceHeight * GameBoardHeight);
                    }

                }
        }

        public void ResetWater()
        {
            for (int y = 0; y < GameBoardHeight; y++)
                for (int x = 0; x < GameBoardWidth; x++)
                    boardSquares[x, y].RemoveSuffix("W");
        }

        public void FillPiece(int x, int y)
        {
            boardSquares[x, y].AddSuffix("W");
        }

        public void PropagateWater(int x, int y, string fromDirection)
        {
            if ((y >= 0) && (y < GameBoardHeight) && (x >= 0) && (x < GameBoardWidth))
            {
                if (boardSquares[x, y].HasConnector(fromDirection) && !boardSquares[x, y].Suffix.Contains("W"))
                {
                    FillPiece(x, y);
                    WaterTracker.Add(new Vector2(x, y));
                    foreach(string end in boardSquares[x,y].GetOtherEnds(fromDirection))
                        switch (end)
                        {
                            case "Left": PropagateWater(x - 1, y, "Right"); break;
                            case "Right": PropagateWater(x + 1, y, "Left"); break;
                            case "Top": PropagateWater(x, y - 1, "Bottom"); break;
                            case "Bottom": PropagateWater(x, y + 1, "Top"); break;
                        }
                }
            }
        }

        public List<Vector2> GetWaterChain(int y)
        {
            WaterTracker.Clear();
            PropagateWater(0, y, "Left");
            return WaterTracker;
        }

        public void AddFallingPiece(int x, int y, string pieceName, int verticalOffset)
        {
            FallingPieces[x.ToString() + "_" + y.ToString()] = new FallingPiece(pieceName, verticalOffset);
        }

        public void AddRotatingPiece(int x, int y, string pieceName, bool clockwise)
        {
            RotatingPieces[x.ToString() + "_" + y.ToString()] = new RotatingPiece(pieceName, clockwise);
        }

        public void AddFadingPiece(int x, int y,string pieceName)
        {
            FadingPieces[x.ToString() + "_" + y.ToString()] = new FadingPiece(pieceName, "W");
        }

        public bool ArePiecesAnimating()
        {
            if ((FallingPieces.Count == 0) && (RotatingPieces.Count == 0) && (FadingPieces.Count == 0))
                return false;
            else return true;
        }

        void updateFadingPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach (string thisKey in FadingPieces.Keys)
            {
                FadingPieces[thisKey].UpdatePiece();

                if (FadingPieces[thisKey].AlphaLevel == 0.0f)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                FadingPieces.Remove(RemoveKeys.Dequeue());
        }

        void updateFallingPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach (string thisKey in FallingPieces.Keys)
            {
                FallingPieces[thisKey].UpdatePiece();

                if (FallingPieces[thisKey].VerticalOffset == 0)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                FallingPieces.Remove(RemoveKeys.Dequeue());
        }

        void updateRotatingPieces()
        {
            Queue<string> RemoveKeys = new Queue<string>();

            foreach (string thisKey in RotatingPieces.Keys)
            {
                RotatingPieces[thisKey].UpdatePiece();

                if (RotatingPieces[thisKey].rotationTicksRemaining == 0)
                    RemoveKeys.Enqueue(thisKey.ToString());
            }
            while (RemoveKeys.Count > 0)
                RotatingPieces.Remove(RemoveKeys.Dequeue());
        }

        public void UpdateAnimatedPieces()
        {
            if (FadingPieces.Count == 0)
            {
                updateFallingPieces();
                updateRotatingPieces();
            }
            else updateFadingPieces();
        }


    }
}
