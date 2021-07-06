using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static Socket Client;
        static void Main(string[] args)
        {
            StartClient();
            Console.ReadLine();
        }

        private static void StartClient()
        {
            Console.Title = "Client";
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (!Client.Connected)
            {
                try
                {
                    Client.Connect(IPAddress.Loopback, 9999);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Waiting for a connection...");
                }
            }

            Console.Clear();
            Console.WriteLine("The client is Connected");

            ClientSend();
        }
        private static void ClientSend()
        {
            while (true)
            {
                Console.WriteLine("\nRequest: ");
                string request = Console.ReadLine();

                byte[] bytesToRequest = Encoding.ASCII.GetBytes(request);
                Client.Send(bytesToRequest);

                byte[] bytes = new byte[1024];
                int bytesNumber = Client.Receive(bytes);
                byte[] bytesToRead = new byte[bytesNumber];
                Array.Copy(bytes, bytesToRead, bytesNumber);

                Console.WriteLine("Response: " + Encoding.ASCII.GetString(bytesToRead));
            }
        }
    }
}
