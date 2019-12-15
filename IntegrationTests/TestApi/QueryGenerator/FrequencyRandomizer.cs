using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator
{
    public class FrequencyRandomizer
    {
        private List<int> _values = default;
        private static Random _generator = default;
        public void Insert(int value,int frequency = 1)
        {
            for(int i = 0; i< frequency*3; i++)
            {
                _values.Add(value);
            }
        }

        public int GetRandom()
        {
            return _values[_generator.Next(0, _values.Count - 1)];
        }
    }
}
