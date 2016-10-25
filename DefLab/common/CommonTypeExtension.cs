public static class CommonTypeExtension
{
    public static bool isBad(this double value)
    {
        return double.IsNaN(value)
        || double.IsInfinity(value)
        || double.IsNegativeInfinity(value)
        || double.IsPositiveInfinity(value);
    }
    public static bool isBad(this double? value)
    {
        return value == null || isBad(value.Value);
    }

}