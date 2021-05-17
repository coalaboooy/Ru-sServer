using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LaserSIM_ServerGUI
{
    class Server
    {
        TcpListener server = null;
        //основная форма
        Form1 MainForm = System.Windows.Forms.Control.FromHandle(Form1.MainFormHandle) as Form1;

        /// <summary>
        /// <listheader>Состояния датчика лазертага</listheader>
        /// <list type="number">
        /// <item>
        /// <term>Inactive</term>
        /// <description>Датчик включен, но не активен</description>
        /// </item>
        /// <item>
        /// <term>Hits_0</term>
        /// <description>По датчику было произведено ноль попаданий</description>
        /// </item>
        /// <item>
        /// <term>Hits_1</term>
        /// <description>По датчику было произведено одно попадание</description>
        /// </item>
        /// <item>
        /// <term>Hits_2</term>
        /// <description>По датчику было произведено два попадания</description>
        /// </item>
        /// <item>
        /// <term>Dead</term>
        /// <description>Датчик выведен из игры. Активируется при попадании по датчику, имеющему состояние <c>States.Hits_2</c></description>
        /// </item>
        /// <item>
        /// <term>Off</term>
        /// <description>Датчик выключается, завершая работу. Используется для завершения соединения</description>
        /// </item>
        /// </list>
        /// </summary>
        enum States : byte
        {
            Inactive = 0,
            Hits_0 = 2,
            Hits_1 = 4,
            Hits_2 = 8,
            Dead = 16,
            Off = 255
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="Server"/> с указанными IP-адресом и портом.
        /// </summary>
        /// <param name="ip">IP-адрес сервера. Строка, состоящая из четырех десятичных чисел, разделенных точкой.</param>
        /// <param name="port">Порт сервера, принимающий подключения. Целое число.</param>
        Server(string ip, int port)
        {
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
        }

        /// <summary>
        /// Запускает сервер и прослушивает подключения клиентов.
        /// </summary>
        private void Start()
        {
            server.Start();
            MainForm.GetMessage("Server started\r\n");

            try
            {
                while (true)
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

        /// <summary>
        /// Обрабатывает подключения клиента, считывая получаемые данные и выводя их в текстовое поле формы.
        /// </summary>
        /// <param name="client">Клиент, отправлющий данные.</param>
        private void HandleConnection(TcpClient client)
        {
            //поток для чтения данных
            NetworkStream stream = client.GetStream();
            //байты для чтения
            byte[] bytes = new byte[32];
            int byteCount;
            //сообщение клиента
            string data = null;

            try
            {
                while ((byteCount = stream.Read(bytes, 0, bytes.Length)) != 0) //читаем сообщение, если есть что читать
                {
                    //обработка и вывод приветственного сообщения
                    if (byteCount > 2)
                    {
                        //байты в строку
                        data = Encoding.ASCII.GetString(bytes, 0, byteCount);
                        //вывод сообщения в текстбокс
                        if (MainForm != null)
                            MainForm.GetMessage(data);
                        else
                            Console.WriteLine("No active forms present at the moment");
                    }
                    //обработка и вывод сообщений состояния датчика
                    else
                    {
                        data = "Client " + bytes[0].ToString() + " - " + ((States)bytes[1]).ToString();
                        if (MainForm != null)
                            MainForm.GetMessage(data);
                        else
                            Console.WriteLine("No active forms present at the moment");
                        //Завершение сеанса, если клиент отправил States.Off
                        if (bytes[1] == 255)
                        {
                            MainForm.GetMessage($"Client {bytes[0]} has ended the connection\r\n");
                            stream.Close();
                            client.Close();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                stream.Close();
                client.Close();
            }
        }

        /// <summary>
        /// Метод - делегат, создающий экземпляр сервера.
        /// </summary>
        public static void Run()
        {
            Server myServer = new Server("127.0.0.1", 9999);
            myServer.Start();
        }
    }
}
