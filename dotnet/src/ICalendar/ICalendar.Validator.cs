using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ICalendar
{
    /// <summary>
    /// Validator for iCalendar components according to RFC 5545
    /// </summary>
    public class ICalendarValidator
    {
        public ValidationResult Validate(VCalendar calendar)
        {
            var result = new ValidationResult();

            ValidateCalendar(calendar, result);

            return result;
        }

        private void ValidateCalendar(VCalendar calendar, ValidationResult result)
        {
            // VCALENDAR MUST have VERSION and PRODID
            ValidateRequiredProperty(calendar, "VERSION", result);
            ValidateRequiredProperty(calendar, "PRODID", result);

            // VERSION must be 2.0
            var version = calendar.GetProperty("VERSION");
            if (version != null && version.Value != "2.0")
            {
                result.AddError($"VERSION must be 2.0, found: {version.Value}");
            }

            // CALSCALE is optional, but if present must be GREGORIAN (or custom)
            var calscale = calendar.GetProperty("CALSCALE");
            if (calscale != null && !string.IsNullOrEmpty(calscale.Value))
            {
                // CALSCALE is typically GREGORIAN
                // We'll accept any value but warn if not GREGORIAN
                if (!calscale.Value.Equals("GREGORIAN", StringComparison.OrdinalIgnoreCase))
                {
                    result.AddWarning($"CALSCALE is not GREGORIAN: {calscale.Value}");
                }
            }

            // Validate all events
            foreach (var vevent in calendar.Events)
            {
                ValidateEvent(vevent, result);
            }

            // Validate all todos
            foreach (var vtodo in calendar.Todos)
            {
                ValidateTodo(vtodo, result);
            }

            // Validate all journals
            foreach (var vjournal in calendar.Journals)
            {
                ValidateJournal(vjournal, result);
            }

            // Validate all free/busy
            foreach (var vfreebusy in calendar.FreeBusies)
            {
                ValidateFreeBusy(vfreebusy, result);
            }

            // Validate all time zones
            foreach (var vtimezone in calendar.TimeZones)
            {
                ValidateTimeZone(vtimezone, result);
            }
        }

        private void ValidateEvent(VEvent vevent, ValidationResult result)
        {
            // VEVENT MUST have UID and DTSTAMP
            ValidateRequiredProperty(vevent, "UID", result, "VEVENT");
            ValidateRequiredProperty(vevent, "DTSTAMP", result, "VEVENT");

            // Validate UID format
            var uid = vevent.GetProperty("UID");
            if (uid != null && string.IsNullOrWhiteSpace(uid.Value))
            {
                result.AddError("VEVENT UID cannot be empty");
            }

            // Validate DTSTAMP format
            ValidateDateTimeFormat(vevent.GetProperty("DTSTAMP"), result, "DTSTAMP");

            // DTSTART is optional, but recommended
            var dtstart = vevent.GetProperty("DTSTART");
            if (dtstart != null)
            {
                ValidateDateTimeFormat(dtstart, result, "DTSTART");
            }

            // DTEND and DURATION are mutually exclusive
            var dtend = vevent.GetProperty("DTEND");
            var duration = vevent.GetProperty("DURATION");

            if (dtend != null && duration != null)
            {
                result.AddError("VEVENT cannot have both DTEND and DURATION");
            }

            if (dtend != null)
            {
                ValidateDateTimeFormat(dtend, result, "DTEND");
            }

            if (duration != null)
            {
                ValidateDurationFormat(duration, result);
            }

            // Validate STATUS if present
            var status = vevent.GetProperty("STATUS");
            if (status != null)
            {
                var validStatuses = new[] { "TENTATIVE", "CONFIRMED", "CANCELLED" };
                if (!validStatuses.Contains(status.Value.ToUpperInvariant()))
                {
                    result.AddError($"Invalid VEVENT STATUS: {status.Value}. Must be one of: {string.Join(", ", validStatuses)}");
                }
            }

            // Validate alarms
            foreach (var alarm in vevent.Alarms)
            {
                ValidateAlarm(alarm, result);
            }
        }

        private void ValidateTodo(VTodo vtodo, ValidationResult result)
        {
            // VTODO MUST have UID and DTSTAMP
            ValidateRequiredProperty(vtodo, "UID", result, "VTODO");
            ValidateRequiredProperty(vtodo, "DTSTAMP", result, "VTODO");

            // Validate UID format
            var uid = vtodo.GetProperty("UID");
            if (uid != null && string.IsNullOrWhiteSpace(uid.Value))
            {
                result.AddError("VTODO UID cannot be empty");
            }

            // Validate DTSTAMP format
            ValidateDateTimeFormat(vtodo.GetProperty("DTSTAMP"), result, "DTSTAMP");

            // Validate STATUS if present
            var status = vtodo.GetProperty("STATUS");
            if (status != null)
            {
                var validStatuses = new[] { "NEEDS-ACTION", "COMPLETED", "IN-PROCESS", "CANCELLED" };
                if (!validStatuses.Contains(status.Value.ToUpperInvariant()))
                {
                    result.AddError($"Invalid VTODO STATUS: {status.Value}. Must be one of: {string.Join(", ", validStatuses)}");
                }
            }

            // Validate PERCENT-COMPLETE if present
            var percentComplete = vtodo.GetProperty("PERCENT-COMPLETE");
            if (percentComplete != null)
            {
                if (!int.TryParse(percentComplete.Value, out int percent) || percent < 0 || percent > 100)
                {
                    result.AddError($"Invalid PERCENT-COMPLETE: {percentComplete.Value}. Must be an integer between 0 and 100");
                }
            }

            // Validate COMPLETED format
            ValidateDateTimeFormat(vtodo.GetProperty("COMPLETED"), result, "COMPLETED");

            // Validate DUE format
            ValidateDateTimeFormat(vtodo.GetProperty("DUE"), result, "DUE");

            // Validate alarms
            foreach (var alarm in vtodo.Alarms)
            {
                ValidateAlarm(alarm, result);
            }
        }

        private void ValidateJournal(VJournal vjournal, ValidationResult result)
        {
            // VJOURNAL MUST have UID and DTSTAMP
            ValidateRequiredProperty(vjournal, "UID", result, "VJOURNAL");
            ValidateRequiredProperty(vjournal, "DTSTAMP", result, "VJOURNAL");

            // Validate UID format
            var uid = vjournal.GetProperty("UID");
            if (uid != null && string.IsNullOrWhiteSpace(uid.Value))
            {
                result.AddError("VJOURNAL UID cannot be empty");
            }

            // Validate DTSTAMP format
            ValidateDateTimeFormat(vjournal.GetProperty("DTSTAMP"), result, "DTSTAMP");
        }

        private void ValidateFreeBusy(VFreeBusy vfreebusy, ValidationResult result)
        {
            // VFREEBUSY MUST have UID and DTSTAMP
            ValidateRequiredProperty(vfreebusy, "UID", result, "VFREEBUSY");
            ValidateRequiredProperty(vfreebusy, "DTSTAMP", result, "VFREEBUSY");

            // Validate UID format
            var uid = vfreebusy.GetProperty("UID");
            if (uid != null && string.IsNullOrWhiteSpace(uid.Value))
            {
                result.AddError("VFREEBUSY UID cannot be empty");
            }

            // Validate DTSTAMP format
            ValidateDateTimeFormat(vfreebusy.GetProperty("DTSTAMP"), result, "DTSTAMP");

            // Validate DTSTART and DTEND if present
            ValidateDateTimeFormat(vfreebusy.GetProperty("DTSTART"), result, "DTSTART");
            ValidateDateTimeFormat(vfreebusy.GetProperty("DTEND"), result, "DTEND");
        }

        private void ValidateTimeZone(VTimeZone vtimezone, ValidationResult result)
        {
            // VTIMEZONE MUST have TZID
            ValidateRequiredProperty(vtimezone, "TZID", result, "VTIMEZONE");

            // VTIMEZONE must have at least one STANDARD or DAYLIGHT
            if (vtimezone.Standards.Count == 0 && vtimezone.Daylights.Count == 0)
            {
                result.AddError("VTIMEZONE must contain at least one STANDARD or DAYLIGHT component");
            }

            // Validate STANDARD components
            foreach (var standard in vtimezone.Standards)
            {
                ValidateTimeZoneComponent(standard, result, "STANDARD");
            }

            // Validate DAYLIGHT components
            foreach (var daylight in vtimezone.Daylights)
            {
                ValidateTimeZoneComponent(daylight, result, "DAYLIGHT");
            }
        }

        private void ValidateTimeZoneComponent(CalendarComponent component, ValidationResult result, string componentType)
        {
            // STANDARD/DAYLIGHT MUST have DTSTART, TZOFFSETFROM, TZOFFSETTO
            ValidateRequiredProperty(component, "DTSTART", result, componentType);
            ValidateRequiredProperty(component, "TZOFFSETFROM", result, componentType);
            ValidateRequiredProperty(component, "TZOFFSETTO", result, componentType);

            // Validate DTSTART format
            ValidateDateTimeFormat(component.GetProperty("DTSTART"), result, "DTSTART");

            // Validate UTC offset format
            ValidateUtcOffsetFormat(component.GetProperty("TZOFFSETFROM"), result, "TZOFFSETFROM");
            ValidateUtcOffsetFormat(component.GetProperty("TZOFFSETTO"), result, "TZOFFSETTO");
        }

        private void ValidateAlarm(VAlarm valarm, ValidationResult result)
        {
            // VALARM MUST have ACTION
            ValidateRequiredProperty(valarm, "ACTION", result, "VALARM");

            var action = valarm.GetProperty("ACTION");
            if (action != null)
            {
                var validActions = new[] { "AUDIO", "DISPLAY", "EMAIL" };
                var actionValue = action.Value.ToUpperInvariant();

                if (!validActions.Contains(actionValue))
                {
                    result.AddWarning($"Unknown VALARM ACTION: {action.Value}");
                }

                // DISPLAY and EMAIL actions require DESCRIPTION
                if (actionValue == "DISPLAY" || actionValue == "EMAIL")
                {
                    ValidateRequiredProperty(valarm, "DESCRIPTION", result, $"VALARM with ACTION={actionValue}");
                }

                // EMAIL action requires SUMMARY
                if (actionValue == "EMAIL")
                {
                    ValidateRequiredProperty(valarm, "SUMMARY", result, "VALARM with ACTION=EMAIL");
                }
            }

            // VALARM MUST have TRIGGER
            ValidateRequiredProperty(valarm, "TRIGGER", result, "VALARM");
        }

        private void ValidateRequiredProperty(CalendarComponent component, string propertyName, ValidationResult result, string? componentType = null)
        {
            var property = component.GetProperty(propertyName);
            if (property == null)
            {
                var componentName = componentType ?? component.ComponentType;
                result.AddError($"{componentName} is missing required property: {propertyName}");
            }
        }

        private void ValidateDateTimeFormat(CalendarProperty property, ValidationResult result, string propertyName)
        {
            if (property == null)
                return;

            var value = property.Value;

            // DATE-TIME format: YYYYMMDD'T'HHMMSS or YYYYMMDD'T'HHMMSS'Z'
            // DATE format: YYYYMMDD
            var dateTimePattern = @"^\d{8}T\d{6}Z?$";
            var datePattern = @"^\d{8}$";

            if (!Regex.IsMatch(value, dateTimePattern) && !Regex.IsMatch(value, datePattern))
            {
                result.AddError($"Invalid date/time format for {propertyName}: {value}. Expected YYYYMMDD or YYYYMMDDTHHMMSS[Z]");
            }
        }

        private void ValidateDurationFormat(CalendarProperty property, ValidationResult result)
        {
            if (property == null)
                return;

            var value = property.Value;

            // DURATION format: ["+"/"-"] "P" (dur-date / dur-time / dur-week)
            var durationPattern = @"^[+-]?P(\d+W)|(\d+D)?(T(\d+H)?(\d+M)?(\d+S)?)?$";

            if (!Regex.IsMatch(value, durationPattern))
            {
                result.AddError($"Invalid DURATION format: {value}. Expected ISO 8601 duration format (e.g., P1D, PT1H30M)");
            }
        }

        private void ValidateUtcOffsetFormat(CalendarProperty property, ValidationResult result, string propertyName)
        {
            if (property == null)
                return;

            var value = property.Value;

            // UTC offset format: ["+"/"-"] HHMM or HHMMSS
            var offsetPattern = @"^[+-]\d{4}(\d{2})?$";

            if (!Regex.IsMatch(value, offsetPattern))
            {
                result.AddError($"Invalid UTC offset format for {propertyName}: {value}. Expected +/-HHMM or +/-HHMMSS");
            }
        }
    }

    /// <summary>
    /// Result of validation operation
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public string GetSummary()
        {
            var summary = $"Validation Result: {(IsValid ? "VALID" : "INVALID")}\n";
            summary += $"Errors: {Errors.Count}\n";
            summary += $"Warnings: {Warnings.Count}\n\n";

            if (Errors.Count > 0)
            {
                summary += "Errors:\n";
                foreach (var error in Errors)
                {
                    summary += $"  - {error}\n";
                }
                summary += "\n";
            }

            if (Warnings.Count > 0)
            {
                summary += "Warnings:\n";
                foreach (var warning in Warnings)
                {
                    summary += $"  - {warning}\n";
                }
            }

            return summary;
        }
    }
}
