using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace LaserSIM_Client
{
    class Client
    {
        byte clientId;
        string ip;
        int port;
        TcpClient client;
        NetworkStream stream;
        Timer timer;
        States currentState;

        /// <summary>
        /// <listheader>Состояния датчика лазертага.</listheader>
        /// <list type="number">
        /// <item>
        /// <term>Inactive</term>
        /// <description>Датчик включен, но не активен.</description>
        /// </item>
        /// <item>
        /// <term>Hits_0</term>
        /// <description>По датчику было произведено ноль попаданий.</description>
        /// </item>
        /// <item>
        /// <term>Hits_1</term>
        /// <description>По датчику было произведено одно попадание.</description>
        /// </item>
        /// <item>
        /// <term>Hits_2</term>
        /// <description>По датчику было произведено два попадания.</description>
        /// </item>
        /// <item>
        /// <term>Dead</term>
        /// <description>Датчик выведен из игры. Активируется при попадании по датчику, имеющему состояние <see cref="Hits_2"></see>.</description>
        /// </item>
        /// <item>
        /// <term>Off</term>
        /// <description>Датчик выключается, завершая работу. Используется для завершения соединения.</description>
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
        /// Создает новый экземпляр класса <see cref="Client"/> для подключения к серверу по указанным IP-адресу и порту.
        /// </summary>
        /// <param name="id">Уникальный номер клиента. Должен иметь целочисленное значение в диапазоне 0-255.</param>
        /// <param name="Ip">IP-адрес сервера. Строка, состоящая из четырех десятичных чисел, разделенных точкой.</param>
        /// <param name="Port">Порт сервера. Целое число.</param>
        public Client(byte Id, string Ip, int Port)
        {
            clientId = Id;
            ip = Ip;
            port = Port;
            currentState = States.Inactive;
        }

        /// <summary>
        /// Основной метод, выполняющий подключение и периодическую отсылку состоняния датчика, а также его изменение.
        /// </summary>
        public void Connect()
        {
            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
                //отправка сообщения о подключении клиента
                string message = "Client " + clientId + " connected";
                byte[] greeting = Encoding.ASCII.GetBytes(message);
                stream.Write(greeting, 0, greeting.Length);

                //создание таймера для отсылки состояния датчика каждую секунду
                //изначальное состояние - Inactive
                byte state = (byte)States.Inactive;
                TimerCallback tm = new TimerCallback(Send);
                timer = new Timer(tm, state, 0, 1000);
                
                //изменение состояния датчика
                while(true) 
                {
                    state = GetState();
                    if (state != (byte)currentState)
                    {
                        timer.Dispose();
                        timer = new Timer(tm, state, 0, 1000);
                    }
                    if (state == (byte)States.Off)
                    {
                        Send(state);
                        break;
                    }
                }
                timer.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }


        /// <summary>
        /// Метод-делегат, выполняющий отправку сообщения.
        /// </summary>
        /// <param name="obj">Передаваемое состояние датчика. Обязателен для использования метода как делегата таймера.</param>
        void Send(object obj)
        {
            byte state = (byte)obj;
            currentState = (States)state;
            //составляем отправляемое сообщение
            byte[] data = { clientId, (byte)currentState };
            stream.Write(data, 0, data.Length);
            //строчка для дебага, показывает отсылаемые байты
            //Console.WriteLine($"Sent {clientId} {state}");
            if (state == (byte)States.Off) 
            {
                Console.WriteLine("Closing connection");
                stream.Close();
                client.Close();
            }
        }

        /// <summary>
        /// Метод, используемый для симуляции изменения состояний датчика лазертага в зависимости от ввода пользователя.
        /// </summary>
        /// <returns>
        /// Одно из состояний перечисления <see cref="States"></see>.
        /// </returns>
        byte GetState ()
        {
            Console.WriteLine("Введите символы для получения состояния датчика.\n0 - неактивен\n1 - ноль попаданий\n'+' - увеличение количества попаданий\n'-' - выключение датчика\nЛюбые другие символы не будут изменять значение");
            switch(Console.ReadKey().KeyChar)
            {
                case '0':
                    return (byte)States.Inactive;
                case '1':
                    return (byte)States.Hits_0;
                case '-':
                    return (byte)States.Off;
                case '+':
                    switch (currentState)
                    {
                        case States.Hits_0:
                            return (byte)States.Hits_1;
                        case States.Hits_1:
                            return (byte)States.Hits_2;
                        case States.Hits_2:
                            return (byte)States.Dead;
                        default:
                            return (byte)currentState;
                    }
                default:
                    return (byte)currentState;
            }
        }
    }
}
