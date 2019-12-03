using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleClientServer;
using DataBaseEngine;
using TransactionManagement;

namespace SunflowerDB
{
    public class SunflowerDBServer : Server , IDisposable
    {
        private DataBase _core = new DataBase(20, new DataBaseEngineMain(), new TransactionScheduler());
        private bool _disposed=false;

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
                formatter.Serialize(fs, _core.ExecuteSqlSequence(query));
                return fs.ToArray();
            }
        }
    }
    public class ConsoleServer
    {
        private static void Main(string[] args)
        {
            Server _server = default; // сервер
            Thread _listenThread = default; // потока для прослушивания

            try
            {
                _server = new SunflowerDBServer();
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
