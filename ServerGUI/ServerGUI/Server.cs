using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ServerGUI
{
    class Server
    {
        //Т(Д)ЦП слушатель подключений
        TcpListener server = null;
        //основная форма
        Form1 MainForm = System.Windows.Forms.Control.FromHandle(Form1.MainFormHandle) as Form1;
        Server(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
        }

        private void Start()
        {
            server.Start();

            try
            {
                while (true) //не заканичвается, вечно ожидает соединений
                {
                    //принимаем соединение
                    TcpClient client = server.AcceptTcpClient();

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
                    //байты в строку
                    data = Encoding.ASCII.GetString(bytes, 0, byteCount);
                    //вывод сообщения в текстбокс
                    if (MainForm != null)
                        MainForm.GetMessage(data);
                    else
                        Console.WriteLine("No active forms present at the moment");
                    //Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}) received: {data}");
                    if (Regex.IsMatch(data, @"\(\d+\)end", RegexOptions.IgnoreCase)) //проверяем, если клиент отправил "end", то закрываем соединение
                    {
                        //Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}) ended due to client closing conection");
                        MainForm.GetMessage($"Client in the thread ({Thread.CurrentThread.ManagedThreadId}) ended the connection\r\n");
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
