using System.Runtime.InteropServices;

namespace RemoteShellClient;

public static class SimulatePcControl
{

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);
    
    private const int VkReturn = 0x0D;

    public static void Enter()
    {
        keybd_event(VkReturn, 0, 0, 0);
    }

}