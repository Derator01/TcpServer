using System.Text;

namespace TcpServer.Common;

public record Message
{
    public string Text { get; init; }
    public MessageHandling.MessageType MessageType { get; init; }

    public Message(byte[] arr)
    {
        MessageType = (MessageHandling.MessageType)arr[0];

        Text = Encoding.UTF8.GetString(arr, 1, arr.Length - 1);
    }
    public Message(byte[] arr, MessageHandling.MessageType messageType)
    {
        MessageType = messageType;

        Text = Encoding.UTF8.GetString(arr);
    }
    public Message(string rawMessage, MessageHandling.MessageType messageType)
    {
        Text = rawMessage;
        MessageType = messageType;
    }

    public byte[] ToBytes()
    {
        byte[] output = new byte[Text.Length + 1];

        output[0] = (byte)MessageType;
        Encoding.UTF8.GetBytes(Text).CopyTo(output, 1);

        return output;
    }
}

public static class MessageHandling
{
    public enum MessageType : byte
    {
        HandShake = 0x01,
        Message = 0x02,
        Command = 0x03,
        Image = 0x04
    }
}