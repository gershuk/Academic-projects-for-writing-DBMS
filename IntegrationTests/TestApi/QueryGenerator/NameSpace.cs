using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegrationTests.TestApi.QueryGenerator.TableDescription;

namespace IntegrationTests.TestApi.QueryGenerator
{
    public class NameSpace
    {

        public bool IsRandom = false;
        private double _notexpar;
        public Double NotExistedParam
        {
            get => _notexpar;
            set
            {
                if (value <= 0.00001)
                {
                    IsRandom = false;
                    _notexpar = 0;
                }
                else
                {
                    IsRandom = true;
                    _notexpar = value;
                }
            }
        }
        private bool CanEditTables => _tables.Count > 0;
        private readonly HashSet<string> _tables;
        private readonly Dictionary<string, TableDescription> _descriptions;
        private static Random _generator = default;

        NameSpace (HashSet<string> tables = default, Dictionary<string, TableDescription> descriptions = default)
        {
            _tables = tables;
            _descriptions = descriptions;
            foreach (var i in _tables.ToArray())
            {
                if (!_descriptions.ContainsKey(i))
                {
                    _tables.Remove(i);
                }
            }
            foreach (var i in _descriptions.ToArray())
            {
                if (!_tables.Contains(i.Value.Name))
                {
                    _tables.Add(i.Value.Name);
                }
            }
        }

        private static string RandomString ()
        {
            var length = _generator.Next(5, 255);
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(1, length).Select(_ => chars[_generator.Next(chars.Length)]).ToArray());
        }

        private string GetRandomName ()
        {
            var str = RandomString();
            return str.Substring(0, Math.Min(str.Length - 1, _generator.Next(5, 10)));
        }

        public string GetTableName ()
        {
            return _generator.NextDouble()*_tables.Count <= NotExistedParam * _tables.Count ? GetRandomName()  : _tables.ToArray()[_generator.Next(_tables.Count())];
        }

        public Column GetTableColumn (string table)
        {
            return _generator.NextDouble() < NotExistedParam ? new Column(GetRandomName()) : !_tables.Contains(table) ? null : _descriptions[table].GetAnyTableColumn();
        }

        public Column GetIntTableColumn (string table)
        {
            return _generator.NextDouble() < NotExistedParam ? new Column(GetRandomName()) : !_tables.Contains(table) ? null : _descriptions[table].GetIntTableColumn() ;
        }

        public Column GetDoubleTableColumn (string table)
        {
            return _generator.NextDouble() < NotExistedParam ? new Column(GetRandomName()) : !_tables.Contains(table) ? null : _descriptions[table].GetDoubleTableColumn();
        }

        public Column GetCharTableColumn (string table)
        {
            return _generator.NextDouble() < NotExistedParam ? new Column(GetRandomName()) : !_tables.Contains(table) ? null : _descriptions[table].GetCharTableColumn();
        }

        public void AddTable (string name)
        {
            _tables.Add(name);
            _descriptions[name] = new TableDescription(name);
        }

        public void AddTableColumb (string table, string name, ColumnType type)
        {
            _descriptions[name].AddColumn(name, type);
        }
    }
}
