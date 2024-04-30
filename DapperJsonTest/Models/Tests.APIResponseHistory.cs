using Dapper.Contrib.Extensions;

namespace Dapper.Json.Tests.Models;

[Table("apiresponsehistories")]
public class APIResponseHistory
{
    public int id { get; set; }
    public List<APIRequestParameter> parameters { get; set; }
    public APIResponse response { get; set; }
}