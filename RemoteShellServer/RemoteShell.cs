using System.Net;
using System.Net.Sockets;
using Extensions;

namespace RemoteShellServer;

public class RemoteShell
{
    private readonly TcpListener _tcpListener;
    private CancellationTokenSource _cts = new();

    public RemoteShell(string ip, int port)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
    }

    public async Task Listen()
    {
        _tcpListener.Start();

        var tcpClient = await _tcpListener.AcceptTcpClientAsync();
        await ServeTcpClient(tcpClient, _cts.Token);
    }

    private static async Task ServeTcpClient(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _ = Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = tcpClient.ReceiveMessage();
                Console.WriteLine(message);
            }
        }, cancellationToken);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = Console.ReadLine()!;
            await tcpClient.SendAsync(message);
        }
    }
}