using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DataBaseEngine;

namespace SunflowerDB
{

    public class ClientObject
    {
        public TcpClient client;
        public DataBase core;

        public ClientObject(TcpClient tcpClient, DataBase db)
        {
            client = tcpClient;
            core = db;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                var data = new byte[64];
                while (true)
                {
                    var builder = new StringBuilder();
                    var bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    var query = builder.ToString();
                    var ans = core.SendSqlSequence(query);
                    ans.AnswerNotify.WaitOne();

                    data = Encoding.Unicode.GetBytes(ans.ToString());
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (client != null)
                {
                    client.Close();
                }
            }
        }
    }

    class ConsoleServer
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        static TcpListener listener;

        static void Main(string[] args)
        {
            var core = new DataBase(20, new DataBaseEngineMain());

            try
            {
                listener = new TcpListener(IPAddress.Parse(address), port);
                listener.Start();

                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    var clientObject = new ClientObject(client, core);

                    var clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }

            core.Dispose();
        }
    }
}


