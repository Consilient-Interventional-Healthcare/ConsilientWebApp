namespace Consilient.Common.Helpers;

/// <summary>
/// Helper class for enum operations.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Converts an enum type to a list of its values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert.</typeparam>
    /// <returns>A list of all values in the enum.</returns>
    public static List<TEnum> ToList<TEnum>() where TEnum : struct, Enum
    {
        return [.. Enum.GetValues<TEnum>()];
    }

    /// <summary>
    /// Converts an enum type to a list of key-value pairs (name and value).
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert.</typeparam>
    /// <returns>A list of tuples containing the enum name and numeric value.</returns>
    public static List<(string Name, int Value)> ToKeyValueList<TEnum>() where TEnum : struct, Enum
    {
        return [.. Enum.GetValues<TEnum>().Select(e => (Name: e.ToString(), Value: Convert.ToInt32(e)))];
    }
}
