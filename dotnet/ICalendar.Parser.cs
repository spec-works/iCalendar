using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ICalendar
{
    /// <summary>
    /// Parser for text/calendar (iCalendar) format
    /// </summary>
    public class ICalendarParser
    {
        private List<string> _lines;
        private int _currentLine;

        public VCalendar Parse(string calendarText)
        {
            _lines = UnfoldLines(calendarText);
            _currentLine = 0;

            if (_currentLine >= _lines.Count)
            {
                throw new ParseException("Empty calendar data");
            }

            var line = _lines[_currentLine];
            if (!line.Equals("BEGIN:VCALENDAR", StringComparison.OrdinalIgnoreCase))
            {
                throw new ParseException($"Expected BEGIN:VCALENDAR but got: {line}");
            }

            _currentLine++;
            var calendar = new VCalendar();
            ParseComponent(calendar);

            return calendar;
        }

        public VCalendar ParseFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            return Parse(content);
        }

        private List<string> UnfoldLines(string calendarText)
        {
            var unfoldedLines = new List<string>();
            var lines = calendarText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            StringBuilder currentLine = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
                {
                    // Continuation line - remove leading whitespace and append
                    currentLine.Append(line.Substring(1));
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        var unfoldedLine = currentLine.ToString();
                        if (!string.IsNullOrWhiteSpace(unfoldedLine))
                        {
                            unfoldedLines.Add(unfoldedLine);
                        }
                    }
                    currentLine = new StringBuilder(line);
                }
            }

            if (currentLine.Length > 0)
            {
                var unfoldedLine = currentLine.ToString();
                if (!string.IsNullOrWhiteSpace(unfoldedLine))
                {
                    unfoldedLines.Add(unfoldedLine);
                }
            }

            return unfoldedLines;
        }

        private void ParseComponent(CalendarComponent component)
        {
            while (_currentLine < _lines.Count)
            {
                var line = _lines[_currentLine];

                if (line.StartsWith("BEGIN:", StringComparison.OrdinalIgnoreCase))
                {
                    var subComponentType = line.Substring(6).ToUpperInvariant();
                    _currentLine++;

                    var subComponent = CreateComponent(subComponentType);
                    ParseComponent(subComponent);
                    component.SubComponents.Add(subComponent);
                }
                else if (line.StartsWith("END:", StringComparison.OrdinalIgnoreCase))
                {
                    var endComponentType = line.Substring(4).ToUpperInvariant();
                    if (endComponentType != component.ComponentType)
                    {
                        throw new ParseException($"Mismatched END tag: expected END:{component.ComponentType} but got END:{endComponentType}");
                    }
                    _currentLine++;
                    return;
                }
                else
                {
                    var property = ParseProperty(line);
                    component.AddProperty(property);
                    _currentLine++;
                }
            }

            throw new ParseException($"Unexpected end of input while parsing {component.ComponentType}");
        }

        private CalendarComponent CreateComponent(string componentType)
        {
            return componentType switch
            {
                "VCALENDAR" => new VCalendar(),
                "VEVENT" => new VEvent(),
                "VTODO" => new VTodo(),
                "VJOURNAL" => new VJournal(),
                "VFREEBUSY" => new VFreeBusy(),
                "VTIMEZONE" => new VTimeZone(),
                "STANDARD" => new VTimeZoneStandard(),
                "DAYLIGHT" => new VTimeZoneDaylight(),
                "VALARM" => new VAlarm(),
                _ => throw new ParseException($"Unknown component type: {componentType}")
            };
        }

        private CalendarProperty ParseProperty(string line)
        {
            var colonIndex = FindUnquotedChar(line, ':');
            if (colonIndex == -1)
            {
                throw new ParseException($"Invalid property line (missing colon): {line}");
            }

            var nameAndParams = line.Substring(0, colonIndex);
            var value = line.Substring(colonIndex + 1);

            // Unescape value
            value = UnescapeValue(value);

            // Parse name and parameters
            var semicolonIndex = FindUnquotedChar(nameAndParams, ';');
            string propertyName;
            string paramsPart = null;

            if (semicolonIndex != -1)
            {
                propertyName = nameAndParams.Substring(0, semicolonIndex).ToUpperInvariant();
                paramsPart = nameAndParams.Substring(semicolonIndex + 1);
            }
            else
            {
                propertyName = nameAndParams.ToUpperInvariant();
            }

            var property = new CalendarProperty(propertyName, value);

            if (paramsPart != null)
            {
                ParseParameters(paramsPart, property);
            }

            return property;
        }

        private void ParseParameters(string paramsPart, CalendarProperty property)
        {
            var parameters = SplitParameters(paramsPart);

            foreach (var param in parameters)
            {
                var equalsIndex = param.IndexOf('=');
                if (equalsIndex == -1)
                {
                    throw new ParseException($"Invalid parameter (missing equals): {param}");
                }

                var paramName = param.Substring(0, equalsIndex).ToUpperInvariant();
                var paramValue = param.Substring(equalsIndex + 1);

                // Remove quotes if present
                if (paramValue.StartsWith("\"") && paramValue.EndsWith("\"") && paramValue.Length >= 2)
                {
                    paramValue = paramValue.Substring(1, paramValue.Length - 2);
                }

                // Handle comma-separated values
                var values = SplitParameterValues(paramValue);
                foreach (var value in values)
                {
                    property.AddParameter(paramName, value);
                }
            }
        }

        private List<string> SplitParameters(string paramsPart)
        {
            var parameters = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < paramsPart.Length; i++)
            {
                char c = paramsPart[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == ';' && !inQuotes)
                {
                    parameters.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                parameters.Add(current.ToString());
            }

            return parameters;
        }

        private List<string> SplitParameterValues(string paramValue)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < paramValue.Length; i++)
            {
                char c = paramValue[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                values.Add(current.ToString());
            }

            return values;
        }

        private int FindUnquotedChar(string str, char target)
        {
            bool inQuotes = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (str[i] == target && !inQuotes)
                {
                    return i;
                }
            }
            return -1;
        }

        private string UnescapeValue(string value)
        {
            return value
                .Replace("\\n", "\n")
                .Replace("\\N", "\n")
                .Replace("\\;", ";")
                .Replace("\\,", ",")
                .Replace("\\\\", "\\");
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
