//Klienti
//1. Të krijohet socket lidhja me server;
//2.Njëri nga pajisjet(klientët) të ketë privilegjet write(), read(), execute() (qasje të plotë;
//execute() përfshin ekzekutimin e komandave të ndryshme në server);
//3.Klientët tjerë të kenë vetëm read() permission;
//4.Të behet lidhja me serverin duke përcaktuar saktë portin dhe IP Adresën e serverit;
//5.Të definohen saktë socket-at e serverit dhe lidhja të mos dështojë;
//6.Të jetë në gjendje të lexojë përgjigjet që i kthehen nga serveri;
//7.Të dërgojë mesazh serverit në formë të tekstit;
//8.Të ketë qasje të plotë në folderat/përmbajtjen në server;
//9.Klientët me privilegje të plota të kenë kohë përgjigjeje më të shpejtë se klientët e tjerë që
//kanë vetëm read permission.


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Client
{
    private static readonly int port = 8080;
    private static readonly IPAddress serverIpAddress = IPAddress.Parse("127.0.0.1");
    private static Socket clientSocket;

    public static void Start()
    {
        try
        {
            // Krijo socket dhe lidhu me serverin
            clientSocket = new Socket(serverIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(serverIpAddress, port));
            Console.WriteLine("Connected to the server.");

            // Krijo një thread për të pranuar mesazhe nga serveri
            //Thread receiveThread = new Thread(ReceiveMessages);
            //receiveThread.Start();

            // Dërgo mesazhe te serveri
            while (true)
            {
                string message = Console.ReadLine();
               // SendMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        Client.Start();
    }
}


