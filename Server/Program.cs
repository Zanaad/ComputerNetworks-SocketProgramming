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
    private static int maxConnections = 4; // Set the maximum number of connections
    private static Semaphore connectionSemaphore = new Semaphore(maxConnections, maxConnections);


    private static readonly string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Files");

    private static Queue<Socket> fullAccessQueue = new Queue<Socket>();
    private static Queue<Socket> readOnlyQueue = new Queue<Socket>();

    static void Main(string[] args)
    {
        Console.WriteLine("Starting server...");
        Directory.CreateDirectory(baseDirectory);
        StartServer();
    }
    private static void StartServer()
    {
        string localIP = GetLocalIPAddress();
        Console.WriteLine("Server IP Address: " + localIP);

        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Parse(localIP), port));
        listener.Listen(maxConnections);

        Console.WriteLine($"Server listening on port {port}. Max connections: {maxConnections}");

        while (true)
        {
            if (!connectionSemaphore.WaitOne(0)) // Instant check without blocking
            {
                Console.WriteLine("Max connections reached, new connections are being put on hold.");
                connectionSemaphore.WaitOne(); // Block until a slot is available
            }

            // Accept new client connection
            Socket clientSocket = listener.Accept();
            clientSockets.Add(clientSocket);

            Thread clientThread = new Thread(() =>
            {
                try
                {
                    HandleClient(clientSocket);
                }
                finally
                {
                    // Release the spot when done
                    connectionSemaphore.Release();
                }
            });
            clientThread.Start();
        }
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

        // Add to the appropriate queue based on client permission
        if (clientPermission == "Full")
        {
            lock (fullAccessQueue)
            {
                fullAccessQueue.Enqueue(clientSocket);
            }
        }
        else
        {
            lock (readOnlyQueue)
            {
                readOnlyQueue.Enqueue(clientSocket);
            }
        }

        // Handle requests based on priority (Full-access first)
        ProcessClientRequests();

        // Inactivity timeout logic
        Timer inactivityTimer = new Timer(state =>
        {
            Console.WriteLine($"No message received from {clientIP} within the allowed time. Closing connection.");
            clientSocket.Close(); // Close the connection
        }, null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan); // Set timeout to 1 minute

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int receivedBytes = clientSocket.Receive(buffer);
                if (receivedBytes == 0) break;

                // Reset inactivity timer when a message is received
                inactivityTimer.Change(TimeSpan.FromMinutes(5), Timeout.InfiniteTimeSpan); // Reset the timer

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
                    if (parts[0].Equals("LIST", StringComparison.OrdinalIgnoreCase))
                    {
                        ListFilesForReadOnlyClient(clientSocket);
                    }
                    else if (parts[0].Equals("READ", StringComparison.OrdinalIgnoreCase))
                    {
                        string filename = parts.Length > 1 ? parts[1] : "server_log.txt";
                        string fullpath = Path.Combine(baseDirectory, filename);
                        SendFileContent(clientSocket, fullpath);
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

    private static void ProcessClientRequests()
    {
        while (fullAccessQueue.Count > 0)
        {
            Socket clientSocket = fullAccessQueue.Dequeue();
            Console.WriteLine("Processing full-access request from client.");

        }

        while (readOnlyQueue.Count > 0)
        {
            Socket clientSocket = readOnlyQueue.Dequeue();
            Console.WriteLine("Processing read-only request from client.");
        }
    }

  
 

    private static void HandleFullAccessCommands(Socket clientSocket, string command)
    {
        string[] parts = command.Split(' ', 3);
        string action = parts[0].ToUpper();
        string filename = parts.Length > 1 ? parts[1] : "server_file.txt";
        string fullPath = Path.Combine(baseDirectory, filename);
        string content = parts.Length > 2 ? parts[2] : null;

        switch (action)
        {
            case "INFO":
                SendHelpMessage(clientSocket);
                break;
            case "LIST":
                ListFilesInDirectory(clientSocket);
                break;
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


    private static void SendMessage(Socket clientSocket, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(data);
    }

    private static void SendHelpMessage(Socket clientSocket)
    {
        string helpMessage = "Available Commands:\n" +
                             "LIST - List all files in the Files folder\n" +
                             "INFO - Display a list of all available commands\n" +
                             "CREATE <filename> - Create a new file\n" +
                             "READ <filename> - Read the contents of a file\n" +
                             "WRITE <filename> <content> - Write content to a file\n" +
                             "DELETE <filename> - Delete a file\n" +
                             "EXIT - Disconnect from the server";
        SendMessage(clientSocket, helpMessage);
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

    private static void LogConnection(Socket client, string accessType)
    {
        var clientEndPoint = client.RemoteEndPoint.ToString();
        string logEntry = $"[{DateTime.Now}] - {clientEndPoint} - Client connected - {accessType}";
        Console.WriteLine(logEntry);
        File.AppendAllText(Path.Combine(baseDirectory, "server_log.txt"), logEntry + Environment.NewLine);
    }

    private static void LogRequest(string clientIP, string permission, string request)
    {
        string logEntry = $"[{DateTime.Now}] {clientIP} ({permission}) requested: {request}";
        Console.WriteLine(logEntry);
        File.AppendAllText(Path.Combine(baseDirectory, "server_log.txt"), logEntry + Environment.NewLine);
    }

    private static void LogMessageForMonitoring(string clientIP, string message)
    {
        string logEntry = $"[{DateTime.Now}] {clientIP}: {message}";
        File.AppendAllText(Path.Combine(baseDirectory, "client_messages_log.txt"), logEntry + Environment.NewLine);
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

    private static void ListFilesInDirectory(Socket clientSocket)
    {
        try
        {
            if (Directory.Exists(baseDirectory))
            {
                string[] files = Directory.GetFiles(baseDirectory);
                StringBuilder sb = new StringBuilder("Files in the server:\n");

                foreach (string file in files)
                {
                    sb.AppendLine(Path.GetFileName(file));
                }

                SendMessage(clientSocket, sb.ToString());
            }
            else
            {
                SendMessage(clientSocket, "Directory does not exist.");
            }
        }
        catch (Exception ex)
        {
            SendMessage(clientSocket, $"Error listing directory contents: {ex.Message}");
        }
    }

    private static void ListFilesForReadOnlyClient(Socket clientSocket)
    {
        try
        {
            if (Directory.Exists(baseDirectory))
            {
                string[] files = Directory.GetFiles(baseDirectory);
                StringBuilder sb = new StringBuilder("Files available on the server:\n");

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName != "server_log.txt" && fileName != "client_messages_log.txt")
                    {
                        sb.AppendLine(fileName);
                    }
                }

                SendMessage(clientSocket, sb.ToString());
            }
            else
            {
                SendMessage(clientSocket, "Directory does not exist.");
            }
        }
        catch (Exception ex)
        {
            SendMessage(clientSocket, $"Error listing directory contents: {ex.Message}");
        }
    }


}
