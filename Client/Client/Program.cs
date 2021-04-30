using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            //вводим ID. Должен быть intом
            Console.WriteLine("Enter client ID to start a new client:");
            Client.Run(int.Parse(Console.ReadLine()));
        }
    }
}
