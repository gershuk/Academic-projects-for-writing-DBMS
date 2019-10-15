using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class FieldDefNode : SqlNode
    {
        public string Id { get; set; }
        public DataBaseType FieldType { get; set; }
        public double? TypeParamOpt { get; set; }
        public List<string> ConstaraintList { get; set; }
        public NullSpecOpt NullSpecOpt { get; set; }

        public override void CollectInfoFromChild()
        {
            Id = FindChildNodesByType<IdNode>()[0].Id;
            FieldType = FindChildNodesByType<TypeNameNode>()[0].FieldType;
            TypeParamOpt = FindChildNodesByType<TypeParamsOptNode>()[0].TypeParamOpt;
            ConstaraintList = FindChildNodesByType<ConstraintListOptNodes>()[0].ConstraintList;
            NullSpecOpt = FindChildNodesByType<NullSpectOptNode>()[0].NullSpecOpt;
        }
    }
}
