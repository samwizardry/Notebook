using Asp.Versioning;
using Asp.Versioning.Builder;

namespace RottenRest.Web.Endpoints;
// QNA: Как подключать версионирование в minimal api
// QNA: Swagger не видит всех версий
public static class ApiVersioning
{
    public static ApiVersionSet VersionSet { get; private set; } = null!;

    public static IEndpointRouteBuilder UseApiVersionSet(this IEndpointRouteBuilder app)
    {
        VersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1.0d))
            .HasApiVersion(new ApiVersion(2.0d))
            .ReportApiVersions()
            .Build();

        return app;
    }
}
