using FluentValidation;

using Library.Web.Data;
using Library.Web.Endpoints;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    //WebRootPath = "./wwwroot",
    //EnvironmentName = Environment.GetEnvironmentVariable("env"),
    //ApplicationName = "Library.Web"
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.IncludeFields = true;
});

builder.Services.AddCors(cors =>
{
    cors.AddPolicy("AnyOrigin", p => p.AllowAnyOrigin());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("Sqlite")!));

builder.Services.AddTransient<DatabaseInitializer>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
//    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
//builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseAuthorization();

app.UseEndpoints<Program>();

{
    await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
}

app.Run();
