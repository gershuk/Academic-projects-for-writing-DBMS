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
                "UpdateStmt" => null,
                "AlterStmt" => null,
                "InsertStmt" => null,
            };

            Engine.Commit();
            return ans;
        }

        public OperationResult<Table> Select(ParseTreeNode node)
        {
            var _selListNode = FindChildNodeByName(node, "selList")[0];
            var _allStateNode = FindChildNodeByName(_selListNode, "*");
            var _columnItemList = FindChildNodeByName(_selListNode, "columnItemList");

            var _selsId = new List<String>();

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
            var _fromId = new List<String>();

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

            //var _whereClauseOptList = FindChildNodeByName(node, "whereClauseOpt");
            //string _whereClauseOpt;

            //if (_whereClauseOptList.Count > 0)
            //{
            //    var _whereClauseOptNode = _whereClauseOptList[0];
            //    //костыль ->
            //    var _binExpr = FindChildNodeByName(_whereClauseOptNode, "binExpr");
            //    if (_binExpr.Count > 0)
            //    {
            //        var _binExprNode = _binExpr[0];
            //        foreach (var child in _binExprNode.ChildNodes)
            //        {

            //        }
            //    }
            //}

            return null;
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
