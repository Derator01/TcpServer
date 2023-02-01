using System.Net;
using System.Net.Sockets;
using TcpServer.Common;

namespace TcpServer.ServerSide;

public class Server
{
    public bool IsRunning { get; set; }

    public readonly IPEndPoint ServerEndPoint;
    protected readonly TcpListener Listener;

    private readonly List<Client> _clients = new();

    public delegate void OnClientConnected(Client client);
    public event OnClientConnected? ClientConnected;

    public delegate void OnMessage(Client sender, MessageEventArgs args);
    public event OnMessage? MessageCame;

    public delegate void OnClientDisconnected(Client client);
    public event OnClientDisconnected? ClientDisconnected;
    public Server(IPAddress ip, int port)
    {
        ServerEndPoint = new IPEndPoint(ip, port);

        Listener = new TcpListener(ServerEndPoint);
    }
    public Server(IPEndPoint serverEndPoint)
    {
        ServerEndPoint = serverEndPoint;

        Listener = new TcpListener(ServerEndPoint);
    }

    public void Start()
    {
        if (IsRunning)
            return;

        try
        {
            Listener.Start();

            IsRunning = true;

            new Thread(ClientAcceptLoop).Start();
            new Thread(MessageRecieveLoop).Start();
        }
        catch
        {
            IsRunning = false;
            Listener.Stop();
        }
    }

    public bool HasAnyClients() => _clients.Count > 0;

    protected void ClientAcceptLoop()
    {
        while (IsRunning)
        {
            if (Listener.Pending())
                OnAccept();

            Thread.Sleep(500);
        }
    }

    /// <summary>
    /// When called it is guarantied that <see cref="Listener"/> has incoming connection.
    /// </summary>
    protected virtual void OnAccept()
    {
        Client client = new(Listener.AcceptTcpClient(), $"TEMPORARY {new Random().Next()}");
        _clients.Add(client);

        ClientConnected?.Invoke(client);
    }

    private void MessageRecieveLoop()
    {
        while (IsRunning)
        {
            CheckForIncomingMessagesLoop();

            Thread.Sleep(100);
        }
    }

    private void CheckForIncomingMessagesLoop()
    {
        foreach (var client in from client in _clients
                               where client.IsMessagePending()
                               select client)
        {
            HandleMessage(client);
        }
    }

    private void HandleMessage(Client client)
    {
        var message = client.RecieveMessage();

        switch (message.MessageType)
        {
            case MessageHandling.MessageType.Message:
                MessageCame?.Invoke(client, new MessageEventArgs(message));
                break;
            case MessageHandling.MessageType.Command:
                OnCommandPacket(client, message);
                break;
            case MessageHandling.MessageType.HandShake:
                client.Name = message.Text.Trim();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void OnCommandPacket(Client sender, Message message)
    {
        switch (message.Text)
        {
            case "Disconnect":
                sender.Disconnect();
                break;
        }
    }

    public virtual void SendMessage(Message message, string name)
    {
        if (name is null)
            return;

        _clients.Where(client => client.Name == name).FirstOrDefault()?.SendMessage(message);
    }
    public virtual void SendMessage(Message message, Func<Client, bool> predicate)
    {
        _clients.Where(predicate).FirstOrDefault()?.SendMessage(message);
    }
    public virtual void SendMessageToEveryone(Message message)
    {
        foreach (var client in _clients)
        {
            client.SendMessage(message);
        }
    }

    public void Disconnect()
    {
        foreach (var client in _clients)
        {
            client.Disconnect();
        }

        Listener.Stop();
    }
    public void Disconnect(string name)
    {
        _clients.Where(client => client.Name == name).FirstOrDefault()?.Disconnect();
    }
}