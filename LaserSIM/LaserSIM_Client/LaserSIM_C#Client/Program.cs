using System;

namespace LaserSIM_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите ID клиента (целое число в диапазоне 0-255):");
            new Client(byte.Parse(Console.ReadLine()), "127.0.0.1", 9999).Connect(); //Заменить на настоящие ip/порт
        }
    }
}
