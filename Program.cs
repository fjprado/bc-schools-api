using AutoMapper;
using bc_schools_api.Domain;
using bc_schools_api.Infra;
using bc_schools_api.Infra.Interfaces;
using bc_schools_api.Repository;
using bc_schools_api.Services;
using bc_schools_api.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

var _configuration = builder.Configuration
 .SetBasePath(Directory.GetCurrentDirectory())
 .AddJsonFile($"appsettings.json", optional: false)
 .AddJsonFile($"appsettings.{env}.json", optional: true)
 .AddEnvironmentVariables()
 .Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ISettings settings = new Settings();
_configuration.GetSection("ProjectSettings").Bind(settings);
builder.Services.AddScoped(svc => settings);

builder.Services.AddDbContext<DatabaseContext>(x => x.UseSqlServer(settings.SchoolDbConnectionString));
builder.Services.ConfigureAll<IDbConnection>(options =>
{
    options.ConnectionString = settings.SchoolDbConnectionString;
});
builder.Services.AddScoped<IDbConnection, DbConnection>(x => new SqlConnection(settings.SchoolDbConnectionString));

builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});
IMapper mapper = mapperConfig.CreateMapper();

builder.Services.AddSingleton(mapper);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
