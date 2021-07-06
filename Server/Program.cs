using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static List<Socket> Clients = new List<Socket>();
        private static Socket Server;
        private static byte[] Bytes = new Byte[1024];
        static void Main(string[] args)
        {
            StartServer();
            Console.ReadLine();
        }

        public static void StartServer()
        {
            Console.Title = "Server";
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9999);

            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Server.Bind(localEndPoint);
            Server.Listen(100);

            Console.WriteLine("Waiting for a connection...");

            Server.BeginAccept(new AsyncCallback(AsyncAccept), null);
        }

        private static void AsyncAccept(IAsyncResult result)
        {
            Socket newClient = Server.EndAccept(result);
            Clients.Add(newClient);
            Console.WriteLine("Client-" + newClient.RemoteEndPoint + "-connected");

            newClient.BeginReceive(Bytes, 0, Bytes.Length, SocketFlags.None, new AsyncCallback(AsyncRead), newClient);
            Server.BeginAccept(new AsyncCallback(AsyncAccept), null);
        }
        private static void AsyncRead(IAsyncResult result)
        {
            Socket currentClient = (Socket)result.AsyncState;

            int bytesNumber = currentClient.EndReceive(result);
            byte[] bytesToRead = new byte[bytesNumber];
            Array.Copy(Bytes, bytesToRead, bytesNumber);

            string requestContent = Encoding.ASCII.GetString(bytesToRead);
            requestContent = Regex.Replace(requestContent, @"\s+", "");

            string responseContent = ReadRequestContent(requestContent);

            byte[] bytesToResponse = Encoding.ASCII.GetBytes(responseContent);
            currentClient.BeginSend(bytesToResponse, 0, bytesToResponse.Length, SocketFlags.None, new AsyncCallback(AsyncSend), currentClient);
            currentClient.BeginReceive(Bytes, 0, Bytes.Length, SocketFlags.None, new AsyncCallback(AsyncRead), currentClient);
        }

        private static void AsyncSend(IAsyncResult result)
        {
            Socket currentClient = (Socket)result.AsyncState;
            currentClient.EndSend(result);
        }

        private static string ReadRequestContent(string requestContent)
        {
            if (!String.IsNullOrEmpty(requestContent))
            {
                Console.WriteLine("Request Content: " + requestContent);
                Regex rx = new Regex(@"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)");
                if (rx.IsMatch(requestContent))
                {
                    char[] operators = { '+', '-', '*', '/' };
                    string[] numbers = requestContent.Split(operators);
                    int count = 0;
                    int res = Convert.ToInt32(numbers[0]);

                    for (int i = 0; i < requestContent.Length; i++)
                    {
                       switch(requestContent[i])
                       {
                            case '+':
                                res += Convert.ToInt32(numbers[++count]);
                                break;
                            case '-':
                                 res -= Convert.ToInt32(numbers[++count]);
                                break;
                            case '*':
                                res *= Convert.ToInt32(numbers[++count]);
                                break;
                            case '/':
                                res /= Convert.ToInt32(numbers[++count]);
                                break;
                       }
                    }

                    return "Result = " + res.ToString();
                } else
                {
                    return "Request content is not VALID!";
                }

            }
            else
            {
                return "Request Content is Empty!";
            }
        }
    }
}
