using System;
using System.Collections.Generic;

namespace Air.UnityGameCore.Runtime.Pool
{
  public class UnityObjectPool<T> : IPool<T>
  {
    readonly Stack<T> _stack;
    readonly Func<T> _createFunc;
    readonly Action<T> _onGet;
    readonly Action<T> _onRelease;
    readonly Action<T> _onDestroy;
    readonly bool _collectionCheck;
    readonly int _maxSize;

    int _countAll;

    public int CountAll => _countAll;
    public int CountInactive => _stack.Count;
    public int CountActive => _countAll - _stack.Count;

    public UnityObjectPool(
      Func<T> createFunc,
      Action<T> onGet = null,
      Action<T> onRelease = null,
      Action<T> onDestroy = null,
      int defaultCapacity = 0,
      int maxSize = int.MaxValue,
      bool collectionCheck = false)
    {
      _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
      _onGet = onGet;
      _onRelease = onRelease;
      _onDestroy = onDestroy;
      _collectionCheck = collectionCheck;
      _maxSize = maxSize <= 0 ? int.MaxValue : maxSize;
      _stack = new Stack<T>(Math.Max(defaultCapacity, 0));

      if (defaultCapacity > 0)
        Prewarm(defaultCapacity);
    }

    public virtual T Get()
    {
      var element = _stack.Count > 0 ? _stack.Pop() : CreateItem();
      _onGet?.Invoke(element);
      return element;
    }

    public virtual void Release(T element)
    {
      if (element == null)
        throw new ArgumentNullException(nameof(element), "Cannot release a null instance back to the pool.");

      if (_collectionCheck && _stack.Contains(element))
        throw new InvalidOperationException("Trying to release an object that is already in the pool.");

      if (_stack.Count >= _maxSize)
      {
        OnItemDestroyed(element);
        return;
      }

      _onRelease?.Invoke(element);
      _stack.Push(element);
    }

    public void Prewarm(int count)
    {
      for (var i = _stack.Count; i < count; i++)
      {
        var element = CreateItem();
        _stack.Push(element);
      }
    }

    public virtual void Clear()
    {
      while (_stack.Count > 0)
      {
        var element = _stack.Pop();
        OnItemDestroyed(element);
      }

      _countAll = 0;
    }

    protected virtual T CreateItem()
    {
      var element = _createFunc();
      _countAll++;
      OnItemCreated(element);
      return element;
    }

    protected virtual void OnItemCreated(T element)
    {
    }

    protected virtual void OnItemDestroyed(T element)
    {
      _onDestroy?.Invoke(element);
      if (_countAll > 0)
        _countAll--;
    }
  }
}
