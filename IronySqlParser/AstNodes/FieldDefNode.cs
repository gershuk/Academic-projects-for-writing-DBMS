using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    internal class FieldDefNode : SqlNode
    {
        public List<string> Id { get; set; }
        public DataBaseType FieldType { get; set; }
        public double? TypeParamOpt { get; set; }
        public List<string> ConstaraintList { get; set; }
        public NullSpecOpt NullSpecOpt { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindFirstChildNodeByType<IdNode>().Id;
            FieldType = FindFirstChildNodeByType<TypeNameNode>().FieldType;
            TypeParamOpt = FindFirstChildNodeByType<TypeParamsOptNode>().TypeParamOpt;
            ConstaraintList = FindFirstChildNodeByType<ConstraintListOptNodes>().ConstraintList;
            NullSpecOpt = FindFirstChildNodeByType<NullSpectOptNode>().NullSpecOpt;
        }
    }
}
