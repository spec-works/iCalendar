using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace ICalendar.Tests
{
    /// <summary>
    /// Tests that verify reading and writing iCalendar data produces consistent results
    /// </summary>
    public class RoundTripTests
    {
        [Fact]
        public void RoundTrip_SimpleCalendar_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            // Act
            var serializer = new ICalendarSerializer();
            var icalText = serializer.Serialize(original);

            var parser = new ICalendarParser();
            var parsed = parser.Parse(icalText);

            // Assert
            parsed.Version.Should().Be(original.Version);
            parsed.ProductId.Should().Be(original.ProductId);
        }

        [Fact]
        public void RoundTrip_CalendarWithEvent_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event123@example.com",
                DateTimeStamp = "20231201T120000Z",
                DateTimeStart = "20231225T100000Z",
                DateTimeEnd = "20231225T110000Z",
                Summary = "Test Event",
                Description = "This is a test event",
                Location = "Test Location",
                Status = "CONFIRMED"
            };

            original.SubComponents.Add(vevent);

            // Act
            var serializer = new ICalendarSerializer();
            var icalText = serializer.Serialize(original);

            var parser = new ICalendarParser();
            var parsed = parser.Parse(icalText);

            // Assert
            parsed.Events.Should().HaveCount(1);
            var parsedEvent = parsed.Events[0];
            parsedEvent.Uid.Should().Be(vevent.Uid);
            parsedEvent.DateTimeStamp.Should().Be(vevent.DateTimeStamp);
            parsedEvent.DateTimeStart.Should().Be(vevent.DateTimeStart);
            parsedEvent.DateTimeEnd.Should().Be(vevent.DateTimeEnd);
            parsedEvent.Summary.Should().Be(vevent.Summary);
            parsedEvent.Description.Should().Be(vevent.Description);
            parsedEvent.Location.Should().Be(vevent.Location);
            parsedEvent.Status.Should().Be(vevent.Status);
        }

        [Fact]
        public void RoundTrip_EventWithDuration_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "duration-event@example.com",
                DateTimeStamp = "20231201T120000Z",
                DateTimeStart = "20231225T100000Z",
                Duration = "PT2H30M",
                Summary = "Event with Duration"
            };

            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var parser = new ICalendarParser();
            var parsed = parser.Parse(icalText);

            // Assert
            parsed.Events[0].Duration.Should().Be("PT2H30M");
            parsed.Events[0].DateTimeEnd.Should().BeNull();
        }

        [Fact]
        public void RoundTrip_EventWithAlarms_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "alarm-event@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Event with Alarms"
            };

            var alarm1 = new VAlarm
            {
                Action = "DISPLAY",
                Trigger = "-PT15M",
                Description = "First reminder"
            };

            var alarm2 = new VAlarm
            {
                Action = "AUDIO",
                Trigger = "-PT5M"
            };

            vevent.SubComponents.Add(alarm1);
            vevent.SubComponents.Add(alarm2);
            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.Events[0].Alarms.Should().HaveCount(2);
            parsed.Events[0].Alarms[0].Action.Should().Be("DISPLAY");
            parsed.Events[0].Alarms[0].Trigger.Should().Be("-PT15M");
            parsed.Events[0].Alarms[0].Description.Should().Be("First reminder");
            parsed.Events[0].Alarms[1].Action.Should().Be("AUDIO");
            parsed.Events[0].Alarms[1].Trigger.Should().Be("-PT5M");
        }

        [Fact]
        public void RoundTrip_Todo_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vtodo = new VTodo
            {
                Uid = "todo123@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Complete project",
                Status = "IN-PROCESS",
                PercentComplete = "50",
                Due = "20231231T235959Z"
            };

            original.SubComponents.Add(vtodo);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.Todos.Should().HaveCount(1);
            var parsedTodo = parsed.Todos[0];
            parsedTodo.Uid.Should().Be(vtodo.Uid);
            parsedTodo.Summary.Should().Be(vtodo.Summary);
            parsedTodo.Status.Should().Be(vtodo.Status);
            parsedTodo.PercentComplete.Should().Be(vtodo.PercentComplete);
            parsedTodo.Due.Should().Be(vtodo.Due);
        }

        [Fact]
        public void RoundTrip_Journal_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vjournal = new VJournal
            {
                Uid = "journal123@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Daily Notes",
                Description = "Today was productive"
            };

            original.SubComponents.Add(vjournal);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.Journals.Should().HaveCount(1);
            parsed.Journals[0].Uid.Should().Be(vjournal.Uid);
            parsed.Journals[0].Summary.Should().Be(vjournal.Summary);
            parsed.Journals[0].Description.Should().Be(vjournal.Description);
        }

        [Fact]
        public void RoundTrip_TimeZone_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vtimezone = new VTimeZone
            {
                TzId = "America/New_York"
            };

            var standard = new VTimeZoneStandard
            {
                DateTimeStart = "20231105T020000",
                TzOffsetFrom = "-0400",
                TzOffsetTo = "-0500"
            };

            var daylight = new VTimeZoneDaylight
            {
                DateTimeStart = "20240310T020000",
                TzOffsetFrom = "-0500",
                TzOffsetTo = "-0400"
            };

            vtimezone.SubComponents.Add(standard);
            vtimezone.SubComponents.Add(daylight);
            original.SubComponents.Add(vtimezone);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.TimeZones.Should().HaveCount(1);
            var parsedTz = parsed.TimeZones[0];
            parsedTz.TzId.Should().Be("America/New_York");
            parsedTz.Standards.Should().HaveCount(1);
            parsedTz.Daylights.Should().HaveCount(1);
            parsedTz.Standards[0].TzOffsetFrom.Should().Be("-0400");
            parsedTz.Standards[0].TzOffsetTo.Should().Be("-0500");
        }

        [Fact]
        public void RoundTrip_PropertyWithParameters_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "params-event@example.com",
                DateTimeStamp = "20231201T120000Z"
            };

            var dtstart = new CalendarProperty("DTSTART", "20231225T100000");
            dtstart.AddParameter("TZID", "America/New_York");
            vevent.AddProperty(dtstart);

            var attendee = new CalendarProperty("ATTENDEE", "mailto:john@example.com");
            attendee.AddParameter("CN", "John Doe");
            attendee.AddParameter("ROLE", "REQ-PARTICIPANT");
            attendee.AddParameter("RSVP", "TRUE");
            vevent.AddProperty(attendee);

            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            var parsedEvent = parsed.Events[0];
            var parsedDtstart = parsedEvent.GetProperty("DTSTART");
            parsedDtstart.GetParameter("TZID").Should().Be("America/New_York");

            var parsedAttendee = parsedEvent.GetProperty("ATTENDEE");
            parsedAttendee.GetParameter("CN").Should().Be("John Doe");
            parsedAttendee.GetParameter("ROLE").Should().Be("REQ-PARTICIPANT");
            parsedAttendee.GetParameter("RSVP").Should().Be("TRUE");
        }

        [Fact]
        public void RoundTrip_EscapedCharacters_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "escaped@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Meeting, Planning; Review",
                Description = "Line 1\nLine 2\nLine 3",
                Location = "Room\\Building A"
            };

            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            var parsedEvent = parsed.Events[0];
            parsedEvent.Summary.Should().Be("Meeting, Planning; Review");
            parsedEvent.Description.Should().Contain("Line 1\nLine 2\nLine 3");
            parsedEvent.Location.Should().Be("Room\\Building A");
        }

        [Fact]
        public void RoundTrip_MultipleEvents_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            for (int i = 1; i <= 5; i++)
            {
                var vevent = new VEvent
                {
                    Uid = $"event{i}@example.com",
                    DateTimeStamp = "20231201T120000Z",
                    Summary = $"Event {i}"
                };
                original.SubComponents.Add(vevent);
            }

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.Events.Should().HaveCount(5);
            for (int i = 0; i < 5; i++)
            {
                parsed.Events[i].Summary.Should().Be($"Event {i + 1}");
            }
        }

        [Fact]
        public void RoundTrip_MixedComponents_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "event@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Meeting"
            };

            var vtodo = new VTodo
            {
                Uid = "todo@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Task"
            };

            var vjournal = new VJournal
            {
                Uid = "journal@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Notes"
            };

            original.SubComponents.Add(vevent);
            original.SubComponents.Add(vtodo);
            original.SubComponents.Add(vjournal);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            parsed.Events.Should().HaveCount(1);
            parsed.Todos.Should().HaveCount(1);
            parsed.Journals.Should().HaveCount(1);
        }

        [Fact]
        public void RoundTrip_LongLines_AreFoldedCorrectly()
        {
            // Arrange
            var longDescription = new string('A', 200);
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "long@example.com",
                DateTimeStamp = "20231201T120000Z",
                Description = longDescription
            };

            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var lines = icalText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Assert - lines should be folded (no line longer than 75 chars except possibly the last)
            foreach (var line in lines.Take(lines.Length - 1))
            {
                line.Length.Should().BeLessOrEqualTo(75);
            }

            // Parse back and verify
            var parsed = new ICalendarParser().Parse(icalText);
            parsed.Events[0].Description.Should().Be(longDescription);
        }

        [Fact]
        public void RoundTrip_MultipleAttendees_Succeeds()
        {
            // Arrange
            var original = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "meeting@example.com",
                DateTimeStamp = "20231201T120000Z",
                Summary = "Team Meeting"
            };

            var attendee1 = new CalendarProperty("ATTENDEE", "mailto:john@example.com");
            attendee1.AddParameter("CN", "John Doe");
            vevent.AddProperty(attendee1);

            var attendee2 = new CalendarProperty("ATTENDEE", "mailto:jane@example.com");
            attendee2.AddParameter("CN", "Jane Smith");
            vevent.AddProperty(attendee2);

            var attendee3 = new CalendarProperty("ATTENDEE", "mailto:bob@example.com");
            attendee3.AddParameter("CN", "Bob Wilson");
            vevent.AddProperty(attendee3);

            original.SubComponents.Add(vevent);

            // Act
            var icalText = original.ToICalendar();
            var parsed = new ICalendarParser().Parse(icalText);

            // Assert
            var attendees = parsed.Events[0].GetProperties("ATTENDEE");
            attendees.Should().HaveCount(3);
            attendees[0].GetParameter("CN").Should().Be("John Doe");
            attendees[1].GetParameter("CN").Should().Be("Jane Smith");
            attendees[2].GetParameter("CN").Should().Be("Bob Wilson");
        }
    }
}
