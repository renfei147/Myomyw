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

        private Operator.Operator LeftOperator;

        private Operator.Operator RightOperator;

        public OperatorIdentity CurrentOperatorIdentity { get; protected set; }

        public ChessBoard Board { get; set; }

        private bool GameEnd { get; set; }

        public Operator.Operator GetCurrentOperator()
        {
            switch (CurrentOperatorIdentity)
            {
                case OperatorIdentity.Left: return LeftOperator;
                case OperatorIdentity.Right: return RightOperator;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Operator.Operator GetNextOperator()
        {
            switch (CurrentOperatorIdentity)
            {
                case OperatorIdentity.Left: return RightOperator;
                case OperatorIdentity.Right: return LeftOperator;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual bool BeginCurrentRound()
        {
            return !GameEnd;
        }

        public virtual void StartGame()
        {
            CurrentOperatorIdentity = OperatorIdentity.Left;
            GetCurrentOperator().OnGameStart();
            GetNextOperator().OnGameStart();
            RoundTimerDoOnStart();
        }

        public virtual bool EndCurrentRound()
        {
            if (!GameEnd)
            {
                RoundTimerDoOnExpire();
                CurrentOperatorIdentity = CurrentOperatorIdentity == OperatorIdentity.Left
                    ? OperatorIdentity.Right
                    : OperatorIdentity.Left;
                RoundTimerDoOnStart();
            }
            else
            {
                RoundTimerDoOnExpire();
            }

            return true;
        }

        public virtual bool PushBall(int currentPad)
        {
            if (!GameEnd)
            {
                switch (CurrentOperatorIdentity)
                {
                    case OperatorIdentity.Left:
                        PushLeft(currentPad);
                        break;
                    case OperatorIdentity.Right:
                        PushRight(currentPad);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return true;
            }

            return false;
        }

        public virtual bool Surrender(OperatorIdentity identity)
        {
            if (!GameEnd)
            {
                GameEnd = true;
                EndCurrentRound();
                CurrentOperatorIdentity = identity;
                GetNextOperator().OnGameEnd(true);
                GetCurrentOperator().OnGameEnd(false);

                return true;
            }

            return false;
        }

        protected virtual void RoundTimerDoOnStart()
        {
            GetCurrentOperator().OnSelfRoundTimerStart();
            GetNextOperator().OnOpponentRoundTimerStart();
        }

        protected virtual void RoundTimerDoOnExpire()
        {
            GetCurrentOperator().OnSelfRoundTimerExpire();
            GetNextOperator().OnOpponentRoundTimerExpire();
        }

        private void PushLeft(int pad)
        {
            var chessOut = Board[pad, Board.SizeRight - 1];
            for (var right = Board.SizeRight - 1; right > 0; --right)
                Board[pad, right] = Board[pad, right - 1];
            ChessTypes.Get(chessOut).Process(this);
        }

        private void PushRight(int pad)
        {
            var chessOut = Board[Board.SizeLeft - 1, pad];
            for (var left = Board.SizeLeft - 1; left > 0; --left)
                Board[left, pad] = Board[left - 1, pad];
            ChessTypes.Get(chessOut).Process(this);
        }
    }

    public class GameLocal : Game
    {
    }

    public class GameRemote : Game
    {
    }
}