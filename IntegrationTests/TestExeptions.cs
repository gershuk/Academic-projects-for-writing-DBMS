using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class ConnectIssue : Exception
    {
        public ConnectIssue ()
        {
        }
        public ConnectIssue (string message)
            : base(message)
        {
        }
        public ConnectIssue (string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
