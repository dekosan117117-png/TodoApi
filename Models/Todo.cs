using System.ComponentModel.DataAnnotations;

public class Todo
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Titleは必須だよ！")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Titleは1〜100文字で入力してね！")]
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}