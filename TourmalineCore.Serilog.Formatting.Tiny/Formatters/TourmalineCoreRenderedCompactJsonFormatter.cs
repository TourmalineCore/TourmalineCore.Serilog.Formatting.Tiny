// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   An  that writes events in a compact JSON format, for consumption in environments
//   without message template support. Message templates are rendered into text and a hashed event id is included.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace TourmalineCore.Serilog.Formatting.Tiny.Formatters
{
    /// <summary>
    /// An <see cref="ITextFormatter" /> that writes events in a compact JSON format, for consumption in environments
    /// without message template support. Message templates are rendered into text and a hashed event id is included.
    /// </summary>
    public class TourmalineCoreRenderedCompactJsonFormatter : ITextFormatter
    {
        /// <summary>
        /// The version.
        /// </summary>
        private const string FormatVersion = "2";

        /// <summary>
        /// The _value formatter.
        /// </summary>
        private readonly JsonValueFormatter _valueFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TourmalineCoreRenderedCompactJsonFormatter" /> class.
        /// Construct a <see cref="CompactJsonFormatter" />, optionally supplying a formatter for
        /// <see cref="LogEventPropertyValue" />s on the event.
        /// </summary>
        /// <param name="valueFormatter">
        /// A value formatter, or null.
        /// </param>
        public TourmalineCoreRenderedCompactJsonFormatter(JsonValueFormatter valueFormatter = null)
        {
            _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
        }

        /// <summary>
        /// Format the log event into the output. Subsequent events will be newline-delimited.
        /// [ECK]{"@v":"%version%","@t":"%timestamp%",
        /// "@m":"%message%","@i":"%index%","@l":"%loglevel%","@x":"%exception%","@e":"%environment%","@n":"%applicationName%"}
        /// [ECK] - stands for ElastiCKibana use for FileBeat triggering
        /// @f - synonym of formatVersion, %formatVersion% - версия сообщения, просто одно число, инкрементируем при
        /// добавлении/удалении полей
        /// @t - synonym of timestamp, %timestamp% - временная метка в формате 2020-07-28T06:17:22.553038375Z
        /// @m - synonym of message , %message% - непосредственно само сообщение лога
        /// @i - synonym of index, %index% - is a computed 32-bit event type based on the message template(hash, not depends on
        /// message vars)
        /// @l - synonym of loglevel, %loglevel% - уровень детализации лога, например INF, ERR
        /// @x - synonym of exception, %exception% - текст эксепшена со стектрейсом(этот параметр присутствует в логе только в
        /// случае эксепшена)
        /// далее идет словарь параметров, из которых два имеют имена, которые начинаются с символа "@", но помимо этих параметров
        /// в этом словаре будут параметры сопутствующие logEvent
        /// @e - synonym of environment, %environment% - переменная имени окружения, например dev, uat, prod
        /// @n - synonym of application name, %applicationName% - имя приложения, например clever-api
        /// @v - synonym of version, %version% - версия приложения в формате n.n.n.
        /// </summary>
        /// <param name="logEvent">
        /// The event to format.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatEvent(logEvent, output, _valueFormatter);
            output.WriteLine();
        }

        /// <summary>
        /// Format the log event into the output.
        /// </summary>
        /// <param name="logEvent">
        /// The event to format.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="valueFormatter">
        /// A value formatter for <see cref="LogEventPropertyValue" />s on the event.
        /// </param>
        public void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (valueFormatter == null)
            {
                throw new ArgumentNullException(nameof(valueFormatter));
            }

            output.Write("[ECK]{\"@f\":\"");
            output.Write(FormatVersion);
            output.Write("\"");

            output.Write(",\"@t\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));

            output.Write("\",\"@m\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            output.Write(",\"@i\":\"");
            var id = EventIdHash.Compute(logEvent.MessageTemplate.Text);
            output.Write(id.ToString("x8"));
            output.Write('"');

            output.Write(",\"@l\":\"");
            output.Write(logEvent.Level);
            output.Write('\"');

            if (logEvent.Exception != null)
            {
                output.Write(",\"@x\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                var name = property.Key;

                if (name.Length != 2 && name[0] == '@')
                {
                    // Escape first '@' by doubling
                    name = '@' + name;
                }

                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
        }
    }
}