using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter() : base(
        d => d.ToDateTime(TimeOnly.MinValue), // Convert DateOnly to DateTime for storage
        dt => DateOnly.FromDateTime(dt)       // Convert DateTime back to DateOnly for retrieval
    )
    {
    }
}
