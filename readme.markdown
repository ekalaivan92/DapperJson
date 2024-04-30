# Dapper.Json

## Introduction

Dapper.Json is a lightweight library designed to facilitate the storage and retrieval of JSON content within database columns when using Dapper ORM with various database management systems such as SQL Server, PostgreSQL, and SQLite. This library simplifies the serialization and deserialization process, enabling seamless interaction with JSON data in your C# applications by having your own custome type.

The primary focus of Dapper.Json is to extend support for storing JSON data, particularly targeting the Postgresql `json` or `jsonb` data types, which Dapper ORM lacks native support for. However, it is versatile enough to be utilized with other databases like MS SQL Server and SQLite, where JSON content can be stored as plain text within columns.

The need for this library or approach arises when dealing with third-party APIs. Often, we must store the response from these APIs. Subsequently, we discovered that PostgreSQL offers built-in data types to support such data structures, allowing for direct querying and indexing over JSON data. However, a challenge arose with Dapper lacking support for custom types to handle inbound or outbound data from these columns. Dapper does have some workarounds to resolve this problem, which are simplified in this library. This issue was encountered multiple times and resolved through various means. The approach presented here emerged as the most effective solution in our experience.

## Features

**Effortless Serialization/Deserialization**

Dapper.Json automates the process of converting JSON data to C# classes and vice versa, eliminating the need for manual handling of serialization and deserialization tasks.

**Support for Various Database Systems**

While initially intended for Postgresql, Dapper.Json is compatible with SQL Server, PostgreSQL, and SQLite, making it adaptable to a range of database environments.

**Integration with Dapper ORM**

Seamlessly integrate Dapper.Json with your Dapper ORM operations, enabling smooth handling of JSON data within your database interactions.

## Integration

**Inheritance of `IJson` Interface**

To utilize Dapper.Json's functionality for storing and retrieving JSON content, the custom types must inherit the `IJson` interface from the `Dapper.Json.Types` namespace. This inheritance ensures that the types are recognized and processed correctly by Dapper.Json during database operations.

**Registration of Custom Types**

Before establishing a database connection or within the `Program.cs` file, it is essential to register the custom types created for JSON support with Dapper. This registration process ensures that Dapper recognizes and handles the custom types appropriately when interacting with JSON data columns in the database. Onetime process of every program execution.

To accomplish this, invoke the `RegisterJsonBTypeHandlers()` extension method available in `Dapper.Json.Extensions` on the executing assembly. This step enables Dapper to map the custom types to their corresponding JSON column types, facilitating seamless serialization and deserialization operations.

Example:
```csharp
using Dapper.Json.Extensions;
using System.Reflection;

// Register custom types for JSON support with Dapper
Assembly.GetExecutingAssembly().RegisterJsonTypeHandlers();
```

### Notes

**Supported Types**

Dapper.Json currently supports serialization and deserialization of single objects or lists of objects. This means that you can use Dapper.Json seamlessly with single instances of your custom JSON types or collections (e.g., `List<T>`).

**Custom Type Registration**

**If** you intend to use other collection types such as `IEnumerable<T>`, you must explicitly register them for proper handling by Dapper.Json. To achieve this, utilize the following code snippet:

```csharp
using System.Collections.Generic;

var enumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(YourType));
TypeHandlerExtensions.RegisterJsonTypeHandlers(enumerableType);
```

Replace `YourType` with the specific type you want to register. This code dynamically creates the `IEnumerable<YourType>` type and registers it for JSON support with Dapper.Json. This ensures that Dapper.Json can correctly serialize and deserialize instances of `IEnumerable<YourType>` when interacting with JSON data columns in the database.

Ensure that this registration code is executed before performing any database operations involving the custom JSON types to ensure proper integration with Dapper.Json.

Ensure that this registration code is executed before performing any database operations involving the custom JSON types to ensure proper integration with Dapper.Json.

## Usage Example

Consider a scenario where you need to store API responses along with request parameters in a database table named `apiresponsehistories`. Here's how you can utilize Dapper.Json to achieve this:

### Type Definition

Define C# classes representing the JSON structures for API responses and request parameters. These classes should implement the `IJson` interface from the `Dapper.Json.Types` namespace.

```C#
public class APIResponse : IJson
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public int Qty { get; set; }
    public decimal Amount { get; set; }
}

public class APIRequestParameter : IJson
{
    public string ParameterName { get; set; }
    public string ParameterType { get; set; }
    public object ParameterValue { get; set; }
}
```

### Entity Definition

Define the entity class representing the database table `apiresponsehistories`, incorporating the JSON properties using the previously defined classes.

```C#
[Table("apiresponsehistories")]
public class APIResponseHistory
{
    public int Id { get; set; }
    public List<APIRequestParameter> Parameters { get; set; }
    public APIResponse Response { get; set; }
}
```

### Table Definition

```Sql
--SQLite
create table apiresponsehistories(id integer primary key, parameters text null, response text null)

--PostgreSQL - JsonB
create table if not exists apiresponsehistories(id serial primary key, parameters jsonb null, response jsonb null)

--PostgreSQL - Json
create table if not exists apiresponsehistories(id serial primary key, parameters json null, response json null)

--MS SQL Server
create table apiresponsehistories(id int identity(1,1) primary key, parameters varchar(max) null, response varchar(max) null)
```

### Usage

Utilize Dapper.Json within your database operations for seamless handling of JSON data.

```C#
var apiResponseHistory = new APIResponseHistory
{
    Parameters = new List<APIRequestParameter> { new APIRequestParameter { ParameterName = "RequestPath", ParameterType = "String", ParameterValue = requestPath } },
    Response = new APIResponse { StatusCode = statusCode, Message = message, Amount = amount, Qty = qty }
};

// Insert operation
connection.Insert(apiResponseHistory);

// Update operation
connection.Update(apiResponseHistory);

// Query examples
var rows = connection.Query<APIResponseHistory>("SELECT * FROM apiresponsehistories");
var rows = connection.Query<APIResponse>("SELECT response FROM apiresponsehistories");

// Specific PostgreSQL queries
var rows = connection.Query<string>("SELECT response->>'Message' FROM apiresponsehistories ORDER BY response->>'Message'");
var rows = connection.QueryFirst<List<APIResponse>>("SELECT jsonb_agg(response) AS responses FROM apiresponsehistories");
var rows = connection.Query<YourOwnViewClass>("SELECT id, response FROM apiresponsehistories");

```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Conclusion

Dapper.Json simplifies the management of JSON data within your database applications, providing a convenient solution for storing and retrieving JSON content across different database systems. With its seamless integration with Dapper ORM and support for various databases, Dapper.Json streamlines your development process, allowing you to focus on building robust applications.

For More details 