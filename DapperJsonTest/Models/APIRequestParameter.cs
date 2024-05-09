using Dapper.Json.Attributes;
using Dapper.Json.Types;

namespace Dapper.Json.Tests.Models;

[JsonContent]
public class APIRequestParameter //: IJson
{
    public string ParameterName { get; set; }
    public string ParameterType { get; set; }
    public object ParameterValue { get; set; }
}