using EconnectService;

var builder = WebApplication.CreateBuilder(args);

// Register our two helper classes so they can be used in the endpoints
builder.Services.AddSingleton<GpReader>();
builder.Services.AddSingleton<GpWriter>();

var app = builder.Build();

// Every request under /api must have the correct access key in the X-Api-Key header.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var expectedKey = app.Configuration["AccessKey"];
        var givenKey = context.Request.Headers["X-Api-Key"].ToString();

        if (string.IsNullOrEmpty(expectedKey) || givenKey != expectedKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing or wrong access key");
            return;
        }
    }
    await next();
});

var api = app.MapGroup("/api");

// Used by the "Test connection" button
api.MapGet("/ping", () => "OK");

api.MapGet("/classes", async (GpReader reader) =>
    Results.Ok(await reader.GetClassesAsync()));

api.MapGet("/assets", async (GpReader reader, int page = 1, int pageSize = 500) =>
    Results.Ok(await reader.GetAssetsAsync(page, pageSize)));

api.MapPost("/assets", async (NewAssetRequest request, GpWriter writer) =>
{
    // Basic checks before we bother GP
    if (string.IsNullOrWhiteSpace(request.Barcode) ||
        string.IsNullOrWhiteSpace(request.Description) ||
        string.IsNullOrWhiteSpace(request.Class))
    {
        return Results.Ok(new CreateResult { Success = false, Error = "Barcode, description and class are required." });
    }

    var result = await writer.CreateAssetAsync(request);
    return Results.Ok(result);
});

app.Run();
