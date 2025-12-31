using System;
using System.IO;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace ICalendar.Tests
{
    /// <summary>
    /// Tests for reading and writing various types of iCalendar payloads
    /// </summary>
    public class PayloadTests
    {
        [Fact]
        public void ReadWrite_GoogleCalendarEvent_Succeeds()
        {
            var googleCalendarPayload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Google Inc//Google Calendar 70.9054//EN
CALSCALE:GREGORIAN
METHOD:PUBLISH
BEGIN:VEVENT
UID:google-event-001@google.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
DTEND:20231225T110000Z
SUMMARY:Christmas Team Lunch
DESCRIPTION:Annual holiday celebration lunch with the team
LOCATION:Restaurant XYZ\, 123 Main St
STATUS:CONFIRMED
SEQUENCE:0
CREATED:20231120T100000Z
LAST-MODIFIED:20231201T110000Z
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(googleCalendarPayload);

            calendar.Version.Should().Be("2.0");
            calendar.ProductId.Should().Contain("Google");
            calendar.Events.Should().HaveCount(1);

            var vevent = calendar.Events[0];
            vevent.Summary.Should().Be("Christmas Team Lunch");
            vevent.Location.Should().Be("Restaurant XYZ, 123 Main St");

            var serializer = new ICalendarSerializer();
            var output = serializer.Serialize(calendar);
            output.Should().Contain("BEGIN:VEVENT");
            output.Should().Contain("Christmas Team Lunch");
        }

        [Fact]
        public void ReadWrite_OutlookRecurringEvent_Succeeds()
        {
            var outlookPayload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Microsoft Corporation//Outlook 16.0 MIMEDIR//EN
METHOD:PUBLISH
BEGIN:VEVENT
UID:outlook-recurring-001@outlook.com
DTSTAMP:20231201T120000Z
DTSTART:20231204T140000Z
DTEND:20231204T150000Z
SUMMARY:Weekly Team Standup
LOCATION:Conference Room B
RRULE:FREQ=WEEKLY;BYDAY=MO;UNTIL=20240331T235959Z
TRANSP:OPAQUE
SEQUENCE:1
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(outlookPayload);

            calendar.Events.Should().HaveCount(1);
            var vevent = calendar.Events[0];
            vevent.Summary.Should().Be("Weekly Team Standup");

            var rrule = vevent.GetProperty("RRULE");
            rrule.Should().NotBeNull();
            rrule.Value.Should().Contain("FREQ=WEEKLY");

            var serializer = new ICalendarSerializer();
            var output = serializer.Serialize(calendar);

            var reparsed = parser.Parse(output);
            reparsed.Events[0].GetProperty("RRULE").Value.Should().Contain("FREQ=WEEKLY");
        }

        [Fact]
        public void ReadWrite_AppleCalendarAllDayEvent_Succeeds()
        {
            var applePayload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Apple Inc.//macOS 14.0//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
UID:apple-allday-001@icloud.com
DTSTAMP:20231201T120000Z
DTSTART;VALUE=DATE:20231225
DTEND;VALUE=DATE:20231226
SUMMARY:Christmas Day
TRANSP:TRANSPARENT
SEQUENCE:0
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(applePayload);

            var vevent = calendar.Events[0];
            vevent.Summary.Should().Be("Christmas Day");

            var dtstart = vevent.GetProperty("DTSTART");
            dtstart.GetParameter("VALUE").Should().Be("DATE");
            dtstart.Value.Should().Be("20231225");

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Events[0].GetProperty("DTSTART").Value.Should().Be("20231225");
        }

        [Fact]
        public void ReadWrite_EventWithMultipleAlarms_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
BEGIN:VEVENT
UID:multi-alarm@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231215T140000Z
SUMMARY:Important Meeting
BEGIN:VALARM
ACTION:DISPLAY
TRIGGER;VALUE=DURATION:-PT1H
DESCRIPTION:Meeting in 1 hour
END:VALARM
BEGIN:VALARM
ACTION:DISPLAY
TRIGGER;VALUE=DURATION:-PT15M
DESCRIPTION:Meeting in 15 minutes
END:VALARM
BEGIN:VALARM
ACTION:EMAIL
TRIGGER;VALUE=DURATION:-P1D
SUMMARY:Meeting Tomorrow
DESCRIPTION:Don't forget the meeting tomorrow
ATTENDEE:mailto:user@example.com
END:VALARM
BEGIN:VALARM
ACTION:AUDIO
TRIGGER;VALUE=DURATION:-PT5M
ATTACH;FMTTYPE=audio/wav:http://example.com/alert.wav
END:VALARM
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            calendar.Events[0].Alarms.Should().HaveCount(4);
            calendar.Events[0].Alarms.Count(a => a.Action == "DISPLAY").Should().Be(2);
            calendar.Events[0].Alarms.Count(a => a.Action == "EMAIL").Should().Be(1);
            calendar.Events[0].Alarms.Count(a => a.Action == "AUDIO").Should().Be(1);

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Events[0].Alarms.Should().HaveCount(4);
        }

        [Fact]
        public void ReadWrite_TodoWithPriority_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
BEGIN:VTODO
UID:todo-priority@example.com
DTSTAMP:20231201T120000Z
SUMMARY:Critical Bug Fix
PRIORITY:1
STATUS:IN-PROCESS
PERCENT-COMPLETE:75
DUE:20231205T170000Z
CATEGORIES:DEVELOPMENT,BUG,URGENT
END:VTODO
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            var vtodo = calendar.Todos[0];
            vtodo.Summary.Should().Be("Critical Bug Fix");
            vtodo.GetProperty("PRIORITY").Value.Should().Be("1");
            vtodo.GetProperty("CATEGORIES").Value.Should().Be("DEVELOPMENT,BUG,URGENT");

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Todos[0].GetProperty("PRIORITY").Value.Should().Be("1");
        }

        [Fact]
        public void ReadWrite_ComplexTimeZoneDefinition_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
