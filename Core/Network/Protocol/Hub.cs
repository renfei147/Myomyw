using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace Core.Network.Protocol
{
    public class Hub
    {
        private readonly ReaderWriterLockSlim AccessLock = new ReaderWriterLockSlim();

        private readonly Dictionary<string, int> IndexTable = new Dictionary<string, int>();

        private readonly List<IProtocol> Protocols = new List<IProtocol>();

        private EndPoint _currentEndPoint;

        private bool _dominant;

        public IProtocol Get(string actionName)
        {
            return Protocols[IndexTable[actionName]];
        }

        public IProtocol Get(int name)
        {
            return Protocols[name];
        }

        public void Add<T>(T protocol) where T : Group<T>
        {
            if (!_dominant)
            {
                var pp = protocol.GetClientProtocol();
                var id = InfoFetch.Query(_currentEndPoint, pp.Name);
                AccessLock.EnterWriteLock();
                Protocols.Capacity = Math.Max(Protocols.Capacity, id);
                Protocols[id] = pp;
                IndexTable[pp.Name] = id;
                protocol.SetId(id);
                AccessLock.ExitWriteLock();
            }
            else
            {
                var pp = protocol.GetServerProtocol();
                AccessLock.EnterWriteLock();
                var id = Protocols.Count;
                Protocols.Add(pp);
                IndexTable[pp.Name] = id;
                protocol.SetId(id);
                AccessLock.ExitWriteLock();
            }
        }

        public void SetCurrent(EndPoint io, bool dominant)
        {
            if (_currentEndPoint != null || _dominant)
            {
                if (dominant) Add(new InfoFetch(this));
                _dominant = dominant;
                _currentEndPoint = io;
            }
            else
            {
                throw new Exception("Multi Current in One Session is not allowed");
            }
        }

        public void LockAccess()
        {
            AccessLock.EnterReadLock();
        }

        public void ReleaseAccess()
        {
            AccessLock.ExitReadLock();
        }

        private class InfoFetch : Group<InfoFetch>
        {
            private static readonly int IdLen = MessagePackSerializer.Serialize(0).Length;

            private static readonly ConcurrentDictionary<string, TaskCompletionSource<int>> Sessions =
                new ConcurrentDictionary<string, TaskCompletionSource<int>>();

            private readonly Hub _protocolHub;

            static InfoFetch()
            {
                Name = "<Internal>.Core.Network.Protocol.Hub.InfoFetch";
            }

            public InfoFetch(Hub hub)
            {
                _protocolHub = hub;
            }

            public static int Query(EndPoint io, string name)
            {
                var complete = Sessions.GetOrAdd(name, new TaskCompletionSource<int>()).Task;
                var nameOut = MessagePackSerializer.Serialize(name);
                io.BeginRequest(Id);
                io.WriteByte((byte) nameOut.Length);
                io.Send(nameOut);
                io.EndRequest();
                return complete.Result;
            }

            public override IProtocol GetServerProtocol()
            {
                return new Server(_protocolHub);
            }

            public override IProtocol GetClientProtocol()
            {
                return new Client();
            }

            private class Server : ProtocolBase
            {
                private readonly Hub _hub;

                public Server(Hub hub)
                {
                    _hub = hub;
                }

                public override void Handle(EndPoint io)
                {
                    var length = io.ReadByte();
                    var nameIn = new byte[length];
                    io.Receive(nameIn);
                    var name = MessagePackSerializer.Deserialize<string>(nameIn);
                    var id = _hub.IndexTable[name];
                    io.BeginRequest(Id);
                    io.Send(nameIn);
                    io.Send(MessagePackSerializer.SerializeUnsafe(id));
                    io.EndRequest();
                }
            }

            private class Client : ProtocolBase
            {
                public override void Handle(EndPoint io)
                {
                    var length = io.ReadByte();
                    var nameIn = new byte[length + IdLen];
                    io.Receive(nameIn);
                    var name = MessagePackSerializer.Deserialize<string>(nameIn);
                    var id = MessagePackSerializer.Deserialize<int>(new ArraySegment<byte>(nameIn, length, IdLen));
                    Sessions[name].SetResult(id);
                }
            }
        }
    }
}