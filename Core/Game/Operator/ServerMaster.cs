using Core.Network;
using Core.Network.Protocol;

namespace Core.Game.Operator
{
    public class ServerMasterProtocol : Group<ServerMasterProtocol>
    {
        static ServerMasterProtocol()
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

        private class Client : ProtocolBase
        {
            public ClientSlave Slave;

            public override void Handle(EndPoint io)
            {
                var function = io.ReadByte();
                var operand = io.ReadByte();
                Slave.Execute(function, operand);
            }
        }

        private class Server : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
            }
        }

        public void Request(EndPoint client, byte function, byte operand)
        {
            client.BeginRequest(Id);
            client.WriteByte(function);
            client.WriteByte(operand);
            client.EndRequest();
        }
    }

    public class ServerMaster : Server
    {
        public ServerMaster(Game game, EndPoint endPoint) : base(game, endPoint)
        {
        }
    }

    public class ClientSlave : Client
    {
        protected ClientSlave(Game game, Network.Client connection) : base(game, connection)
        {
        }

        public bool Execute(int function, int operand)
        {
            switch (function)
            {
                case 0: return BeginRound((byte) operand);
                case 1: return EndRound();
                case 2: return PushBall();
                case 3:
                    Surrender();
                    return true;
                default:
                    return false;
            }
        }
    }
}