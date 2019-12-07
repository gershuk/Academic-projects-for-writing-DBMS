using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ConsoleClientServer;
using DataBaseEngine;
using DataBaseType;
using TransactionManagement;
using ZeroFormatter;

namespace SunflowerDB
{
    public sealed class SunflowerDBServer : Server, IDisposable
    {
        private readonly DataBase _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
        private bool _disposed = false;

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _core.Dispose();
            }

            _disposed = true;
        }

        ~SunflowerDBServer()
        {
            Dispose(false);
        }

        public override byte[] ExecuteQuery(string query)
        {
            var formatter = new BinaryFormatter();
            using (var fs = new MemoryStream())
            {
                return ZeroFormatterSerializer.Serialize( _core.ExecuteSqlSequence(query));
            }
        }

        public override string ConvertMessageToString(byte[] messege)
        {
            return messege.ToString();
            throw new NotImplementedException();
            /*
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
    }
    public class ConsoleServer
    {
        private static void Main(string[] args)
        {
            SunflowerDBServer _server = default; // сервер
            Thread _listenThread = default; // потока для прослушивания

            try
            {
                _server = new SunflowerDBServer();
                _server.Listen();
            }
            catch (Exception ex)
            {
                _server?.Disconnect();
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _server.Dispose();
            }
        }
    }
}
