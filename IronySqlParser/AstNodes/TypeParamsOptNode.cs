using System;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    public class TypeParamsOptNode : SqlNode
    {
        public double? TypeParamOpt { get; set; }

        public override void CollectDataFromChildren ()
        {
            var param = "";

            foreach (var child in ChildNodes)
            {
                param += child.Tokens.First<Token>().Text + ",";
            }

            param = param.TrimEnd(',');

            TypeParamOpt = param.Length == 0 ? null : (double?)Convert.ToDouble(param);
        }
    }
}
