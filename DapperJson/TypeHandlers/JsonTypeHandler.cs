using System.Data;
using Newtonsoft.Json;
using Dapper.Json.Extensions;

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
        if (parameter.IsNpgqlType())
        {
            parameter.SetNpgqlDbType();
        }
        else if(parameter.IsSQLiteType() || parameter.IsSQLServerType())
        {
            parameter.DbType = DbType.String;
        }
    }
}
