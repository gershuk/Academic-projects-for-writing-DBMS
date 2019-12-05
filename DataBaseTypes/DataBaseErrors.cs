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
    public class FileNotExistException : DBError
    {
        public FileNotExistException ()
        {
        }

        public FileNotExistException (string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    [ZeroFormattable]
    public class DataBaseIsCorruptException : DBError
    {
        public DataBaseIsCorruptException ()
        {
        }

        public DataBaseIsCorruptException (string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }

    [ZeroFormattable]
    public class FileMarkNotExistException : DBError
    {
        public FileMarkNotExistException ()
        {
        }

        public FileMarkNotExistException (string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }

    [ZeroFormattable]
    public class TableNotExistException : DBError
    {
        public TableNotExistException ()
        {
        }

        public TableNotExistException (string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }

    [ZeroFormattable]
    public class TableAlreadyExistException : DBError
    {
        public TableAlreadyExistException ()
        {
        }

        public TableAlreadyExistException (string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }

    [ZeroFormattable]
    public class ColumnAlreadyExistException : DBError
    {
        public ColumnAlreadyExistException ()
        {
        }

        public ColumnAlreadyExistException (string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }

    [ZeroFormattable]
    public class ColumnNotExistException : DBError
    {
        public ColumnNotExistException ()
        {
        }

        public ColumnNotExistException (string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }

    [ZeroFormattable]
    public class CastFieldException : DBError
    {
        public CastFieldException ()
        {
        }

        public CastFieldException (List<string> columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }

    [ZeroFormattable]
    public class ParsingRequestException : DBError
    {
        public ParsingRequestException ()
        {
        }

        public ParsingRequestException (string message, string errorLocation)
            : base($"{message} {errorLocation}")
        { }
    }
}
