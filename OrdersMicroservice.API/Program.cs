using BusinessLogicLayer;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;

var builder = WebApplication.CreateBuilder(args);




//ADD DAL AND BAL SERVICES
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

//SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();


app.UseExceptionHandlingMiddleware();
app.UseRouting();

app.UseCors("AllowAll");

//SWAGGER
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersMicroservice.API v1");
});

//AUTH
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//ENDPOINTS
app.MapControllers();


app.Run();
