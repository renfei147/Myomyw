namespace Core.Network.Protocol
{
    public interface IProtocol
    {
        string Name { get; }
        int Id { get; }
        void Handle(EndPoint io);
    }
}