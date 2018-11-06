// 
// Core: Server.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2018 NEWorld Team
// 
// NEWorld is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// NEWorld is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General 
// Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with NEWorld.  If not, see <http://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Network.Protocol;

namespace Core.Network
{
    public class Server
    {
        private readonly Hub _hub = new Hub();
        private readonly TcpListener _listener;

        private List<SEndPoint> _clients;

        private int _deadEndPoint;

        private bool _exit;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        private void AddDeadEndPoint()
        {
            Interlocked.Increment(ref _deadEndPoint);
        }

        public void Run()
        {
            Boot();
            ListenConnections().Wait();
            ShutDown();
        }

        public async Task RunAsync()
        {
            Boot();
            await ListenConnections();
            ShutDown();
        }

        public void Add<T>(T protocol) where T : Group<T>
        {
            _hub.Add(protocol);
        }

        public void Stop()
        {
            _exit = true;
        }

        public int Count()
        {
            return _clients.Count - _deadEndPoint;
        }

        private void Boot()
        {
            _exit = false;
            _listener.Start();
        }

        private void ShutDown()
        {
            _listener.Stop();
        }

        private async Task ListenConnections()
        {
            while (!_exit)
            {
                try
                {
                    _clients.Add(new SEndPoint(this, await _listener.AcceptTcpClientAsync(), _hub));
                }
                catch
                {
                    // ignored
                }

                SweepInvalidConnectionsIfNecessary();
            }

            CloseAll();
        }

        private void CloseAll()
        {
            foreach (var client in _clients) client.Close();
        }

        private void SweepInvalidConnectionsIfNecessary()
        {
            if (_deadEndPoint <= 100) return;
            var swap = new List<SEndPoint>();
            foreach (var client in _clients)
                if (client.Valid())
                    swap.Add(client);
            _clients = swap;
        }

        private class SEndPoint : TcpEndPoint
        {
            private readonly Server _srv;

            public SEndPoint(Server srv, TcpClient client, Hub hub) : base(client, hub)
            {
                _srv = srv;
            }

            public override void Close()
            {
                base.Close();
                _srv.AddDeadEndPoint();
            }
        }
    }
}