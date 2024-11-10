using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    private static Socket clientSocket;
    private static bool isReadOnly = false; // Track if client has read-only access
    private static string serverIP = "192.168.0.109";
    private static int port = 5000;
    static void Main(string[] args)
    {
        while (true)  // Loop for reconnection attempts
        {
            try
            {
                ConnectToServer(serverIP, port);
                break;  // Exit loop if connection is successful
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to the server: " + ex.Message);
                Console.WriteLine("Retrying in 5 seconds...");
                Thread.Sleep(5000);  // Wait for 5 seconds before retrying
            }
        }

        // Check access level based on the server's initial response
        string accessResponse = ReceiveMessage();

        if (accessResponse.Contains("Read-Only"))
        {
            isReadOnly = true;
            Console.WriteLine("You have been granted read-only access. Use 'READ [filename]', 'LIST' to view files, or 'EXIT' to disconnect.");
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

            try
            {
                SendMessage(command);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Connection lost. Reconnecting...");
                clientSocket.Close();
                ConnectToServer(serverIP, port);  // Attempt to reconnect
                SendMessage(command);  // Retry sending the message after reconnection
            }

            if (command.ToUpper().StartsWith("EXIT")) break;

            string response = ReceiveMessage();
            Console.WriteLine("Server response: " + response);
        }

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

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
