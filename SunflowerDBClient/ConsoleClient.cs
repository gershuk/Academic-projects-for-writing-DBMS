using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleClientServer;

namespace SunflowerDBClient
{
    public class ConsoleClient
    {
        private static void Main(string[] args)
        {
            const string BaseHost = "127.0.0.1";
            const int BasePort = 8888;
            Client client;

            if (args.Length < 2)
            {
                client = new Client(BaseHost, BasePort);
            }
            else
            {

                client = new Client(args[0], int.Parse(args[1]));
            }

            client.SendResieveMessage();
            client.Dispose();
        }
    }
}
