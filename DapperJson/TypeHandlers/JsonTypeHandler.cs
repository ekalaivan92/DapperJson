using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace Dapper.Json.TypeHandlers;

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override T Parse(object value)
    {
        if (value == null || value is DBNull) return default(T);

        var result = JsonConvert.DeserializeObject<T>(value.ToString());

        return result;
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
        else if(parameter is SQLiteParameter sqliteParameter)
        {
            sqliteParameter.DbType = DbType.String;
        }
        else if(parameter is SqlParameter sqParameter)
        {
            sqParameter.DbType = DbType.String;
        }
    }
}
