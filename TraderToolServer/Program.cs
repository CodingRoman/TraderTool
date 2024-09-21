var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KrakenCrypto>();

// Register SignalR services
builder.Services.AddSignalR();

// Register hosted services
builder.Services.AddHostedService<CryptoDataService>();
builder.Services.AddHostedService<TickerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<TickerHub>("/tickerHub");

app.Run();
