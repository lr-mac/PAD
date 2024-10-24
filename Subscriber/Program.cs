using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Subscriber
{
    private static TcpClient client;
    private static NetworkStream stream;

    static void Main(string[] args)
    {
        client = new TcpClient("127.0.0.1", 9999);
        stream = client.GetStream();
        Console.WriteLine("Subscriber");
        Console.WriteLine("Waiting for a connection...");

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        // Așteptăm să ne conectăm
        Console.WriteLine("Subscriber connected to broker.");

        while (true)
        {
            Console.Write("Enter the topic: "); // Mesajul modificat
            string topic = Console.ReadLine();
            SendMessage($"!subscribe {topic}");
        }
    }

    private static void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    private static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Client disconnected

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message); // Afișează mesajul direct
            }
            catch
            {
                Console.WriteLine("Subscriber has disconnected.");
                break;
            }
        }

        client.Close();
    }
}
