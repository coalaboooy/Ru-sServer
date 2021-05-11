using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Client
{
    class Client
    {
        private static void Connect(string ip, int port, int clientId)
        {
            try
            {
                //клиент, должен быть тот же порт и IP, что у сервера
                TcpClient client = new TcpClient(ip, port);
                NetworkStream stream = client.GetStream();
                //сообщение
                string message = null;

                message = "client (" + clientId + ") connected";
                byte[] greeting = Encoding.ASCII.GetBytes(message);
                stream.Write(greeting, 0, greeting.Length);

                while (true)
                {
                    //считываем сообщение и добавляем ID клиента для идентификации
                    message = "(" + clientId + ")" + Console.ReadLine();
                    //кодируем в байты
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    //пишем в поток
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine($"Client({clientId}) sent: {message}");

                    if (Regex.IsMatch(message, @"\(\d+\)end", RegexOptions.IgnoreCase)) //если сообщение == "end", закрываем соединение
                    {
                        Console.WriteLine($"Client({clientId}) closing connection");
                        stream.Close();
                        client.Close();
                        break;
                    }
                    //Получить ответы
                    //Byte[] buffer = new Byte[1024];
                    //int byteCount = stream.Read(buffer, 0, buffer.Length);
                    //string response = System.Text.Encoding.ASCII.GetString(buffer, 0, byteCount);
                    //Console.WriteLine($"Client({clientId}) received: {response}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
            Console.Read();
        }

        public static void Run(int clientID)
        {
            //вот так не работает, поэтому просто запускай много клиентов через .exe

            //Task.Run(() =>
            //{
            //    Connect("127.0.0.1", 9999, clientID);
            //});
            Connect("127.0.0.1", 9999, clientID);
        }
    }
}
