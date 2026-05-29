public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ITodoRepository repository, ILogger<TodoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<TodoResponseDto>> GetAllAsync()
    {
        _logger.LogInformation("全Todoを取得します");
        var todos = await _repository.GetAllAsync();
        _logger.LogInformation("{Count}件取得しました", todos.Count);
        return todos.Select(t => new TodoResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted
        }).ToList();
    }

    public async Task<(bool isConflict, TodoResponseDto? result)> CreateAsync(TodoCreateDto dto)
    {
        _logger.LogInformation("Todo作成開始: Title={Title}", dto.Title);
        var todo = new Todo
        {
            Title = dto.Title,
            IsCompleted = dto.IsCompleted
        };
        await _repository.AddAsync(todo);
        await _repository.SaveAsync();
        _logger.LogInformation("Todo作成完了: Id={Id}", todo.Id);
        return (false, new TodoResponseDto
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted
        });
    }

    public async Task<(bool isNotFound, TodoResponseDto? result)> UpdateAsync(int id, TodoCreateDto dto)
    {
        _logger.LogInformation("Todo更新開始: Id={Id}", id);
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Todo更新失敗: Id={Id} が見つかりません", id);
            return (true, null);
        }
        existing.Title = dto.Title;
        existing.IsCompleted = dto.IsCompleted;
        await _repository.UpdateAsync(existing);
        await _repository.SaveAsync();
        _logger.LogInformation("Todo更新完了: Id={Id}", id);
        return (false, new TodoResponseDto
        {
            Id = existing.Id,
            Title = existing.Title,
            IsCompleted = existing.IsCompleted
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Todo削除開始: Id={Id}", id);
        var todo = await _repository.GetByIdAsync(id);
        if (todo == null)
        {
            _logger.LogWarning("Todo削除失敗: Id={Id} が見つかりません", id);
            return false;
        }
        await _repository.DeleteAsync(todo);
        await _repository.SaveAsync();
        _logger.LogInformation("Todo削除完了: Id={Id}", id);
        return true;
    }
}