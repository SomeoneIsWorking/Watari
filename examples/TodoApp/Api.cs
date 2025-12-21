using System.Text.Json;

public class Api
{
    private static List<TodoItem> _todos = new();
    private static readonly string _dataFile = "todos.json";

    static Api()
    {
        LoadTodos();
    }

    public List<TodoItem> GetTodos()
    {
        return _todos;
    }

    public TodoItem AddTodo(string text)
    {
        var todo = new TodoItem
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Completed = false
        };
        _todos.Add(todo);
        SaveTodos();
        return todo;
    }

    public bool UpdateTodo(string id, string text, bool completed)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return false;
        todo.Text = text;
        todo.Completed = completed;
        SaveTodos();
        return true;
    }

    public bool DeleteTodo(string id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return false;
        _todos.Remove(todo);
        SaveTodos();
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