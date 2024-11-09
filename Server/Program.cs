using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static Socket listener;
    private static List<Socket> clientSockets = new List<Socket>();
    private static int port = 5000;
    private static string fullAccessClient = null;
    private static object lockObj = new object();

    // Define base directories for logs and client files
    private static readonly string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\");
    private static readonly string logsDirectory = Path.Combine(baseDirectory, "Logs");
    private static readonly string clientFilesDirectory = Path.Combine(baseDirectory, "ClientFiles");

    static void Main(string[] args)
    {
        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(ipAddress, port));
        listener.Listen(connectionThreshold);
        Console.WriteLine("Server started...");

Task.Run(() => AcceptClientsAsync());

Console.ReadLine();

        while (true)
        {
            if (clients.Count < connectionThreshold)
            {
                Socket client = listener.Accept();
                clients.Add(client);
                Console.WriteLine("Client connected.");

                LogMessage(client, "Client connected");
            }
            else
            {
                Console.WriteLine("Connection threshold reached. New connections will wait.");
            }
        }
    }
    private static void LogMessage(Socket client, string message)
    {
        var clientEndPoint = client.RemoteEndPoint.ToString();
        string logMessage = $"{DateTime.Now} - {clientEndPoint} - {message}";
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }

    private static void LogRequest(string clientIP, string permission, string request)
    {
        string logEntry = $"[{DateTime.Now}] {clientIP} ({permission}) requested: {request}";
        Console.WriteLine(logEntry);
        File.AppendAllText(Path.Combine(logsDirectory, "server_log.txt"), logEntry + Environment.NewLine);
    }

    private static void LogMessageForMonitoring(string clientIP, string message)
    {
        string logEntry = $"[{DateTime.Now}] {clientIP}: {message}";
        File.AppendAllText(Path.Combine(logsDirectory, "client_messages_log.txt"), logEntry + Environment.NewLine);
    }

    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}

