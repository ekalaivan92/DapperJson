using System.Net;
using Dapper.Json.Types;

namespace Dapper.Json.Tests.Models;

public class APIResponse : IJson
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public int Qty { get; set; }
    public decimal Amount { get; set; }
}