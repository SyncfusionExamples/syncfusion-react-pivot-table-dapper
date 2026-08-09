var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Enable CORS to allow requests from React client
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient",
        policy => policy
            .WithOrigins("https://localhost:7086")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// CORS must be registered BEFORE UseHttpsRedirection so that
// preflight OPTIONS requests are not redirected.
app.UseCors("ReactClient");
app.UseHttpsRedirection();
// UseAuthorization is registered without an authentication scheme for
// this sample. Add authentication and authorization services in
// production (for example, JWT bearer authentication).
app.UseAuthorization();
app.MapControllers();

app.Run();
