namespace Web.API.Infrastructure;

public static class EnvironmentLoader
{
    public static void LoadDotEnvIfPresent()
    {
        var envFilePath = FindEnvFile(Directory.GetCurrentDirectory());
        if (envFilePath is null)
        {
            return;
        }
        foreach (var rawLine in File.ReadAllLines(envFilePath))
        {
            ApplyEnvironmentLine(rawLine);
        }
    }

    private static string? FindEnvFile(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }
            current = current.Parent;
        }

        return null;
    }

    private static void ApplyEnvironmentLine(string rawLine)
    {
        var trimmed = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
        {
            return;
        }
        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex <= 0)
        {
            return;
        }
        var key = trimmed[..separatorIndex].Trim();
        var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
