using System;

namespace Core.Network.Protocol
{
    public abstract class Group<T> where T : Group<T>
    {
        private static T _internalInstance;
        protected static string Name { get; set; }

        protected static int Id { get; set; }

        public static T Instance => _internalInstance ?? (_internalInstance = Activator.CreateInstance<T>());

        public void SetId(int id)
        {
            Id = id;
        }

        public abstract IProtocol GetServerProtocol();

        public abstract IProtocol GetClientProtocol();

        protected abstract class ProtocolBase : IProtocol
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public string Name => Group<T>.Name;
            public int Id => Group<T>.Id;
            public abstract void Handle(EndPoint io);
        }
    }
}