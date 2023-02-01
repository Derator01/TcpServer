using System.Net;
using System.Net.Sockets;
using TcpServer.Common;

namespace TcpServer.ServerSide;

public class Client
{
    /// <summary>
    /// Note: Must be the same on Client side.
    /// </summary>
    private const int PACKET_SIZE = 1024;

    public bool Connected { get => _tcpClient.Connected; }

    public string Name { get; set; }
    public EndPoint? EndPoint { get => _tcpClient.Client.RemoteEndPoint; }

    private readonly TcpClient _tcpClient;

    private readonly NetworkStream _stream;

    public Client(TcpClient tcpClient, string name)
    {
        _tcpClient = tcpClient;
        _stream = _tcpClient.GetStream();
        Name = name;
    }

    public bool IsMessagePending() => _stream.DataAvailable;

    /// <summary>
    /// No nullchecking.
    /// </summary>
    internal Message RecieveMessage()
    {
        byte[] buffer = new byte[PACKET_SIZE];
        _stream.Read(buffer, 0, buffer.Length);

        return new Message(buffer);
    }

    /// <param name="message">Text length must be less than <see cref="PACKET_SIZE"/></param>
    internal void SendMessage(Message message)
    {
        if (!Connected)
            return;

        _stream.Write(message.ToBytes());
    }

    internal void Disconnect()
    {
        if (!Connected)
            return;

        _stream.Close();
        _tcpClient.Close();
    }
}