using System;

namespace Core.Game
{
    public class ChessBoard
    {
        private ChessId[] _board;

        public ChessBoard() : this(3, 3)
        {
        }

        public ChessBoard(int sizeLeft, int sizeRight)
        {
            Resize(sizeLeft, sizeRight);
        }

        public int SizeLeft { get; private set; }

        public int SizeRight { get; private set; }

        public ChessId this[int left, int right]
        {
            get => _board[left * SizeLeft + right];
            set => _board[left * SizeLeft + right] = value;
        }

        public void ResizeDelta(int deltaLeft, int deltaRight)
        {
            Resize(SizeLeft + deltaLeft, SizeRight + deltaRight);
        }

        private void Resize(int newSizeLeft, int newSizeRight)
        {
            var board = new ChessId[newSizeLeft * newSizeRight];
            for (var i = 0; i < Math.Min(SizeLeft, newSizeLeft); ++i)
            for (var j = 0; j < Math.Min(SizeRight, newSizeRight); ++j)
                board[i * newSizeLeft + j] = this[i, j];
            _board = board;
            SizeLeft = newSizeLeft;
            SizeRight = newSizeRight;
        }
    }
}