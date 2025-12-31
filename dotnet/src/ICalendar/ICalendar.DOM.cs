using System;
using System.Collections.Generic;
using System.Linq;

namespace ICalendar
{
    /// <summary>
    /// Base class for all iCalendar components
    /// </summary>
    public abstract class CalendarComponent
    {
        public Dictionary<string, List<CalendarProperty>> Properties { get; set; } = new Dictionary<string, List<CalendarProperty>>();
        public List<CalendarComponent> SubComponents { get; set; } = new List<CalendarComponent>();

        public void AddProperty(CalendarProperty property)
        {
            if (!Properties.ContainsKey(property.Name))
            {
                Properties[property.Name] = new List<CalendarProperty>();
            }
            Properties[property.Name].Add(property);
        }

        public CalendarProperty GetProperty(string name)
        {
            return Properties.ContainsKey(name) ? Properties[name].FirstOrDefault() : null;
        }

        public List<CalendarProperty> GetProperties(string name)
        {
            return Properties.ContainsKey(name) ? Properties[name] : new List<CalendarProperty>();
        }

        public abstract string ComponentType { get; }
    }

    /// <summary>
    /// Represents a calendar property with parameters and value
    /// </summary>
    public class CalendarProperty
    {
        public string Name { get; set; }
        public Dictionary<string, List<string>> Parameters { get; set; } = new Dictionary<string, List<string>>();
        public string Value { get; set; }

        public CalendarProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void AddParameter(string paramName, string paramValue)
        {
            if (!Parameters.ContainsKey(paramName))
            {
                Parameters[paramName] = new List<string>();
            }
            Parameters[paramName].Add(paramValue);
        }

        public string GetParameter(string paramName)
        {
            return Parameters.ContainsKey(paramName) ? Parameters[paramName].FirstOrDefault() : null;
        }

        public List<string> GetParameters(string paramName)
        {
            return Parameters.ContainsKey(paramName) ? Parameters[paramName] : new List<string>();
        }
    }

    /// <summary>
    /// Root calendar object (VCALENDAR)
    /// </summary>
    public class VCalendar : CalendarComponent
    {
        public override string ComponentType => "VCALENDAR";

        public string Version
        {
            get => GetProperty("VERSION")?.Value;
            set => AddProperty(new CalendarProperty("VERSION", value));
        }

        public string ProductId
        {
            get => GetProperty("PRODID")?.Value;
            set => AddProperty(new CalendarProperty("PRODID", value));
        }

        public string CalendarScale
        {
            get => GetProperty("CALSCALE")?.Value;
            set => AddProperty(new CalendarProperty("CALSCALE", value));
        }

        public string Method
        {
            get => GetProperty("METHOD")?.Value;
            set => AddProperty(new CalendarProperty("METHOD", value));
        }

        public List<VEvent> Events => SubComponents.OfType<VEvent>().ToList();
        public List<VTodo> Todos => SubComponents.OfType<VTodo>().ToList();
        public List<VJournal> Journals => SubComponents.OfType<VJournal>().ToList();
        public List<VFreeBusy> FreeBusies => SubComponents.OfType<VFreeBusy>().ToList();
        public List<VTimeZone> TimeZones => SubComponents.OfType<VTimeZone>().ToList();
    }

    /// <summary>
    /// Event component (VEVENT)
    /// </summary>
    public class VEvent : CalendarComponent
    {
        public override string ComponentType => "VEVENT";

        public string Uid
        {
            get => GetProperty("UID")?.Value;
            set => AddProperty(new CalendarProperty("UID", value));
        }

        public string DateTimeStamp
        {
            get => GetProperty("DTSTAMP")?.Value;
            set => AddProperty(new CalendarProperty("DTSTAMP", value));
        }

        public string DateTimeStart
        {
            get => GetProperty("DTSTART")?.Value;
            set => AddProperty(new CalendarProperty("DTSTART", value));
        }

        public string DateTimeEnd
        {
            get => GetProperty("DTEND")?.Value;
            set => AddProperty(new CalendarProperty("DTEND", value));
        }

        public string Duration
        {
            get => GetProperty("DURATION")?.Value;
            set => AddProperty(new CalendarProperty("DURATION", value));
        }

        public string Summary
        {
            get => GetProperty("SUMMARY")?.Value;
            set => AddProperty(new CalendarProperty("SUMMARY", value));
        }

        public string Description
        {
            get => GetProperty("DESCRIPTION")?.Value;
            set => AddProperty(new CalendarProperty("DESCRIPTION", value));
        }

        public string Location
        {
            get => GetProperty("LOCATION")?.Value;
            set => AddProperty(new CalendarProperty("LOCATION", value));
        }

        public string Status
        {
            get => GetProperty("STATUS")?.Value;
            set => AddProperty(new CalendarProperty("STATUS", value));
        }

        public string Organizer
        {
            get => GetProperty("ORGANIZER")?.Value;
            set => AddProperty(new CalendarProperty("ORGANIZER", value));
        }

        public List<VAlarm> Alarms => SubComponents.OfType<VAlarm>().ToList();
    }

    /// <summary>
    /// To-do component (VTODO)
    /// </summary>
    public class VTodo : CalendarComponent
    {
        public override string ComponentType => "VTODO";

        public string Uid
        {
            get => GetProperty("UID")?.Value;
            set => AddProperty(new CalendarProperty("UID", value));
        }

        public string DateTimeStamp
        {
            get => GetProperty("DTSTAMP")?.Value;
            set => AddProperty(new CalendarProperty("DTSTAMP", value));
        }

