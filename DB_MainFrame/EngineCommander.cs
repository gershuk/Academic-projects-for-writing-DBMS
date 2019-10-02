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

        public OperationExecutionState CreateTable(ParseTreeNode node)
        {
            ParseTreeNode idNode = null;
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.Term.Name == "id")
                {
                    idNode = childNode;
                }
            }

            var name = new StringBuilder();

            foreach (var childNode in idNode?.ChildNodes)
            {
                name.Append(childNode.Token.Text + ".");
            }
            var state = Engine.CreateTable(name.ToString());

            if (state == OperationExecutionState.failed)
            {
                return OperationExecutionState.failed;
            }

            var columns = new List<(string type, string name)>();

            //ToDo: Get column info from tree

            foreach (var column in columns)
            {
                //ToDo: Convert string to type
                state = Engine.AddColumnToTable<object>(column.name);
                if (state == OperationExecutionState.failed)
                {
                    return OperationExecutionState.failed;
                }
            }

            return state;
        }

        public OperationExecutionState DropTable(ParseTreeNode node)
        {
            ParseTreeNode idNode = null;
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.Term.Name == "id")
                {
                    idNode = childNode;
                }
            }

            var name = new StringBuilder();

            foreach (var childNode in idNode?.ChildNodes)
            {
                name.Append(childNode.Token.Text + ".");
            }

            return Engine.DeleteTable(name.ToString());
        }

        public OperationResult<TableMetaInf> ShowTable(ParseTreeNode node)
        {
            ParseTreeNode idNode = null;
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.Term.Name == "id")
                {
                    idNode = childNode;
                }
            }

            var name = new StringBuilder();

            foreach (var childNode in idNode?.ChildNodes)
            {
                name.Append(childNode.Token.Text + ".");
            }

            return Engine.GetTableMetaInf(name.ToString());
        }
    }
}
