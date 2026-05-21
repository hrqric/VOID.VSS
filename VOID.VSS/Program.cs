using Microsoft.AspNetCore.Authentication.JwtBearer;
using VOID.VSS.Application;
using VOID.VSS.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://SEU-PROJETO.supabase.co/auth/v1";
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = false
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => 
            policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();


app.MapGet("/", () => "Starting Void Stock System API..." +
                      $"\n v0.2.0" ); // mudar versão td vez que mudar algo
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
