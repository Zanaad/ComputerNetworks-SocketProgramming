using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private static Socket clientSocket;
    private static bool isReadOnly = false; 
    private static string serverIP;
    private static int port;
    private const int reconnectDelay = 5000; // Delay in milliseconds before reconnecting

    static void Main(string[] args)
    {
        Console.Write("Enter Server IP: ");
        serverIP = Console.ReadLine();

        Console.Write("Enter Server Port: ");
        port = int.Parse(Console.ReadLine());

        ConnectAndInitialize();

        // Main command loop
        while (true)
        {
            try
            {
                if (!isReadOnly)
                {
                    Console.WriteLine("Enter a command (type INFO for a list of commands):");
                }
                else
                {
                    Console.WriteLine("Enter command (READ [filename], LIST, EXIT):");
                }

                string command = Console.ReadLine();

                // Check command validity based on access level
                if (isReadOnly &&
                    !command.StartsWith("READ", StringComparison.OrdinalIgnoreCase) &&
                    !command.Equals("LIST", StringComparison.OrdinalIgnoreCase) &&
                    !command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("You have read-only access. Only 'READ [filename]', 'LIST', and 'EXIT' commands are allowed.");
                    continue;
                }

                SendMessage(command);

                if (command.ToUpper().StartsWith("EXIT")) break;

                string response = ReceiveMessage();
                Console.WriteLine("Server response: " + response);
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection lost. Attempting to reconnect...");
                ConnectAndInitialize();
            }
        }

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    private static void ConnectAndInitialize()
    {
        while (true)
        {
            try
            {
                ConnectToServer();

                // Check access level based on the server's initial response
                string accessResponse = ReceiveMessage();

                if (accessResponse.Contains("Read-Only"))
                {
                    isReadOnly = true;
                    Console.WriteLine("You have been granted read-only access. Use 'READ [filename]', 'LIST' to view files, or 'EXIT' to disconnect.");
                }
                else
                {
                    isReadOnly = false;
                    Console.WriteLine("You have full access.");
                }
                break; // Exit the loop if connection is successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to connect to server: {ex.Message}. Retrying in {reconnectDelay / 1000} seconds...");
                Thread.Sleep(reconnectDelay); // Wait before retrying
            }
        }
    }

    private static void ConnectToServer()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(IPAddress.Parse(serverIP), port);
        Console.WriteLine("Connected to server.");
    }

    private static void SendMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(buffer);
    }

    private static string ReceiveMessage()
    {
        byte[] buffer = new byte[1024];
        int received = clientSocket.Receive(buffer);
        return Encoding.UTF8.GetString(buffer, 0, received);
    }
}
