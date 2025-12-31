using System;
using System.Linq;
using Xunit;

namespace ICalendar.Tests
{
    public class ICalendarParserTests
    {
        [Fact]
        public void Parse_SimpleVCalendar_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Equal("2.0", calendar.Version);
            Assert.Equal("-//My Company//My Product//EN", calendar.ProductId);
        }

        [Fact]
        public void Parse_VCalendarWithEvent_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:12345@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
DTEND:20231225T110000Z
SUMMARY:Christmas Meeting
DESCRIPTION:Discuss holiday plans
LOCATION:Conference Room A
STATUS:CONFIRMED
ORGANIZER:mailto:organizer@example.com
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);

            var vevent = calendar.Events[0];
            Assert.Equal("12345@example.com", vevent.Uid);
            Assert.Equal("20231201T120000Z", vevent.DateTimeStamp);
            Assert.Equal("20231225T100000Z", vevent.DateTimeStart);
            Assert.Equal("20231225T110000Z", vevent.DateTimeEnd);
            Assert.Equal("Christmas Meeting", vevent.Summary);
            Assert.Equal("Discuss holiday plans", vevent.Description);
            Assert.Equal("Conference Room A", vevent.Location);
            Assert.Equal("CONFIRMED", vevent.Status);
            Assert.Equal("mailto:organizer@example.com", vevent.Organizer);
        }

        [Fact]
        public void Parse_VCalendarWithTodo_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VTODO
UID:todo-123@example.com
DTSTAMP:20231201T120000Z
SUMMARY:Complete project documentation
STATUS:IN-PROCESS
PERCENT-COMPLETE:50
DUE:20231231T235959Z
END:VTODO
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Todos);

            var vtodo = calendar.Todos[0];
            Assert.Equal("todo-123@example.com", vtodo.Uid);
            Assert.Equal("20231201T120000Z", vtodo.DateTimeStamp);
            Assert.Equal("Complete project documentation", vtodo.Summary);
            Assert.Equal("IN-PROCESS", vtodo.Status);
            Assert.Equal("50", vtodo.PercentComplete);
            Assert.Equal("20231231T235959Z", vtodo.Due);
        }

        [Fact]
        public void Parse_EventWithAlarm_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event-with-alarm@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
SUMMARY:Important Meeting
BEGIN:VALARM
ACTION:DISPLAY
TRIGGER:-PT15M
DESCRIPTION:Meeting reminder
END:VALARM
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            var vevent = calendar.Events[0];
            Assert.Single(vevent.Alarms);

            var valarm = vevent.Alarms[0];
            Assert.Equal("DISPLAY", valarm.Action);
            Assert.Equal("-PT15M", valarm.Trigger);
            Assert.Equal("Meeting reminder", valarm.Description);
        }

        [Fact]
        public void Parse_EventWithDuration_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event-duration@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
DURATION:PT1H30M
SUMMARY:Meeting with duration
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            var vevent = calendar.Events[0];
            Assert.Equal("PT1H30M", vevent.Duration);
            Assert.Null(vevent.DateTimeEnd);
        }

        [Fact]
        public void Parse_PropertyWithParameters_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event-params@example.com
DTSTAMP:20231201T120000Z
DTSTART;TZID=America/New_York:20231225T100000
SUMMARY:Event with timezone
ATTENDEE;CN=""John Doe"";ROLE=REQ-PARTICIPANT:mailto:john@example.com
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            var vevent = calendar.Events[0];

            var dtstart = vevent.GetProperty("DTSTART");
            Assert.NotNull(dtstart);
            Assert.Equal("20231225T100000", dtstart.Value);
            Assert.Equal("America/New_York", dtstart.GetParameter("TZID"));

            var attendee = vevent.GetProperty("ATTENDEE");
            Assert.NotNull(attendee);
            Assert.Equal("John Doe", attendee.GetParameter("CN"));
            Assert.Equal("REQ-PARTICIPANT", attendee.GetParameter("ROLE"));
            Assert.Equal("mailto:john@example.com", attendee.Value);
        }

        [Fact]
        public void Parse_UnfoldedLines_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:unfolded@example.com
DTSTAMP:20231201T120000Z
DESCRIPTION:This is a very long description that spans multiple lines
 and should be unfolded correctly when parsed by the iCalendar parser i
 mplementation.
SUMMARY:Event with long description
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            var vevent = calendar.Events[0];
            var description = vevent.Description;
            Assert.Contains("very long description", description);
            Assert.DoesNotContain("\n", description);
        }

        [Fact]
        public void Parse_EscapedCharacters_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:escaped@example.com
