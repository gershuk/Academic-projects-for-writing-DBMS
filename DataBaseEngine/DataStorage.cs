using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DataBaseType;

using ZeroFormatter;

namespace StorageEngine
{

    public interface IDataStorage
    {
        OperationResult<Table> LoadTable (Id tableName);
        OperationResult<bool> ContainsTable (Id tableName);
        OperationResult<string> AddTable (Table table);
        OperationResult<string> RemoveTable (Id tableName);

        OperationResult<Tuple<long, long>> UpdateAllRow (Id tableName, Row newRow, Predicate<Row> match);
        OperationResult<Tuple<long, long>> UpdateAllRow (Id tableName, Func<Row, Row> match);
        OperationResult<Tuple<long, long>> InsertRow (Id tableName, Row fields);
        OperationResult<Tuple<long, long>> RemoveAllRow (Id tableName, Predicate<Row> match);
    }

    public class DataStorageInFiles : IDataStorage
    {
        public string PathToDataBase { get; set; }

        private const string _fileExtension = ".tdb";
        private readonly int _blockSize = 4096;
        public DataStorageInFiles (string path, int blockSize)
        {
            _blockSize = blockSize;
            PathToDataBase = path;
            if (!Directory.Exists(path))
            {
                CreateDataStorageFolder(path);
            }
        }

        public OperationResult<Table> LoadTable (Id tableName)
        {
            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<Table>(ExecutionState.failed, null, new TableNotExistError(FullTableName(tableName)));
            }
            Table table;
            using (var tManager = new TableFileManager(new FileStream(GetTableFileName(tableName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096)))
            {
                table = tManager.LoadTable();
                table.TableData = new DataStorageRowsInFiles(GetTableFileName(tableName));
            }

            return new OperationResult<Table>(ExecutionState.performed, table);
        }
        private static string FullTableName (Id tableName) => tableName.ToString();

        public OperationResult<bool> ContainsTable (Id tableName) => File.Exists(GetTableFileName(tableName)) ? new OperationResult<bool>(ExecutionState.performed, true)
                                                                                                                       : new OperationResult<bool>(ExecutionState.failed, false, new TableNotExistError(FullTableName(tableName)));



