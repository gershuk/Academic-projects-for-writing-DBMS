using ConsoleClientServer;

using DataBaseType;

using ProtoBuf;

namespace SunflowerDBClient
{
    public class SunflowerDBClient : Client
    {
        public SunflowerDBClient (string host, int port) : base(host, port)
        {
        }

        public override string ConvertMessageToString (byte[] messege)
        {
            var value = Serializer.Deserialize<OperationResult<SqlSequenceResult>>(messege);
            var result = "";
            switch (value.State)
            {
                case ExecutionState.notProcessed:
                    break;
                case ExecutionState.parserError:
                case ExecutionState.failed:
                    result += "Error" + "\n";
                    result += value.OperationError + "\n";
                    break;
                case ExecutionState.performed:
                    foreach (var info in value.Result.Answer)
                    {
                        result += info.ToString() + "\n";
                        result += "\n";
                    }
                    break;
            }
            result += "*";
            return result.ToString();
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