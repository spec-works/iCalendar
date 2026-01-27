# Specworks.iCalendar Documentation

Parse, validate, and serialize iCalendar data according to [RFC 5545](https://www.rfc-editor.org/rfc/rfc5545).

[![NuGet](https://img.shields.io/nuget/v/Specworks.iCalendar.svg)](https://www.nuget.org/packages/Specworks.iCalendar/)

## What is Specworks.iCalendar?

Specworks.iCalendar is a .NET library that provides comprehensive support for parsing, validating, and serializing iCalendar data. It implements the Internet Calendaring and Scheduling Core Object Specification (iCalendar) as defined in RFC 5545.

## Installation

Install via NuGet:

```bash
dotnet add package Specworks.iCalendar
```

## Features

- ✅ **RFC 5545 Compliant** - Full implementation of the iCalendar specification
- ✅ **Parse iCalendar** - Parse .ics files and iCalendar strings
- ✅ **Validate Data** - Comprehensive validation of iCalendar components and properties
- ✅ **Serialize to iCalendar** - Generate valid .ics files
- ✅ **DOM Support** - Complete Document Object Model for iCalendar data
- ✅ **Type-Safe API** - Strong typing with nullable reference types
- ✅ **Multi-Target** - Supports .NET 10.0+

## Quick Start

### Parsing iCalendar

```csharp
using Specworks.ICalendar;

// Parse from string
var calendar = ICalendarParser.Parse(icalendarString);

// Parse from file
var calendar = ICalendarParser.ParseFile("calendar.ics");

// Access events
foreach (var vevent in calendar.Events)
{
    Console.WriteLine($"Event: {vevent.Summary}");
    Console.WriteLine($"Start: {vevent.DtStart}");
    Console.WriteLine($"End: {vevent.DtEnd}");
}
```

### Creating iCalendar

```csharp
using Specworks.ICalendar;

// Create a calendar
var calendar = new VCalendar
{
    ProductId = "-//My Company//My Product//EN",
    Version = "2.0"
};

// Create an event
var vevent = new VEvent
{
    Uid = Guid.NewGuid().ToString(),
    DtStart = DateTime.UtcNow,
    DtEnd = DateTime.UtcNow.AddHours(1),
    Summary = "Team Meeting",
    Description = "Discuss project status"
};

calendar.Events.Add(vevent);

// Serialize to string
string ics = ICalendarSerializer.Serialize(calendar);

// Save to file
File.WriteAllText("calendar.ics", ics);
```

### Validating iCalendar

```csharp
using Specworks.ICalendar;

var calendar = ICalendarParser.Parse(icalendarString);

// Validate
var validationResults = ICalendarValidator.Validate(calendar);

if (validationResults.IsValid)
{
    Console.WriteLine("Calendar is valid!");
}
else
{
    foreach (var error in validationResults.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

## Use Cases

### Event Management Systems

Build calendar applications:

```csharp
// Import events from .ics file
var calendar = ICalendarParser.ParseFile("events.ics");

foreach (var vevent in calendar.Events)
{
    // Process each event
    AddEventToDatabase(vevent);
}
```

### Calendar Integration

Integrate with calendar services:

```csharp
// Generate calendar invite
var invite = new VCalendar();
var meeting = new VEvent
{
    Uid = Guid.NewGuid().ToString(),
    DtStart = meetingTime,
    DtEnd = meetingTime.AddHours(1),
    Summary = "Project Review",
    Organizer = "mailto:organizer@example.com",
    Attendees = new[]
    {
        "mailto:attendee1@example.com",
        "mailto:attendee2@example.com"
    }
};

invite.Events.Add(meeting);

// Send as email attachment
SendCalendarInvite(ICalendarSerializer.Serialize(invite));
```

### Scheduling Applications

Build scheduling systems:

```csharp
// Parse recurring events
var calendar = ICalendarParser.Parse(icalendarString);

foreach (var vevent in calendar.Events)
{
    if (vevent.RRule != null)
    {
        // Handle recurring event
        var occurrences = CalculateOccurrences(vevent);
    }
}
```

## API Reference

- [API Documentation](api/ICalendar.html) - Complete API reference

## Specification Compliance

This library implements [RFC 5545 - Internet Calendaring and Scheduling Core Object Specification (iCalendar)](https://www.rfc-editor.org/rfc/rfc5545).

### Supported Components

| Component | RFC Section | Status |
|-----------|-------------|--------|
| VCALENDAR | 3.4 | ✅ Supported |
| VEVENT | 3.6.1 | ✅ Supported |
| VTODO | 3.6.2 | ✅ Supported |
| VJOURNAL | 3.6.3 | ✅ Supported |
| VFREEBUSY | 3.6.4 | ✅ Supported |
| VTIMEZONE | 3.6.5 | ✅ Supported |
| VALARM | 3.6.6 | ✅ Supported |

### Supported Properties

The library supports all standard iCalendar properties including:

- Date/Time properties (DTSTART, DTEND, DUE, etc.)
- Descriptive properties (SUMMARY, DESCRIPTION, LOCATION, etc.)
- Relationship properties (ORGANIZER, ATTENDEE, UID, etc.)
- Recurrence properties (RRULE, RDATE, EXDATE, etc.)
- Alarm properties (ACTION, TRIGGER, DURATION, etc.)

## Requirements

- .NET 10.0 or later
- C# 10.0 or later

## Source Code

View the source code on [GitHub](https://github.com/spec-works/iCalendar).

## Contributing

Contributions welcome! See the [repository](https://github.com/spec-works/iCalendar) for:
- Issue tracking
- Pull request guidelines
- Architecture Decision Records (ADRs)

## License

MIT License - see [LICENSE](https://github.com/spec-works/iCalendar/blob/main/LICENSE) for details.

## Links

- **NuGet Package**: [nuget.org/packages/Specworks.iCalendar](https://www.nuget.org/packages/Specworks.iCalendar/)
- **GitHub Repository**: [github.com/spec-works/iCalendar](https://github.com/spec-works/iCalendar)
- **RFC 5545 Specification**: [rfc-editor.org/rfc/rfc5545](https://www.rfc-editor.org/rfc/rfc5545)
- **SpecWorks Factory**: [spec-works.github.io](https://spec-works.github.io)
