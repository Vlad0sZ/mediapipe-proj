using System;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.Infrastructure.Stacks
{
    public sealed class ShuffledItemStack<T> : IStack<T>
    {
        private readonly List<T> _items;
        private readonly Stack<T> _activeStack;

        public ShuffledItemStack(IEnumerable<T> items)
        {
            _items = items.ToList();
            _activeStack = new Stack<T>(_items.Count);
        }

        public T GetNext()
        {
            if (_activeStack.Count == 0)
                Reset();

            return _activeStack.Pop();
        }


        public void Reset()
        {
            _activeStack.Clear();
            IOrderedEnumerable<T> orderedEnumerable = _items.OrderBy(x => Guid.NewGuid());
            foreach (var i in orderedEnumerable)
                _activeStack.Push(i);
        }
    }
}