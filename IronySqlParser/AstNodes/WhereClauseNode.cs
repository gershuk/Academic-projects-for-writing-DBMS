using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronySqlParser.AstNodes
{
    class WhereClauseNode:SqlNode
    {
        public ExpressionNode Expression { get; set; }

        public override void CollectInfoFromChild()
        {
            Expression = FindChildNodesByType<ExpressionNode>()[0];
        }
    }
}

