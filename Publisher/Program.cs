using System;
using System.Net.Sockets;
using System.Text;

class Publisher
{
    private static TcpClient client;
    private static NetworkStream stream;

    static void Main(string[] args)
    {
        client = new TcpClient("127.0.0.1", 9999);
        stream = client.GetStream();
        Console.WriteLine("Publisher");
        Console.WriteLine("Sender connected to broker");

        while (true)
        {
            Console.Write("Enter the topic: ");
            string topic = Console.ReadLine();

            Console.Write("Enter the message: ");
            string message = Console.ReadLine();

            SendMessage($"!publish {topic} {message}");
        }
    }

    private static void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}
