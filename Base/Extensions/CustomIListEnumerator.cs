using System.Collections.Generic;

public ref struct CustomIListEnumerator<T>
{
    private int _current;
    private readonly int _end;
    private readonly bool _invalid;
    private readonly int _offset;
    readonly IList<T> _list;
    public CustomIListEnumerator(IList<T> list, int from, int to)
    {
        _list = list;
        _offset = from > to ? -1 : 1;
        _current = from -= _offset;
        _end = to;
        _invalid = false;
    }
    public CustomIListEnumerator(bool _)
    {
        _invalid = true;
        _list = default;
        _current = default;
        _end = default;
        _offset = default;
    }
    public T Current => _list[_current];
    public bool MoveNext()
    {
        if (_invalid)
            return false;
        _current += _offset;
        return _current.CompareTo(_end) != _offset;
    }
}
