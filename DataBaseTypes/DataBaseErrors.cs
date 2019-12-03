using System;
using System.Collections.Generic;

namespace DataBaseType
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class FileNotExistException : Exception
    {
        public FileNotExistException(string path)
            : base($"Error, File named {path} doesn't exist ")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class DataBaseIsCorruptException : Exception
    {
        public DataBaseIsCorruptException(string path)
            : base($"Error, DataBase named {path} is corrupt")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class FileMarkNotExistException : Exception
    {
        public FileMarkNotExistException(string path, string fileMark)
            : base($"Error, File named {path} doesn't contain 'file mark' '{fileMark}'")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class TableNotExistException : Exception
    {
        public TableNotExistException(string tableName)
            : base($"Error, Table named {tableName} doesn't exist")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class TableAlreadyExistException : Exception
    {
        public TableAlreadyExistException(string tableName)
            : base($"Error, Table with name {tableName} already exist.")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class ColumnAlreadyExistException : Exception
    {
        public ColumnAlreadyExistException(string columnName, string tableName)
            : base($"Error, Column with name {columnName} alredy exist in Table {tableName}")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class ColumnNotExistException : Exception
    {
        public ColumnNotExistException(string columnName, string tableName)
            : base($"Error, Column with name {columnName} not exist in Table {tableName}")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class CastFieldException : Exception
    {
        public CastFieldException(List<string> columnName, string type, string member)
            : base($"Error cast field, Column with name {columnName} and type {type} with member {member}")
        { }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2229:Implement serialization constructors", Justification = "<Ожидание>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Реализуйте стандартные конструкторы исключения", Justification = "<Ожидание>")]
    public class ParsingRequestException : Exception
    {
        public ParsingRequestException(string message, string errorLocation)
            : base($"{message} {errorLocation}")
        { }

        public override string ToString() => Message;
    }
}