DTSTAMP:20231201T120000Z
SUMMARY:Meeting\, Planning\; Review
DESCRIPTION:Line 1\nLine 2\nLine 3
LOCATION:Room\\Building A
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.Events);
            var vevent = calendar.Events[0];
            Assert.Equal("Meeting, Planning; Review", vevent.Summary);
            Assert.Contains("\n", vevent.Description);
            Assert.Contains("Line 1", vevent.Description);
            Assert.Contains("Line 2", vevent.Description);
            Assert.Equal("Room\\Building A", vevent.Location);
        }

        [Fact]
        public void Parse_VTimeZone_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VTIMEZONE
TZID:America/New_York
BEGIN:STANDARD
DTSTART:20231105T020000
TZOFFSETFROM:-0400
TZOFFSETTO:-0500
END:STANDARD
BEGIN:DAYLIGHT
DTSTART:20240310T020000
TZOFFSETFROM:-0500
TZOFFSETTO:-0400
END:DAYLIGHT
END:VTIMEZONE
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Single(calendar.TimeZones);

            var vtimezone = calendar.TimeZones[0];
            Assert.Equal("America/New_York", vtimezone.TzId);
            Assert.Single(vtimezone.Standards);
            Assert.Single(vtimezone.Daylights);

            var standard = vtimezone.Standards[0];
            Assert.Equal("20231105T020000", standard.DateTimeStart);
            Assert.Equal("-0400", standard.TzOffsetFrom);
            Assert.Equal("-0500", standard.TzOffsetTo);

            var daylight = vtimezone.Daylights[0];
            Assert.Equal("20240310T020000", daylight.DateTimeStart);
            Assert.Equal("-0500", daylight.TzOffsetFrom);
            Assert.Equal("-0400", daylight.TzOffsetTo);
        }

        [Fact]
        public void Parse_MultipleEvents_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event1@example.com
DTSTAMP:20231201T120000Z
SUMMARY:Event 1
END:VEVENT
BEGIN:VEVENT
UID:event2@example.com
DTSTAMP:20231201T130000Z
SUMMARY:Event 2
END:VEVENT
BEGIN:VEVENT
UID:event3@example.com
DTSTAMP:20231201T140000Z
SUMMARY:Event 3
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            Assert.NotNull(calendar);
            Assert.Equal(3, calendar.Events.Count);
            Assert.Equal("Event 1", calendar.Events[0].Summary);
            Assert.Equal("Event 2", calendar.Events[1].Summary);
            Assert.Equal("Event 3", calendar.Events[2].Summary);
        }

        [Fact]
        public void Parse_EmptyCalendar_ThrowsException()
        {
            var icalData = "";
            var parser = new ICalendarParser();

            Assert.Throws<ParseException>(() => parser.Parse(icalData));
        }

        [Fact]
        public void Parse_MissingBeginVCalendar_ThrowsException()
        {
            var icalData = @"VERSION:2.0
PRODID:-//My Company//My Product//EN
END:VCALENDAR";

            var parser = new ICalendarParser();
            Assert.Throws<ParseException>(() => parser.Parse(icalData));
        }

        [Fact]
        public void Parse_MismatchedEndTag_ThrowsException()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event@example.com
