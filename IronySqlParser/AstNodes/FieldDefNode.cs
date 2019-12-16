using System.Collections.Generic;

using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class FieldDefNode : SqlNode
    {
        public Id Id { get; set; }
        public DataType FieldType { get; set; }
        public double? TypeParamOpt { get; set; }
        public List<string> ConstaraintList { get; set; }
        public NullSpecOpt NullSpecOpt { get; set; }

        public override void CollectDataFromChildren ()
        {
            Id = new Id(FindFirstChildNodeByType<IdNode>().Id);
            FieldType = FindFirstChildNodeByType<TypeNameNode>().FieldType;
            TypeParamOpt = FindFirstChildNodeByType<TypeParamsOptNode>().TypeParamOpt;
            ConstaraintList = FindFirstChildNodeByType<ConstraintListOptNodes>().ConstraintList;
            NullSpecOpt = FindFirstChildNodeByType<NullSpectOptNode>().NullSpecOpt;
        }
    }
}
