namespace FamilyBudget.Persistence.DesignTime;

internal static class DesignTimeEnvironmentFileLoader
{
    private const int MaxParentDepth = 8;

    public static void Load()
    {
        var envFile = FindEnvironmentFile();
        if (envFile is null)
            return;

        foreach (var sourceLine in File.ReadLines(envFile))
        {
            var line = sourceLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line[7..].TrimStart();

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            if (key.Length == 0 || Environment.GetEnvironmentVariable(key) is not null)
                continue;

            var value = line[(separatorIndex + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindEnvironmentFile()
    {
        foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startPath);
            for (var depth = 0;
                 directory is not null && depth <= MaxParentDepth;
                 depth++, directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, ".env");
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }
}
