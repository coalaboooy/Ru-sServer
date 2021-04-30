using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Server
{
    class Server
    {
        //Т(Д)ЦП слушатель подключений
        TcpListener server = null;
        Server (string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
        }

        private void Start()
        {
            Console.WriteLine("Starting Server");
            server.Start();
            
            try
            {
                while (true) //не заканичвается, вечно ожидает соединений
                { 
                    Console.WriteLine("Waiting for a connection...");
                    //принимаем соединение
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    //запускаем обработку подключенияв отдельном потоке
                    Task.Run(() => HandleConnection(client));
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e.Message}");
                server.Stop();
            }
        }

        private void HandleConnection(TcpClient client)
        {
            //поток для чтения данных
            NetworkStream stream = client.GetStream();
            //сообщение
            string data = null;
            //байты для чтения
            byte[] bytes = new byte[1024];
            int byteCount;

            try
            {
                while ((byteCount = stream.Read(bytes, 0, bytes.Length)) != 0) //читаем сообщение, если есть что читать
                {
                    data = Encoding.ASCII.GetString(bytes, 0, byteCount); //байты в строку
                    Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}) received: {data}");
                    if (Regex.IsMatch(data, @"\(\d+\)end", RegexOptions.IgnoreCase)) //проверяем, если клиент отправил "end", то закрываем соединение
                    {
                        Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}) ended due to client closing conection");
                        stream.Close();
                        client.Close();
                        break;
                    }

                    //ответы
                    //string str = $"Welcome!";
                    //Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);
                    //stream.Write(reply, 0, reply.Length);
                    //Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}) sent: {str}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                stream.Close();
                client.Close();
            }
        }

        public static void Run()
        {
            //локальный IP, любой свободный порт
            Server myServer = new Server("127.0.0.1", 9999);
            myServer.Start();
        }
    }
}