BEGIN:VTIMEZONE
TZID:America/Los_Angeles
TZURL:http://tzurl.org/zoneinfo/America/Los_Angeles
X-LIC-LOCATION:America/Los_Angeles
BEGIN:DAYLIGHT
TZOFFSETFROM:-0800
TZOFFSETTO:-0700
TZNAME:PDT
DTSTART:19700308T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:-0700
TZOFFSETTO:-0800
TZNAME:PST
DTSTART:19701101T020000
RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
UID:tz-event@example.com
DTSTAMP:20231201T120000Z
DTSTART;TZID=America/Los_Angeles:20231215T090000
DTEND;TZID=America/Los_Angeles:20231215T100000
SUMMARY:Pacific Time Meeting
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            calendar.TimeZones.Should().HaveCount(1);
            var tz = calendar.TimeZones[0];
            tz.TzId.Should().Be("America/Los_Angeles");
            tz.Daylights.Should().HaveCount(1);
            tz.Standards.Should().HaveCount(1);

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.TimeZones[0].TzId.Should().Be("America/Los_Angeles");
        }

        [Fact]
        public void ReadWrite_FreeBusyComponent_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
METHOD:REPLY
BEGIN:VFREEBUSY
UID:freebusy@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231210T000000Z
DTEND:20231215T235959Z
ORGANIZER:mailto:organizer@example.com
ATTENDEE:mailto:attendee@example.com
FREEBUSY;FBTYPE=BUSY:20231210T090000Z/20231210T100000Z
FREEBUSY;FBTYPE=BUSY:20231210T140000Z/20231210T160000Z
FREEBUSY;FBTYPE=BUSY-TENTATIVE:20231211T100000Z/20231211T120000Z
FREEBUSY;FBTYPE=BUSY-UNAVAILABLE:20231212T000000Z/20231213T000000Z
END:VFREEBUSY
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            calendar.FreeBusies.Should().HaveCount(1);
            var fb = calendar.FreeBusies[0];
            fb.DateTimeStart.Should().Be("20231210T000000Z");
            fb.DateTimeEnd.Should().Be("20231215T235959Z");

            var freebusyProps = fb.GetProperties("FREEBUSY");
            freebusyProps.Should().HaveCount(4);

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.FreeBusies[0].GetProperties("FREEBUSY").Should().HaveCount(4);
        }

        [Fact]
        public void ReadWrite_EventWithAttachments_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
BEGIN:VEVENT
UID:attach-event@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231215T140000Z
SUMMARY:Project Review
ATTACH;FMTTYPE=application/pdf:http://example.com/docs/report.pdf
ATTACH;FMTTYPE=application/vnd.ms-powerpoint:http://example.com/docs/slides
 .ppt
ATTACH;ENCODING=BASE64;VALUE=BINARY:VGhpcyBpcyBhIHRlc3Q=
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            var attachments = calendar.Events[0].GetProperties("ATTACH");
            attachments.Should().HaveCount(3);

            var pdfAttach = attachments[0];
            pdfAttach.GetParameter("FMTTYPE").Should().Be("application/pdf");
            pdfAttach.Value.Should().Contain("report.pdf");

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Events[0].GetProperties("ATTACH").Should().HaveCount(3);
        }

        [Fact]
        public void ReadWrite_MultilingualEvent_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
