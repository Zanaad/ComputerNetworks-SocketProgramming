//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;

//class ClientHandler
//{
//    private const string Admin = "admin";

//    public TcpClient Client { get; private set; }
//    public string Name { get; private set; }
//    public string IPAddress { get; private set; }
//    private NetworkStream Stream;
//    private DateTime LastActivityTime;

//    public bool IsDisconnected { get; private set; } = false;
//    public bool IsTimedOutFlag { get; set; } = false;
//    public List<string> Permissions { get; private set; }

//    public ClientHandler(TcpClient client, string name)
//    {
//        Client = client;
//        Name = name;
//        Stream = client.GetStream();
//        LastActivityTime = DateTime.Now;

//        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
//        IPAddress = remoteEndPoint.Address.ToString();

//        // Initialize permissions based on user name
//        Permissions = GetPermissionsForUser(name);
//    }

//    private List<string> GetPermissionsForUser(string name)
//    {
//        var permissions = new List<string> { "list", "read", "info", "exit" };

//        if (name == Admin)
//        {
//            permissions.AddRange(new[] { "write", "create", "delete" });
//        }

//        return permissions;
//    }

//    public void UpdateActivity() => LastActivityTime = DateTime.Now;

//    public bool IsTimedOut(TimeSpan timeout) => DateTime.Now - LastActivityTime > timeout;

//    public void SendMessage(string message)
//    {
//        byte[] data = Encoding.ASCII.GetBytes(message + "\n");
//        Stream.Write(data, 0, data.Length);
//    }

//    public void Close()
//    {
//        Stream.Close();
//        Client.Close();
//    }

//    public void MarkAsDisconnected()
//    {
//        IsDisconnected = true;
//    }
//}

//class Server
//{
//    private const string Admin = "admin";

//    private TcpListener Listener;
//    private List<ClientHandler> Clients = new List<ClientHandler>();
//    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
//    private const int MaxClients = 4;
//    private readonly object clientsLock = new object();
//    private static object logLock = new object();

//    public Server(int port)
//    {
//        Listener = new TcpListener(IPAddress.Any, port);

//        string dataDirectory = "data";
//        if (!Directory.Exists(dataDirectory))
//        {
//            Directory.CreateDirectory(dataDirectory);
//        }
//    }

//    public void Start()
//    {
//        Listener.Start();
//        Console.WriteLine("Server started...");

//        var timeoutThread = new Thread(() => CheckForTimeoutsLoop())
//        {
//            IsBackground = true
//        };
//        timeoutThread.Start();

//        while (true)
//        {
//            var client = Listener.AcceptTcpClient();

//            lock (clientsLock)
//            {
//                if (Clients.Count >= MaxClients)
//                {
//                    using (var stream = client.GetStream())
//                    using (var writer = new StreamWriter(stream, Encoding.ASCII))
//                    {
//                        writer.WriteLine("Server is full. Please wait and try again.");
//                        writer.Flush();
//                    }
//                    client.Close();
//                    Console.WriteLine("Client attempted to connect but server is full.");
//                    continue;
//                }
//                else
//                {
//                    NetworkStream stream = client.GetStream();
//                    stream.ReadTimeout = 20000;
//                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);

//                    string name = null;
//                    try
//                    {
//                        name = reader.ReadLine();
//                    }
//                    catch (IOException)
//                    {
//                        client.Close();
//                        Console.WriteLine("Client did not send a name, connection closed.");
//                        continue;
//                    }

//                    if (string.IsNullOrEmpty(name))
//                    {
//                        client.Close();
//                        Console.WriteLine("Client did not send a name, connection closed.");
//                        continue;
//                    }

//                    var handlerThread = new Thread(() => HandleClient(client, name))
//                    {
//                        IsBackground = true
//                    };

