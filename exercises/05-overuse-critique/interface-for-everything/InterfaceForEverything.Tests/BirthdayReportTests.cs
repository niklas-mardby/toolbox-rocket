namespace InterfaceForEverything.Tests;

// These tests must stay green before AND after your simplification. Don't change them.
public class BirthdayReportTests
{
    /// <summary>A clock fixed at noon UTC, with UTC as local time zone so tests behave the same everywhere.</summary>
    private class FixedTime(int year, int month, int day) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(year, month, day, 12, 0, 0, TimeSpan.Zero);
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static string ReportOn(int year, int month, int day) =>
        Composition.CreateReportService(new FixedTime(year, month, day)).BuildReport();

    [Fact]
    public void Lists_upcoming_birthdays_soonest_first()
    {
        Assert.Equal(
            "🎂 Födelsedagar de närmaste 14 dagarna\n" +
            "- Anna Andersson fyller 35 idag ⭐\n" +
            "- Erik Ek fyller 56 imorgon\n" +
            "- Bo Berg fyller 41 om 3 dagar",
            ReportOn(2026, 11, 5));
    }

    [Fact]
    public void Includes_a_birthday_exactly_14_days_ahead_but_not_15()
    {
        var report = ReportOn(2026, 11, 6); // Cecilia: 20 November = 14 days

        Assert.Contains("- Cecilia Carlsson fyller 26 om 14 dagar", report);
        Assert.DoesNotContain("Cecilia", ReportOn(2026, 11, 5)); // 15 days
    }

    [Fact]
    public void Leap_day_birthdays_are_celebrated_on_28_february_in_normal_years()
    {
        Assert.Equal(
            "🎂 Födelsedagar de närmaste 14 dagarna\n- David Dahl fyller 31 om 8 dagar ⭐",
            ReportOn(2027, 2, 20));
    }

    [Fact]
    public void Days_until_a_leap_day_birthday_are_counted_to_28_february()
    {
        var report = ReportOn(2027, 2, 15); // 28 Feb 2027 is 13 days away

        Assert.Contains("David Dahl fyller 31 om 13 dagar", report);
    }

    [Fact]
    public void Says_so_when_there_are_no_birthdays()
    {
        Assert.Equal("🎂 Födelsedagar de närmaste 14 dagarna\nInga födelsedagar.", ReportOn(2026, 7, 1));
    }
}
