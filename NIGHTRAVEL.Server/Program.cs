using NIGHTRAVEL.Server.Model.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RoomContextRepository>();
// Add MagicOnion services to the container and enable JSON transcoding feature.
var magicOnion = builder.Services.AddMagicOnion();
if (builder.Environment.IsDevelopment())
{
    magicOnion.AddJsonTranscoding();
    // Add MagicOnion JSON transcoding Swagger support.
    builder.Services.AddMagicOnionJsonTranscodingSwagger();
    builder.Services.AddSwaggerGen(options =>
    {

    });
}
builder.Services.AddMvcCore();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

// Configure the HTTP request pipeline.
app.MapMagicOnionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapGet("/test", (RoomContextRepository repos) => 
"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();