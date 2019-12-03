using System;
using System.Net.Sockets;
using System.Text;

namespace ConsoleClientServer
{
    public interface IListener
    {
        void Process();
    }

    public class Listener : IListener
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }

        private readonly TcpClient _client;
        private readonly Server _server; // объект сервера

        public Listener(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            _client = tcpClient;
            _server = serverObject;
            serverObject?.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = _client.GetStream();
                string message;

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        var data = _server.ExecuteQuery(message);
                        Stream.Write(data, 0, data.Length); //передача данных всем
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                _server.RemoveConnection(Id);
                Close();
            }
        }

        private string GetMessage()
        {
            var data = new byte[128];
            var builder = new StringBuilder();
            var bytes = 0;

            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
            {
                Stream.Close();
            }

            if (_client != null)
            {
                _client.Close();
            }
        }
    }
}