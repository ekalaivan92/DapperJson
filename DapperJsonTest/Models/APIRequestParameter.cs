using Dapper.Json.Types;

namespace Dapper.Json.Tests.Models;

public class APIRequestParameter : IJson
{
    public string ParameterName { get; set; }
    public string ParameterType { get; set; }
    public object ParameterValue { get; set; }
}