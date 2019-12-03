using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleClientServer
{
    public interface IServer
    {
        void AddConnection(Listener clientObject);
        void Disconnect();
        void Listen();
        void RemoveConnection(string id);
        byte[] ExecuteQuery(string query);
    }

    public abstract class Server : IServer
    {
        private TcpListener _tcpListener; // сервер для прослушивания
        private readonly List<Listener> _clients = new List<Listener>(); // все подключения
        private Thread _comandsThread;

        public void AddConnection(Listener clientObject)
        {
            if (clientObject is null)
            {
                throw new ArgumentNullException(nameof(clientObject));
            }

            Console.WriteLine($"{clientObject.Id} conection established");
            _clients.Add(clientObject);
        }

        public void RemoveConnection(string id)
        {
            Console.WriteLine($"{id} conection lost");
            _clients.RemoveAll(client => client.Id == id);
        }

        // прослушивание входящих подключений
        public void Listen()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, 8888);
                _tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                _comandsThread = new Thread(new ThreadStart(SereverConsoleReader));
                _comandsThread.Start();

                while (true)
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    var clientObject = new Listener(tcpClient, this);
                    var clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal static void SereverConsoleReader()
        {
            while (true)
            {
                var comand = Console.ReadLine();
                //Отправка команды в бд
                var result = comand;
                Console.WriteLine(result);
            }
        }

        // трансляция сообщения всем подключенным клиентам на случай чего
        protected internal void BroadcastMessage(string message)
        {
            var data = Encoding.Unicode.GetBytes(message);
            for (var i = 0; i < _clients.Count; i++)
            {
                _clients[i].Stream.Write(data, 0, data.Length); //передача данных всем
            }
        }

        // отключение всех клиентов
        public void Disconnect()
        {
            _tcpListener.Stop(); //остановка сервера

            for (var i = 0; i < _clients.Count; i++)
            {
                _clients[i].Close(); //отключение клиента
            }

            Environment.Exit(0); //завершение процесса
        }

        public abstract byte[] ExecuteQuery(string query);
    }
}