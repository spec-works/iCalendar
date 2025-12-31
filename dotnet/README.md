# ICalendar.Net

A complete .NET library for parsing, validating, and serializing iCalendar (text/calendar) data according to RFC 5545.

[![NuGet](https://img.shields.io/nuget/v/ICalendar.Net.svg)](https://www.nuget.org/packages/ICalendar.Net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Complete DOM**: Object-oriented representation of all iCalendar components
  - VCALENDAR (root calendar)
  - VEVENT (events)
  - VTODO (to-do items)
  - VJOURNAL (journal entries)
  - VFREEBUSY (free/busy time)
  - VTIMEZONE (time zone definitions)
  - VALARM (alarms)

- **RFC 5545 Compliant Parser**
  - Line unfolding support
  - Property parameter parsing
  - Escape sequence handling
  - Nested component support

- **iCalendar Serializer**
  - Write iCalendar format
  - Automatic line folding
  - Property escaping
  - Extension methods for easy serialization

- **Comprehensive Validator**
  - Required property validation
  - Property format validation (DATE-TIME, DURATION, UTC-OFFSET)
  - Component-specific rules
  - STATUS value validation
  - Mutual exclusivity checks (DTEND vs DURATION)

- **Extensive Test Coverage**
  - 60+ unit tests
  - Parser tests
  - Serializer tests
  - Validator tests
  - Round-trip tests
  - Real-world payload tests

## Installation

### Via NuGet (when published)

```bash
dotnet add package ICalendar.Net
```

### Building from Source

```bash
git clone https://github.com/yourusername/icalendar-net.git
cd icalendar-net
dotnet build
```

Run the tests:

```bash
dotnet test
```

## Usage

### Parsing an iCalendar String

```csharp
using ICalendar;

var icalData = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//My Company//My Product//EN
BEGIN:VEVENT
UID:event@example.com
DTSTAMP:20231201T120000Z
DTSTART:20231225T100000Z
SUMMARY:Christmas Meeting
END:VEVENT
END:VCALENDAR";

var parser = new ICalendarParser();
var calendar = parser.Parse(icalData);

Console.WriteLine($"Calendar has {calendar.Events.Count} event(s)");
Console.WriteLine($"First event: {calendar.Events[0].Summary}");
```

### Parsing from a File

```csharp
var parser = new ICalendarParser();
var calendar = parser.ParseFile("calendar.ics");
```

### Creating a Calendar Programmatically

```csharp
var calendar = new VCalendar
{
    Version = "2.0",
    ProductId = "-//My App//Calendar//EN"
};

var vevent = new VEvent
{
    Uid = "event-123@example.com",
    DateTimeStamp = "20231201T120000Z",
    DateTimeStart = "20231225T100000Z",
    DateTimeEnd = "20231225T110000Z",
    Summary = "Team Meeting",
    Location = "Conference Room"
};

calendar.SubComponents.Add(vevent);
```

### Serializing to iCalendar Format

```csharp
// Using the serializer
var serializer = new ICalendarSerializer();
string icalText = serializer.Serialize(calendar);

// Or using extension method
string icalText = calendar.ToICalendar();

// Save to file
calendar.SaveToFile("meeting.ics");

// Or using serializer
serializer.SerializeToFile(calendar, "meeting.ics");
```

### Validating a Calendar

```csharp
var validator = new ICalendarValidator();
var result = validator.Validate(calendar);

if (result.IsValid)
{
    Console.WriteLine("Calendar is valid!");
}
else
{
    Console.WriteLine("Validation errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}

// Get detailed summary
Console.WriteLine(result.GetSummary());
```

### Working with Properties and Parameters

```csharp
// Access properties
var dtstart = vevent.GetProperty("DTSTART");
Console.WriteLine($"Start time: {dtstart.Value}");

// Access property parameters
var tzid = dtstart.GetParameter("TZID");
if (tzid != null)
{
    Console.WriteLine($"Time zone: {tzid}");
}

// Get multiple properties with the same name
var attendees = vevent.GetProperties("ATTENDEE");
foreach (var attendee in attendees)
{
    var cn = attendee.GetParameter("CN");
    Console.WriteLine($"Attendee: {cn} - {attendee.Value}");
}
```

### Working with Alarms

```csharp
var alarm = new VAlarm
{
    Action = "DISPLAY",
    Trigger = "-PT15M",
    Description = "Meeting reminder"
};

vevent.SubComponents.Add(alarm);

// Access alarms
foreach (var alarm in vevent.Alarms)
{
    Console.WriteLine($"Alarm: {alarm.Action} at {alarm.Trigger}");
}
```

## DOM Structure

### Component Hierarchy

```
CalendarComponent (abstract base)
├── VCalendar (root)
│   ├── VEvent
│   │   └── VAlarm
│   ├── VTodo
│   │   └── VAlarm
│   ├── VJournal
│   ├── VFreeBusy
│   └── VTimeZone
│       ├── VTimeZoneStandard
│       └── VTimeZoneDaylight
```

### Key Classes

- **CalendarComponent**: Base class for all components with properties and sub-components
- **CalendarProperty**: Represents a property with name, value, and parameters
- **VCalendar**: Root calendar object
- **VEvent**: Event component
- **VTodo**: To-do item component
- **VJournal**: Journal entry component
- **VAlarm**: Alarm/reminder component

## Validation Rules

The validator checks for:

### VCALENDAR
- Required: VERSION, PRODID
- VERSION must be "2.0"
- CALSCALE warning if not "GREGORIAN"

### VEVENT
- Required: UID, DTSTAMP
- UID must not be empty
- DTEND and DURATION are mutually exclusive
- STATUS must be: TENTATIVE, CONFIRMED, or CANCELLED
- Date/time format validation

### VTODO
- Required: UID, DTSTAMP
- STATUS must be: NEEDS-ACTION, COMPLETED, IN-PROCESS, or CANCELLED
- PERCENT-COMPLETE must be 0-100

### VALARM
- Required: ACTION, TRIGGER
- DISPLAY/EMAIL actions require DESCRIPTION
- EMAIL action requires SUMMARY

### Format Validation
- DATE-TIME: YYYYMMDD or YYYYMMDDTHHMMSS[Z]
- DURATION: ISO 8601 duration (e.g., PT1H30M, P1D)
- UTC-OFFSET: +/-HHMM or +/-HHMMSS

## RFC 5545 Compliance

This implementation follows RFC 5545 (Internet Calendaring and Scheduling Core Object Specification):

- Line folding/unfolding (3.1)
- Content lines (3.1)
- Property parameters (3.2)
- Property value data types (3.3)
- iCalendar object (3.4)
- Component properties (3.7, 3.8)
- Escape sequences for special characters

## Examples

See `Example.cs` for comprehensive usage examples including:

1. Parsing simple calendars
2. Parsing and validating events
3. Creating calendars programmatically
4. Parsing complex calendars with time zones
5. Handling validation errors

Run the examples:

```csharp
// Compile and run
csc Example.cs ICalendar.DOM.cs ICalendar.Parser.cs ICalendar.Validator.cs
Example.exe
```

## Testing

The test suite includes:

- **Parser Tests**: 15+ tests covering various parsing scenarios
- **Validator Tests**: 20+ tests for validation rules
- **DOM Tests**: Tests for object model manipulation

Run all tests:

```bash
dotnet test
```

## Project Structure

```
ICalendar.Net/
├── src/
│   └── ICalendar/              # Main library
│       ├── ICalendar.DOM.cs           # Document Object Model
│       ├── ICalendar.Parser.cs        # RFC 5545 parser
│       ├── ICalendar.Serializer.cs    # iCalendar serializer
│       ├── ICalendar.Validator.cs     # Validation logic
│       └── ICalendar.csproj           # Library project file
├── tests/
│   └── ICalendar.Tests/        # Test project
│       ├── ICalendar.Tests.cs         # Unit tests
│       ├── RoundTripTests.cs          # Read/write tests
│       ├── PayloadTests.cs            # Real-world payload tests
│       └── ICalendar.Tests.csproj     # Test project file
├── ICalendar.sln               # Solution file
└── README.md                   # This file
```

## Architecture

### Parser
1. **Line Unfolding**: Handles multi-line property values
2. **Tokenization**: Splits content lines into name, parameters, and value
3. **Component Parsing**: Recursively parses nested components
4. **Property Parsing**: Extracts properties with parameters
5. **Value Unescaping**: Handles escape sequences (\n, \;, \\, etc.)

### Serializer
1. **Component Serialization**: Recursively serializes components and sub-components
2. **Property Formatting**: Formats properties with parameters
3. **Line Folding**: Automatically folds lines longer than 75 characters
4. **Value Escaping**: Escapes special characters (\n, \;, \\, etc.)

### Validator
1. **Component Validation**: Checks required properties for each component type
2. **Format Validation**: Validates date/time, duration, and offset formats
3. **Rule Validation**: Enforces RFC 5545 rules (mutual exclusivity, value constraints)
4. **Result Reporting**: Provides detailed error and warning messages

## Publishing to NuGet

### Build and Pack

```bash
# Build in Release mode
dotnet build -c Release

# Run tests
dotnet test -c Release

# Create NuGet package
dotnet pack src/ICalendar/ICalendar.csproj -c Release -o ./artifacts

# Publish to NuGet
dotnet nuget push ./artifacts/ICalendar.Net.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Version Updates

Update version in `src/ICalendar/ICalendar.csproj`:

```xml
<Version>1.0.1</Version>
<PackageReleaseNotes>Bug fixes and improvements</PackageReleaseNotes>
```

## License

MIT License - This implementation is provided for educational and commercial use.

## References

- [RFC 5545 - Internet Calendaring and Scheduling Core Object Specification (iCalendar)](https://tools.ietf.org/html/rfc5545)
- [iCalendar.org](https://icalendar.org/)
- [NuGet Package](https://www.nuget.org/packages/ICalendar.Net/)

## Contributing

Contributions are welcome! Please ensure all tests pass before submitting pull requests.

### Development

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass: `dotnet test`
6. Submit a pull request
