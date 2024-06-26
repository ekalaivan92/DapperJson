using System.Net;
using System.Reflection;
using Dapper.Contrib.Extensions;
using Npgsql;
using Dapper.Json.Extensions;
using Newtonsoft.Json;
using System.Data;
using Dapper.Json.Tests.Models;

namespace Dapper.Json.Tests;

public class PostgreSQLJsonBTests
{
    private string _connectionString = @"";

    private IDbConnection GetDbConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        return connection;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            Assert.Ignore("Datasouce not configured");
        }

        Assembly.GetExecutingAssembly().RegisterJsonTypeHandlers();

        using (var connection = GetDbConnection())
            connection.Execute(@"create table if not exists apiresponsehistories(id serial primary key, parameters jsonb null, response jsonb null)");

        Console.WriteLine("[PostgreSQL-JsonB] One Time Setup Completed");
    }

    [SetUp]
    public void Setup()
    {
        Console.WriteLine("[PostgreSQL-JsonB] Test Setup Completed");
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine("[PostgreSQL-JsonB] Test Completed");
    }


    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (!string.IsNullOrEmpty(_connectionString))
        {
            using (var connection = GetDbConnection())
                connection.Execute(@"drop table if exists apiresponsehistories");
        }

        Console.WriteLine("[PostgreSQL-JsonB] Full Test Completed");
    }


    [TestCase(HttpStatusCode.OK, "Entry Created 1", 101, 11, "req-123", "/api/v1/test"), Order(1)]
    [TestCase(HttpStatusCode.Forbidden, "Entry Created 2", 102, 12, "req-123", "/api/v1/test1")]
    [TestCase(HttpStatusCode.NotFound, "Entry Created 3", 103, 13, null, null)]
    [TestCase(HttpStatusCode.Unauthorized, "Entry Created 4", 104, 14, "req-123", null)]
    [TestCase(HttpStatusCode.UnsupportedMediaType, "Entry Created 5", 105, 15, null, "/api/v1/test")]
    public void Insert(HttpStatusCode statuscode, string message, decimal amount, int qty, string requestId, string requstPath)
    {
        using var connection = GetDbConnection();

        var apiResponseHistory = new APIResponseHistory
        {
            response = new() { StatusCode = statuscode, Message = message, Amount = amount, Qty = qty }
        };

        if (!string.IsNullOrEmpty(requestId))
        {
            apiResponseHistory.parameters ??= new();
            apiResponseHistory.parameters.Add(new() { ParameterName = "RequestID", ParameterType = "String", ParameterValue = requestId });
        }

        if (!string.IsNullOrEmpty(requstPath))
        {
            apiResponseHistory.parameters ??= new();
            apiResponseHistory.parameters.Add(new() { ParameterName = "RequestPath", ParameterType = "String", ParameterValue = requstPath });
        }

        var id = connection.Insert(apiResponseHistory);
        Console.WriteLine("[PostgreSQL-JsonB] New entry created. [Entry={0}]", JsonConvert.SerializeObject(apiResponseHistory));

        var row = connection.Get<APIResponseHistory>(id);
        Console.WriteLine("[PostgreSQL-JsonB] Read created entry. [Entry={0}]", JsonConvert.SerializeObject(row));

        Assert.That(apiResponseHistory.response,
            Has.Property(nameof(APIResponse.StatusCode)).EqualTo(row.response.StatusCode) &
            Has.Property(nameof(APIResponse.Message)).EqualTo(row.response.Message) &
            Has.Property(nameof(APIResponse.Amount)).EqualTo(row.response.Amount) &
            Has.Property(nameof(APIResponse.Qty)).EqualTo(row.response.Qty));
    }

    [TestCase(HttpStatusCode.OK, "Entry Updated 1", 101, 11), Order(2)]
    [TestCase(HttpStatusCode.OK, "Entry Updated 2", 102, 12)]
    [TestCase(HttpStatusCode.OK, "Entry Updated 3", 103, 13)]
    [TestCase(HttpStatusCode.OK, "Entry Updated 4", 104, 14)]
    [TestCase(HttpStatusCode.OK, "Entry Updated 5", 105, 15)]
    public void Update(HttpStatusCode statuscode, string message, decimal amount, int qty)
    {
        using var connection = GetDbConnection();

        var row = connection.QueryFirst<APIResponseHistory>("select * from apiresponsehistories where response is not null");
        Console.WriteLine("[PostgreSQL-JsonB] Read a entry. [Entry={0}]", JsonConvert.SerializeObject(row));

        row.response.StatusCode = statuscode;
        row.response.Message = message;
        row.response.Amount = amount;
        row.response.Qty = qty;
        Console.WriteLine("[PostgreSQL-JsonB] Modified entry. [Entry={0}]", JsonConvert.SerializeObject(row));

        _ = connection.Update(row);
        var updatedRow = connection.Get<APIResponseHistory>(row.id);
        Console.WriteLine("[PostgreSQL-JsonB] Entry updated. [Entry={0}]", JsonConvert.SerializeObject(updatedRow));

        Assert.That(row.response,
            Has.Property(nameof(APIResponse.StatusCode)).EqualTo(updatedRow.response.StatusCode) &
            Has.Property(nameof(APIResponse.Message)).EqualTo(updatedRow.response.Message) &
            Has.Property(nameof(APIResponse.Amount)).EqualTo(updatedRow.response.Amount) &
            Has.Property(nameof(APIResponse.Qty)).EqualTo(updatedRow.response.Qty));
    }

    [Test, Order(3)]
    public void InsertNull()
    {
        var apiResponseHistory = new APIResponseHistory();

        using var connection = GetDbConnection();

        var id = connection.Insert(apiResponseHistory);
        Console.WriteLine("[PostgreSQL-JsonB] New entry created with NULL. [Entry={0}]", JsonConvert.SerializeObject(apiResponseHistory));

        var row = connection.Get<APIResponseHistory>(id);
        Console.WriteLine("[PostgreSQL-JsonB] Read created entry. [Entry={0}]", JsonConvert.SerializeObject(row));

        Assert.IsNull(row.response);
    }


    [Test, Order(4)]
    public void ReadAll()
    {
        using var connection = GetDbConnection();

        var rows = connection.GetAll<APIResponseHistory>().OrderBy(x => x.id);
        Console.WriteLine("[PostgreSQL-JsonB] Read all. [Entries={0}]", JsonConvert.SerializeObject(rows));

        Assert.Pass();
    }

    [Test, Order(5)]
    public void ReadOneKey()
    {
        using var connection = GetDbConnection();

        var rows = connection.Query<string>("select response['Message'] from apiresponsehistories order by response['Message']");
        Console.WriteLine("[PostgreSQL-JsonB] Read only one key. [key Values={0}]", JsonConvert.SerializeObject(rows));

        Assert.Pass();
    }

    [Test, Order(6)]
    public void ReadAggregatedJsonB()
    {
        using var connection = GetDbConnection();

        var rows = connection.QueryFirst<List<APIResponse>>("select jsonb_agg(response) as responses from apiresponsehistories");
        Console.WriteLine("[PostgreSQL-JsonB] Read aggragated. [Entries={0}]", JsonConvert.SerializeObject(rows));

        Assert.Pass();
    }

    [Test, Order(7)]
    public void ReadList()
    {
        using var connection = GetDbConnection();

        var rows = connection.Query<APIResponseView>("select jsonb_agg(response) as responses from apiresponsehistories");
        Console.WriteLine("[PostgreSQL-JsonB] Read into view. [View={0}]", JsonConvert.SerializeObject(rows));

        Assert.Pass();
    }
}