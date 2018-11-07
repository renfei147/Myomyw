using System;

namespace Core.Game
{
    public class Game
    {
        public enum OperatorIdentity
        {
            Left,
            Right
        }

        public OperatorIdentity CurrentOperatorIdentity { get; protected set; }
        public ChessBoard Board { get; set; }
        public Operator.Operator LeftOperator;
        public Operator.Operator RightOperator;

        private Operator.Operator GetCurrentOperator()
        {
            switch (CurrentOperatorIdentity)
            {
                case OperatorIdentity.Left: return LeftOperator;
                case OperatorIdentity.Right: return RightOperator;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Operator.Operator GetNextOperator()
        {
            switch (CurrentOperatorIdentity)
            {
                case OperatorIdentity.Left: return RightOperator;
                case OperatorIdentity.Right: return LeftOperator;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool BeginCurrentRound()
        {
            throw new System.NotImplementedException();
        }

        public bool EndCurrentRound()
        {
            throw new System.NotImplementedException();
        }

        public bool PushBall(int currentPad)
        {
            throw new System.NotImplementedException();
        }

        public bool Surrender(OperatorIdentity identity)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GameLocal : Game
    {
    }

    public class GameRemote : Game
    {
    }
}