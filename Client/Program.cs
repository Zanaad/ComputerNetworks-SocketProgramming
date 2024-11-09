using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    private static Socket clientSocket;
    private static bool isReadOnly = false; // Track if client has read-only access

    static void Main(string[] args)
    {
        Console.Write("Enter Server IP: ");
        string serverIP = Console.ReadLine();

        Console.Write("Enter Server Port: ");
        int port = int.Parse(Console.ReadLine());

        ConnectToServer(serverIP, port);

        // Check access level based on the server's initial response
        string accessResponse = ReceiveMessage();

        if (accessResponse.Contains("Read-Only"))
        {
            isReadOnly = true;
            Console.WriteLine("You have been granted read-only access. Use 'READ [filename]' to read from a file or 'EXIT' to disconnect.");
        }
        else
        {
            Console.WriteLine("You have full access.");
        }

        // Main command loop
        while (true)
        {
            if (!isReadOnly)
            {
                Console.WriteLine("Enter a command (type INFO for a list of commands):");
            }
            else
            {
                Console.WriteLine("Enter command (READ [filename], EXIT):");
            }

            string command = Console.ReadLine();

            // Check command validity based on access level
            if (isReadOnly && !command.StartsWith("READ", StringComparison.OrdinalIgnoreCase) && !command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("You have read-only access. Only 'READ [filename]' and 'EXIT' commands are allowed.");
                continue;
            }

            SendMessage(command);

            if (command.ToUpper().StartsWith("EXIT")) break;

            string response = ReceiveMessage();
            Console.WriteLine("Server response: " + response);
        }

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    // Metodat ...
    private static void ConnectToServer(string serverIP, int port)
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(IPAddress.Parse(serverIP), port);
            Console.WriteLine("Connected to server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to connect to server: " + ex.Message);
        }
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