        public string Summary
        {
            get => GetProperty("SUMMARY")?.Value;
            set => AddProperty(new CalendarProperty("SUMMARY", value));
        }

        public string Status
        {
            get => GetProperty("STATUS")?.Value;
            set => AddProperty(new CalendarProperty("STATUS", value));
        }

        public string Due
        {
            get => GetProperty("DUE")?.Value;
            set => AddProperty(new CalendarProperty("DUE", value));
        }

        public string Completed
        {
            get => GetProperty("COMPLETED")?.Value;
            set => AddProperty(new CalendarProperty("COMPLETED", value));
        }

        public string PercentComplete
        {
            get => GetProperty("PERCENT-COMPLETE")?.Value;
            set => AddProperty(new CalendarProperty("PERCENT-COMPLETE", value));
        }

        public List<VAlarm> Alarms => SubComponents.OfType<VAlarm>().ToList();
    }

    /// <summary>
    /// Journal component (VJOURNAL)
    /// </summary>
    public class VJournal : CalendarComponent
    {
        public override string ComponentType => "VJOURNAL";

        public string Uid
        {
            get => GetProperty("UID")?.Value;
            set => AddProperty(new CalendarProperty("UID", value));
        }

        public string DateTimeStamp
        {
            get => GetProperty("DTSTAMP")?.Value;
            set => AddProperty(new CalendarProperty("DTSTAMP", value));
        }

        public string Summary
        {
            get => GetProperty("SUMMARY")?.Value;
            set => AddProperty(new CalendarProperty("SUMMARY", value));
        }

        public string Description
        {
            get => GetProperty("DESCRIPTION")?.Value;
            set => AddProperty(new CalendarProperty("DESCRIPTION", value));
        }
    }

    /// <summary>
    /// Free/busy component (VFREEBUSY)
    /// </summary>
    public class VFreeBusy : CalendarComponent
    {
        public override string ComponentType => "VFREEBUSY";

        public string Uid
        {
            get => GetProperty("UID")?.Value;
            set => AddProperty(new CalendarProperty("UID", value));
        }

        public string DateTimeStamp
        {
            get => GetProperty("DTSTAMP")?.Value;
            set => AddProperty(new CalendarProperty("DTSTAMP", value));
        }

        public string DateTimeStart
        {
            get => GetProperty("DTSTART")?.Value;
            set => AddProperty(new CalendarProperty("DTSTART", value));
        }

        public string DateTimeEnd
        {
            get => GetProperty("DTEND")?.Value;
            set => AddProperty(new CalendarProperty("DTEND", value));
        }
    }

    /// <summary>
    /// Time zone component (VTIMEZONE)
    /// </summary>
    public class VTimeZone : CalendarComponent
    {
        public override string ComponentType => "VTIMEZONE";

        public string TzId
        {
            get => GetProperty("TZID")?.Value;
            set => AddProperty(new CalendarProperty("TZID", value));
        }

        public List<VTimeZoneStandard> Standards => SubComponents.OfType<VTimeZoneStandard>().ToList();
        public List<VTimeZoneDaylight> Daylights => SubComponents.OfType<VTimeZoneDaylight>().ToList();
    }

    /// <summary>
    /// Standard time zone component (STANDARD)
    /// </summary>
    public class VTimeZoneStandard : CalendarComponent
    {
        public override string ComponentType => "STANDARD";

        public string DateTimeStart
        {
            get => GetProperty("DTSTART")?.Value;
            set => AddProperty(new CalendarProperty("DTSTART", value));
        }

        public string TzOffsetFrom
        {
            get => GetProperty("TZOFFSETFROM")?.Value;
            set => AddProperty(new CalendarProperty("TZOFFSETFROM", value));
        }

        public string TzOffsetTo
        {
            get => GetProperty("TZOFFSETTO")?.Value;
            set => AddProperty(new CalendarProperty("TZOFFSETTO", value));
        }
    }

    /// <summary>
    /// Daylight time zone component (DAYLIGHT)
    /// </summary>
    public class VTimeZoneDaylight : CalendarComponent
    {
        public override string ComponentType => "DAYLIGHT";

        public string DateTimeStart
        {
            get => GetProperty("DTSTART")?.Value;
            set => AddProperty(new CalendarProperty("DTSTART", value));
        }

        public string TzOffsetFrom
        {
            get => GetProperty("TZOFFSETFROM")?.Value;
            set => AddProperty(new CalendarProperty("TZOFFSETFROM", value));
        }

        public string TzOffsetTo
        {
            get => GetProperty("TZOFFSETTO")?.Value;
            set => AddProperty(new CalendarProperty("TZOFFSETTO", value));
        }
    }

    /// <summary>
    /// Alarm component (VALARM)
    /// </summary>
    public class VAlarm : CalendarComponent
    {
        public override string ComponentType => "VALARM";

        public string Action
        {
            get => GetProperty("ACTION")?.Value;
            set => AddProperty(new CalendarProperty("ACTION", value));
        }

        public string Trigger
        {
            get => GetProperty("TRIGGER")?.Value;
            set => AddProperty(new CalendarProperty("TRIGGER", value));
        }

        public string Description
        {
            get => GetProperty("DESCRIPTION")?.Value;
            set => AddProperty(new CalendarProperty("DESCRIPTION", value));
        }

        public string Summary
        {
            get => GetProperty("SUMMARY")?.Value;
            set => AddProperty(new CalendarProperty("SUMMARY", value));
        }
    }
}
