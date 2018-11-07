using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Network.Protocol;

namespace Core.Network
{
    public abstract class EndPoint
    {
        private static readonly Mutex AccessLock = new Mutex();
        public abstract void Send(ArraySegment<byte> message);

        public void Send(byte[] message)
        {
            Send(new ArraySegment<byte>(message));
        }

        public abstract void Receive(ArraySegment<byte> message);

        public void Receive(byte[] message)
        {
            ReceiveAsync(new ArraySegment<byte>(message));
        }

        public abstract Task ReceiveAsync(ArraySegment<byte> message);

        public Task ReceiveAsync(byte[] message)
        {
            return ReceiveAsync(new ArraySegment<byte>(message));
        }

        public abstract int ReadByte();

        public abstract void WriteByte(byte val);

        private void SendRequestHeader(int protocol)
        {
            Send(new[]
            {
                (byte) 'N', (byte) 'W', (byte) 'R', (byte) 'C',
                (byte) (protocol >> 24),
                (byte) ((protocol >> 16) & 0xFF),
                (byte) ((protocol >> 18) & 0xFF),
                (byte) (protocol & 0xFF)
            });
        }

        public void BeginRequest(int protocol)
        {
            AccessLock.WaitOne();
            SendRequestHeader(protocol);
        }

        public bool TryBeginRequest(int protocol)
        {
            var success = AccessLock.WaitOne(0);
            if (success)
                SendRequestHeader(protocol);
            return success;
        }

        public void EndRequest()
        {
            AccessLock.ReleaseMutex();
        }
    }

    public class EndPointStable : EndPoint
    {
        protected readonly Hub _hub;
        private readonly Stream _stream;

        protected EndPointStable(Stream stream, Hub hub)
        {
            _stream = stream;
            _hub = hub;
        }

        public override void Send(ArraySegment<byte> message)
        {
            if (message.Array != null) _stream.Write(message.Array, message.Offset, message.Count);
        }

        public override void Receive(ArraySegment<byte> message)
        {
            if (message.Array == null) return;
            var read = message.Offset;
            while (read < message.Count)
                read += _stream.Read(message.Array, read, message.Count - read);
        }

        public override async Task ReceiveAsync(ArraySegment<byte> message)
        {
            if (message.Array == null) return;
            var read = message.Offset;
            while (read < message.Count)
                read += await _stream.ReadAsync(message.Array, read, message.Count - read);
        }

        public override void WriteByte(byte val)
        {
            _stream.WriteByte(val);
        }

        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public virtual void Close()
        {
            _stream.Close();
        }

        protected async Task ListenAsyncImpl()
        {
            var headerCache = new byte[8]; // ["NWRC"] + Int32BE(Protocol Id)
            while (_stream.CanRead)
                try
                {
                    await ReceiveAsync(headerCache);
                    if (VerifyPackageValidity(headerCache))
                        try
                        {
                            _hub.LockAccess();
                            var protocol = _hub.Get(GetProtocolId(headerCache));
                            _hub.ReleaseAccess();
                            protocol.Handle(this);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    else
                        Console.WriteLine("Bad Package Received");
                }
                catch (IOException)
                {
                    return;
                }
        }

        private static int GetProtocolId(byte[] head)
        {
            return (head[4] << 24) | (head[5] << 16) | (head[6] << 8) | head[7];
        }

        private static bool VerifyPackageValidity(byte[] head)
        {
            return head[0] == 'N' && head[1] == 'W' && head[2] == 'R' && head[3] == 'C';
        }
    }

    public class TcpEndPoint : EndPointStable
    {
        private bool _valid = true;

        protected TcpEndPoint(TcpClient client, Hub hub) : base(client.GetStream(), hub)
        {
        }

        protected async Task ListenAsync()
        {
            try
            {
                await ListenAsyncImpl();
            }
            finally
            {
                Close();
            }
        }

        public override void Close()
        {
            if (!_valid) return;
            _valid = false;
            base.Close(); // Cancellation Token Doesn't Work. Hard Close is adopted.
        }

        public bool Valid()
        {
            return _valid;
        }
    }

    public class Client : TcpEndPoint
    {
        public Client(string address, int port) : base(new TcpClient(address, port), new Hub())
        {
        }

        public void Add<T>(T protocol) where T : Group<T>
        {
            _hub.Add(protocol);
        }

        public T Get<T>(string actionName) where T : class, IProtocol
        {
            return _hub.Get(actionName) as T;
        }
    }
}