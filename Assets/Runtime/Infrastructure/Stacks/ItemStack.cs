using System.Collections.Generic;
using System.Linq;

namespace Runtime.Infrastructure.Stacks
{
    public sealed class ItemStack<T> : IStack<T>
    {
        private readonly List<T> _items;
        private int _nextIndex;

        public ItemStack(IEnumerable<T> items) => _items = items.ToList();

        public T GetNext() =>
            _items[_nextIndex++ % _items.Count];

        public void Reset() => _nextIndex = 0;
    }
}