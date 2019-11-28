using System;
using System.Collections.Generic;

namespace DataBaseErrors
{
    internal class FileNotExistExeption : Exception
    {
        public FileNotExistExeption(string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    internal class DataBaseIsCorruptExeption : Exception
    {
        public DataBaseIsCorruptExeption(string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }

    internal class FileMarkNotExistExeption : Exception
    {
        public FileMarkNotExistExeption(string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }

    internal class TableNotExistExeption : Exception
    {
        public TableNotExistExeption(string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }

    internal class TableAlreadyExistExeption : Exception
    {
        public TableAlreadyExistExeption(string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }

    internal class ColumnAlreadyExistExeption : Exception
    {
        public ColumnAlreadyExistExeption(string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }

    internal class ColumnNotExistExeption : Exception
    {
        public ColumnNotExistExeption(string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }

    internal class CastFieldExeption : Exception
    {
        public CastFieldExeption(List<string> columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }
}
