using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Network;
using Core.Network.Protocol;

namespace Core.Game.Operator
{
    public class ServerSlaveProtocol : Group<ServerSlaveProtocol>
    {
        static ServerSlaveProtocol()
        {
            Name = "Core.Game.ServerSlave";
        }

        private class Server : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
                var idLow = io.ReadByte();
                var idHigh = io.ReadByte();
                var id = idHigh << 8 | idLow;
                var function = io.ReadByte();
                var operand = io.ReadByte();
                io.BeginRequest(Id);
                io.WriteByte((byte) (_hosts[id].Execute(function, operand) ? 1 : 0));
                io.EndRequest();
            }

            private readonly List<ServerSlave> _hosts = new List<ServerSlave>();
        }

        public class Client : ProtocolBase
        {
            public override void Handle(EndPoint io)
            {
                _completion.SetResult((byte) io.ReadByte());
            }

            public bool Request(Network.Client client, int operatorId, byte function, byte operand)
            {
                _completion = new TaskCompletionSource<byte>();
                var end = _completion.Task;
                var idLow = (byte) operatorId;
                var idHigh = (byte) (operatorId >> 8);
                client.BeginRequest(Id);
                client.WriteByte(idLow);
                client.WriteByte(idHigh);
                client.WriteByte(function);
                client.WriteByte(operand);
                client.EndRequest();
                return end.Result > 0;
            }

            private TaskCompletionSource<byte> _completion;
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
    }

    public class ClientMaster : Client
    {
        private readonly ServerSlaveProtocol.Client _protocol;
        public int RoomId { get; set; }
        public int OperatorId { get; set; }

        public ClientMaster(Game game, Network.Client client) : base(game, client)
        {
            _protocol = ServerSlaveProtocol.GetClient(client);
        }
        
        public override Game.OperatorIdentity Identity { get; set; }

        public override bool BeginRound(byte pad)
        {
            var success = _protocol.Request(Connection, OperatorId, 0, pad);
            if (success)
                base.BeginRound(pad);
            return success;
        }

        public override bool EndRound()
        {
            var success = _protocol.Request(Connection, OperatorId, 1, 0);
            if (success)
                base.EndRound();
            return success;
        }

        public override bool PushBall()
        {
            var success = _protocol.Request(Connection, OperatorId, 2, 0);
            if (success)
                base.PushBall();
            return success;
        }

        public override bool Surrender()
        {
            var success = _protocol.Request(Connection, OperatorId, 3, 0);
            if (success)
                base.Surrender();
            return success;
        }
    }

    public class ServerSlave : Server
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

        public ServerSlave(Game game, EndPoint endPoint) : base(game, endPoint)
        {
        }
    }
}