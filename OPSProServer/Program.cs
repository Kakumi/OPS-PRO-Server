using Microsoft.AspNetCore.SignalR;
using OPSProServer.Hubs;
using OPSProServer.Hubs.Filters;
using OPSProServer.Managers;
using OPSProServer.Models;
using OPSProServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.AddFilter<ErrorHandlerFilter>();
    hubOptions.AddFilter<PlayerTurnFilter>();
});
builder.Services.Configure<OpsPro>(builder.Configuration.GetSection("OpsPro"));
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<IResolverManager, ResolverManager>();
builder.Services.AddScoped<PlayerTurnFilter>();
builder.Services.AddScoped<ErrorHandlerFilter>();
builder.Services.AddSingleton<ICardService, CardService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/ws/game");

app.Run();