//                    if (name == Admin)
//                    {
//                        handlerThread.Priority = ThreadPriority.Highest;
//                        Console.WriteLine($"Setting high priority for user: {name}");
//                    }
//                    else
//                    {
//                        handlerThread.Priority = ThreadPriority.Normal;
//                    }

//                    handlerThread.Start();
//                }
//            }
//        }
//    }

//    private void CheckForTimeoutsLoop()
//    {
//        try
//        {
//            while (true)
//            {
//                Thread.Sleep(60000);
//                CheckForTimeouts();
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Timeout checker thread encountered an error: {ex.Message}");
//        }
//    }

//    private void HandleClient(TcpClient client, string name)
//    {
//        ClientHandler clientHandler = null;
//        try
//        {
//            var stream = client.GetStream();
//            var reader = new StreamReader(stream, Encoding.ASCII);
//            var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

//            clientHandler = new ClientHandler(client, name);

//            lock (clientsLock)
//            {
//                Clients.Add(clientHandler);
//                DisplayConnectedClients();
//            }

//            Console.WriteLine($"Client connected: {name}");

//            string initialMessage = GetUserInfo(clientHandler);
//            clientHandler.SendMessage(initialMessage);

//            while (client.Connected && !clientHandler.IsDisconnected)
//            {
//                if (stream.DataAvailable)
//                {
//                    var command = reader.ReadLine();

//                    lock (clientsLock)
//                    {
//                        if (clientHandler.IsTimedOutFlag)
//                        {
//                            clientHandler.IsTimedOutFlag = false;
//                            clientHandler.UpdateActivity();
//                            Console.WriteLine($"Client {clientHandler.Name} reconnected after timeout.");

//                            clientHandler.SendMessage("You have been reconnected after timeout due to inactivity.");
//                        }
//                        else
//                        {
//                            clientHandler.UpdateActivity();
//                        }
//                    }

//                    Log($"{DateTime.Now} [{clientHandler.IPAddress}] {clientHandler.Name}: {command}");

//                    HandleCommand(clientHandler, command);
//                }
//                else
//                {
//                    Thread.Sleep(100);
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Client connection error: {ex.Message}");
//        }
//        finally
//        {
//            if (clientHandler != null)
//            {
//                lock (clientsLock)
//                {
//                    Clients.Remove(clientHandler);
//                    clientHandler.Close();
//                    Console.WriteLine($"Client disconnected: {clientHandler.Name}");
//                    DisplayConnectedClients();
//                }
//            }
//        }
//    }

//    private void HandleCommand(ClientHandler client, string command)
//    {
//        var args = command.Split(' ', 3);  // Split into 3 parts: command, filename, and content
//        string response = "";

//        switch (args[0].ToLower())
//        {
//            case "list":
//                response = ListFiles();
//                break;
//            case "read":
//                response = ReadFile(args.Length > 1 ? args[1] : null);
//                break;
//            case "write":
//                if (client.Permissions.Contains("write"))
//                    response = WriteFile(args.Length > 2 ? args[1] : null, args.Length > 2 ? args[2] : null);
//                else
//                    response = "Permission denied.";
//                break;
//            case "create":
//                if (client.Permissions.Contains("create"))
//                    response = CreateFile(args.Length > 1 ? args[1] : null);
//                else
//                    response = "Permission denied.";
//                break;
//            case "delete":
//                if (client.Permissions.Contains("delete"))
//                    response = DeleteFile(args.Length > 1 ? args[1] : null);
//                else
//                    response = "Permission denied.";
//                break;
//            case "info":
//                response = GetUserInfo(client);
//                break;
//            case "exit":
//                response = "Goodbye!";
//                client.SendMessage(response);
//                client.MarkAsDisconnected();
//                return;
//            default:
//                response = "Unknown command.";
//                break;
//        }

//        try
//        {
//            client.SendMessage(response);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error sending message to client {client.Name}: {ex.Message}");
//        }
//    }

//    private string GetUserInfo(ClientHandler client)
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine($"Hello {client.Name}!");
//        sb.AppendLine("You have the following permissions:");

