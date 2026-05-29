using Microsoft.EntityFrameworkCore;
public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _db;

    public TodoRepository(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<List<Todo>> GetAllAsync() =>
        await _db.Todos.ToListAsync();

    public async Task<Todo?> GetByIdAsync(int id) =>
        await _db.Todos.FindAsync(id);

    public async Task<bool> ExistsAsync(int id) =>
        await _db.Todos.AnyAsync(t => t.Id == id);

    public async Task AddAsync(Todo todo) =>
        await _db.Todos.AddAsync(todo);

    public async Task UpdateAsync(Todo todo) =>
        _db.Todos.Update(todo);

    public async Task DeleteAsync(Todo todo) =>
        _db.Todos.Remove(todo);

    public async Task SaveAsync() =>
        await _db.SaveChangesAsync();
}