# iCalendar Test Cases

Shared, language-independent test cases for the iCalendar component.

## Format

Each test case consists of a `.ics` input file paired with a `.json` file
describing the expected parse result, following the pattern used by the
[vCard](https://github.com/spec-works/vCard) Part.

### Positive Tests

Files in the root of this directory are valid iCalendar documents per
[RFC 5545](https://www.rfc-editor.org/rfc/rfc5545.html).

| File | Description |
|------|-------------|
| `simple-event.ics` / `.json` | Minimal VEVENT with required properties |
| `event-with-alarm.ics` / `.json` | VEVENT containing a VALARM sub-component |
| `simple-todo.ics` / `.json` | VTODO with status and percent-complete |
| `timezone.ics` / `.json` | VTIMEZONE with STANDARD and DAYLIGHT rules |
| `multiple-events.ics` / `.json` | Calendar containing three VEVENTs |
| `escaped-characters.ics` / `.json` | Properties with escaped commas, semicolons, newlines, and backslashes |

### Negative Tests

Files in the `negative/` subdirectory are invalid documents that validators
SHOULD reject.

| File | Description |
|------|-------------|
| `missing-begin.ics` | Missing BEGIN:VCALENDAR |
| `mismatched-end.ics` | END tag does not match BEGIN |
