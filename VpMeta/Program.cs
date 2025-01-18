using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using VpMeta.Api;
using VpMeta.Application;
using VpMeta.Repositories;

var schemaStream = typeof(Program).Assembly.GetManifestResourceStream("VpMeta.schema.json") ?? throw new Exception("Missing schema resource");
var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<FormOptions>(x => {
    x.MultipartBodyLengthLimit = builder.Configuration.GetValue("MaxUploadLimit", x.MultipartBodyLengthLimit);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ClinicalTrialMetadataRepository>(
    opt => _ = builder.Configuration.GetValue("Db:Type", "SqLite")?.ToLower() switch 
    {
        "memory" => opt.UseInMemoryDatabase("ClinicalTrialMetadata"),
        "sqlite" => opt.UseSqlite(
            builder.Configuration["Db:ConnectionString"],
            x => x.MigrationsAssembly("VpMeta.Migrations.Sqlite")
        ),
        "mssql" => opt.UseSqlServer(
            builder.Configuration["Db:ConnectionString"],
            x => x.MigrationsAssembly("VpMeta.Migrations.SqlServer")
        ),
        _ => throw new Exception($"Unsupported provider: {builder.Configuration.GetValue("Db:Type", "SqLite")}")
    });

builder.Services.AddSingleton<IJsonParserWithValidation>(new NJsonParser(schemaStream));
builder.Services.AddScoped<ClinicalTrialMetadataService, ClinicalTrialMetadataService>();
builder.Services.AddControllers();
builder.Services.AddHttpLogging(logging =>
{
    logging.CombineLogs = true;
    logging.LoggingFields = 
        HttpLoggingFields.RequestMethod | 
        HttpLoggingFields.RequestPath | 
        HttpLoggingFields.ResponseStatusCode | 
        HttpLoggingFields.Duration;
});


var app = builder.Build();

if (builder.Configuration.GetValue("Migration", false))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicalTrialMetadataRepository>();

    // Check and apply pending migrations
    var pendingMigrations = dbContext.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
    {
        Console.WriteLine("Applying pending migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
    }
    else
    {
        Console.WriteLine("No pending migrations found.");
    }
    Console.WriteLine("Exiting...");
    return;
}

app.UseHttpLogging();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
