using System.Text.Json;
using Watari;
using Microsoft.Extensions.Logging;

public class Api(WatariContext context, ILogger<Api> logger)
{
    private static List<TodoItem> _todos = new();
    private static readonly string _dataFile = "todos.json";
    private readonly WatariContext _context = context;

    static Api()
    {
        LoadTodos();
    }

    public List<TodoItem> GetTodos()
    {
        return _todos;
    }

    public async Task<TodoItem> AddTodo(string text)
    {
        var todo = new TodoItem
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Completed = false
        };
        _todos.Add(todo);
        SaveTodos();
        await _context.Server.EmitEvent("todoAdded", todo);
        return todo;
    }

    public async Task<bool> UpdateTodo(string id, string text, bool completed)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return false;
        todo.Text = text;
        todo.Completed = completed;
        SaveTodos();
        await _context.Server.EmitEvent("todoUpdated", todo);
        return true;
    }

    public async Task<bool> DeleteTodo(string id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return false;
        _todos.Remove(todo);
        SaveTodos();
        await _context.Server.EmitEvent("todoDeleted", new { id });
        return true;
    }

    private static void LoadTodos()
    {
        if (File.Exists(_dataFile))
        {
            var json = File.ReadAllText(_dataFile);
            _todos = JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new List<TodoItem>();
        }
    }

    private static void SaveTodos()
    {
        var json = JsonSerializer.Serialize(_todos);
        File.WriteAllText(_dataFile, json);
    }

    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }

    public void MoveWindowUp()
    {
        var (x, y) = _context.MainWindow!.GetPosition();
        _context.MainWindow.Move(x, y - 50);
    }

    public void MoveWindowDown()
    {
        var (x, y) = _context.MainWindow!.GetPosition();
        _context.MainWindow.Move(x, y + 50);
    }

    public void MoveWindowLeft()
    {
        var (x, y) = _context.MainWindow!.GetPosition();
        _context.MainWindow.Move(x - 50, y);
    }

    public void MoveWindowRight()
    {
        logger.LogInformation("Moving window right");
        var (x, y) = _context.MainWindow!.GetPosition();
        _context.MainWindow.Move(x + 50, y);
    }

    public X GetX(Y y)
    {
        return new X { Name = "Example" };
    }
}

public class TodoItem
{
    public required string Id { get; set; }
    public required string Text { get; set; }
    public bool Completed { get; set; }
}

public class X
{
    public required string Name { get; set; }
}

public class Y 
{
    public int Value { get; set; }
}

public class Api2
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}