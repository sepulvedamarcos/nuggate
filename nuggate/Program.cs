using System.Reflection;
using Microsoft.OpenApi.Models;

var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var version = builder.Configuration.GetValue<string>("Version");
    var fecha = builder.Configuration.GetValue<string>("Fecha");

    //Titulo
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NuGet Server",
        Version = version,
        Description = $"Servidor de NuGet \n- Versión {version} - Fecha {fecha}"
    });


    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
