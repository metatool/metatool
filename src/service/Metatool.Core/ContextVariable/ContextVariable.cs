using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Metatool.Service;

namespace Metatool.Core
{
    public class ContextVariable : IContextVariable
    {
        class VariableValue
        {
            public int    Generation = 0;
            public object Value;

            public readonly Func<Task<object>> Generator;

            public VariableValue(Func<Task<object>> generator)
            {
                Generator = generator;
            }
        }

        private int                       _generation = 1;
        readonly Dictionary<string, VariableValue> _variables  = new Dictionary<string, VariableValue>();

        public void NewGeneration()
        {
            _generation++;
        }

        public void AddVariable(string key, Func<Task<object>> generator)
        {
            _variables.Add(key, new VariableValue(generator));
        }

        public async Task<T> GetVariable<T>(string key, bool force = false)
        {
            var varValue = _variables[key];

            if (_generation > varValue.Generation || force)
            {
                varValue.Value = await varValue.Generator();

                varValue.Generation = _generation;
            }
            return (T) varValue.Value;
        }
    }
}