using System;
using ICalendar;

namespace ICalendar.Examples
{
    public class ExampleUsage
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== iCalendar Parser, DOM, and Validator Examples ===\n");

            // Example 1: Parse a simple iCalendar string
            Example1_ParseSimpleCalendar();

            // Example 2: Parse and validate an event
            Example2_ParseAndValidateEvent();

            // Example 3: Create a calendar programmatically
            Example3_CreateCalendarProgrammatically();

            // Example 4: Parse complex calendar with time zones
            Example4_ParseComplexCalendar();

            // Example 5: Validation errors
            Example5_ValidationErrors();

            Console.WriteLine("\n=== Examples Complete ===");
        }

        static void Example1_ParseSimpleCalendar()
        {
            Console.WriteLine("Example 1: Parse a simple iCalendar string");
            Console.WriteLine("-------------------------------------------");

            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VEVENT
UID:meeting-001@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
DTEND:20231225T110000Z
SUMMARY:Christmas Planning Meeting
DESCRIPTION:Discuss holiday arrangements and year-end activities
LOCATION:Conference Room A
STATUS:CONFIRMED
ORGANIZER:mailto:manager@example.com
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Console.WriteLine($"Calendar Version: {calendar.Version}");
            Console.WriteLine($"Product ID: {calendar.ProductId}");
            Console.WriteLine($"Number of Events: {calendar.Events.Count}");

            if (calendar.Events.Count > 0)
            {
                var vevent = calendar.Events[0];
                Console.WriteLine($"\nEvent Details:");
                Console.WriteLine($"  UID: {vevent.Uid}");
                Console.WriteLine($"  Summary: {vevent.Summary}");
                Console.WriteLine($"  Start: {vevent.DateTimeStart}");
                Console.WriteLine($"  End: {vevent.DateTimeEnd}");
                Console.WriteLine($"  Location: {vevent.Location}");
                Console.WriteLine($"  Status: {vevent.Status}");
            }

            Console.WriteLine();
        }

        static void Example2_ParseAndValidateEvent()
        {
            Console.WriteLine("Example 2: Parse and validate an event");
            Console.WriteLine("---------------------------------------");

            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Example Corp//Calendar App//EN
BEGIN:VEVENT
UID:task-reminder@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231215T140000Z
DURATION:PT2H
SUMMARY:Project Review Meeting
DESCRIPTION:Review Q4 project milestones and deliverables
STATUS:CONFIRMED
BEGIN:VALARM
ACTION:DISPLAY
TRIGGER:-PT30M
DESCRIPTION:Meeting starts in 30 minutes
END:VALARM
BEGIN:VALARM
ACTION:AUDIO
TRIGGER:-PT5M
END:VALARM
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Console.WriteLine("Parsed successfully!");
            Console.WriteLine($"Event: {calendar.Events[0].Summary}");
            Console.WriteLine($"Duration: {calendar.Events[0].Duration}");
            Console.WriteLine($"Number of Alarms: {calendar.Events[0].Alarms.Count}");

            // Validate the calendar
            var validator = new ICalendarValidator();
            var validationResult = validator.Validate(calendar);

            Console.WriteLine($"\nValidation Result: {(validationResult.IsValid ? "VALID" : "INVALID")}");
            Console.WriteLine($"Errors: {validationResult.Errors.Count}");
            Console.WriteLine($"Warnings: {validationResult.Warnings.Count}");

            Console.WriteLine();
        }

        static void Example3_CreateCalendarProgrammatically()
        {
            Console.WriteLine("Example 3: Create a calendar programmatically");
            Console.WriteLine("----------------------------------------------");

            // Create a new calendar
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//My Application//Calendar Builder//EN",
                CalendarScale = "GREGORIAN"
            };

            // Create an event
            var vevent = new VEvent
            {
                Uid = $"event-{Guid.NewGuid()}@myapp.example.com",
                DateTimeStamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"),
                DateTimeStart = "20240101T120000Z",
                DateTimeEnd = "20240101T130000Z",
                Summary = "New Year Celebration",
                Description = "Ring in the new year with colleagues",
                Location = "Office Rooftop",
                Status = "CONFIRMED"
            };

            // Add an alarm
            var alarm = new VAlarm
            {
                Action = "DISPLAY",
                Trigger = "-PT1H",
                Description = "Event starts in 1 hour!"
            };
            vevent.SubComponents.Add(alarm);

            // Add event to calendar
            calendar.SubComponents.Add(vevent);

            // Create a todo
            var vtodo = new VTodo
            {
                Uid = $"todo-{Guid.NewGuid()}@myapp.example.com",
                DateTimeStamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"),
                Summary = "Prepare presentation slides",
                Status = "IN-PROCESS",
                PercentComplete = "75",
                Due = "20231231T170000Z"
            };

            calendar.SubComponents.Add(vtodo);

            Console.WriteLine("Created calendar with:");
            Console.WriteLine($"  - {calendar.Events.Count} event(s)");
            Console.WriteLine($"  - {calendar.Todos.Count} todo(s)");

            // Validate
            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);
            Console.WriteLine($"\nValidation: {(result.IsValid ? "PASSED" : "FAILED")}");

            Console.WriteLine();
        }

        static void Example4_ParseComplexCalendar()
        {
            Console.WriteLine("Example 4: Parse complex calendar with time zones");
            Console.WriteLine("--------------------------------------------------");

            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Global Corp//Meeting Scheduler//EN
BEGIN:VTIMEZONE
TZID:America/New_York
BEGIN:STANDARD
DTSTART:20231105T020000
TZOFFSETFROM:-0400
TZOFFSETTO:-0500
TZNAME:EST
END:STANDARD
BEGIN:DAYLIGHT
DTSTART:20240310T020000
TZOFFSETFROM:-0500
TZOFFSETTO:-0400
TZNAME:EDT
END:DAYLIGHT
END:VTIMEZONE
BEGIN:VEVENT
UID:multizone-meeting@example.com
DTSTAMP:20231201T120000Z
DTSTART;TZID=America/New_York:20240315T140000
DTEND;TZID=America/New_York:20240315T153000
SUMMARY:International Team Sync
ATTENDEE;CN=""John Doe"";ROLE=CHAIR:mailto:john@example.com
ATTENDEE;CN=""Jane Smith"";ROLE=REQ-PARTICIPANT;RSVP=TRUE:mailto:jane@example.com
ATTENDEE;CN=""Bob Wilson"";ROLE=OPT-PARTICIPANT:mailto:bob@example.com
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Console.WriteLine($"Number of Time Zones: {calendar.TimeZones.Count}");
            if (calendar.TimeZones.Count > 0)
            {
                var tz = calendar.TimeZones[0];
                Console.WriteLine($"Time Zone ID: {tz.TzId}");
                Console.WriteLine($"  Standards: {tz.Standards.Count}");
                Console.WriteLine($"  Daylights: {tz.Daylights.Count}");
            }

            Console.WriteLine($"\nNumber of Events: {calendar.Events.Count}");
            if (calendar.Events.Count > 0)
            {
                var vevent = calendar.Events[0];
                Console.WriteLine($"Event: {vevent.Summary}");

                var dtstart = vevent.GetProperty("DTSTART");
                if (dtstart != null)
                {
                    Console.WriteLine($"  Start: {dtstart.Value}");
                    var tzid = dtstart.GetParameter("TZID");
                    if (tzid != null)
                    {
                        Console.WriteLine($"  Time Zone: {tzid}");
                    }
                }

                var attendees = vevent.GetProperties("ATTENDEE");
                Console.WriteLine($"  Attendees: {attendees.Count}");
                foreach (var attendee in attendees)
                {
                    var cn = attendee.GetParameter("CN");
                    var role = attendee.GetParameter("ROLE");
                    Console.WriteLine($"    - {cn} ({role})");
                }
            }

            Console.WriteLine();
        }

        static void Example5_ValidationErrors()
        {
            Console.WriteLine("Example 5: Validation errors");
            Console.WriteLine("-----------------------------");

            // Create an invalid calendar (missing required properties)
            var calendar = new VCalendar
            {
                Version = "2.0"
                // Missing PRODID
            };

            var vevent = new VEvent
            {
                // Missing UID
                DateTimeStamp = "20231201T120000Z",
                DateTimeEnd = "20231225T110000Z",
                Duration = "PT1H", // Invalid: both DTEND and DURATION
                Status = "INVALID-STATUS" // Invalid status
            };

            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Console.WriteLine($"Validation Result: {(result.IsValid ? "VALID" : "INVALID")}\n");

            if (result.Errors.Count > 0)
            {
                Console.WriteLine("Errors found:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            if (result.Warnings.Count > 0)
            {
                Console.WriteLine("\nWarnings:");
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
            }

            Console.WriteLine();
        }
    }
}
