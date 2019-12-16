using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator.GeneratorNodes
{
    public abstract class IBaseNode
    {
        protected static readonly Random _generator = new Random();
        abstract override public string ToString ();
    }
}
