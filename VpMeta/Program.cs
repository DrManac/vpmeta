using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using VpMeta.Api;
using VpMeta.Application;
using VpMeta.Repositories;

var schemaStream = typeof(Program).Assembly.GetManifestResourceStream("VpMeta.schema.json") ?? throw new Exception("Missing schema resource");
var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<FormOptions>(x => {
    long val = x.MultipartBodyLengthLimit;
    if (long.TryParse(builder.Configuration["MaxUploadLimit"], out val))
        x.MultipartBodyLengthLimit = val;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<ClinicalTrialMetadataRepository>(opt => opt.UseInMemoryDatabase("ClinicalTrialMetadata") );
builder.Services.AddDbContext<ClinicalTrialMetadataRepository>(opt => opt.UseSqlite(builder.Configuration["ConnectionStrings:SQLiteDefault"]));
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
