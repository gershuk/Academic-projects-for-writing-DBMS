using System;
using System.Collections.Generic;
using System.Text;
using DataBaseTable;
using Newtonsoft.Json;
using System.IO;

namespace DataBaseEngine
{
    public interface IDataStorage
    {
        OperationResult<Dictionary<string, Table>> LoadTablePoolMetaInf();
        OperationResult<string> SaveTablePoolMetaInf(Dictionary<string, Table> tablePool);
        OperationResult<string> SaveTableData(Table table);
    }
    public class DataStorageInFiles : IDataStorage
    {
        public string PathToTableMetaInf { get; set; }
        private const string _fileMarkTableMetaInf = "DATA_BASE_TABLE_METAINF_FILE";
        public DataStorageInFiles(string path)
        {
            PathToTableMetaInf = path;
        }
        public OperationResult<Dictionary<string, Table>> LoadTablePoolMetaInf()
        {
            var path = PathToTableMetaInf;
            Dictionary<string, Table> TablePool;
            if (!File.Exists(path))
            {
                using (var sw = new StringWriter())
                {
                    sw.WriteLine("Error LoadTablePool, File named {0} doesn't exist ", path);
                    return new OperationResult<Dictionary<string, Table>>(OperationExecutionState.failed, null);
                }
            }

            using (var sr = new StreamReader(path))
            {
                if (sr.ReadLine() != _fileMarkTableMetaInf)
                {
                    using (var sw = new StringWriter())
                    {
                        sw.WriteLine("Error LoadTablePool, File named {0} doesn't contain 'file mark' '{1}'  ", path, _fileMarkTableMetaInf);
                        return new OperationResult<Dictionary<string, Table>>(OperationExecutionState.failed, null);
                    }
                }

                TablePool = new Dictionary<string, Table>();
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    var table = new Table();
                    table.LoadTableMetaInf(line);
                    TablePool.Add(table.TableMetaInf.Name, table);
                }
            }

            return new OperationResult<Dictionary<string, Table>>(OperationExecutionState.performed, TablePool);
        }

        public OperationResult<string> SaveTablePoolMetaInf(Dictionary<string, Table> tablePool)
        {
            using (var sw = new StreamWriter(PathToTableMetaInf))
            {
                sw.WriteLine(_fileMarkTableMetaInf);
                foreach (var keyValue in tablePool)
                {
                    sw.Write(keyValue.Value.SerializeTableMetaInf().Result);
                    sw.WriteLine("");
                }
            }

            return new OperationResult<string>(OperationExecutionState.performed, "ok");
        }

        public OperationResult<string> SaveTableData(Table table) => throw new NotImplementedException();
    }

}
