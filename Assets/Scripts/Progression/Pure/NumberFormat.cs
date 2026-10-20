using System.Globalization;

public static class NumberFormat
{
    public static string Mb(long value)
    {
        if (value < 0)
        {
            value = 0;
        }

        if (value < 1000)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        double v = value;
        if (value < 10_000)
        {
            return (v / 1_000d).ToString("0.00", CultureInfo.InvariantCulture) + "K";
        }

        if (value < 1_000_000)
        {
            return (v / 1_000d).ToString("0.0", CultureInfo.InvariantCulture) + "K";
        }

        if (value < 10_000_000)
        {
            return (v / 1_000_000d).ToString("0.0", CultureInfo.InvariantCulture) + "M";
        }

        if (value < 1_000_000_000)
        {
            return (v / 1_000_000d).ToString("0.00", CultureInfo.InvariantCulture) + "M";
        }

        if (value < 10_000_000_000L)
        {
            return (v / 1_000_000_000d).ToString("0.00", CultureInfo.InvariantCulture) + "B";
        }

        if (value < 1_000_000_000_000L)
        {
            return (v / 1_000_000_000d).ToString("0.0", CultureInfo.InvariantCulture) + "B";
        }

        return (v / 1_000_000_000_000d).ToString("0.00", CultureInfo.InvariantCulture) + "T";
    }
}
