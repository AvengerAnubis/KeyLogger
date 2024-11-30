using KeyLogger.Macros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyLogger.Bindings
{
    public class BindingContainer
    {
        private List<Binding> _bindings;
        public ref List<Binding> Bindings
        {
            get => ref _bindings;
        }

        public BindingContainer()
        {
            _bindings = new List<Binding>();
        }
        public BindingContainer(List<Binding> bindings)
        {
            _bindings = bindings;
        }
        public BindingContainer(JsonDocument bindingsJsonData)
        {
            _bindings = new List<Binding>();
            JsonElement.ArrayEnumerator jsonArray = bindingsJsonData.RootElement.EnumerateArray();
            foreach (var binding in jsonArray)
            {
                _bindings.Add(new Binding(binding));
            }
        }
        public void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (Binding binding in _bindings)
            {
                binding.WriteToJson(ref writer);
            }
            writer.WriteEndArray();
        }
    }
}
