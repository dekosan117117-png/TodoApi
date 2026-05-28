using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.MapGet("/todos", async (TodoDbContext db) =>
    await db.Todos.ToListAsync());

app.MapPost("/todos", async (Todo todo, TodoDbContext db) =>
{
    // Validationチェック追加
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(todo);
    if (!Validator.TryValidateObject(todo, context, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }
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
    
    // Validationチェック追加
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(todo);
    if (!Validator.TryValidateObject(todo, context, validationResults, true))
    {
        return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
    }
    var existing = await db.Todos.FindAsync(id);
    if (existing == null)
    {
        return Results.NotFound("そのIDは存在しないよ！");
    }
    existing.Title = todo.Title;
    existing.IsCompleted = todo.IsCompleted;
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

public class Todo
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Titleは必須だよ！")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Titleは1〜100文字で入力してね！")]
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}