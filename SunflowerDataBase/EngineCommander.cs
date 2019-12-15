using System;
using System.Collections.Generic;

using DataBaseEngine;

using DataBaseType;

using IronySqlParser.AstNodes;

namespace SunflowerDB
{
    public interface IEngineCommander
    {
        public (ExecutionState state, DBError exception) ExecuteCommands (Guid transactionGuid, List<SqlCommandNode> sqlCommand);
        public void RollBackTransaction (Guid transactionGuid);
        public void CommitTransaction (Guid transactionGuid);
        public OperationResult<Table> GetTableByName (Guid transactionGuid, Id tableName);
        public void StartTransaction (Guid transactionGuid);
    }

    public class EngineCommander : IEngineCommander, ISqlNodeExecutor
    {
        public IDataBaseEngine Engine { get; private set; }

        public EngineCommander (IDataBaseEngine engine) => Engine = engine ?? throw new ArgumentNullException(nameof(engine));

        public (ExecutionState state, DBError exception) ExecuteCommands (Guid transactionGuid, List<SqlCommandNode> sqlCommands)
        {
            foreach (dynamic command in sqlCommands)
            {
                var result = ExecuteSqlNode(transactionGuid, command) as OperationResult<Table>;
                if (result.State != ExecutionState.performed)
                {
                    return (result.State, result.OperationError);
                }
            }

            return (ExecutionState.performed, null);
        }

        public object ExecuteSqlNode (Guid id, CreateTableCommandNode node)
        {
            var createResult = Engine.CreateTableCommand(id, node.TableName);

            if (createResult.State != ExecutionState.performed)
            {
                return createResult;
            }

            foreach (var def in node.FieldDefList)
            {
                var column = new Column(def.Id, def.FieldType, def.TypeParamOpt, def.ConstaraintList, def.NullSpecOpt);
                var addResult = Engine.AddColumnCommand(id, node.TableName, column);

                if (addResult.State != ExecutionState.performed)
                {
                    return addResult;
                }
            }

            node.SetReturnedTableName(createResult.Result.TableMetaInf.Name);

            return createResult;
        }

        public object ExecuteSqlNode (Guid id, DropTableCommandNode node)
        {
            var dropResult = Engine.DropTableCommand(id, node.TableName);

            if (dropResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(dropResult.Result.TableMetaInf.Name);
            }

            return dropResult;
        }

        public object ExecuteSqlNode (Guid id, ShowTableCommandNode node)
        {
            var showResult = Engine.ShowTableCommand(id, node.TableName);

            if (showResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(showResult.Result.TableMetaInf.Name);
            }

            return showResult;
        }

        public object ExecuteSqlNode (Guid id, InsertCommandNode node)
        {
            Id TableName = null;

            foreach (var insertObject in node.InsertDataNode.InsertDataListNode.InsertObjects)
            {
                var parmsList = new List<ExpressionFunction>();

                foreach (var param in insertObject.ObjectParams)
                {
                    parmsList.Add(new ExpressionFunction(param.Calc, param.VariablesNames));
                }

                var insertResult = Engine.InsertCommand(id, node.TableName, node.ColumnNames?.IdListNode?.IdList, parmsList);

                if (insertResult.State != ExecutionState.performed)
                {
                    return insertResult;
                }

                TableName = insertResult.Result.TableMetaInf.Name;
            }

            node.SetReturnedTableName(TableName);

            return new OperationResult<Table>(ExecutionState.performed, null);
        }

        public object ExecuteSqlNode (Guid id, DeleteCommandNode node)
        {
            var expression = new ExpressionFunction(node.WhereClauseNode.Expression.Calc, node.WhereClauseNode.Expression.VariablesNames);

            var deleteResult = Engine.DeleteCommand(id, node.TableName, expression);

            if (deleteResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(deleteResult.Result.TableMetaInf.Name);
            }

            return deleteResult;
        }

        public object ExecuteSqlNode (Guid id, SelectCommandNode node)
        {
            var expression = node.WhereExpression != null ? new ExpressionFunction(node.WhereExpression.Calc, node.WhereExpression.VariablesNames) : null;

            var selectResult = Engine.SelectCommand(id, node.TableName, node.ColumnIdList, expression);

            if (selectResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(selectResult.Result.TableMetaInf.Name);
            }

            return selectResult;
        }

        public object ExecuteSqlNode (Guid id, UpdateCommandNode node)
        {
            var assignmentsList = new List<Assigment>();

            foreach (var assignment in node.Assignments)
            {
                var assigExp = new ExpressionFunction(assignment.Expression.Calc, assignment.Expression.VariablesNames);

                var assigmnet = new Assigment(assignment.Id, assigExp);

                assignmentsList.Add(assigmnet);
            }

            var expression = node.WhereExpression != null ? new ExpressionFunction(node.WhereExpression.Calc, node.WhereExpression.VariablesNames) : null;

            var updateResult = Engine.UpdateCommand(id, node.TableName, assignmentsList, expression);

            if (updateResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(updateResult.Result.TableMetaInf.Name);
            }

            return updateResult;
        }

        public object ExecuteSqlNode (Guid id, JoinChainOptNode node)
        {
            var expNode = node.JoinStatementNode.ExpressionNode;
            var expression = new ExpressionFunction(expNode.Calc, expNode.VariablesNames);
            var joinResult = Engine.JoinCommand(id,
                                                node.LeftId,
                                                node.RightId,
                                                expression);

            if (joinResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(joinResult.Result.TableMetaInf.Name);
            }

            return joinResult;
        }

        public object ExecuteSqlNode (Guid id, UnionChainOptNode node)
        {
            var unionResult = Engine.UnionCommand(id, node.LeftId, node.RightId, node.UnionKind);

            if (unionResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(unionResult.Result.TableMetaInf.Name);
            }

            return unionResult;
        }

        public object ExecuteSqlNode (Guid id, IntersectChainOptNode node)
        {
            var intersectResult = Engine.IntersectCommand(id, node.LeftId, node.RightId);

            if (intersectResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(intersectResult.Result.TableMetaInf.Name);
            }

            return intersectResult;
        }

        public object ExecuteSqlNode (Guid id, ExceptChainOptNode node)
        {
            var exceptResult = Engine.ExceptCommand(id, node.LeftId, node.RightId);

            if (exceptResult.State == ExecutionState.performed)
            {
                node.SetReturnedTableName(exceptResult.Result.TableMetaInf.Name);
            }

            return exceptResult;
        }

        public OperationResult<Table> GetTableByName (Guid guid, Id tableName) => Engine.GetTableCommand(guid, tableName);

        public void RollBackTransaction (Guid transactionGuid) => Engine.RollBackTransaction(transactionGuid);
        public void CommitTransaction (Guid transactionGuid) => Engine.CommitTransaction(transactionGuid);
        public void StartTransaction (Guid transactionGuid) => Engine.StartTransaction(transactionGuid);
    }
}
