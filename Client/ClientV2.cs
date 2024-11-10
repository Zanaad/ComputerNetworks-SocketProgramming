//using System;
//using System.IO;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;

//class Client
//{
//    static volatile bool exitSent = false;
//    static StreamWriter writer;
//    static StreamReader reader;
//    static NetworkStream stream;
//    static TcpClient client;
//    static bool isRunning = true;
//    static bool connectionClosed = false;
//    static Thread readThread;
//    static string userName;

//    static void Main()
//    {
//        Console.Write("Enter your name: ");
//        userName = Console.ReadLine();

//        while (true)
//        {
//            try
//            {
//                if (client == null || !client.Connected)
//                {
//                    if (!ConnectToServer())
//                    {
//                        Console.WriteLine("Retrying in 10 seconds...");
//                        Thread.Sleep(10000);
//                        continue;
//                    }
//                }

//                while (isRunning)
//                {
//                    if (connectionClosed)
//                    {
//                        Console.WriteLine("Attempting to reconnect...");
//                        if (!Reconnect())
//                        {
//                            Console.WriteLine("Retrying in 10 seconds...");
//                            Thread.Sleep(10000);
//                            continue;
//                        }
//                    }

//                    Console.Write("> ");
//                    string command = Console.ReadLine();

//                    if (command.ToLower() == "exit")
//                    {
//                        writer.WriteLine(command);
//                        exitSent = true;
//                        isRunning = false;
//                        break;
//                    }

//                    writer.WriteLine(command);
//                }

//                if (exitSent)
//                    break;

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error: " + ex.Message);
//                Console.WriteLine("Retrying in 10 seconds...");
//                Thread.Sleep(10000);
//            }
//        }

//        if (readThread != null && readThread.IsAlive)
//        {
//            readThread.Join();
//        }
//        if (writer != null)
//            writer.Close();
//        if (reader != null)
//            reader.Close();
//        if (stream != null)
//            stream.Close();
//        if (client != null)
//            client.Close();
//    }

//    static bool ConnectToServer()
//    {
//        try
//        {
//            client = new TcpClient();
//            client.Connect("127.0.0.1", 5000);

//            stream = client.GetStream();
//            writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
//            reader = new StreamReader(stream, Encoding.ASCII);


//            writer.WriteLine(userName);


//            ReadInitialInfoMessage();


//            isRunning = true;
//            connectionClosed = false;
//            readThread = new Thread(ReadServerMessages);
//            readThread.Start();

//            return true;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Could not connect to server: " + ex.Message);
//            return false;
//        }
//    }

//    static bool Reconnect()
//    {

//        if (writer != null)
//            writer.Close();
//        if (reader != null)
//            reader.Close();
//        if (stream != null)
//            stream.Close();
//        if (client != null)
//            client.Close();

//        return ConnectToServer();
//    }

//    static void ReadInitialInfoMessage()
//    {
//        try
//        {
//            StringBuilder messageBuilder = new StringBuilder();
//            while (true)
//            {
//                string line = reader.ReadLine();
//                if (string.IsNullOrEmpty(line))
//                {
//                    break;
//                }
//                messageBuilder.AppendLine(line);
//            }
//            Console.WriteLine("\n" + messageBuilder.ToString());
//        }
//        catch (IOException)
//        {

//            Console.WriteLine("Error reading initial message from server.");
//        }
//    }

//    static void ReadServerMessages()
//    {
//        try
//        {
//            while (isRunning)
//            {
//                string response = reader.ReadLine();
//                if (response == null)
//                {
//                    Console.WriteLine("\nServer disconnected.");
//                    isRunning = false;
//                    connectionClosed = true;
//                    break;
//                }

//                Console.WriteLine("\nServer response: " + response);
//                Console.Write("> ");
//            }
//        }
//        catch (IOException)
//        {

//            if (!exitSent)
//            {
//                Console.WriteLine("\nConnection lost.");
//                isRunning = false;
//                connectionClosed = true;
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("\nError reading from server: " + ex.Message);
//            isRunning = false;
//            connectionClosed = true;
//        }
//    }

//    static void CancelHandler(object sender, ConsoleCancelEventArgs e)
//    {
//        e.Cancel = true;

//        if (!exitSent && writer != null)
//        {
//            exitSent = true;

//            try
//            {
//                writer.WriteLine("exit");
//                writer.Flush();
//            }
//            catch
//            {
//                // Ignore any exceptions if the writer is unavailable
//            }
//        }

//        Environment.Exit(0);
//    }
//}