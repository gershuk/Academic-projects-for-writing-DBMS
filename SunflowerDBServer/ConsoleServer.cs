using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleClientServer;

namespace SunflowerDB
{
    public class ConsoleServer
    {
        private static void Main(string[] args)
        {
            Server _server = default; // сервер
            Thread _listenThread = default; // потока для прослушивания

            try
            {
                _server = new Server();
                _listenThread = new Thread(new ThreadStart(_server.Listen));
                _listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                _server?.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
