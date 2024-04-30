using System.Reflection;
using Dapper.Json.Types;
using Dapper.Json.TypeHandlers;
using static Dapper.SqlMapper;

namespace Dapper.Json.Extensions;

public static class TypeHandlerExtensions
{
    public static void RegisterJsonTypeHandlers(this Assembly assembly)
    { 
        var types = assembly
            .GetTypes()
            .Where(type => typeof(IJson).IsAssignableFrom(type));

        foreach (var type in types)
        {
            Console.WriteLine("Found new json type. [Type= {0}]", type.FullName);
            RegisterJsonTypeHandlers(type);

            var listType = typeof(List<>).MakeGenericType(type);
            RegisterJsonTypeHandlers(listType);
        }
    }

    public static void RegisterJsonTypeHandlers(Type type)
    {
        var handlerType = typeof(JsonTypeHandler<>).MakeGenericType(type);
            var handler = (ITypeHandler)Activator.CreateInstance(handlerType);

            AddTypeHandler(type, handler);
    }
}



