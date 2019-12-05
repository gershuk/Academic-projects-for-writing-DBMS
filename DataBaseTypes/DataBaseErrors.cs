using System;
using System.Collections.Generic;

namespace DataBaseType
{
    abstract public class DBError
    {
        public string Message { get; set; }

        public DBError(string message) => Message = message;

        public override string ToString() => Message;
    }

    public class FileNotExistException : DBError
    {
        public FileNotExistException(string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    public class DataBaseIsCorruptException : DBError
    {
        public DataBaseIsCorruptException(string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }




    public class FileMarkNotExistException : DBError
    {
        public FileMarkNotExistException(string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }




    public class TableNotExistException : DBError
    {
        public TableNotExistException(string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }




    public class TableAlreadyExistException : DBError
    {
        public TableAlreadyExistException(string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }




    public class ColumnAlreadyExistException : DBError
    {
        public ColumnAlreadyExistException(string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }




    public class ColumnNotExistException : DBError
    {
        public ColumnNotExistException(string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }




    public class CastFieldException : DBError
    {
        public CastFieldException(List<string> columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }




    public class ParsingRequestException : DBError
    {
        public ParsingRequestException(string message, string errorLocation)
            : base($"{message} {errorLocation}")
        { }
    }
}
