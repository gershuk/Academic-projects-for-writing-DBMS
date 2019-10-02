using System;
using System.Collections.Generic;
using System.Text;
using DataBaseEngine;
using DataBaseTable;
using Irony.Parsing;

namespace DB_MainFrame
{
    class EngineCommander
    {
        public EngineCommander(SimpleDataBaseEngine engine) => Engine = engine ?? throw new ArgumentNullException(nameof(engine));

        public SimpleDataBaseEngine Engine { get; set; }

        public OperationResult<string> CreateTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];
            var fieldDefList = FindChildNodeByName(node, "fieldDefList")[0];
            var fieldDefs = FindChildNodeByName(fieldDefList, "fieldDef");

            var tableName = BuildNameFromId(idNode);

            var state = Engine.CreateTable(tableName);

            if (state.State == OperationExecutionState.failed)
            {
                return new OperationResult<string>(OperationExecutionState.failed, tableName + " created fail");
            }

            foreach (var fieldDef in fieldDefs)
            {
                var id = FindChildNodeByName(fieldDef, "id")[0];
                var _typeNameNode = FindChildNodeByName(fieldDef, "typeName")[0];
                var _typeParamsNode = FindChildNodeByName(fieldDef, "typeParams")[0];
                var _columnName = BuildNameFromId(id);
                var _type = ParseEnum<DataBaseTypes>(_typeNameNode.ChildNodes[0].Token.Text);
                var _typeParams = _typeParamsNode?.ChildNodes.Count > 0 ? _typeParamsNode?.ChildNodes[0].Token.Text : null;
                var _constraintListOptNode = FindChildNodeByName(fieldDef, "constraintListOpt")[0];
                var _constraintDefList = FindChildNodeByName(_constraintListOptNode, "constraintDef");

                var _constraintList = new List<string>();
                foreach (var constDef in _constraintDefList)
                {
                    var constrain = "";
                    foreach (var childNode in constDef.ChildNodes)
                    {
                        string _id = null;
                        if (childNode.Term.Name == "id")
                        {
                            _id = BuildNameFromId(childNode);
                            if (_id != null)
                            {
                                constrain += _id + " ";
                            }
                        }
                        else
                        {
                            constrain += childNode.Token.Text + " ";
                        }
                    }
                    constrain = constrain.Trim();
                    _constraintList.Add(constrain);
                }

                var _state = Engine.AddColumnToTable(tableName, _columnName, _type, _typeParams, _constraintList.ToArray());

                if (_state.State == OperationExecutionState.failed)
                {
                    return new OperationResult<string>(OperationExecutionState.failed, "added column " + _columnName + " faild");
                }
            }
            return state;
        }

        public OperationResult<string> DropTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];

            var name = BuildNameFromId(idNode);

            return Engine.DeleteTable(name);
        }

        public OperationResult<string> ShowTable(ParseTreeNode node)
        {
            var idNode = FindChildNodeByName(node, "id")[0];

            var name = BuildNameFromId(idNode);

            return Engine.GetTableMetaInf(name);
        }

        private List<ParseTreeNode> FindChildNodeByName(ParseTreeNode treeNode, string name)
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

        private string BuildNameFromId(ParseTreeNode treeNode)
        {
            var name = new StringBuilder();

            foreach (var childNode in treeNode?.ChildNodes)
            {
                name.Append(childNode.Token.Text + ".");
            }

            name.Remove(name.Length - 1, 1);

            return name.ToString();
        }

        private static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}
