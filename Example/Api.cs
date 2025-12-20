public class Api
{
    public string Hello(string name)
    {
        return $"Hello, {name}!";
    }

    public X GetX(Y y)
    {
        return new X { Name = "Example" };
    }
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