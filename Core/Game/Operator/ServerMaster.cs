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

        private class Client : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
                var function = io.ReadByte();
                var operand = io.ReadByte();
                io.BeginRequest(Id);
                io.WriteByte((byte) (Slave.Execute(function, operand) ? 1 : 0));
                io.EndRequest();
            }

            public ClientSlave Slave;
        }

        public class Server : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
            }

            public void Request(Network.Client client, int operatorId, byte function, byte operand)
            {
                client.BeginRequest(Id);
                client.WriteByte(function);
                client.WriteByte(operand);
                client.EndRequest();
            }
        }

        public override IProtocol GetServerProtocol()
        {
            return new Server();
        }

        public override IProtocol GetClientProtocol()
        {
            return new Client();
        }
    }

    public class ServerMaster : Local
    {
        
    }

    public class ClientSlave : Remote
    {
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