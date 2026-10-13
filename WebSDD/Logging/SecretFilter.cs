namespace WebSDD.Logging;

// Filtro base de secretos para Serilog (T-003): excluye eventos cuyo mensaje
// o propiedades contengan marcadores de secreto. Sin PII en logs.
public static class SecretFilter
{
    private static readonly string[] Markers =
    [
        "password", "passwd", "secret", "token", "apikey",
        "privatekey", "connectionstring", "bearer", "pwd",
    ];

    public static bool ShouldExclude(string? renderedMessage, IEnumerable<(string Name, string? Value)> properties)
    {
        if (renderedMessage is not null && ContainsMarker(renderedMessage))
        {
            return true;
        }

        foreach (var (name, value) in properties)
        {
            if (ContainsMarker(name))
            {
                return true;
            }

            if (value is not null && ContainsMarker(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsMarker(string text)
    {
        return Markers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));
    }
}
