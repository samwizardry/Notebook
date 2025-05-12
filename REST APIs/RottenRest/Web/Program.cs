using System.Text;

using Asp.Versioning;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using RottenRest.Application.Data;
using RottenRest.Web;
using RottenRest.Web.Auth;
using RottenRest.Web.Endpoints;
using RottenRest.Web.Health;
using RottenRest.Web.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

//builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(c => c.Cache());
    options.AddPolicy("MovieCache", c => c.Cache()
        .Expire(TimeSpan.FromSeconds(60))
        .SetVaryByQuery(["title", "year", "orderBy", "page", "pageSize"])
        .Tag("movies")
    );
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.AddApplication();
builder.AddDatabase();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,
            ValidateIssuer = true,
            ValidateAudience = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy(AuthConstants.AdminUserPolicyName, policy =>
    //{
    //    policy.RequireClaim(AuthConstants.AdminUserClaimName, "true");
    //});

    options.AddPolicy(AuthConstants.AdminUserPolicyName,
        policy => policy.AddRequirements(new AdminAuthRequirement(builder.Configuration["ApiKey"]!)));

    options.AddPolicy(AuthConstants.TrustedMemberPolicyName, policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            return ctx.User.HasClaim(c => (c.Type == AuthConstants.AdminUserClaimName && c.Value == "true")
                || (c.Type == AuthConstants.TrustedMemberClaimName && c.Value == "true"));
        });
    });
});

builder.Services
    .AddApiVersioning(options =>
    {
        options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        // reporting api versions will return the headers
        // "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
    })
    .AddApiExplorer();

//builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerDefaultValues>();
});

var app = builder.Build();

app.UseApiVersionSet();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.MapHealthChecks("_health");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// UseCors should be called before UseResponseCaching and before UseOutputCache
//app.UseCors();
//app.UseResponseCaching();
app.UseOutputCache();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapApiEndpoints();

{
    await app.Services.GetRequiredService<DbInitializer>().InitializeAsync();
}

app.Run();
