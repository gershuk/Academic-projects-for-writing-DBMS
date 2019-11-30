using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using DataBaseType;

using Newtonsoft.Json;

using ProtoBuf;

namespace DataBaseEngine
{
    public interface IDataStorage
    {
        OperationResult<Dictionary<string, Table>> LoadTablePoolMetaInf();
        OperationResult<string> SaveTablePoolMetaInf(Dictionary<string, Table> tablePool);
        OperationResult<string> SaveTableData(Table table);
        OperationResult<string> InsertToEndTableData(Table table, List<Dictionary<string, Field>> rows);
        OperationResult<string> LoadTableData(Table table);
    }

    public class DataStorageInFiles : IDataStorage
    {
        public string PathToDataBase { get; set; }
        private const string _fileMarkTableMetaInf = "DATA_BASE_TABLE_METAINF_FILE";
        private const string _fileTableMetaInf = "TablesMetaInf.json";

        public DataStorageInFiles(string path) => PathToDataBase = path;

        public OperationResult<Dictionary<string, Table>> LoadTablePoolMetaInf()
        {
            Dictionary<string, Table> TablePool;

            if (!File.Exists(PathToDataBase))
            {
                return new OperationResult<Dictionary<string, Table>>
                                 (OperationExecutionState.failed, null, new FileNotExistExeption(PathToDataBase));
            }

            using (var zipToOpen = new FileStream(PathToDataBase, FileMode.OpenOrCreate))
            {
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

                if (!archive.Entries.Any(u => u.FullName == _fileTableMetaInf))
                {
                    return new OperationResult<Dictionary<string, Table>>
                               (OperationExecutionState.failed, null, new DataBaseIsCorruptExeption(PathToDataBase));
                }

                using var sr = new StreamReader(archive.GetEntry(_fileTableMetaInf).Open());

                if (sr.ReadLine() != _fileMarkTableMetaInf)
                {
                    return new OperationResult<Dictionary<string, Table>>(OperationExecutionState.failed, null, new FileMarkNotExistExeption(PathToDataBase, _fileMarkTableMetaInf));
                }

                TablePool = new Dictionary<string, Table>();

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var table = new Table
                    {
                        TableMetaInf = JsonConvert.DeserializeObject<TableMetaInf>(line)
                    };
                    TablePool.Add(table.TableMetaInf.Name.ToString(), table);
                }
            }

            return new OperationResult<Dictionary<string, Table>>(OperationExecutionState.performed, TablePool);
        }

        public OperationResult<string> SaveTablePoolMetaInf(Dictionary<string, Table> tablePool)
        {
            using (var zipToOpen = new FileStream(PathToDataBase, FileMode.OpenOrCreate))
            {
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

                var entry = !archive.Entries.Any(u => u.FullName == _fileTableMetaInf)
                    ? archive.CreateEntry(_fileTableMetaInf)
                    : archive.GetEntry(_fileTableMetaInf);

                using var sw = new StreamWriter(entry.Open());

                sw.WriteLine(_fileMarkTableMetaInf);

                foreach (var keyValue in tablePool)
                {
                    sw.Write(JsonConvert.SerializeObject(keyValue.Value.TableMetaInf));
                    sw.WriteLine("");
                }

            }
            return new OperationResult<string>(OperationExecutionState.performed, "");
        }

        public OperationResult<string> SaveTableData(Table table)
        {
            if (table.TableData == null)
            {
                return new OperationResult<string>(OperationExecutionState.failed, null, new NullReferenceException());
            }

            using (var zipToOpen = new FileStream(PathToDataBase, FileMode.OpenOrCreate))
            {
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

                ZipArchiveEntry entry;

                var fileName = table.TableMetaInf.Name + ".bin";

                entry = !archive.Entries.Any(u => u.FullName == fileName) ? archive.CreateEntry(fileName) : archive.GetEntry(fileName);

                using var sw = new BinaryWriter(entry.Open());

                foreach (var row in table.TableData.Rows)
                {
                    using var memStream = new MemoryStream();

                    Serializer.Serialize(memStream, row);
                    sw.Write((int)memStream.Length);
                    memStream.WriteTo(sw.BaseStream);
                }
            }

            return new OperationResult<string>(OperationExecutionState.performed, "");
        }

        public OperationResult<string> LoadTableData(Table table)
        {

            if (!File.Exists(PathToDataBase))
            {
                return new OperationResult<string>
                                 (OperationExecutionState.failed, null, new FileNotExistExeption(PathToDataBase));
            }

            var rows = new List<Dictionary<string, Field>>();

            using (var zipToOpen = new FileStream(PathToDataBase, FileMode.OpenOrCreate))
            {
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

                var fileName = table.TableMetaInf.Name + ".bin";

                if (!archive.Entries.Any(u => u.FullName == fileName))
                {

                    return new OperationResult<string>
                               (OperationExecutionState.failed, null, new DataBaseIsCorruptExeption(PathToDataBase));
                }

                using var sr = new BinaryReader(archive.GetEntry(fileName).Open());

                int len;

                try
                {
                    while ((len = sr.ReadInt32()) != 0)
                    {

                        var data = sr.ReadBytes(len);
                        var stream = new MemoryStream(data);
                        rows.Add(Serializer.Deserialize<Dictionary<string, Field>>(stream));
                    }
                }
                catch
                {

                }
            }

            table.TableData = new TableData
            {
                Rows = rows
            };

            return new OperationResult<string>(OperationExecutionState.performed, "");
        }

        public OperationResult<string> InsertToEndTableData(Table table, List<Dictionary<string, Field>> rows) => throw new NotImplementedException();
    }
}
