using System;
using System.Collections.Generic;
using System.Text;

using DataBaseEngine;

using DataBaseTable;

using Irony.Parsing;

namespace SunflowerDB
{
    public class EngineCommander
    {
        public EngineCommander(DataBaseEngineMain engine) => Engine = engine ?? throw new ArgumentNullException(nameof(engine));

        public DataBaseEngineMain Engine { get; set; }

        public OperationResult<Table> ExecuteCommand(ParseTreeNode treeNode)
        {
            var ans = treeNode.Term.Name switch
            {
                "DropTableStmt" => DropTable(treeNode),
                "CreateTableStmt" => CreateTable(treeNode),
                "ShowTableStmt" => ShowTable(treeNode),
                "SelectStmt" => Select(treeNode),
                "UpdateStmt" => Update(treeNode),
                "AlterStmt" => null,
                "InsertStmt" => Insert(treeNode),
                _ => null
            };

            Engine.Commit();
            return ans;
        }

        public OperationResult<Table> Update(ParseTreeNode node)
        {
            var _idNode = FindChildNodeByName(node, "id")[0];
            var _idTable = BuildNameFromId(_idNode);

            var _assignListNode = FindChildNodeByName(node, "assignList")[0];
            var _assigmentList = FindChildNodeByName(_assignListNode, "assignment");

            var _values = new Dictionary<string, string>();

            foreach (var _assigment in _assigmentList)
            {
                _values.Add(BuildNameFromId(_assigment.ChildNodes[0]), _assigment.ChildNodes[2].Token.Text);
            }

            return Engine.Update(_idTable, _values);
        }

        public OperationResult<Table> Insert(ParseTreeNode node)
        {
            var _idNode = FindChildNodeByName(node, "id")[0];
            var _idTable = BuildNameFromId(_idNode);
            var _columnNamesNode = FindChildNodeByName(node, "columnNames")[0];
            var _columnNames = new List<string>();

            if (_columnNamesNode.ChildNodes.Count > 0)
            {
                var _idListNode = _columnNamesNode.ChildNodes[0].ChildNodes[0];

                foreach (var _idColumnNode in _idListNode.ChildNodes)
                {
                    _columnNames.Add(BuildNameFromId(_idColumnNode));
                }
            }
            else
            {
                _columnNames = null;
            }

            var _insertData = FindChildNodeByName(node, "InsertData")[0];
            var _expressionListNode = FindChildNodeByName(_insertData, "exprList")[0];
            var _expressionList = FindChildNodeByName(_expressionListNode, "exprList");

            var _values = new List<List<string>>();

            foreach (var _expressionNode in _expressionList)
            {
                var _valueList = new List<string>();
                foreach (var _value in _expressionNode.ChildNodes)
                {
                    _valueList.Add(_value.Token.Text);
                }

                _values.Add(_valueList);
            }
            return Engine.Insert(_idTable, _columnNames, _values);
        }

        public OperationResult<Table> Select(ParseTreeNode node)
        {
            var _selListNode = FindChildNodeByName(node, "selList")[0];
            var _allStateNode = FindChildNodeByName(_selListNode, "*");
            var _columnItemList = FindChildNodeByName(_selListNode, "columnItemList");

            var _selsId = new List<string>();

            if (_allStateNode.Count == 0)
            {
                foreach (var columnItem in _columnItemList[0].ChildNodes)
                {
                    var _columnSourceNode = FindChildNodeByName(columnItem, "columnSource")[0];
                    var _idList = FindChildNodeByName(_columnSourceNode, "id");

                    foreach (var id in _idList)
                    {
                        _selsId.Add(BuildNameFromId(id));
                    }
                }
            }
            else
            {
                _selsId.Add("*");
            }

            var _fromClauseOpt = FindChildNodeByName(node, "fromClauseOpt");
            var _fromId = new List<string>();

            if (_fromClauseOpt.Count > 0)
            {
                var _fromClauseOptNode = _fromClauseOpt[0];
                var _idListNode = FindChildNodeByName(_fromClauseOptNode, "idList")[0];
                var _idList = FindChildNodeByName(_idListNode, "id");
                foreach (var id in _idList)
                {
                    _fromId.Add(BuildNameFromId(id));
                }
            }

            var selsIdTuple = new List<Tuple<string, string>>();
            foreach (var L in _selsId)
            {
                var strs = L.Split('.');
                if (strs.Length == 1)
                {
                    selsIdTuple.Add(new Tuple<string, string>(_fromId[0], strs[0]));
                }
                else
                {
                    selsIdTuple.Add(new Tuple<string, string>(strs[0], strs[1]));
                }
            }

            return Engine.Select(_fromId, selsIdTuple);
        }

