using System.Linq;
using DataBaseType;

namespace IronySqlParser.AstNodes
{
    public class TypeNameNode : SqlNode
    {
        public DataType FieldType { get; set; }

        public override void CollectInfoFromChild ()
        {
            var type = (ChildNodes.First<ISqlNode>() as SqlKeyNode).Text;
            FieldType = ParseEnum<DataType>(type);
        }
    }
}
