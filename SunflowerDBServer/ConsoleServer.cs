using System;
using System.IO;

using ConsoleClientServer;

using DataBaseEngine;

using DataBaseType;

using ProtoBuf;

using TransactionManagement;

namespace SunflowerDB
{
    public sealed class SunflowerDBServer : Server, IDisposable
    {
        private readonly DataBase _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
        private bool _disposed = false;

        public void Dispose ()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        private void Dispose (bool disposing)
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

        ~SunflowerDBServer ()
        {
            Dispose(false);
        }

        public override byte[] ExecuteQuery (string query)
        {
            using (var binaryData = new MemoryStream())
            {
                try
                {
                    Serializer.Serialize(binaryData, _core.ExecuteSqlSequence(query));
                }
                catch (Exception ex)
                {
                    var res = new OperationResult<SqlSequenceResult>
                    {
                        State = ExecutionState.failed,
                        OperationError = new DataBaseIsCorruptError("\b\b\b\b\b\bNot implemented")
                    };
                    Serializer.Serialize(binaryData, res);
                }
                return binaryData.ToArray();
            }
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
    }
    public class ConsoleServer
    {
        private static void Main (string[] args)
        {
            SunflowerDBServer _server = default; // сервер

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
