using System;
using System.Collections.Generic;

namespace Core.Game
{
    public enum ChessTypeName
    {
        Common = 0,
        Key = 1,
        Flip = 2,
        AddCol = 3,
        DelCol = 4
    }

    public abstract class ChessType
    {
        public delegate void OnOperationEvent();

        public abstract string Name { get; }
        public abstract ChessTypeName Type { get; }

        public void Process(Game game)
        {
            BeforeChessProcessed?.Invoke();
            DoProcess(game);
            AfterChessProcessed?.Invoke();
        }

        protected virtual void DoProcess(Game game)
        {
        }

        public event OnOperationEvent BeforeChessProcessed;
        public event OnOperationEvent AfterChessProcessed;
    }

    public class ChessCommon : ChessType
    {
        public override string Name => "Common";
        public override ChessTypeName Type => ChessTypeName.Common;
    }

    public class ChessKey : ChessType
    {
        public override string Name => "Key";
        public override ChessTypeName Type => ChessTypeName.Key;
    }

    public class ChessFlip : ChessType
    {
        public override string Name => "Flip";
        public override ChessTypeName Type => ChessTypeName.Flip;

        protected override void DoProcess(Game game)
        {
            var oldBoard = game.Board;
            var newBoard = new ChessBoard(oldBoard.SizeRight, oldBoard.SizeLeft);
            for (var i = 0; i < oldBoard.SizeLeft; ++i)
            for (var j = 0; j < oldBoard.SizeRight; ++j)
                newBoard[j, i] = oldBoard[i, j];
            game.Board = newBoard;
        }
    }

    public class ChessAddCol : ChessType
    {
        public override string Name => "AddCol";
        public override ChessTypeName Type => ChessTypeName.AddCol;

        protected override void DoProcess(Game game)
        {
            game.Board.ResizeDelta(1, 0);
        }
    }

    public class ChessDelCol : ChessType
    {
        public override string Name => "DelCol";
        public override ChessTypeName Type => ChessTypeName.DelCol;

        protected override void DoProcess(Game game)
        {
            game.Board.ResizeDelta(-1, 0);
        }
    }

    public static class ChessTypeManager
    {
        private static readonly Dictionary<ChessTypeName, ChessType> Types = new Dictionary<ChessTypeName, ChessType>();

        static ChessTypeManager()
        {
            Register(new ChessCommon());
            Register(new ChessKey());
            Register(new ChessFlip());
            Register(new ChessAddCol());
            Register(new ChessDelCol());
        }

        public static ChessType Get(ChessTypeName name)
        {
            return Types[name];
        }

        private static void Register(ChessType type)
        {
            Types.Add(type.Type, type);
        }
    }

    public class Game
    {
        public ChessBoard Board { get; set; }
    }

    public class ChessBoard
    {
        private ChessTypeName[] _board;

        public ChessBoard() : this(3, 3)
        {
        }

        public ChessBoard(int sizeLeft, int sizeRight)
        {
            Resize(sizeLeft, sizeRight);
        }

        public int SizeLeft { get; private set; }

        public int SizeRight { get; private set; }

        public ChessTypeName this[int left, int right]
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
            var board = new ChessTypeName[newSizeLeft * newSizeRight];
            for (var i = 0; i < Math.Min(SizeLeft, newSizeLeft); ++i)
            for (var j = 0; j < Math.Min(SizeRight, newSizeRight); ++j)
                board[i * newSizeLeft + j] = this[i, j];
            _board = board;
            SizeLeft = newSizeLeft;
            SizeRight = newSizeRight;
        }
    }
}