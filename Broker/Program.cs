using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Broker
{
    private static TcpListener listener;
    private static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
    private static Dictionary<string, List<TcpClient>> subscriptions = new Dictionary<string, List<TcpClient>>();

    static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
        listener.Start();
        Console.WriteLine("Broker ");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        clients.Add(client, ""); // Initialize client alias
        NetworkStream stream = client.GetStream();

        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Client disconnected

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessMessage(message, client);
            }
            catch
            {
                break;
            }
        }

        Console.WriteLine("A subscriber has disconnected."); // Notify on disconnect
        clients.Remove(client);
        client.Close();
    }

    private static void ProcessMessage(string message, TcpClient client)
    {
        if (message.StartsWith("!publish"))
        {
            var parts = message.Split(new[] { ' ' }, 3);
            if (parts.Length == 3)
            {
                string topic = parts[1];
                string msgContent = parts[2];
                BroadcastToSubscribers(topic, msgContent);
            }
        }
        else if (message.StartsWith("!subscribe"))
        {
            var parts = message.Split(' ');
            if (parts.Length == 2)
            {
                string topic = parts[1];
                if (!subscriptions.ContainsKey(topic))
                {
                    subscriptions[topic] = new List<TcpClient>();
                }
                subscriptions[topic].Add(client);
                SendMessage(client, $"Subscribed to {topic}");
            }
        }
    }

    private static void BroadcastToSubscribers(string topic, string message)
    {
        if (subscriptions.ContainsKey(topic))
        {
            foreach (var subscriber in subscriptions[topic])
            {
                SendMessage(subscriber, message);
            }
        }
    }

    private static void SendMessage(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}
