using BackendPortafolio.Data;
using BackendPortafolio.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        //extraer mensajes de error en de los campos de los dto
        var errores = string.Join(", ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));

        //Crear el objecto ApiResponse ya estandarizado
        var respuesta = new ApiResponse<string>
        {
            Exito = false,
            Mensaje = "Errores de validación: " + errores,
            Datos = null
        };

        //Devolver un 400 con nuestra estructura
        return new BadRequestObjectResult(respuesta);

    };

});

// contexto de la base de datos con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirTodo");
app.UseHttpsRedirection();

app.MapControllers();


app.Run();

