using OPS_Pro_Server.Hubs;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.Configure<OpsPro>(builder.Configuration.GetSection("OpsPro"));
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddSingleton<IUserManager, UserManager>();

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
