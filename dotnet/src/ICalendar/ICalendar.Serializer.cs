using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICalendar
{
    /// <summary>
    /// Serializer for text/calendar (iCalendar) format
    /// </summary>
    public class ICalendarSerializer
    {
        private const int MaxLineLength = 75;

        /// <summary>
        /// Serializes a VCalendar object to iCalendar format string
        /// </summary>
        public string Serialize(VCalendar calendar)
        {
            var builder = new StringBuilder();
            SerializeComponent(calendar, builder);
            return builder.ToString();
        }

        /// <summary>
        /// Serializes a VCalendar object to a file
        /// </summary>
        public void SerializeToFile(VCalendar calendar, string filePath)
        {
            var content = Serialize(calendar);
            File.WriteAllText(filePath, content);
        }

        private void SerializeComponent(CalendarComponent component, StringBuilder builder)
        {
            // Write BEGIN
            WriteLine(builder, $"BEGIN:{component.ComponentType}");

            // Write properties
            foreach (var propertyList in component.Properties.Values)
            {
                foreach (var property in propertyList)
                {
                    SerializeProperty(property, builder);
                }
            }

            // Write sub-components
            foreach (var subComponent in component.SubComponents)
            {
                SerializeComponent(subComponent, builder);
            }

            // Write END
            WriteLine(builder, $"END:{component.ComponentType}");
        }

        private void SerializeProperty(CalendarProperty property, StringBuilder builder)
        {
            var line = new StringBuilder();
            line.Append(property.Name);

            // Add parameters
            if (property.Parameters.Count > 0)
            {
                foreach (var parameterList in property.Parameters)
                {
                    var paramName = parameterList.Key;
                    var paramValues = parameterList.Value;

                    foreach (var paramValue in paramValues)
                    {
                        line.Append(';');
                        line.Append(paramName);
                        line.Append('=');

                        // Quote parameter value if it contains special characters
                        if (NeedsQuoting(paramValue))
                        {
                            line.Append('"');
                            line.Append(paramValue);
                            line.Append('"');
                        }
                        else
                        {
                            line.Append(paramValue);
                        }
                    }
                }
            }

            line.Append(':');
            line.Append(EscapeValue(property.Value));

            WriteLine(builder, line.ToString());
        }

        private void WriteLine(StringBuilder builder, string line)
        {
            if (line.Length <= MaxLineLength)
            {
                builder.AppendLine(line);
                return;
            }

            // Fold long lines (RFC 5545 Section 3.1)
            var firstLine = line.Substring(0, MaxLineLength);
            builder.AppendLine(firstLine);

            var remaining = line.Substring(MaxLineLength);
            while (remaining.Length > 0)
            {
                var chunkLength = Math.Min(MaxLineLength - 1, remaining.Length);
                var chunk = remaining.Substring(0, chunkLength);
                builder.Append(' ');
                builder.AppendLine(chunk);
                remaining = remaining.Substring(chunkLength);
            }
        }

        private bool NeedsQuoting(string value)
        {
            return value.Contains(':') ||
                   value.Contains(';') ||
                   value.Contains(',') ||
                   value.Contains(' ') ||
                   value.Contains('\t');
        }

        private string EscapeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\n", "\\n")
                .Replace("\r", "");
        }
    }

    /// <summary>
    /// Extension methods for convenient serialization
    /// </summary>
    public static class CalendarSerializerExtensions
    {
        /// <summary>
        /// Converts a VCalendar to iCalendar format string
        /// </summary>
        public static string ToICalendar(this VCalendar calendar)
        {
            var serializer = new ICalendarSerializer();
            return serializer.Serialize(calendar);
        }

        /// <summary>
        /// Saves a VCalendar to a file in iCalendar format
        /// </summary>
        public static void SaveToFile(this VCalendar calendar, string filePath)
        {
            var serializer = new ICalendarSerializer();
            serializer.SerializeToFile(calendar, filePath);
        }
    }
}
