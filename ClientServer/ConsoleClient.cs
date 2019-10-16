 using System;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        
        static void Main(string[] args)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                var stream = client.GetStream();

                while (true)
                {
                    Console.Write(">> ");
                    var query = Console.ReadLine().Trim();
                    
                    if (query == "exit")
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(query))
                    {
                        continue;
                    }

                    var data = Encoding.Unicode.GetBytes(query);
                    stream.Write(data, 0, data.Length);

                    data = new byte[64];
                    var builder = new StringBuilder();
                    var bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    query = builder.ToString();
                    Console.WriteLine(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
        }
    }
}