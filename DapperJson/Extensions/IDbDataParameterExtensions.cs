using System.Data;
using System.Reflection;

namespace Dapper.Json.Extensions;

public static class IDbDataParameterExtensions
{
    public static bool IsNpgqlType(this IDbDataParameter instance)
        => instance.GetType().Name.Equals("NpgsqlParameter", StringComparison.OrdinalIgnoreCase);

    public static void SetNpgqlDbType(this IDbDataParameter instance)
        => SetPropertyEnumValue(instance, "NpgsqlDbType", "Npgsql", "NpgsqlTypes.NpgsqlDbType", "Json");

    public static bool IsSQLiteType(this IDbDataParameter instance)
            => instance.GetType().Name.Equals("SQLiteParameter", StringComparison.OrdinalIgnoreCase);

    public static bool IsSQLServerType(this IDbDataParameter instance)
            => instance.GetType().Name.Equals("SqlParameter", StringComparison.OrdinalIgnoreCase);

    public static void SetPropertyEnumValue(this IDbDataParameter instance, string propertyName, string assemblyName, string typeName, string valueName)
    {
        var assembly = Assembly.Load(assemblyName);
        var enumType = assembly.GetType(typeName);
        var enumValue = Enum.Parse(enumType, valueName);

        instance.GetType().GetProperty(propertyName).SetValue(instance, enumValue);
    }
}