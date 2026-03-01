public ref struct CustomIntEnumerator
{
    private int _current;
    private readonly int _end;
    private readonly bool _invalid;
    private readonly int _offset;
    public CustomIntEnumerator(int from, int to)
    {
        _offset = from > to ? -1 : 1;
        _current = from -= _offset;
        _end = to;
        _invalid = false;
    }
    public CustomIntEnumerator(bool _)
    {
        _invalid = true;
        _current = default;
        _end = default;
        _offset = default;
    }
    public int Current => _current;
    public bool MoveNext()
    {
        if (_invalid)
            return false;
        _current += _offset;
        return _current.CompareTo(_end) != _offset;
    }
}