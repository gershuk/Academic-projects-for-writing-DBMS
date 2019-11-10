using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class SqlCommandNode : SqlNode
    {
        public List<string> ReturnedTableName { get; set; } = new List<string>();
    }
}
