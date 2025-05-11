using System.Xml;

using Microsoft.Extensions.Configuration;

namespace Library.Web.Endpoints;

public static class EndpointExtensions
{
    public static void AddEndpoints(this IServiceCollection services, Type typeMarker, IConfiguration configuration)
    {
        IEnumerable<System.Reflection.TypeInfo> endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!.Invoke(null, [services, configuration]);
        }
    }

    public static void AddEndpoints<T>(this IServiceCollection services, IConfiguration configuration)
    {
        AddEndpoints(services, typeof(T), configuration);
    }

    public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        IEnumerable<System.Reflection.TypeInfo> endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!.Invoke(null, [app]);
        }
    }

    public static void UseEndpoints<T>(this IApplicationBuilder app)
    {
        UseEndpoints(app, typeof(T));
    }

    private static IEnumerable<System.Reflection.TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        return typeMarker.Assembly.DefinedTypes
            .Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x));
    }
}
