using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [Serializable]
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

        protected ConnectIssue (System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
