public interface ITodoService
{
    Task<List<TodoResponseDto>> GetAllAsync();
    Task<(bool isConflict, TodoResponseDto? result)> CreateAsync(TodoCreateDto dto);
    Task<(bool isNotFound, TodoResponseDto? result)> UpdateAsync(int id, TodoCreateDto dto);
    Task<bool> DeleteAsync(int id);
}