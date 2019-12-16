using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.TestApi.QueryGenerator
{
    public class FrequencyRandomizer
    {
        private List<int> _values = new List<int>();
        private static Random _generator = new Random();
        public void Insert (int value, int frequency = 1)
        {
            for (var i = 0; i < frequency * 3; i++)
            {
                _values.Add(value);
            }
        }
        public void Remove (int value)
        {
            _values.RemoveAll(i => i == value);
        }

        public int GetRandom ()
        {
            return _values[_generator.Next(0, _values.Count - 1)];
        }
    }
}
