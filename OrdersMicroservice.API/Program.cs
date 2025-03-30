using BusinessLogicLayer;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Policies;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;
using Polly;

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


builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
}).AddPolicyHandler( builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy());


builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
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
