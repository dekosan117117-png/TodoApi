public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TodoResponseDto>> GetAllAsync()
    {
        var todos = await _repository.GetAllAsync();
        return todos.Select(t => new TodoResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted
        }).ToList();
    }

    public async Task<(bool isConflict, TodoResponseDto? result)> CreateAsync(TodoCreateDto dto)
    {
        var todo = new Todo
        {
            Title = dto.Title,
            IsCompleted = dto.IsCompleted
        };
        await _repository.AddAsync(todo);
        await _repository.SaveAsync();
        return (false, new TodoResponseDto
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted
        });
    }

    public async Task<(bool isNotFound, TodoResponseDto? result)> UpdateAsync(int id, TodoCreateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return (true, null);

        existing.Title = dto.Title;
        existing.IsCompleted = dto.IsCompleted;
        await _repository.UpdateAsync(existing);
        await _repository.SaveAsync();

        return (false, new TodoResponseDto
        {
            Id = existing.Id,
            Title = existing.Title,
            IsCompleted = existing.IsCompleted
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await _repository.GetByIdAsync(id);
        if (todo == null) return false;

        await _repository.DeleteAsync(todo);
        await _repository.SaveAsync();
        return true;
    }
}