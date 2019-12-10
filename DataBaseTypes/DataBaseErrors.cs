
using ProtoBuf;

namespace DataBaseType
{
    [ProtoContract]
    [ProtoInclude(2, typeof(FileNotExistError))]
    [ProtoInclude(3, typeof(DataBaseIsCorruptError))]
    [ProtoInclude(4, typeof(FileMarkNotExistError))]
    [ProtoInclude(5, typeof(TableNotExistError))]
    [ProtoInclude(6, typeof(TableAlreadyExistError))]
    [ProtoInclude(7, typeof(ColumnAlreadyExistError))]
    [ProtoInclude(8, typeof(ColumnNotExistError))]
    [ProtoInclude(9, typeof(CastFieldError))]
    [ProtoInclude(10, typeof(ParsingRequestError))]
    public abstract class DBError
    {
        [ProtoMember(1)]
        public string Message { get; set; }

        protected DBError ()
        { }

        public DBError (string message) => Message = message;

        public override string ToString () => Message;
    }

    [ProtoContract]
    public class FileNotExistError : DBError
    {
        public FileNotExistError ()
        {
        }

        public FileNotExistError (string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    [ProtoContract]
    public class NullError : DBError
    {
        public NullError ()
        {
        }

        public NullError (string mess)
            : base($"{mess} is null")
        { }
    }

    [ProtoContract]
    public class DataBaseIsCorruptError : DBError
    {
        public DataBaseIsCorruptError ()
        {
        }

        public DataBaseIsCorruptError (string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }

    [ProtoContract]
    public class FileMarkNotExistError : DBError
    {
        public FileMarkNotExistError ()
        {
        }

        public FileMarkNotExistError (string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }

    [ProtoContract]
    public class TableNotExistError : DBError
    {
        public TableNotExistError ()
        {
        }

        public TableNotExistError (string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }

    [ProtoContract]
    public class TableAlreadyExistError : DBError
    {
        public TableAlreadyExistError ()
        {
        }

        public TableAlreadyExistError (string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }

    [ProtoContract]
    public class ColumnAlreadyExistError : DBError
    {
        public ColumnAlreadyExistError ()
        {
        }

        public ColumnAlreadyExistError (string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }

    [ProtoContract]
    public class ColumnNotExistError : DBError
    {
        public ColumnNotExistError ()
        {
        }

        public ColumnNotExistError (string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }

    [ProtoContract]
    public class CastFieldError : DBError
    {
        public CastFieldError ()
        {
        }

        public CastFieldError (string columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }

    [ProtoContract]
    public class ParsingRequestError : DBError
    {
        public ParsingRequestError ()
        {
        }

        public ParsingRequestError (string message, string errorLocation)
            : base($"{message} {errorLocation}")
        { }
    }
}
