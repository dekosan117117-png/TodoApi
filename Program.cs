using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.MapGet("/todos", async (TodoDbContext db) =>
    await db.Todos.ToListAsync());

app.MapPost("/todos", async (Todo todo, TodoDbContext db) =>
{
    if (await db.Todos.AnyAsync(t => t.Id == todo.Id))
    {
        return Results.Conflict("そのIDは既に存在してるよ！");
    }
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id}", async (int id, Todo todo, TodoDbContext db) =>
{
    var existing = await db.Todos.FindAsync(id);
    if (existing == null)
    {
        return Results.NotFound("そのIDは存在しないよ！");
    }
    existing = existing with { Title = todo.Title, IsCompleted = todo.IsCompleted };
    db.Todos.Update(existing);
    await db.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/todos/{id}", async (int id, TodoDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound("そのIDは存在しないよ！");
    }
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.Ok("削除したよ！");
});

app.Run();

public record Todo(int Id, string Title, bool IsCompleted);