        public OperationResult<Table> CreateTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];
            var fieldDefList = FindChildNodeByName(node, "fieldDefList")[0];
            var fieldDefs = FindChildNodeByName(fieldDefList, "fieldDef");

            var tableName = BuildNameFromId(idNode);

            var state = Engine.CreateTable(tableName);

            if (state.State == OperationExecutionState.failed)
            {
                return state;
            }

            foreach (var fieldDef in fieldDefs)
            {
                var _idfieldDef = FindChildNodeByName(fieldDef, "id")[0];
                var _typeNameNode = FindChildNodeByName(fieldDef, "typeName")[0];
                var _typeParamsNode = FindChildNodeByName(fieldDef, "typeParams")[0];

                var _columnName = BuildNameFromId(_idfieldDef);

                var _type = ParseEnum<ColumnDataType>(_typeNameNode.ChildNodes[0].Token.Text);
                var _typeParams = _typeParamsNode?.ChildNodes.Count > 0 ?
                                  _typeParamsNode?.ChildNodes[0].Token.Text : null;

                var _constraintListOptNode = FindChildNodeByName(fieldDef, "constraintListOpt")[0];
                var _constraintDefList = FindChildNodeByName(_constraintListOptNode, "constraintDef");

                var _constraintList = BuildConstraintList(_constraintDefList);

                var _typeParamsInt = _typeParams == null ? 0 : int.Parse(_typeParams);

                var _nullSpecOptNode = FindChildNodeByName(fieldDef, "nullSpecOpt")[0];
                var _nullSpecOptName = "";

                foreach (var childs in _nullSpecOptNode.ChildNodes)
                {
                    _nullSpecOptName += childs.Term.Name;
                }

                var _nullSpecOpt = _nullSpecOptName == "" ? NullSpecOpt.Empty : ParseEnum<NullSpecOpt>(_nullSpecOptName);

                var column = new Column(_columnName, _type, _typeParamsInt, _constraintList, _nullSpecOpt);

                var _state = Engine.AddColumnToTable(tableName, column);

                if (_state.State == OperationExecutionState.failed)
                {
                    return _state;
                }
            }

            return state;
        }

        public OperationResult<Table> DropTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];

            var name = BuildNameFromId(idNode);

            return Engine.DeleteTable(name);
        }

        public OperationResult<Table> ShowTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];

            var name = BuildNameFromId(idNode);

            return Engine.GetTable(name);
        }

        private static List<ParseTreeNode> FindChildNodeByName(ParseTreeNode treeNode, string name)
        {
            var nodeList = new List<ParseTreeNode>();

            foreach (var childNode in treeNode.ChildNodes)
            {
                if (childNode.Term.Name == name)
                {
                    nodeList.Add(childNode);
                }
            }

            return nodeList;
        }

        public static string BuildNameFromId(ParseTreeNode treeNode)
        {
            var name = new StringBuilder();

            foreach (var childNode in treeNode?.ChildNodes)
            {
                name.Append(childNode.Token.Text + ".");
            }

            name.Remove(name.Length - 1, 1);

            return name.ToString();
        }

        private static List<string> BuildConstraintList(List<ParseTreeNode> _constraintDefList)
        {
            var _constraintList = new List<string>();
            foreach (var _constraintDef in _constraintDefList)
            {
                var constrain = "";
                foreach (var childNode in _constraintDef.ChildNodes)
                {
                    if (childNode.Term.Name == "id")
                    {
                        var _id = BuildNameFromId(childNode);
                        if (_id != null)
                        {
                            constrain += _id + ".";
                        }
                    }
                    else
                    {
                        constrain += childNode.Token.Text + ".";
                    }
                }

                constrain = constrain.Trim('.');
                _constraintList.Add(constrain);
            }

            return _constraintList;
        }

        private static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}
