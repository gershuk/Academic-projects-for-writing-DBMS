using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class DateTimeNode : SqlNode
    {
        public DateTime DateTime { get; set; }

        public override void CollectDataFromChildren () => DateTime = Convert.ToDateTime(Tokens.First<Token>().Value);
    }
}
