using System;
using System.Text;
using ConsoleClientServer;
using DataBaseType;

namespace SunflowerDBClient
{
    public class SunflowerDBClient : Client
    {
        public SunflowerDBClient (string host, int port) : base(host, port)
        {
        }

        public override string ConvertMessageToString(byte[] value)
        {
            return Encoding.ASCII.GetString(value);
            throw new NotImplementedException();/*
            var value = //ToDo
            var result = "";
            switch (value.State)
            {
                case OperationExecutionState.notProcessed:
                    break;
                case OperationExecutionState.parserError:
                case OperationExecutionState.failed:
                    result += value.State + "\n";
                    result += value.OperationException + "\n";
                    result += "\n";
                    break;
                case OperationExecutionState.performed:
                    foreach (var info in value.Result.Answer)
                    {
                        result += info.ToString() + "\n";
                        result += "\n";
                    }
                    break;
            }
            return result.ToString();
            */
        }

        internal class ConsoleClient
        {
            private static void Main (string[] args)
            {
                var BaseHost = "127.0.0.1";
                var BasePort = 8888;
                Client client;

                if (args.Length == 2)
                {
                    BaseHost = args[0];
                    BasePort = int.Parse(args[1]);
                }

                client = new SunflowerDBClient(BaseHost, BasePort);

                client.SendResieveMessage();
                client.Dispose();
            }
        }
    }
}