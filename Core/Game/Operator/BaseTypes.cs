using System;
using Core.Network;
using Core.Network.Protocol;

namespace Core.Game.Operator
{
    public abstract class Operator
    {
        public delegate void GameEndEvent(bool win);

        public delegate void GameStartEvent();

        public delegate void OpponentRoundTimerExpireEvent();

        public delegate void OpponentRoundTimerStartEvent();

        public delegate void SelfRoundTimerExpireEvent();

        public delegate void SelfRoundTimerStartEvent();

        protected readonly Game CurrentGame;

        protected Operator(Game currentGame)
        {
            CurrentGame = currentGame;
        }

        public abstract Game.OperatorIdentity Identity { get; set; }

        public event GameStartEvent GameStarts;
        public event SelfRoundTimerStartEvent SelfRoundTimerStarted;
        public event SelfRoundTimerExpireEvent SelfRoundTimerExpired;
        public event OpponentRoundTimerStartEvent OpponentRoundTimerStarted;
        public event OpponentRoundTimerExpireEvent OpponentRoundTimerExpired;
        public event GameEndEvent GameEnded;
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
        private int _currentPad, _pushedBallCount;
        private bool _roundStarted;

        protected Local(Game currentGame) : base(currentGame)
        {
        }

        public override Game.OperatorIdentity Identity { get; set; }

        public override bool BeginRound(byte pad)
        {
            if (CurrentGame.CurrentOperatorIdentity == Identity)
            {
                _currentPad = pad;
                _pushedBallCount = 0;
                return CurrentGame.BeginCurrentRound();
            }

            return false;
        }

        public override bool EndRound()
        {
            if (_roundStarted) return CurrentGame.EndCurrentRound();

            return false;
        }

        public override bool PushBall()
        {
            if (_roundStarted && _pushedBallCount++ < 5) return CurrentGame.PushBall(_currentPad);

            if (_pushedBallCount == 5) return EndRound();

            return false;
        }

        public override bool Surrender()
        {
            return CurrentGame.Surrender(Identity);
        }

        public override void OnSelfRoundTimerExpire()
        {
            _roundStarted = false;
            base.OnSelfRoundTimerExpire();
        }
    }

    public class ServerNotificationProtocol : Group<ServerNotificationProtocol>
    {
        static ServerNotificationProtocol()
        {
            Name = "Core.Game.ServerMaster";
        }

        public override IProtocol GetServerProtocol()
        {
            return new Server();
        }

        public override IProtocol GetClientProtocol()
        {
            return new Client();
        }

        public static Client GetClient(Network.Client client)
        {
            return client.Get<Client>(Name);
        }

        public static void Request(EndPoint client, byte function, byte operand)
        {
            client.BeginRequest(Id);
            client.WriteByte(function);
            client.WriteByte(operand);
            client.EndRequest();
        }

        public class Client : ProtocolBase
        {
            public Core.Game.Operator.Client Slave;

            public override void Handle(EndPoint io)
            {
                var function = io.ReadByte();
                var operand = io.ReadByte();
                Slave.Notify(function, operand);
            }
        }

        public class Server : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
            }
        }
    }

    public class Server : Local
    {
        private readonly EndPoint _endPoint;

        public Server(Game game, EndPoint endPoint) : base(game)
        {
            _endPoint = endPoint;
        }

        public override void OnGameStart()
        {
            base.OnGameStart();
            ServerNotificationProtocol.Request(_endPoint, 0, 0);
        }

        public override void OnSelfRoundTimerStart()
        {
            base.OnSelfRoundTimerStart();
            ServerNotificationProtocol.Request(_endPoint, 1, 0);
        }

        public override void OnSelfRoundTimerExpire()
        {
            base.OnSelfRoundTimerExpire();
            ServerNotificationProtocol.Request(_endPoint, 2, 0);
        }

        public override void OnOpponentRoundTimerStart()
        {
            base.OnOpponentRoundTimerStart();
            ServerNotificationProtocol.Request(_endPoint, 3, 0);
        }

        public override void OnOpponentRoundTimerExpire()
        {
            base.OnOpponentRoundTimerExpire();
            ServerNotificationProtocol.Request(_endPoint, 4, 0);
        }

        public override void OnGameEnd(bool win)
        {
            base.OnGameEnd(win);
            ServerNotificationProtocol.Request(_endPoint, 5, (byte) (win ? 1 : 0));
        }
    }

    public class Client : Operator
    {
        protected readonly Network.Client Connection;
        private int _currentPad;

        protected Client(Game game, Network.Client connection) : base(game)
        {
            Connection = connection;
            ServerNotificationProtocol.GetClient(Connection).Slave = this;
        }

        public override Game.OperatorIdentity Identity { get; set; }

        public override bool BeginRound(byte pad)
        {
            _currentPad = pad;
            return CurrentGame.BeginCurrentRound();
        }

        public override bool EndRound()
        {
            return CurrentGame.EndCurrentRound();
        }

        public override bool PushBall()
        {
            return CurrentGame.PushBall(_currentPad);
        }

        public override bool Surrender()
        {
            return CurrentGame.Surrender(Identity);
        }

        public void Notify(int message, int operand)
        {
            switch (message)
            {
                case 0:
                    OnGameStart();
                    break;
                case 1:
                    OnSelfRoundTimerStart();
                    break;
                case 2:
                    OnSelfRoundTimerExpire();
                    break;
                case 3:
                    OnOpponentRoundTimerStart();
                    break;
                case 4:
                    OnOpponentRoundTimerExpire();
                    break;
                case 5:
                    OnGameEnd(operand != 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}