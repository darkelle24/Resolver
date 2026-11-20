namespace VMRuntime;

/// <summary>Resizable VM stack that clears released references.</summary>
public sealed class Stack<T>
{
    private T[] _data = new T[256];
    private int _count;
    /// <summary>Pushes a value onto the stack.</summary>
    public void Push(T value)
    {
        if (_count == _data.Length) Array.Resize(ref _data, _data.Length * 2);
        _data[_count++] = value;
    }
    /// <summary>Pops a value or throws if the stack is empty.</summary>
    public T Pull()
    {
        T value = Peek();
        _data[--_count] = default!;
        return value;
    }
    /// <summary>Reads a value without removing it; zero refers to the top.</summary>
    public T Peek(int offset = 0)
    {
        if ((uint)offset >= (uint)_count) throw new InvalidOperationException("VM stack underflow.");
        return _data[_count - 1 - offset];
    }
    /// <summary>Clears all stack values and references.</summary>
    public void Clear() { Array.Clear(_data, 0, _count); _count = 0; }
    /// <summary>Returns the number of values.</summary>
    public int GetCount() => _count;
    /// <summary>Checks whether the stack is empty.</summary>
    public bool IsEmpty() => _count == 0;
}