BEGIN:VEVENT
UID:multilingual@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
SUMMARY:Christmas Party 🎄
SUMMARY;LANGUAGE=es:Fiesta de Navidad
SUMMARY;LANGUAGE=fr:Fête de Noël
SUMMARY;LANGUAGE=de:Weihnachtsfeier
DESCRIPTION:Holiday celebration\nCelebración navideña\nCélébration des fêt
 es\nFeiertagsfeier
LOCATION:Main Office
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            var summaries = calendar.Events[0].GetProperties("SUMMARY");
            summaries.Should().HaveCount(4);

            var spanishSummary = summaries.FirstOrDefault(s => s.GetParameter("LANGUAGE") == "es");
            spanishSummary.Should().NotBeNull();
            spanishSummary.Value.Should().Be("Fiesta de Navidad");

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Events[0].GetProperties("SUMMARY").Should().HaveCount(4);
        }

        [Fact]
        public void ReadWrite_CalendarWithMethod_Succeeds()
        {
            var payload = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test//Test//EN
METHOD:REQUEST
BEGIN:VEVENT
UID:meeting-request@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231215T140000Z
DTEND:20231215T150000Z
SUMMARY:Meeting Request
ORGANIZER;CN=""Meeting Organizer"":mailto:organizer@example.com
ATTENDEE;CN=""John Doe"";RSVP=TRUE;PARTSTAT=NEEDS-ACTION:mailto:john@exampl
 e.com
ATTENDEE;CN=""Jane Smith"";RSVP=TRUE;PARTSTAT=NEEDS-ACTION:mailto:jane@exa
 mple.com
STATUS:TENTATIVE
REQUEST-STATUS:2.0;Success
END:VEVENT
END:VCALENDAR";

            var parser = new ICalendarParser();
            var calendar = parser.Parse(payload);

            calendar.Method.Should().Be("REQUEST");
            calendar.Events[0].Status.Should().Be("TENTATIVE");

            var attendees = calendar.Events[0].GetProperties("ATTENDEE");
            attendees.Should().HaveCount(2);
            attendees.All(a => a.GetParameter("RSVP") == "TRUE").Should().BeTrue();

            var serialized = calendar.ToICalendar();
            var reparsed = parser.Parse(serialized);
            reparsed.Method.Should().Be("REQUEST");
        }

        [Fact]
        public void ReadWrite_FileRoundTrip_Succeeds()
        {
            var tempFile = Path.GetTempFileName();
            tempFile = Path.ChangeExtension(tempFile, ".ics");

            try
            {
                // Create calendar
                var original = new VCalendar
                {
                    Version = "2.0",
                    ProductId = "-//Test//File Test//EN"
                };

                var vevent = new VEvent
                {
                    Uid = "file-test@example.com",
                    DateTimeStamp = "20231201T120000Z",
                    Summary = "File Test Event"
                };
                original.SubComponents.Add(vevent);

                // Write to file
                original.SaveToFile(tempFile);
                File.Exists(tempFile).Should().BeTrue();

                // Read from file
                var parser = new ICalendarParser();
                var loaded = parser.ParseFile(tempFile);

                loaded.Events.Should().HaveCount(1);
                loaded.Events[0].Summary.Should().Be("File Test Event");
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void ReadWrite_VeryLongDescription_FoldsCorrectly()
        {
            var longText = string.Join(" ", Enumerable.Repeat("This is a very long description that will need to be folded across multiple lines according to RFC 5545 specification.", 5));

            var calendar = new VCalendar
            {
                Version = "2.0",
                ProductId = "-//Test//Test//EN"
            };

            var vevent = new VEvent
            {
                Uid = "long-desc@example.com",
                DateTimeStamp = "20231201T120000Z",
                Description = longText
            };
            calendar.SubComponents.Add(vevent);

            var serialized = calendar.ToICalendar();
            var lines = serialized.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Verify folding
            var descriptionLines = lines.Where(l => l.StartsWith("DESCRIPTION:") || (l.StartsWith(" ") && lines[Array.IndexOf(lines.ToArray(), l) - 1].Contains("DESCRIPTION"))).ToList();
            descriptionLines.Should().NotBeEmpty();

            // Parse back
            var parser = new ICalendarParser();
            var reparsed = parser.Parse(serialized);
            reparsed.Events[0].Description.Should().Be(longText);
        }
    }
}
