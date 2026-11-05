using BankingApi.Api.Middlewares;
using BankingApi.Application.UseCases;
using BankingApi.Domain.Repositories;
using BankingApi.Infrastructure.Persistence;
using BankingApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banking API",
        Version = "v1",
        Description = "API for loan payment processing",
        Contact = new OpenApiContact
        {
            Name = "Yeisson",
            Email = "yeisson72@outlook.com"
        }
    });
});
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite("Data Source=banking.db"));

builder.Services.AddScoped<IAccountRepository, EfAccountRepository>();
builder.Services.AddScoped<ITransferRepository, EfTransferRepository>();
builder.Services.AddScoped<ITransactionRepository, EfTransactionRepository>();

builder.Services.AddScoped<OpenAccountUseCase>();
builder.Services.AddScoped<TransferFundsUseCase>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