//        foreach (var permission in client.Permissions)
//        {
//            sb.AppendLine($"- {permission}");
//        }

//        sb.AppendLine("\nAvailable commands:");
//        sb.AppendLine("list               - List all files");
//        sb.AppendLine("read <filename>    - Read the content of a file");
//        if (client.Permissions.Contains("write"))
//            sb.AppendLine("write <filename> <content> - Write content to a file");
//        if (client.Permissions.Contains("create"))
//            sb.AppendLine("create <filename>  - Create a new file");
//        if (client.Permissions.Contains("delete"))
//            sb.AppendLine("delete <filename>  - Delete a file");
//        sb.AppendLine("info               - Display this information");
//        sb.AppendLine("exit               - Disconnect from the server");

//        return sb.ToString();
//    }

//    private void CheckForTimeouts()
//    {
//        lock (clientsLock)
//        {
//            for (int i = 0; i < Clients.Count; i++)
//            {
//                var client = Clients[i];

//                if (!client.IsTimedOutFlag && client.IsTimedOut(Timeout))
//                {
//                    Console.WriteLine($"Client timed out: {client.Name}");
//                    client.IsTimedOutFlag = true;
//                    client.SendMessage("You have been timed out due to inactivity.");
//                }
//            }
//        }
//    }

//    private void DisplayConnectedClients()
//    {
//        lock (clientsLock)
//        {
//            int clientCount = Clients.Count;
//            string clientNames = string.Join(", ", Clients.Select(c => c.Name));

//            Console.WriteLine($"Connected clients ({clientCount}): {clientNames}");
//        }
//    }

//    private string ListFiles()
//    {
//        try
//        {
//            string[] files = Directory.GetFiles("data");
//            return files.Length == 0 ? "No files found." : string.Join("\n", files.Select(f => Path.GetFileName(f)));
//        }
//        catch (Exception ex)
//        {
//            return $"Error listing files: {ex.Message}";
//        }
//    }

//    private string ReadFile(string fileName)
//    {
//        if (fileName == null) return "No file specified.";

//        string filePath = Path.Combine("data", fileName);

//        return File.Exists(filePath) ? File.ReadAllText(filePath) : "File not found.";
//    }

//    private string WriteFile(string fileName, string content)
//    {
//        if (fileName == null) return "No file specified.";
//        if (content == null) return "No content provided.";

//        try
//        {
//            string filePath = Path.Combine("data", fileName);
//            File.WriteAllText(filePath, content);
//            return "File written successfully.";
//        }
//        catch (Exception ex)
//        {
//            return $"Error writing file: {ex.Message}";
//        }
//    }

//    private string CreateFile(string fileName)
//    {
//        if (fileName == null) return "No file specified.";

//        try
//        {
//            string filePath = Path.Combine("data", fileName);
//            if (File.Exists(filePath))
//            {
//                return "File already exists.";
//            }
//            File.Create(filePath).Dispose(); // Create and close the file
//            return "File created successfully.";
//        }
//        catch (Exception ex)
//        {
//            return $"Error creating file: {ex.Message}";
//        }
//    }

//    private string DeleteFile(string fileName)
//    {
//        if (fileName == null) return "No file specified.";

//        try
//        {
//            string filePath = Path.Combine("data", fileName);
//            if (!File.Exists(filePath))
//            {
//                return "File not found.";
//            }
//            File.Delete(filePath);
//            return "File deleted successfully.";
//        }
//        catch (Exception ex)
//        {
//            return $"Error deleting file: {ex.Message}";
//        }
//    }

//    private void Log(string message)
//    {
//        lock (logLock)
//        {
//            File.AppendAllText("logs.txt", message + Environment.NewLine);
//            Console.WriteLine(message);
//        }
//    }
//}

//class Program
//{
//    static void Main()
//    {
//        var server = new Server(5000);
//        server.Start();
//    }
//}