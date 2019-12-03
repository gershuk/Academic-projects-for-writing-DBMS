using System;
using System.Collections.Generic;

using DataBaseEngine;

using DataBaseType;
using IronySqlParser.AstNodes;

namespace SunflowerDB
{
    public interface IEngineCommander
    {
        public (OperationExecutionState state, Exception exception) ExecuteCommandList(List<SqlCommandNode> sqlCommand);
        public void RollBackTransaction(Guid transactionGuid);
        public void CommitTransaction(Guid transactionGuid);
        public OperationResult<Table> GetTableByName(List<string> tableName);
    }

    public class EngineCommander : IEngineCommander, ISqlNodeExecutor
    {
        public IDataBaseEngine Engine { get; private set; }

        public EngineCommander(IDataBaseEngine engine) => Engine = engine ?? throw new ArgumentNullException(nameof(engine));

        public (OperationExecutionState state, Exception exception) ExecuteCommandList(List<SqlCommandNode> sqlCommands)
        {
            foreach (var sqlCommand in sqlCommands)
            {
                var result = ExecuteSqlNode(sqlCommand) as OperationResult<Table>;
                if (result.State != OperationExecutionState.performed)
                {
                    return (result.State, result.OperationException);
                }
            }

            return (OperationExecutionState.performed, null);
        }

        public object ExecuteSqlNode(SqlNode node) => node.Accept(this);

        public object ExecuteSqlNode(CreateTableCommandNode node)
        {
            var createResult = Engine.CreateTableCommand(node.TableName);

            if (createResult.State != OperationExecutionState.performed)
            {
                return createResult;
            }

            foreach (var def in node.FieldDefList)
            {
                var column = new Column(def.Id, def.FieldType, def.TypeParamOpt, def.ConstaraintList, def.NullSpecOpt);
                var addResult = Engine.AddColumnCommand(node.TableName, column);

                if (addResult.State != OperationExecutionState.performed)
                {
                    return addResult;
                }
            }

            node.SetReturnedTableName(createResult.Result.TableMetaInf.Name);

            return createResult;
        }

        public object ExecuteSqlNode(DropTableCommandNode node)
        {
            var dropResult = Engine.DropTableCommand(node.TableName);

            if (dropResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(dropResult.Result.TableMetaInf.Name);
            }

            return dropResult;
        }

        public object ExecuteSqlNode(ShowTableCommandNode node)
        {
            var showResult = Engine.ShowTableCommand(node.TableName);

            if (showResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(showResult.Result.TableMetaInf.Name);
            }

            return showResult;
        }

        public object ExecuteSqlNode(InsertCommandNode node)
        {
            foreach (var insertObject in node.InsertDataNode.InsertDataListNode.InsertObjects)
            {
                var parmsList = new List<ExpressionFunction>();

                foreach (var param in insertObject.ObjectParams)
                {
                    parmsList.Add(new ExpressionFunction(param.Calc, param.Variables));
                }

                var insertResult = Engine.InsertCommand(node.TableName, node.ColumnNames.IdListNode.IdList, parmsList);

                if (insertResult.State != OperationExecutionState.performed)
                {
                    return insertResult;
                }

                node.SetReturnedTableName(insertResult.Result.TableMetaInf.Name);
            }

            return new OperationResult<Table>(OperationExecutionState.performed, null);
        }

        public object ExecuteSqlNode(DeleteCommandNode node)
        {
            var expression = new ExpressionFunction()
            {
                CalcFunc = node.WhereClauseNode.Expression.Calc,
                Variables = node.WhereClauseNode.Expression.Variables
            };

            var deleteResult = Engine.DeleteCommand(node.TableName, expression);

            if (deleteResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(deleteResult.Result.TableMetaInf.Name);
            }

            return deleteResult;
        }

        public object ExecuteSqlNode(SelectCommandNode node)
        {
            var expression = new ExpressionFunction()
            {
                CalcFunc = node.WhereExpression.Calc,
                Variables = node.WhereExpression.Variables
            };

            var selectResult = Engine.SelectCommand(node.TableName, node.ColumnIdList, expression);

            if (selectResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(selectResult.Result.TableMetaInf.Name);
            }

            return selectResult;
        }

        public object ExecuteSqlNode(UpdateCommandNode node)
        {
            var assignmentsList = new List<Assigment>();

            foreach (var assignment in node.Assignments)
            {
                var assigExp = new ExpressionFunction()
                {
                    Variables = assignment.Expression.Variables,
                    CalcFunc = assignment.Expression.Calc
                };

                var assigmnet = new Assigment(assignment.Id, assigExp);

                assignmentsList.Add(assigmnet);
            }

            var expression = new ExpressionFunction()
            {
                Variables = node.WhereExpression.Variables,
                CalcFunc = node.WhereExpression.Calc
            };

            var updateResult = Engine.UpdateCommand(node.TableName, assignmentsList, expression);

            if (updateResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(updateResult.Result.TableMetaInf.Name);
            }

            return updateResult;
        }

        public object ExecuteSqlNode(JoinChainOptNode node)
        {
            var joinResult = Engine.JoinCommand(node.LeftId,
                                         node.RightId,
                                         node.JoinKind,
                                         node.JoinStatementNode.LeftId,
                                         node.JoinStatementNode.RightId);

            if (joinResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(joinResult.Result.TableMetaInf.Name);
            }

            return joinResult;
        }

        public object ExecuteSqlNode(UnionChainOptNode node)
        {
            var unionResult = Engine.UnionCommand(node.LeftId, node.RightId, node.UnionKind);

            if (unionResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(unionResult.Result.TableMetaInf.Name);
            }

            return unionResult;
        }

        public object ExecuteSqlNode(IntersectChainOptNode node)
        {
            var intersectResult = Engine.IntersectCommand(node.LeftId, node.RightId);

            if (intersectResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(intersectResult.Result.TableMetaInf.Name);
            }

            return intersectResult;
        }

        public object ExecuteSqlNode(ExceptChainOptNode node)
        {
            var exceptResult = Engine.ExceptCommand(node.LeftId, node.RightId);

            if (exceptResult.State == OperationExecutionState.performed)
            {
                node.SetReturnedTableName(exceptResult.Result.TableMetaInf.Name);
            }

            return exceptResult;
        }

        public void RollBackTransaction(Guid transactionGuid) => Engine.RollBackTransaction(transactionGuid);
        public void CommitTransaction(Guid transactionGuid) => Engine.CommitTransaction(transactionGuid);
        public OperationResult<Table> GetTableByName(List<string> tableName) => Engine.GetTableCommand(tableName);
    }
}
