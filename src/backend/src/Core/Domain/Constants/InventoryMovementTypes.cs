namespace Domain.Constants;

public static class InventoryMovementTypes
{
    public const string Entry = "ENTRY";
    public const string Exit = "EXIT";

    public static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    public static bool IsValid(string? normalizedValue)
    {
        return normalizedValue is Entry or Exit;
    }
}