        public OperationResult<string> AddTable (Table table)
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));
            if (File.Exists(GetTableFileName(table.TableMetaInf.Name)))
            {
                return new OperationResult<string>(ExecutionState.failed, null, new TableNotExistError(FullTableName(table.TableMetaInf.Name)));
            }
            using var tManager = new TableFileManager(new FileStream(GetTableFileName(table.TableMetaInf.Name), FileMode.Create), table, _blockSize);
            return new OperationResult<string>(ExecutionState.performed, "");
        }

        public OperationResult<string> RemoveTable (Id tableName)
        {
            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<string>(ExecutionState.failed, null, new TableNotExistError(FullTableName(tableName)));
            }

            File.Delete(GetTableFileName(tableName));

            return new OperationResult<string>(ExecutionState.performed, "");
        }

        public OperationResult<Tuple<long,long>> UpdateAllRow (Id tableName, Row newRow, Predicate<Row> match)
        {
            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<Tuple<long, long>>(ExecutionState.failed, new Tuple<long, long>(0, 0), new TableNotExistError(FullTableName(tableName)));
            }
            using var manager = new TableFileManager(new FileStream(GetTableFileName(tableName), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096));

            using var tableData = new DataStorageRowsInFilesEnumerator(manager);
                var isnLast = tableData.MoveNext();
                while (isnLast)
                {
                    isnLast = match(tableData.Current) ? tableData.UpdateCurrentRow(newRow): tableData.MoveNext();

                }
            

            return new OperationResult<Tuple<long, long>>(ExecutionState.performed, new Tuple<long,long>(manager.readCount, manager.writeCount));
        }
        public OperationResult<Tuple<long, long>> UpdateAllRow (Id tableName, Func<Row, Row> match)
        {
            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<Tuple<long, long>>(ExecutionState.failed,new Tuple<long, long>(0,0), new TableNotExistError(FullTableName(tableName)));
            }

            using var manager  = new TableFileManager(new FileStream(GetTableFileName(tableName), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096));

            using var tableData = new DataStorageRowsInFilesEnumerator(manager);
                var isnLast = tableData.MoveNext();
                while (isnLast)
                {
                    var data = match(tableData.Current);
                    isnLast = data != null ? tableData.UpdateCurrentRow(data) : tableData.MoveNext();
                }
            

            return new OperationResult<Tuple<long, long>>(ExecutionState.performed, new Tuple<long, long>(manager.readCount, manager.writeCount));
        }

        public OperationResult<Tuple<long, long>> InsertRow (Id tableName, Row fields)
        {
            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<Tuple<long, long>>(ExecutionState.failed, new Tuple<long, long>(0, 0), new TableNotExistError(FullTableName(tableName)));
            }

            using var manager = new TableFileManager(new FileStream(GetTableFileName(tableName), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096));

            var rowRecord = new RowRecord(fields);
                manager.InsertRecord(rowRecord);
            

            return new OperationResult<Tuple<long, long>>(ExecutionState.performed, new Tuple<long, long>(manager.readCount, manager.writeCount));
        }

        public OperationResult<Tuple<long, long>> RemoveAllRow (Id tableName, Predicate<Row> match)
        {

            if (!File.Exists(GetTableFileName(tableName)))
            {
                return new OperationResult<Tuple<long, long>>(ExecutionState.failed, null, new TableNotExistError(FullTableName(tableName)));
            }

            using var manager = new TableFileManager(new FileStream(GetTableFileName(tableName), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096));

            using var tableData = new DataStorageRowsInFilesEnumerator(manager);
                var isnLast = tableData.MoveNext();
                while (isnLast)
                {
                    isnLast = match(tableData.Current) ? tableData.DeleteCurrentRow() : tableData.MoveNext();
                }
            

            return new OperationResult<Tuple<long, long>>(ExecutionState.performed, new Tuple<long, long>(manager.readCount, manager.writeCount));
        }

        private void CreateDataStorageFolder (string path) => Directory.CreateDirectory(path);

        private string GetTableFileName (Id tableName) => PathToDataBase + "/" + FullTableName(tableName) + _fileExtension;

    }

    internal class TableFileManagerDataBlockNodeEnumerator : IEnumerator<DataBlockNode>
    {
        object IEnumerator.Current => throw new NotImplementedException();

        public DataBlockNode Current { get; private set; }
        public int CurrentOffset { get; set; }

        private readonly TableFileManager _tManager;
        private bool _disposed = false;

        public TableFileManagerDataBlockNodeEnumerator (TableFileManager tManager_)
        {
            _tManager = tManager_;
            Reset();
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose (bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _tManager.Dispose();
                // Free any other managed objects here.
                //
            }

            _disposed = true;
        }

        public bool MoveNext ()
        {
            if (Current == null)
            {
                CurrentOffset = _tManager.metaInfDataStorage.HeadDataBlockList;
                Current = _tManager.LoadHeadDataBlock();
                return Current != null;
            }

            if (Current.NextBlock != 0)
            {
                CurrentOffset = Current.NextBlock;
                Current = _tManager.LoadDataBlock(Current.NextBlock);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset () => CurrentOffset = _tManager.metaInfDataStorage.HeadDataBlockList;// Current = tManager.LoadHeadDataBlock();
    }

    internal class TableFileManager : IDisposable
    {
        private readonly FileStream _fileStream;
        public int RowRecordSize => metaInfDataStorage.RowRecordSize;
        public MetaInfDataStorage metaInfDataStorage;
        public long writeCount = 0;
        public long readCount = 0;
        public TableFileManager (FileStream fileStream)
        {
            _fileStream = fileStream;
            metaInfDataStorage = LoadMetaInfStorage();
        }

        public TableFileManager (FileStream fs_, Table table, int blockSize)
        {
            _fileStream = fs_;
            using var memStream = new MemoryStream();
            ZeroFormatterSerializer.Serialize(memStream, table.TableMetaInf);
            var metaInfStorage = new MetaInfDataStorage { TableMetaInfSize = (int)memStream.Length, RowRecordSize = CalculateRowRecordSize(table), DataBlockSize = blockSize, HeadDataBlockList = 0, HeadFreeBlockList = 0 };
            memStream.WriteTo(_fileStream);
            CreateMetaInfInEnd(metaInfStorage);
            metaInfDataStorage = LoadMetaInfStorage();
        }

        private int CalculateDataBlockNodeSize ()
        {
            using var memStream = new MemoryStream();
            var dataBlock = new DataBlockNode(0, 0, 1);
            ZeroFormatterSerializer.Serialize(memStream, dataBlock);
            return metaInfDataStorage.DataBlockSize - (int)memStream.Length + 1;
        }

        private static int GetCalculateMetaInfDataStorageSize ()
        {
            using var memStream = new MemoryStream();

            var dataBlock = new MetaInfDataStorage
            {
                DataBlockSize = int.MaxValue,
                RowRecordSize = int.MaxValue,
                HeadDataBlockList = int.MaxValue,
                HeadFreeBlockList = int.MaxValue,
                TableMetaInfSize = int.MaxValue
            };

            ZeroFormatterSerializer.Serialize(memStream, dataBlock);

            return (int)memStream.Length;
        }

        private int CalculateRowRecordSize (Table table)
        {
            using var memStream = new MemoryStream();
            var rowRecord = new RowRecord(table.CreateDefaultRow().Result);
            ZeroFormatterSerializer.Serialize(memStream, rowRecord);
            return (int)memStream.Length;
        }

        public void InsertRecord (RowRecord rowRecord)
        {

            var dataBlock = LoadDataBlock(metaInfDataStorage.HeadDataBlockList);

            if (dataBlock == null || !dataBlock.InsertRecord(rowRecord, metaInfDataStorage.RowRecordSize))
            {
                MoveNewBlockToHead();
                dataBlock = LoadDataBlock(metaInfDataStorage.HeadDataBlockList);
                dataBlock.InsertRecord(rowRecord, metaInfDataStorage.RowRecordSize);
            }
            SaveDataBlock(dataBlock, metaInfDataStorage.HeadDataBlockList);
        }

        private void CreateMetaInfInEnd (MetaInfDataStorage meta)
        {
            _fileStream.Seek(0, SeekOrigin.End);
            ZeroFormatterSerializer.Serialize(_fileStream, meta);
            metaInfDataStorage = LoadMetaInfStorage();
        }

        private void SaveMetaInfStorage (MetaInfDataStorage meta)
        {
            _fileStream.Seek(-GetCalculateMetaInfDataStorageSize(), SeekOrigin.End);
            ZeroFormatterSerializer.Serialize(_fileStream, meta);
            metaInfDataStorage = LoadMetaInfStorage();
            writeCount++;
        }

        private MetaInfDataStorage LoadMetaInfStorage ()
        {
            _fileStream.Seek(-GetCalculateMetaInfDataStorageSize(), SeekOrigin.End);
            readCount++;
            return ZeroFormatterSerializer.Deserialize<MetaInfDataStorage>(_fileStream);
            
        }

        public void DeleteBlock (DataBlockNode block)
        {
            var nextBlock = LoadDataBlock(block.NextBlock);
            var prevBlock = LoadDataBlock(block.PrevBlock);
            var curBlockOff = prevBlock == null ? metaInfDataStorage.HeadDataBlockList : prevBlock.NextBlock;

            if (nextBlock != null)
            {
                nextBlock.PrevBlock = block.PrevBlock;
                SaveDataBlock(nextBlock, block.NextBlock);
            }

            if (prevBlock != null)
            {
                prevBlock.NextBlock = block.NextBlock;
                SaveDataBlock(prevBlock, block.PrevBlock);
            }
            else
            {
                metaInfDataStorage.HeadDataBlockList = block.NextBlock;
            }

            if (metaInfDataStorage.HeadFreeBlockList == 0)
            {

                metaInfDataStorage.HeadFreeBlockList = curBlockOff;
                block = new DataBlockNode(0, 0, CalculateDataBlockNodeSize());
            }
            else
            {
                var prevDelBlock = LoadDataBlock(metaInfDataStorage.HeadFreeBlockList);
                block = new DataBlockNode(0, metaInfDataStorage.HeadFreeBlockList, CalculateDataBlockNodeSize());
                metaInfDataStorage.HeadFreeBlockList = curBlockOff;
                prevDelBlock.PrevBlock = metaInfDataStorage.HeadFreeBlockList;
                SaveDataBlock(prevDelBlock, block.NextBlock);
            }

            SaveDataBlock(block, metaInfDataStorage.HeadFreeBlockList);
            SaveMetaInfStorage(metaInfDataStorage);
        }

        public void MoveNewBlockToHead ()
        {
            if (metaInfDataStorage.HeadFreeBlockList == 0)
            {
                CreateAndAddDataBlock();
            }
            else
            {
                var oldBlock = LoadDataBlock(metaInfDataStorage.HeadDataBlockList);
                var oldBlockOffset = metaInfDataStorage.HeadDataBlockList;
                var deletedBlock = LoadDataBlock(metaInfDataStorage.HeadFreeBlockList);
                var deletedBlockNext = LoadDataBlock(deletedBlock.NextBlock);
                var delBlockOffset = metaInfDataStorage.HeadFreeBlockList;
                var delBlockNextOffset = deletedBlock.NextBlock;

                if (deletedBlockNext != null)
                {
                    deletedBlockNext.PrevBlock = 0;
                    SaveDataBlock(deletedBlockNext, delBlockNextOffset);
                }

                metaInfDataStorage.HeadFreeBlockList = delBlockNextOffset;
                deletedBlock.NextBlock = metaInfDataStorage.HeadDataBlockList;
                oldBlock.PrevBlock = delBlockOffset;
                metaInfDataStorage.HeadDataBlockList = delBlockOffset;

                SaveDataBlock(deletedBlock, metaInfDataStorage.HeadDataBlockList);
                SaveDataBlock(oldBlock, oldBlockOffset);
                SaveMetaInfStorage(metaInfDataStorage);
            }
        }
        public void SaveDataBlock (DataBlockNode block, int offset)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            //using var memStream = new MemoryStream();
            block = new DataBlockNode(block);
            ZeroFormatterSerializer.Serialize(_fileStream, block);
            //memStream.CopyTo(fs);
            _fileStream.Flush(true);
            writeCount++;
        }
        public void CreateAndAddDataBlock ()
        {
            var metaInf = metaInfDataStorage;
            DataBlockNode newBlock;

            if (metaInf.HeadDataBlockList != 0)
            {
                var prevBlock = LoadDataBlock(metaInf.HeadDataBlockList);
                newBlock = new DataBlockNode(0, metaInf.HeadDataBlockList, CalculateDataBlockNodeSize());
                metaInf.HeadDataBlockList = (int)_fileStream.Seek(-GetCalculateMetaInfDataStorageSize(), SeekOrigin.End);
                prevBlock.PrevBlock = metaInf.HeadDataBlockList;
                SaveDataBlock(prevBlock, newBlock.NextBlock);
            }
            else
            {
                newBlock = new DataBlockNode(0, 0, CalculateDataBlockNodeSize());
                metaInf.HeadDataBlockList = (int)_fileStream.Seek(-GetCalculateMetaInfDataStorageSize(), SeekOrigin.End);
            }

            SaveDataBlock(newBlock, metaInf.HeadDataBlockList);
            CreateMetaInfInEnd(metaInf);
        }

        public DataBlockNode LoadDataBlock (int offset)
        {
            if (offset == 0)
            {
                return null;
            }

            _fileStream.Seek(offset, SeekOrigin.Begin);
            var buffer = new byte[metaInfDataStorage.DataBlockSize];
            _fileStream.Read(buffer, 0, metaInfDataStorage.DataBlockSize);

            using var memStream = new MemoryStream();
            memStream.Write(buffer, 0, buffer.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            readCount++;
            var block = ZeroFormatterSerializer.Deserialize<DataBlockNode>(memStream);
            block.FilePtr = offset;
            return block;
        }

        public DataBlockNode LoadHeadDataBlock () => LoadDataBlock(metaInfDataStorage.HeadDataBlockList);

        public TableFileManagerDataBlockNodeEnumerator GetBlockEnumarator () => new TableFileManagerDataBlockNodeEnumerator(this);

        public Table LoadTable ()
        {
            var table = new Table();
            _fileStream.Seek(0, SeekOrigin.Begin);
            var rawTable = new byte[metaInfDataStorage.TableMetaInfSize];
            _fileStream.Read(rawTable, 0, rawTable.Length);
            table.TableMetaInf = ZeroFormatterSerializer.Deserialize<TableMetaInf>(rawTable);
            //table.TableData = new DataStorageRowsInFiles(this);
            return table;
        }

        public void Dispose () => _fileStream.Dispose();
    }
    [ZeroFormattable]
    public class DataBlockNode
    {
        [Index(0)]
        public virtual int CountRealRecords { get; set; } = 0;

        [Index(1)]
        public virtual int CountNotDeletedRecords { get; set; } = 0;

        [Index(2)]
        public virtual int NextBlock { get; set; } = 0;

        [Index(3)]
        public virtual int PrevBlock { get; set; } = 0;

        [Index(4)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Ожидание>")]
        public virtual byte[] Data { get; set; } = null;

        [IgnoreFormat]
        public long FilePtr { get; set; } = 0;

        public DataBlockNode ()
        {

        }

        public DataBlockNode (DataBlockNode from)
        {
            if (from is null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            CountRealRecords = from.CountRealRecords;
            CountNotDeletedRecords = from.CountNotDeletedRecords;
            NextBlock = from.NextBlock;
            PrevBlock = from.PrevBlock;
            Data = from.Data;
        }

        public DataBlockNode (int prevBlock, int nextBlock, int dataSize)
        {
            PrevBlock = prevBlock;
            NextBlock = nextBlock;
            Data = Enumerable.Repeat((byte)0x33, dataSize).ToArray();
            Data[0] = 77;
            CountRealRecords = 0;
            CountNotDeletedRecords = 0;
        }
        public bool InsertRecord (RowRecord record, int recordSize)
        {
            if (CountRealRecords * recordSize + recordSize > Data.Length)
            {
                return false;
            }

            SaveRecord(record, CountRealRecords, recordSize);
            CountRealRecords++;
            CountNotDeletedRecords++;
            return true;
        }
        public RowRecord LoadRowRecord (int pos, int recordSize)
        {
            if (pos < CountRealRecords)
            {
                using var memStream = new MemoryStream(Data);
                memStream.Seek(pos * recordSize, SeekOrigin.Begin);
                var recordBytes = new byte[recordSize];
                memStream.Read(recordBytes, 0, recordBytes.Length);
                var record = ZeroFormatterSerializer.Deserialize<RowRecord>(recordBytes);
                record.FilePtrBlock = FilePtr;
                record.InBlockPos = pos;
                return record;
            }
            else
            {
                return null;
            }
        }
        public void SaveRecord (RowRecord record, int pos, int recordSize)
        {
            using var memStream = new MemoryStream(Data);
            memStream.Seek(pos * recordSize, SeekOrigin.Begin);
            var buffer = new byte[recordSize];
            ZeroFormatterSerializer.Serialize(ref buffer, 0, record);
            memStream.Write(buffer, 0, buffer.Length);
        }
        public bool DeleteRecord (int pos, int recordSize)
        {
            var rowRecord = LoadRowRecord(pos, recordSize);
            rowRecord.IsDeleted = true;
            SaveRecord(rowRecord, pos, recordSize);
            CountNotDeletedRecords--;
            return CountNotDeletedRecords == 0;
        }
        public bool UpdateRecord (RowRecord newRecord, int pos, int recordSize)
        {
            SaveRecord(newRecord, pos, recordSize);
            return true;
        }
        public RecordsInDataBlockNodeEnumarator GetRowRecrodsEnumerator (int recordSize) => new RecordsInDataBlockNodeEnumarator(this, recordSize);
    }


    [ZeroFormattable]
    public class RowRecord
    {
        [Index(0)]
        public virtual bool IsDeleted { get; set; }

        [Index(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Ожидание>")]
        public virtual Row Fields { get; set; }
        [IgnoreFormat]
        public long FilePtrBlock { get; set; } = -1;
        [IgnoreFormat]
        public long InBlockPos { get; set; } = -1;

        public RowRecord ()
        {

        }

        public RowRecord (Row fields)
        {
            Fields = fields;
            IsDeleted = false;
        }
    }

    public class RecordsInDataBlockNodeEnumarator : IEnumerator<RowRecord>
    {
        public RowRecord Current { get; private set; }
        object IEnumerator.Current => throw new NotImplementedException();

        private readonly DataBlockNode _dataBlock;
        private readonly int _recordSize;
        private int _curPos;
        private bool _disposed = false;

        public RecordsInDataBlockNodeEnumarator (DataBlockNode dataBlock, int recordSize)
        {
            _dataBlock = dataBlock;
            _recordSize = recordSize;
            Reset();
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose (bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
            }

            _disposed = true;
        }

        public bool MoveNext ()
        {
            _curPos++;
            Current = _dataBlock.LoadRowRecord(_curPos, _recordSize);
            return Current != null ? Current.IsDeleted ? MoveNext() : true : false;
        }

        public bool DeleteCurRow () => _dataBlock.DeleteRecord(_curPos, _recordSize);

        public bool UpdateCurRow (RowRecord rowRecord) => _dataBlock.UpdateRecord(rowRecord, _curPos, _recordSize);

        public void Reset () => _curPos = -1;
    }

    internal class DataStorageRowsInFiles : IEnumerable<Row>
    {
        // private TableFileManager _tManager;
        private readonly string _tableFileName;
        private readonly int _blockSize;
        public DataStorageRowsInFiles (string fileName, int bufferSize = 4096) { _tableFileName = fileName; _blockSize = bufferSize; }
        public IEnumerator<Row> GetEnumerator () => new DataStorageRowsInFilesEnumerator(new TableFileManager(new FileStream(_tableFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, _blockSize)));

        IEnumerator IEnumerable.GetEnumerator () => throw new NotImplementedException();

    }
    internal class DataStorageRowsInFilesWithWrite : IEnumerable<Row>
    {
        // private TableFileManager _tManager;
        private readonly string _tableFileName;
        private readonly int _blockSize;
        public DataStorageRowsInFilesWithWrite (string fileName, int bufferSize) { _tableFileName = fileName; _blockSize = bufferSize; }
        public IEnumerator<Row> GetEnumerator () => new DataStorageRowsInFilesEnumerator(new TableFileManager(new FileStream(_tableFileName, FileMode.Open,FileAccess.ReadWrite,FileShare.Read, _blockSize)));

        IEnumerator IEnumerable.GetEnumerator () => throw new NotImplementedException();

    }

    internal class DataStorageRowsInFilesEnumerator : IEnumerator<Row>
    {
        public Row Current { get; private set; }
        object IEnumerator.Current => throw new NotImplementedException();

        public readonly TableFileManager _tManager;
        private TableFileManagerDataBlockNodeEnumerator _blocks;
        private RecordsInDataBlockNodeEnumarator _curRowRecordsEnumarator;
        private bool _disposed = false;

        public DataStorageRowsInFilesEnumerator (TableFileManager tManager)
        {
            _tManager = tManager;
            Reset();

        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose (bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _tManager.Dispose();
                _blocks.Dispose();
                if (_curRowRecordsEnumarator != null)
                {
                    _curRowRecordsEnumarator.Dispose();
                }

            }

            _disposed = true;
        }

        public bool UpdateCurrentRow (Row newRow)
        {
            _curRowRecordsEnumarator.UpdateCurRow(new RowRecord(newRow));
            _tManager.SaveDataBlock(_blocks.Current, _blocks.CurrentOffset);
            return MoveNext();
        }

        public bool DeleteCurrentRow ()
        {
            var res = _curRowRecordsEnumarator.DeleteCurRow();
            var prevBlock = _blocks.Current;

            if (res)
            {
                _tManager.DeleteBlock(prevBlock);
            }
            else
            {
                _tManager.SaveDataBlock(_blocks.Current, _blocks.CurrentOffset);
            }

            return MoveNext();
        }

        public bool MoveNext ()
        {
            if (_curRowRecordsEnumarator == null)
            {
                var res = _blocks.MoveNext();
                if (!res)
                {
                    return res;
                }
                _curRowRecordsEnumarator = _blocks.Current.GetRowRecrodsEnumerator(_tManager.RowRecordSize);
            }
            if (_curRowRecordsEnumarator.MoveNext())
            {
                Current = _curRowRecordsEnumarator.Current.Fields;
                Current.FilePtrBlock = _curRowRecordsEnumarator.Current.FilePtrBlock;
                Current.InBlockPos = _curRowRecordsEnumarator.Current.InBlockPos;
                return true;
            }
            else
            {
                if (_blocks.MoveNext())
                {
                    _curRowRecordsEnumarator = _blocks.Current.GetRowRecrodsEnumerator(_tManager.RowRecordSize);
                    _curRowRecordsEnumarator.MoveNext();
                    Current = _curRowRecordsEnumarator.Current.Fields;
                    Current.FilePtrBlock = _curRowRecordsEnumarator.Current.FilePtrBlock;
                    Current.InBlockPos = _curRowRecordsEnumarator.Current.InBlockPos;
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public void Reset ()
        {
            _blocks = _tManager.GetBlockEnumarator();
            _curRowRecordsEnumarator = null;
            Current = null;
        }
    }
    [ZeroFormattable]
    public class MetaInfDataStorage
    {
        [Index(0)]
        public virtual int TableMetaInfSize { get; set; } = 0;

        [Index(1)]
        public virtual int RowRecordSize { get; set; } = 0;

        [Index(2)]
        public virtual int DataBlockSize { get; set; } = 0;

        [Index(3)]
        public virtual int HeadFreeBlockList { get; set; } = 0;

        [Index(4)]
        public virtual int HeadDataBlockList { get; set; } = 0;
    }


}
