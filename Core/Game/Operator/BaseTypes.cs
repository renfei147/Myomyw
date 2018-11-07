namespace Core.Game.Operator
{
    public abstract class Operator
    {
        public delegate void GameStartEvent();
        public delegate void SelfRoundTimerStartEvent();
        public delegate void SelfRoundTimerExpireEvent();
        public delegate void OpponentRoundTimerStartEvent();
        public delegate void OpponentRoundTimerExpireEvent();
        public delegate void GameEndEvent(bool win);

        public event GameStartEvent GameStarts;
        public event SelfRoundTimerStartEvent SelfRoundTimerStarted;
        public event SelfRoundTimerExpireEvent SelfRoundTimerExpired;
        public event OpponentRoundTimerStartEvent OpponentRoundTimerStarted;
        public event OpponentRoundTimerExpireEvent OpponentRoundTimerExpired;
        public event GameEndEvent GameEnded;
        
        public abstract Game.OperatorIdentity Identity { get; set; }
        public abstract bool BeginRound(byte pad);
        public abstract bool EndRound();
        public abstract bool PushBall();
        public abstract bool Surrender();

        public virtual void OnGameStart()
        {
            GameStarts?.Invoke();
        }

        public virtual void OnSelfRoundTimerStart()
        {
            SelfRoundTimerStarted?.Invoke();
        }

        public virtual void OnSelfRoundTimerExpire()
        {
            SelfRoundTimerExpired?.Invoke();
        }

        public virtual void OnOpponentRoundTimerStart()
        {
            OpponentRoundTimerStarted?.Invoke();
        }

        public virtual void OnOpponentRoundTimerExpire()
        {
            OpponentRoundTimerExpired?.Invoke();
        }

        public virtual void OnGameEnd(bool win)
        {
            GameEnded?.Invoke(win);
        }
    }

    public class Local : Operator
    {
        public Game CurrentGame;
        private bool RoundStarted;
        private int CurrentPad;
        public override Game.OperatorIdentity Identity { get; set; }
        public override bool BeginRound(byte pad)
        {
            if (CurrentGame.CurrentOperatorIdentity == Identity)
            {
                CurrentPad = pad;
                return CurrentGame.BeginCurrentRound();
            }
            
            return false;
        }

        public override bool EndRound()
        {
            if (RoundStarted)
            {
                RoundStarted = false;
                return CurrentGame.EndCurrentRound();
            }

            return false;
        }

        public override bool PushBall()
        {
            if (RoundStarted)
            {
                return CurrentGame.PushBall(CurrentPad);
            }

            return false;
        }

        public override bool Surrender()
        {
            return CurrentGame.Surrender(Identity);
        }
    }
    
    public class Remote : Operator
    {
        public Game CurrentGame;
        private int CurrentPad;
        public override Game.OperatorIdentity Identity { get; set; }
        public override bool BeginRound(byte pad)
        {
            CurrentPad = pad;
            return CurrentGame.BeginCurrentRound();
        }

        public override bool EndRound()
        {
            return CurrentGame.EndCurrentRound();
        }

        public override bool PushBall()
        {
            return CurrentGame.PushBall(CurrentPad);
        }

        public override bool Surrender()
        {
            return CurrentGame.Surrender(Identity);
        }
    }
}