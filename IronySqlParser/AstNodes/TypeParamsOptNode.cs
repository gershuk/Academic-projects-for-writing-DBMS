using System;
using System.Linq;

namespace IronySqlParser.AstNodes
{
    class TypeParamsOptNode : SqlNode
    {
        public double? TypeParamOpt { get; set; }

        public override void CollectInfoFromChild()
        {
            var param = "";

            foreach (var child in ChildNodes)
            {
                param += child.Tokens.First<Token>().Text + ",";
            }

            param = param.TrimEnd(',');

            if (param == "")
            {
                TypeParamOpt = null;
            }
            else
            {
                TypeParamOpt = Convert.ToDouble(param);
            }
        }
    }
}
