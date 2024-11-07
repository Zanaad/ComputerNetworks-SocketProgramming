//Serveri
//1. Të vendosen variabla te cilat përmbajnë numrin e portit (numri i portit të jetë i
//çfarëdoshëm) dhe IP adresën;
//2.Të jetë në gjendje të dëgjojë (listën) të paktën të gjithë anëtaret e grupit. Nëse numri i
//lidhjeve kalon një prag të caktuar, serveri duhet të refuzojë lidhjet e reja ose t'i vë në pritje;
//3. Të menaxhojë kërkesat e pajisjeve që dërgojnë request (ku secili anëtar i grupit duhet
//ta ekzekutojë të paktën një kërkesë në server) dhe t’i logojë të gjitha për auditim të
//mëvonshëm, duke përfshirë timestamp dhe IP-në e dërguesit;
//4.Të jetë në gjendje të lexoje mesazhet që dërgohen nga klientët dhe t’i ruajë për monitorim;
//5.Nëse një klient nuk dërgon mesazhe brenda një periudhe të caktuar kohe, serveri duhet ta
//mbyllë lidhjen dhe të jetë në gjendje ta rikuperojë atë automatikisht nëse klienti rifutet;
//6.Të jetë në gjendje të jap qasje të plotë të paktën njërit klient për qasje në folderat/
//përmbajtjen në file-t në server.

using System.Net;
using System.Net.Sockets;


public class Server
{
    private static readonly int port = 8080;
    private static readonly IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
    private static Socket listener;
    private static List<Socket> clients = new List<Socket>();
    private const int connectionThreshold = 4;
    private const int timeoutDuration = 60000;
    private static Dictionary<Socket, string> clientPermissions = new Dictionary<Socket, string>();
    private static Socket fullAccessClient = null;
    private static readonly string logFilePath = "server_log.txt";

    public static void Start()
    {
        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(ipAddress, port));
        listener.Listen(connectionThreshold);
        Console.WriteLine("Server started...");

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

}

class Program
{
    static void Main(string[] args)
    {
        Server.Start();
    }
}
