using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "予期しないエラーが発生しました");
            await context.Response.WriteAsJsonAsync(new
            {
                message = "サーバーエラーが発生しました"
            });
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.MapGet("/todos", async (ITodoService service) =>
    Results.Ok(await service.GetAllAsync()));

app.MapPost("/todos", async (TodoCreateDto dto, ITodoService service) =>
{
    var (isConflict, result) = await service.CreateAsync(dto);
    return isConflict
        ? Results.Conflict("そのIDは既に存在してるよ！")
        : Results.Created($"/todos/{result!.Id}", result);
});

app.MapPut("/todos/{id}", async (int id, TodoCreateDto dto, ITodoService service) =>
{
    var (isNotFound, result) = await service.UpdateAsync(id, dto);
    return isNotFound
        ? Results.NotFound("そのIDは存在しないよ！")
        : Results.Ok(result);
});

app.MapDelete("/todos/{id}", async (int id, ITodoService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted
        ? Results.Ok("削除したよ！")
        : Results.NotFound("そのIDは存在しないよ！");
});

app.Run();