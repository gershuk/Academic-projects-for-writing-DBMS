using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator
{
    public class TableDescription
    {


        public string Name;
        private readonly List<Column> _columns = new List<Column>();
        private readonly List<Column> _intcolumns = new List<Column>();
        private readonly List<Column> _doublecolumns = new List<Column>();
        private readonly List<Column> _charcolumns = new List<Column>();
        private static Random _generator = new Random();

        public TableDescription (string name)
        {
            Name = name;
        }

        public void AddColumn (string name, ColumnType type)
        {
            if (!_columns.Exists(i => i._name == name))
            {
                _columns.Add(new Column(name, type));
                switch (type)
                {
                    case ColumnType.Double:
                        _doublecolumns.Add(_columns.Last());
                        break;
                    case ColumnType.Int:
                        _intcolumns.Add(_columns.Last());
                        break;
                    case ColumnType.Char:
                        _charcolumns.Add(_columns.Last());
                        break;
                }
            }
        }

        public Column GetAnyTableColumn ()
        {
            return _columns.Count > 0 ? _columns[_generator.Next(_columns.Count)] : null;
        }
        public Column GetIntTableColumn ()
        {
            return _intcolumns.Count > 0 ? _intcolumns[_generator.Next(_intcolumns.Count)] : null;
        }
        public Column GetDoubleTableColumn ()
        {
            return _doublecolumns.Count > 0 ? _doublecolumns[_generator.Next(_doublecolumns.Count)] : null;
        }
        public Column GetCharTableColumn ()
        {
            return _charcolumns.Count > 0 ? _charcolumns[_generator.Next(_charcolumns.Count)] : null;
        }

        public int GetColumnNum ()
        {
            return _columns.Count();
        }

    }

    public enum ColumnType
    {
        Double,
        Int,
        Char,
        Any,
        Bool
    }

    public class Column
    {
        public string _name;
        public ColumnType _type;
        public Column (string name)
        {
            _name = name;
            _type = ColumnType.Any;
        }

        public Column (string name, ColumnType type)
        {
            _name = name;
            _type = type;
        }
    }
}
