using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    public class CreateTableNode : IBaseNode
    {
        private static Random _generator = new Random();
        private class CreateColumn : IBaseNode
        {
            public string _columnname;
            private string _columntype;
            private string _constraint;

            public CreateColumn (NameSpace ns, string table)
            {
                _columnname = ns.GetRandomName();
                var typechooser = new FrequencyRandomizer();
                typechooser.Insert(0, 3); // int
                typechooser.Insert(1, 3); // double
                typechooser.Insert(2, 4); // char
                var type = ColumnType.Any;
                switch (typechooser.GetRandom())
                {
                    case 0:
                        {
                            _columntype = "int";
                            type = ColumnType.Int;
                            break;
                        }
                    case 1:
                        {
                            _columntype = "double";
                            type = ColumnType.Double;
                            break;
                        }
                    case 2:
                        {
                            _columntype = $"char ({_generator.Next(1256)})";
                            type = ColumnType.Char;
                            break;
                        }
                }
                var constraintchooser = new FrequencyRandomizer();
                constraintchooser.Insert(0, 2); //UNIQUE
                constraintchooser.Insert(1, 2); // PRIMARY KEY
                constraintchooser.Insert(2, 6); // PRIMARY KEY
                switch (typechooser.GetRandom())
                {
                    case 0:
                        {
                            _constraint = "UNIQUE";
                            break;
                        }
                    case 1:
                        {
                            _constraint = "PRIMARY KEY";
                            break;
                        }
                }
                ns.AddTableColumn(table, _columnname, type);
            }

            public override string ToString ()
            {
                return $"{_columnname} {_columntype} {_constraint}";
            }
        }
        private readonly string _tablename;
        private readonly List<CreateColumn> _columns;

        public CreateTableNode (NameSpace ns, int maxdepth)
        {
            _tablename = ns.GetRandomName();
            ns.AddTable(_tablename);
            _columns = Enumerable.Range(0, _generator.Next(50)).Select(_ => new CreateColumn(ns, _tablename)).ToList();
            var check = new HashSet<string>();
            if (!ns.IsRandom)
            {
                foreach (var i in _columns.ToArray())
                {
                    if (check.Contains(i._columnname))
                    {
                        _columns.Remove(i);
                    }
                    else
                    {
                        check.Add(i._columnname);
                    }
                }
            }
        }

        public override string ToString ()
        {
            return $"create table {_tablename} ({String.Join(" ,", _columns)});";
        }
    }
}
