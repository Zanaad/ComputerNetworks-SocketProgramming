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

    private static readonly string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\");
    private static readonly string logsDirectory = Path.Combine(baseDirectory, "Logs");
    private static readonly string clientFilesDirectory = Path.Combine(baseDirectory, "ClientFiles");

    static void Main(string[] args)
    {
        Console.WriteLine("Starting server...");
        Directory.CreateDirectory(baseDirectory);
        StartServer();
    }

    private static void HandleClient(Socket clientSocket)
    {
        IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
        string clientIP = remoteIpEndPoint.Address.ToString();

        string clientPermission;
        lock (lockObj)
        {
            if (fullAccessClient == null)
            {
                fullAccessClient = clientIP;
                clientPermission = "Full";
                LogConnection(clientSocket, "Full-access granted");
            }
            else
            {
                clientPermission = "Read-Only";
                LogConnection(clientSocket, "Read-only access granted");
            }
        }

        SendMessage(clientSocket, $"You have been granted {clientPermission} access.");

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int receivedBytes = clientSocket.Receive(buffer);
                if (receivedBytes == 0) break;

                string receivedText = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                Console.WriteLine($"Received from {clientIP} ({clientPermission}): {receivedText}");

                LogRequest(clientIP, clientPermission, receivedText);
                LogMessageForMonitoring(clientIP, receivedText);

                if (clientPermission == "Full")
                {
                    if (receivedText.ToUpper() == "EXIT")
                    {
                        SendMessage(clientSocket, "Goodbye!");
                        break;
                    }
                    HandleFullAccessCommands(clientSocket, receivedText);
                }
                else if (clientPermission == "Read-Only")
                {
                    string[] parts = receivedText.Split(' ');
                    if (parts[0].Equals("READ", StringComparison.OrdinalIgnoreCase))
                    {
                        string filename = parts.Length > 1 ? parts[1] : "server_log.txt";
                        SendFileContent(clientSocket, filename);
                    }
                    else if (receivedText.ToUpper() == "EXIT")
                    {
                        SendMessage(clientSocket, "Goodbye!");
                        break;
                    }
                    else
                    {
                        SendMessage(clientSocket, "You have read-only access.");
                    }
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine($"Client {clientIP} disconnected unexpectedly.");
        }
        finally
        {
            clientSocket.Close();
            clientSockets.Remove(clientSocket);
            Console.WriteLine($"Connection with {clientIP} closed.");
        }
    }

    private static void HandleFullAccessCommands(Socket clientSocket, string command)
    {
        string[] parts = command.Split(' ', 3);
        string action = parts[0].ToUpper();
        string filename = parts.Length > 1 ? parts[1] : "server_file.txt";
        string fullPath = Path.Combine(clientFilesDirectory, filename);
        string content = parts.Length > 2 ? parts[2] : null;

        switch (action)
        {
            case "READ":
                SendFileContent(clientSocket, fullPath);
                break;
            case "WRITE":
                if (content != null)
                {
                    File.AppendAllText(fullPath, content);
                    SendMessage(clientSocket, $"Content written to {filename}.");
                }
                else
                {
                    SendMessage(clientSocket, "No content provided for writing.");
                }
                break;
            case "CREATE":
                File.Create(fullPath).Close();
                SendMessage(clientSocket, $"File {filename} created successfully.");
                break;
            case "DELETE":
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    SendMessage(clientSocket, $"File {filename} deleted successfully.");
                }
                else
                {
                    SendMessage(clientSocket, $"File {filename} does not exist.");
                }
                break;
            case "EXIT":
                SendMessage(clientSocket, "Goodbye!");
                clientSocket.Close();
                break;
            default:
                SendMessage(clientSocket, "Unknown command.");
                break;
        }
    }

    private static void SendFileContent(Socket clientSocket, string filename)
    {
        if (File.Exists(filename))
        {
            if (new FileInfo(filename).Length == 0)
            {
                SendMessage(clientSocket, $"File {filename} is empty.");
            }
            else
            {
                string content = File.ReadAllText(filename);
                SendMessage(clientSocket, content);
            }
        }
        else
        {
            SendMessage(clientSocket, $"File {filename} does not exist.");
        }
    }

    private static void SendMessage(Socket clientSocket, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(data);
    }

    private static void LogConnection(Socket client, string accessType)
    {
        var clientEndPoint = client.RemoteEndPoint.ToString();
        string logEntry = $"[{DateTime.Now}] - {clientEndPoint} - Client connected - {accessType}";
        Console.WriteLine(logEntry);
        File.AppendAllText(Path.Combine(logsDirectory, "server_log.txt"), logEntry + Environment.NewLine);
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
