using System.Net.Sockets;
using System.Text;
using System;

namespace Extensions;

public static class TcpClientExtensions
{
    private static byte[] Receive(this TcpClient client, int bytesToReceive)
    {
        var response = new byte[bytesToReceive];
        var stream = client.GetStream();
        var bytesReceived = 0;
        do
        {
            bytesReceived = stream.Read(response, bytesReceived, bytesToReceive - bytesReceived);
        } while (bytesReceived < bytesToReceive);

        return response;
    }
    
    private static async Task<byte[]> ReceiveAsync(this TcpClient client, int bytesToReceive)
    {
        var response = new byte[bytesToReceive];
        var stream = client.GetStream();
        var bytesReceived = 0;
        do
        {
            bytesReceived = await stream.ReadAsync(response.AsMemory(bytesReceived, bytesToReceive - bytesReceived));
        } while (bytesReceived < bytesToReceive);

        return response;
    }

    public static void Send(this TcpClient client, string message)
    {
        var stream = client.GetStream();
        var sendBuffer = Encoding.UTF8.GetBytes(message);
        var sendBufferSize = BitConverter.GetBytes(sendBuffer.Length);
        stream.Write(sendBufferSize);
        stream.Write(sendBuffer);
    }
    
    public static async Task SendAsync(this TcpClient client, string message)
    {
        var stream = client.GetStream();
        var sendBuffer = Encoding.UTF8.GetBytes(message);
        var sendBufferSize = BitConverter.GetBytes(sendBuffer.Length);
        await stream.WriteAsync(sendBufferSize);
        await stream.WriteAsync(sendBuffer);
        await stream.FlushAsync();
    }
    
    public static string ReceiveMessage(this TcpClient tcpClient)
    {
        var messageSizeBuffer = tcpClient.Receive(4);
        var messageSize = BitConverter.ToInt32(messageSizeBuffer);
        var messageBuffer = tcpClient.Receive(messageSize);
        return Encoding.UTF8.GetString(messageBuffer);
    }
    
    public static async Task<string> ReceiveMessageAsync(this TcpClient tcpClient)
    {
        var messageSizeBuffer = await tcpClient.ReceiveAsync(4);
        var messageSize = BitConverter.ToInt32(messageSizeBuffer);
        var messageBuffer = await tcpClient.ReceiveAsync(messageSize);
        return Encoding.UTF8.GetString(messageBuffer);
    }
}