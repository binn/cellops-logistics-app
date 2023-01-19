using Serilog.Core;
using Serilog.Events;
using System.Text.Json;

namespace AngelPhoneTrack.Services
{
    public class JsonDocumentDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory _, out LogEventPropertyValue? result)
        {
            if (value is not JsonDocument jdoc)
            {
                result = null;
                return false;
            }

            result = Destructure(jdoc.RootElement);
            return true;
        }

        private static LogEventPropertyValue Destructure(in JsonElement jel)
        {
            return jel.ValueKind switch
            {
                JsonValueKind.Array => new SequenceValue(jel.EnumerateArray().Select(ae => Destructure(in ae))),
                JsonValueKind.False => new ScalarValue(false),
                JsonValueKind.True => new ScalarValue(true),
                JsonValueKind.Null or JsonValueKind.Undefined => new ScalarValue(null),
                JsonValueKind.Number => new ScalarValue(jel.GetDecimal()),
                JsonValueKind.String => new ScalarValue(jel.GetString()),
                JsonValueKind.Object => new StructureValue(jel.EnumerateObject().Select(jp => new LogEventProperty(jp.Name, Destructure(jp.Value)))),
                _ => throw new ArgumentException("Unrecognized value kind " + jel.ValueKind + "."),
            };
        }
    }
}
