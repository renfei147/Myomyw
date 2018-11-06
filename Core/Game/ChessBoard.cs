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

        protected ChessType(string name, ChessTypeName type)
        {
            Name = name;
            Type = type;
        }

        public ChessTypeName Type { get; }
        public string Name { get; }

        public virtual void Process(ChessBoard board)
        {
            OnOperation?.Invoke();
        }

        public event OnOperationEvent OnOperation;

        private delegate void Operation();
    }

    public class ChessCommon : ChessType
    {
        public ChessCommon() : base("Common", ChessTypeName.Common)
        {
        }
    }

    public class ChessKey : ChessType
    {
        public ChessKey() : base("Key", ChessTypeName.Key)
        {
        }

        public override void Process(ChessBoard board)
        {
            base.Process(board);
        }
    }

    public class ChessFlip : ChessType
    {
        public ChessFlip() : base("Flip", ChessTypeName.Flip)
        {
        }

        public override void Process(ChessBoard board)
        {
            base.Process(board);
            var board1 = new ChessTypeName[board.SizeLeft * board.SizeRight];
            for (var i = 0; i < board.SizeLeft; ++i)
            for (var j = 0; j < board.SizeRight; ++j)
                board1[j * board.SizeRight + i] = board.GetChess(i, j);
            var tempSize = board.SizeRight;
            board.SizeRight = board.SizeLeft;
            board.SizeLeft = tempSize;
            board.Board = board1;
        }
    }

    public class ChessAddCol : ChessType
    {
        public ChessAddCol() : base("AddCol", ChessTypeName.AddCol)
        {
        }

        public override void Process(ChessBoard board)
        {
            base.Process(board);
            board.ResizeBoard(board.SizeLeft + 1, board.SizeRight);
        }
    }

    public class ChessDelCol : ChessType
    {
        public ChessDelCol() : base("DelCol", ChessTypeName.DelCol)
        {
        }

        public override void Process(ChessBoard board)
        {
            base.Process(board);
            board.ResizeBoard(board.SizeLeft - 1, board.SizeRight);
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

    public class ChessBoard
    {
        public ChessTypeName[] Board;

        public ChessBoard()
        {
            ResizeBoard(3, 3);
        }

        public static ChessBoard Current { get; private set; }

        public int SizeLeft { get; set; }

        public int SizeRight { get; set; }

        public void MakeCurrent()
        {
            Current = this;
        }

        private void ProcessExtraChess(ChessTypeName typeName)
        {
            ChessTypeManager.Get(typeName).Process(this);
        }

        public ChessTypeName GetChess(int left, int right)
        {
            return Board[left * SizeLeft + right];
        }

        public void SetChess(ChessTypeName chess, int left, int right)
        {
            Board[left * SizeLeft + right] = chess;
        }

        public void ResizeBoard(int newSizeLeft, int newSizeRight)
        {
            var board = new ChessTypeName[newSizeLeft * newSizeRight];
            for (var i = 0; i < Math.Min(SizeLeft, newSizeLeft); ++i)
            for (var j = 0; j < Math.Min(SizeRight, newSizeRight); ++j)
                board[i * newSizeLeft + j] = GetChess(i, j);
            Board = board;
            SizeLeft = newSizeLeft;
            SizeRight = newSizeRight;
        }
    }
}