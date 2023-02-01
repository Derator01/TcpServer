using System.Net;
using System.Net.Sockets;
using TcpServer.Common;

namespace TcpServer.ClientSide;

public class Client
{
    /// <summary>
    /// Note: Must be the same on Server side.
    /// </summary>
    protected const int PACKET_SIZE = 1024;

    public bool Connected { get => _tcpClient.Connected; }

    /// <summary>
    /// Optional name of the client which would be saved on server side.
    /// </summary>
    public string Name { get; }
    public IPEndPoint EndPoint { get; }

    private readonly TcpClient _tcpClient;

    public delegate void OnMessage(MessageEventArgs e);
    public event OnMessage? MessageCame;

    public Client(IPEndPoint ipEndPoint, string name)
    {
        _tcpClient = new();
        EndPoint = ipEndPoint;
        Name = name;
    }
    public Client(IPAddress ipAddress, int port, string name)
    {
        _tcpClient = new();
        EndPoint = new(ipAddress, port);
        Name = name;
    }

    public void Connect()
    {
        if (Connected)
            return;

        try
        {
            _tcpClient.Connect(EndPoint);
            Console.WriteLine("Helllalrst");
            SendHandShakeMessage();

            new Thread(ClientCheckForMessageLoop).Start();
        }
        catch
        {
            _tcpClient.Dispose();
        }
    }

    private void SendHandShakeMessage() => SendMessage(new Message(Name, MessageHandling.MessageType.HandShake));

    public bool IsMessagePending() => _tcpClient.GetStream().DataAvailable;

    private Message RecieveMessage()
    {
        var stream = _tcpClient.GetStream();

        byte[] buffer = new byte[PACKET_SIZE];
        stream.Read(buffer, 0, PACKET_SIZE);

        return new Message(buffer);
    }

    /// <param name="message">Must be less than <see cref="PACKET_SIZE"/></param>
    public void SendMessage(Message message)
    {
        if (!Connected)
            return;

        using var stream = _tcpClient.GetStream();

        stream.Write(message.ToBytes());
    }

    private void ClientCheckForMessageLoop(object? obj)
    {
        while (Connected)
        {
            CheckForMessages();

            Thread.Sleep(100);
        }
    }

    private void CheckForMessages()
    {
        if (_tcpClient.Available > 0)
            return;
        MessageCame?.Invoke(new MessageEventArgs(RecieveMessage()));
    }
}