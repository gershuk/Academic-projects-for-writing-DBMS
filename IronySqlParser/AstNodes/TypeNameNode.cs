using System.Linq;

namespace IronySqlParser.AstNodes
{
    public enum DataBaseType
    {
        DATETIME,
        INT,
        DOUBLE,
        CHAR,
        NCHAR,
        VARCHAR,
        NVARCHAR,
        IMAGE,
        TEXT,
        NTEXT
    }

    public class TypeNameNode : SqlNode
    {
        public DataBaseType FieldType { get; set; }

        public override void CollectInfoFromChild()
        {
            var type = (ChildNodes.First<ISqlNode>() as SqlKeyNode).Text;
            FieldType = ParseEnum<DataBaseType>(type);
        }
    }
}