DTSTAMP:20231201T120000Z
END:VTODO
END:VCALENDAR";

            var parser = new ICalendarParser();
            Assert.Throws<ParseException>(() => parser.Parse(icalData));
        }

        [Fact]
        public void Parse_MultipleParameterValues_Success()
        {
            var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:multi-param@example.com
DTSTAMP:20231201T120000Z
ATTENDEE;MEMBER=""mailto:group1@example.com"",""mailto:group2@example.com"":mailto:attendee@example.com
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(icalData);

            var vevent = calendar.Events[0];
            var attendee = vevent.GetProperty("ATTENDEE");
            var members = attendee.GetParameters("MEMBER");
            Assert.Equal(2, members.Count);
            Assert.Contains("mailto:group1@example.com", members);
            Assert.Contains("mailto:group2@example.com", members);
        }
    }

    public class ICalendarValidatorTests
    {
        [Fact]
        public void Validate_ValidCalendar_Success()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_MissingVersion_Error()
        {
            var calendar = new VCalendar
            {
                ProductId = "-//Test//Test//EN"
            };

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("VERSION"));
        }

        [Fact]
        public void Validate_MissingProductId_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0"
            };

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("PRODID"));
        }

        [Fact]
        public void Validate_InvalidVersion_Error()
        {
            var calendar = new VCalendar
            {
                Version = "1.0",
                ProductId = "-//Test//Test//EN"
            };

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("VERSION") && e.Contains("2.0"));
        }

        [Fact]
        public void Validate_EventMissingUid_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent();
            vevent.AddProperty(new CalendarProperty("DTSTAMP", "20231201T120000Z"));
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("UID"));
        }

        [Fact]
        public void Validate_EventMissingDtstamp_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent();
            vevent.AddProperty(new CalendarProperty("UID", "event@example.com"));
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("DTSTAMP"));
        }

        [Fact]
        public void Validate_EventWithBothDtendAndDuration_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z",
                DateTimeEnd = "20231201T130000Z",
                Duration = "PT1H"
            };
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("DTEND") && e.Contains("DURATION"));
        }

        [Fact]
        public void Validate_EventWithInvalidStatus_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z",
                Status = "INVALID-STATUS"
            };
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("STATUS") && e.Contains("INVALID-STATUS"));
        }

        [Fact]
        public void Validate_ValidEventStatuses_Success()
        {
            var validStatuses = new[] { "TENTATIVE", "CONFIRMED", "CANCELLED" };

            foreach (var status in validStatuses)
            {
                var calendar = new VCalendar
                {
                    Version = "2.0",
                    ProductId = "-//Test//Test//EN"
                };

                var vevent = new VEvent
                {
                    Uid = "event@example.com",
                    DateTimeStamp = "20231201T120000Z",
                    Status = status
                };
                calendar.SubComponents.Add(vevent);

                var validator = new ICalendarValidator();
                var result = validator.Validate(calendar);

                Assert.True(result.IsValid, $"Status {status} should be valid");
            }
        }

        [Fact]
        public void Validate_TodoWithInvalidPercentComplete_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vtodo = new VTodo
            {
                Uid = "todo@example.com",
                DateTimeStamp = "20231201T120000Z",
                PercentComplete = "150"
            };
            calendar.SubComponents.Add(vtodo);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("PERCENT-COMPLETE"));
        }

        [Fact]
        public void Validate_InvalidDateTimeFormat_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "2023-12-01 12:00:00"
            };
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("date/time format"));
        }

        [Fact]
        public void Validate_ValidDateTimeFormats_Success()
        {
            var validFormats = new[]
            {
                "20231201T120000Z",
                "20231201T120000",
                "20231201"
            };

            foreach (var format in validFormats)
            {
                var calendar = new VCalendar
                {
                    Version = "2.0",
                    ProductId = "-//Test//Test//EN"
                };

                var vevent = new VEvent
                {
                    Uid = "event@example.com",
                    DateTimeStamp = format
                };
                calendar.SubComponents.Add(vevent);

                var validator = new ICalendarValidator();
                var result = validator.Validate(calendar);

                Assert.True(result.IsValid, $"Format {format} should be valid");
            }
        }

        [Fact]
        public void Validate_AlarmWithoutAction_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z"
            };

            var valarm = new VAlarm();
            valarm.AddProperty(new CalendarProperty("TRIGGER", "-PT15M"));
            vevent.SubComponents.Add(valarm);
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("ACTION"));
        }

        [Fact]
        public void Validate_DisplayAlarmWithoutDescription_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z"
            };

            var valarm = new VAlarm
            {
                Action = "DISPLAY",
                Trigger = "-PT15M"
            };
            vevent.SubComponents.Add(valarm);
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("DESCRIPTION"));
        }

        [Fact]
        public void Validate_TimeZoneWithoutStandardOrDaylight_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vtimezone = new VTimeZone
            {
                TzId = "Custom/Timezone"
            };
            calendar.SubComponents.Add(vtimezone);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("STANDARD") || e.Contains("DAYLIGHT"));
        }

        [Fact]
        public void Validate_InvalidDurationFormat_Error()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z",
                Duration = "1 hour"
            };
            calendar.SubComponents.Add(vevent);

            var validator = new ICalendarValidator();
            var result = validator.Validate(calendar);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("DURATION"));
        }

        [Fact]
        public void Validate_ValidDurationFormats_Success()
        {
            var validDurations = new[]
            {
                "PT1H",
                "PT1H30M",
                "P1D",
                "P1W",
                "PT30M",
                "+PT1H",
                "-PT1H"
            };

            foreach (var duration in validDurations)
            {
                var calendar = new VCalendar
                {
                    Version = "2.0",
                    ProductId = "-//Test//Test//EN"
                };

                var vevent = new VEvent
                {
                    Uid = "event@example.com",
                    DateTimeStamp = "20231201T120000Z",
                    Duration = duration
                };
                calendar.SubComponents.Add(vevent);

                var validator = new ICalendarValidator();
                var result = validator.Validate(calendar);

                Assert.True(result.IsValid, $"Duration {duration} should be valid");
            }
        }
    }

    public class ICalendarDOMTests
    {
        [Fact]
        public void VCalendar_AddProperty_Success()
        {
            var calendar = new VCalendar();
            calendar.AddProperty(new CalendarProperty("VERSION", "2.0"));
            calendar.AddProperty(new CalendarProperty("PRODID", "-//Test//Test//EN"));

            Assert.Equal("2.0", calendar.GetProperty("VERSION").Value);
            Assert.Equal("-//Test//Test//EN", calendar.GetProperty("PRODID").Value);
        }

        [Fact]
        public void VCalendar_Properties_Success()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN",
                CalendarScale = "GREGORIAN",
                Method = "PUBLISH"
            };

            Assert.Equal("2.0", calendar.Version);
            Assert.Equal("-//Test//Test//EN", calendar.ProductId);
            Assert.Equal("GREGORIAN", calendar.CalendarScale);
            Assert.Equal("PUBLISH", calendar.Method);
        }

        [Fact]
        public void VEvent_Properties_Success()
        {
            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z",
                DateTimeStart = "20231225T100000Z",
                DateTimeEnd = "20231225T110000Z",
                Summary = "Test Event",
                Description = "Test Description",
                Location = "Test Location",
                Status = "CONFIRMED",
                Organizer = "mailto:organizer@example.com"
            };

            Assert.Equal("event@example.com", vevent.Uid);
            Assert.Equal("20231201T120000Z", vevent.DateTimeStamp);
            Assert.Equal("20231225T100000Z", vevent.DateTimeStart);
            Assert.Equal("20231225T110000Z", vevent.DateTimeEnd);
            Assert.Equal("Test Event", vevent.Summary);
            Assert.Equal("Test Description", vevent.Description);
            Assert.Equal("Test Location", vevent.Location);
            Assert.Equal("CONFIRMED", vevent.Status);
            Assert.Equal("mailto:organizer@example.com", vevent.Organizer);
        }

        [Fact]
        public void CalendarProperty_Parameters_Success()
        {
            var property = new CalendarProperty("ATTENDEE", "mailto:user@example.com");
            property.AddParameter("CN", "John Doe");
            property.AddParameter("ROLE", "REQ-PARTICIPANT");
            property.AddParameter("RSVP", "TRUE");

            Assert.Equal("John Doe", property.GetParameter("CN"));
            Assert.Equal("REQ-PARTICIPANT", property.GetParameter("ROLE"));
            Assert.Equal("TRUE", property.GetParameter("RSVP"));
        }

        [Fact]
        public void VCalendar_MultipleEventsAndTodos_Success()
        {
            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var event1 = new VEvent { Uid = "event1@example.com" };
            var event2 = new VEvent { Uid = "event2@example.com" };
            var todo1 = new VTodo { Uid = "todo1@example.com" };

            calendar.SubComponents.Add(event1);
            calendar.SubComponents.Add(event2);
            calendar.SubComponents.Add(todo1);

            Assert.Equal(2, calendar.Events.Count);
            Assert.Single(calendar.Todos);
            Assert.Equal("event1@example.com", calendar.Events[0].Uid);
            Assert.Equal("event2@example.com", calendar.Events[1].Uid);
            Assert.Equal("todo1@example.com", calendar.Todos[0].Uid);
        }

        [Fact]
        public void VEvent_WithAlarms_Success()
        {
            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z"
            };

            var alarm1 = new VAlarm
            {
                Action = "DISPLAY",
                Trigger = "-PT15M",
                Description = "Reminder 1"
            };

            var alarm2 = new VAlarm
            {
                Action = "AUDIO",
                Trigger = "-PT5M"
            };

            vevent.SubComponents.Add(alarm1);
            vevent.SubComponents.Add(alarm2);

            Assert.Equal(2, vevent.Alarms.Count);
            Assert.Equal("DISPLAY", vevent.Alarms[0].Action);
            Assert.Equal("AUDIO", vevent.Alarms[1].Action);
        }

        [Fact]
        public void GetProperties_MultipleWithSameName_Success()
        {
            var vevent = new VEvent();
            vevent.AddProperty(new CalendarProperty("ATTENDEE", "mailto:user1@example.com"));
            vevent.AddProperty(new CalendarProperty("ATTENDEE", "mailto:user2@example.com"));
            vevent.AddProperty(new CalendarProperty("ATTENDEE", "mailto:user3@example.com"));

            var attendees = vevent.GetProperties("ATTENDEE");
            Assert.Equal(3, attendees.Count);
            Assert.Contains(attendees, a => a.Value == "mailto:user1@example.com");
            Assert.Contains(attendees, a => a.Value == "mailto:user2@example.com");
            Assert.Contains(attendees, a => a.Value == "mailto:user3@example.com");
        }
    }
}
