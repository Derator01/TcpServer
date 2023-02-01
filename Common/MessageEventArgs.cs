namespace TcpServer.Common;

public class MessageEventArgs : EventArgs
{
    public string RawMessage { get; }
    public MessageHandling.MessageType MessageType { get; }

    public MessageEventArgs(Message message)
    {
        RawMessage = message.Text;
        MessageType = message.MessageType;
    }
}