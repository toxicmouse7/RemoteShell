using RemoteShellServer;

await new RemoteShell("0.0.0.0", 5555).Listen();