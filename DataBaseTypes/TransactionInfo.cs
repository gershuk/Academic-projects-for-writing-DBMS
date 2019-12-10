using System;
using System.Collections.Generic;
using System.Text;

using ProtoBuf;

namespace DataBaseType
{
    [ProtoContract]
    public class TransactionInfo
    {
        [ProtoMember(1)]
        public Id Name { get; set; }

        [ProtoMember(2)]
        public Guid Guid { get; set; }

        [ProtoMember(3)]
        public DateTime StartTime { get; set; }

        [ProtoMember(4)]
        public DateTime EndTime { get; set; }

        [ProtoMember(5)]
        public List<OperationResult<Table>> OperationsResults { get; set; }

        public TransactionInfo () => OperationsResults = new List<OperationResult<Table>>();

        public TransactionInfo (Id name,
                               Guid guid,
                               DateTime startTime,
                               DateTime endTime,
                               List<OperationResult<Table>> operationsResults)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Guid = guid;
            StartTime = startTime;
            EndTime = endTime;
            OperationsResults = operationsResults ?? throw new ArgumentNullException(nameof(operationsResults));
        }

        public override string ToString ()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("Name:");

            stringBuilder.Append(Name.ToString());
            stringBuilder.Append("\n");
            stringBuilder.Append($"Guid {Guid}\n");
            stringBuilder.Append($"Start Time {StartTime}\n");
            stringBuilder.Append($"End Time {EndTime}\n");

            foreach (var opResult in OperationsResults)
            {
                stringBuilder.Append($"{opResult}\n");
            }

            return stringBuilder.ToString();
        }
    }

    [ProtoContract]
    public class SqlSequenceResult
    {
        [ProtoMember(1)]
        public List<TransactionInfo> Answer { get; private set; }

        public SqlSequenceResult () => Answer = new List<TransactionInfo>();

        public SqlSequenceResult (List<TransactionInfo> answer) => Answer = answer ?? throw new ArgumentNullException(nameof(answer));

        public override string ToString ()
        {
            var stringBuilder = new StringBuilder();

            foreach (var trInf in Answer)
            {
                stringBuilder.Append(trInf.ToString() + "\n");
            }

            return stringBuilder.ToString();
        }
    }
}
