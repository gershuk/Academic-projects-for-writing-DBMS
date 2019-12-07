using System.Collections.Generic;

using ZeroFormatter;

namespace DataBaseType
{
    [ZeroFormattable]
    public abstract class DBError
    {
        [Index(0)]
        public virtual string Message { get; set; }

        protected DBError ()
        { }

        public DBError (string message) => Message = message;

        public override string ToString () => Message;
    }

    [ZeroFormattable]
    public class FileNotExistError : DBError
    {
        public FileNotExistError ()
        {
        }

        public FileNotExistError (string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    [ZeroFormattable]
    public class DataBaseIsCorruptError : DBError
    {
        public DataBaseIsCorruptError ()
        {
        }

        public DataBaseIsCorruptError (string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }

    [ZeroFormattable]
    public class FileMarkNotExistError : DBError
    {
        public FileMarkNotExistError ()
        {
        }

        public FileMarkNotExistError (string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }

    [ZeroFormattable]
    public class TableNotExistError : DBError
    {
        public TableNotExistError ()
        {
        }

        public TableNotExistError (string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }

    [ZeroFormattable]
    public class TableAlreadyExistError : DBError
    {
        public TableAlreadyExistError ()
        {
        }

        public TableAlreadyExistError (string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }

    [ZeroFormattable]
    public class ColumnAlreadyExistError : DBError
    {
        public ColumnAlreadyExistError ()
        {
        }

        public ColumnAlreadyExistError (string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }

    [ZeroFormattable]
    public class ColumnNotExistError : DBError
    {
        public ColumnNotExistError ()
        {
        }

        public ColumnNotExistError (string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }

    [ZeroFormattable]
    public class CastFieldError : DBError
    {
        public CastFieldError ()
        {
        }

        public CastFieldError (List<string> columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }

    [ZeroFormattable]
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
