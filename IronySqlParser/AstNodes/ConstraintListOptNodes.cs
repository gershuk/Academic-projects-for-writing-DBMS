﻿using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    public class ConstraintListOptNodes : SqlNode
    {
        public List<string> ConstraintList { get; set; }

        public override void CollectDataFromChildren ()
        {
            ConstraintList = new List<string>();
            var constraintDefNodes = FindAllChildNodesByType<ConstraintDefNode>();

            foreach (var constraintDefNode in constraintDefNodes)
            {
                ConstraintList.Add(constraintDefNode.ConstraintState);
            }
        }
    }
}
