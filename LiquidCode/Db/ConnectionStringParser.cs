using System.Text.RegularExpressions;

namespace LiquidCode.Db;

public class ConnectionStringParser
{
    // PG_URI example: postgresql://app:jPykLQU85XQyoMZjQ0gYgcD87nQjBAbCNg9mquDkpEr9WKhJgOrvoon3PpFyT29u@liquid-db-rw:5432/app
    public ConnectionStringParser(string uri)
    {
        // generate regex for parse this: "postgresql://postgres:d@localhost:5432/dev-db"
        var pattern =
            @"(?<protocol>(?:[^:]+)s?)?:\/\/(?:(?<user>[^:\n\r]+):(?<pass>[^@\n\r]+)@)?(?<host>(?:www\.)?(?:[^:\/\n\r]+))(?::(?<port>\d+))?\/?(?<request>[^?#\n\r]+)?\??(?<query>[^#\n\r]*)?\#?(?<anchor>[^\n\r]*)?";
        var match = Regex.Match(uri, pattern);

        if (match.Success)
        {
            //string db = match.Groups["protocol"].Value;
            var username = match.Groups["user"].Value;
            var password = match.Groups["pass"].Value;
            var host = match.Groups["host"].Value;
            var port = match.Groups["port"].Value;
            var database = match.Groups["request"].Value;

            EfCoreString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }
        else
        {
            throw new Exception("Failed to parse connection string.");
        }
    }

    public string EfCoreString { get; private set; }
}