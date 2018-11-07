using System;
using System.Collections.Generic;

namespace Core.Game
{
    public enum ChessId
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
        public abstract ChessId Id { get; }

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
        public override ChessId Id => ChessId.Common;
    }

    public class ChessKey : ChessType
    {
        public override string Name => "Key";
        public override ChessId Id => ChessId.Key;

        protected override void DoProcess(Game game)
        {
            game.GetCurrentOperator().Surrender();
        }
    }

    public class ChessFlip : ChessType
    {
        public override string Name => "Flip";
        public override ChessId Id => ChessId.Flip;

        protected override void DoProcess(Game game)
        {
            var oldBoard = game.Board;
            var newBoard = new ChessBoard(oldBoard.SizeRight, oldBoard.SizeLeft);
            for (var i = 0; i < oldBoard.SizeLeft; ++i)
            for (var j = 0; j < oldBoard.SizeRight; ++j)
                newBoard[j, i] = oldBoard[i, j];
            game.Board = newBoard;
            game.GetCurrentOperator().EndRound();
        }
    }

    public class ChessAddCol : ChessType
    {
        public override string Name => "AddCol";
        public override ChessId Id => ChessId.AddCol;

        protected override void DoProcess(Game game)
        {
            switch (game.CurrentOperatorIdentity)
            {
                case Game.OperatorIdentity.Left:
                    game.Board.ResizeDelta(1, 0);
                    break;
                case Game.OperatorIdentity.Right: 
                    game.Board.ResizeDelta(0, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ChessDelCol : ChessType
    {
        public override string Name => "DelCol";
        public override ChessId Id => ChessId.DelCol;

        protected override void DoProcess(Game game)
        {
            switch (game.CurrentOperatorIdentity)
            {
                case Game.OperatorIdentity.Left:
                    game.Board.ResizeDelta(-1, 0);
                    break;
                case Game.OperatorIdentity.Right: 
                    game.Board.ResizeDelta(0, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public static class ChessTypes
    {
        private static readonly Dictionary<ChessId, ChessType> Types = new Dictionary<ChessId, ChessType>();

        static ChessTypes()
        {
            Register(new ChessCommon());
            Register(new ChessKey());
            Register(new ChessFlip());
            Register(new ChessAddCol());
            Register(new ChessDelCol());
        }

        public static ChessType Get(ChessId id)
        {
            return Types[id];
        }

        private static void Register(ChessType type)
        {
            Types.Add(type.Id, type);
        }
    }
}