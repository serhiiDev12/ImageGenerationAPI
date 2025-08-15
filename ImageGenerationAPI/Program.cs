using ImageGenerationAPI.Config;
using ImageGenerationAPI.Hub;
using ImageGenerationAPI.Interfaces;
using ImageGenerationAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<PythonWorkerOptions>(
    builder.Configuration.GetSection("PythonWorker"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddSignalR();
var app = builder.Build();
app.MapHub<StatusHub>("/status-hub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
