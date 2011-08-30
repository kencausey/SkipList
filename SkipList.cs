using System;
using System.Collections.Generic;

namespace SkipList
{
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable
    {

        private SkipListNode<TKey, TValue> head;
        private int count;
        public int Count { get { return count; } }
        public bool IsReadOnly { get { return false; } }
        public TValue this[TKey key]
        {
            get
            {
               return get(key);
            }
            set
            {
                Add(key, value);
            }
        }
        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>(count);
                walkEntries(n => keys.Add(n.key));
                return keys;
            }
        }
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>(count);
                walkEntries(n => values.Add(n.value));
                return values;
            }
        }

        private class SkipListPair<W, X>
        {
            public W first;
            public X second;

            public SkipListPair (W first, X second)
            {
                this.first = first;
                this.second = second;
            }   
        }

        private class SkipListNode<TKey, TValue>
        {
            public SkipListNode<TKey, TValue> forward, back, up, down;
            public SkipListPair<TKey, TValue> keyValue;
            public bool isFront = false;

            public TKey key
            {
                get { return keyValue.first; }
            }
            public TValue value
            {
                get { return keyValue.second; }
                set { keyValue = new SkipListPair<TKey, TValue>(keyValue.first, value); }
            }

            public SkipListNode()
            {
                this.keyValue = new SkipListPair<TKey, TValue>(default(TKey), default(TValue));
                this.isFront = true;
            }

            public SkipListNode(SkipListPair<TKey, TValue> keyValue)
            {
                this.keyValue = keyValue;
            }

            public SkipListNode(TKey key, TValue value)
            {
                this.keyValue = new SkipListPair<TKey, TValue>(key, value);
            }
        }

        public SkipList()
        {
            this.head = new SkipListNode<TKey, TValue>();
            count = 0;
        }

        public void Add(TKey key, TValue value)
        {
            SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>> position = search(key);
            if(position.first != null)
                position.first.value = value;
            else
            {
                SkipListNode<TKey, TValue> newEntry = new SkipListNode<TKey, TValue>((TKey)key, value);
                count++;
                newEntry.back = position.second;
                if(position.second.forward != null)
                    newEntry.forward = position.second.forward;
                position.second.forward = newEntry;
                promote(newEntry);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> keyValue)
        {
            Add(keyValue.Key, keyValue.Value);
        }

        public void Clear()
        {
            head = new SkipListNode<TKey, TValue>();
            count = 0;
            // Must more be done to ensure that all references are released?
        }

        public bool ContainsKey(TKey key)
        {
            SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>> position = search(key);
            if (position.first == null)
                return false;
            return true;
        }

        public bool Contains(KeyValuePair<TKey, TValue> keyValue)
        {
            return ContainsKey(keyValue.Key);       
        }

        public bool Remove(TKey key)
        {
            SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>> position = search(key);
            if(position.first == null)
                return false;
            else
            {
                SkipListNode<TKey, TValue> old = position.first;
                do {
                    position.second.forward = position.first.forward;
                    position.second.forward.back = position.second;
                    old = old.up;
                } while (old != null);
                 count--;
                // Clean up rows with only a head remaining.
                while(head.forward == null) {
                    head = head.down;
                }
                return true;
           }
        }

        public bool Remove(KeyValuePair<TKey, TValue> keyValue)
        {
            return Remove(keyValue.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                value = get(key);
                return true;
            }
            catch (KeyNotFoundException e)
            {
                value = default(TValue);
                return false;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (array.IsReadOnly)
                throw new ArgumentException("The array argument is Read Only and new items cannot be added to it.");
            if (array.IsFixedSize && array.Length < count + index)
                throw new ArgumentException("The array argument does not have sufficient space for the SkipList entries.");

            int i = index;
            walkEntries(n => array[i++] = new KeyValuePair<TKey, TValue>(n.key, n.value));
        }

        private TValue get(IComparable<TKey> key)
        {
            SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>> position = search(key);
            if (position.first == null)
                throw new KeyNotFoundException("Unable to find entry with key \"" + key.ToString() + "\"");
            return position.first.value;
        }

        private void walkEntries(Action<SkipListNode<TKey, TValue>> lambda)
        {
            SkipListNode<TKey, TValue> node = head;
            while(node.down != null)
                node = node.down;
            while(node.forward != null) {
                node = node.forward;
                lambda(node);
            }
        }

        private SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>> search(IComparable<TKey> key)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            SkipListNode<TKey, TValue> current, previous;
            previous = current = this.head;

            while ((current.isFront || key.CompareTo(current.key) >= 0) && (current.forward != null || current.down != null))
            {
                previous = current;
                if (current.forward == null || key.CompareTo(current.key) <= 0)
                {
                    if (current.down == null)
                        return new SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>>(previous, null);
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }

            if (key.CompareTo(current.key) == 0)
                return new SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>>(current, previous);
            else
                return new SkipListPair<SkipListNode<TKey, TValue>, SkipListNode<TKey, TValue>>(null, previous);
        }

        private void promote(SkipListNode<TKey, TValue> node)
        {
            SkipListNode<TKey, TValue> up = node.back;
            SkipListNode<TKey, TValue> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                while (up.up == null && !up.isFront)
                    up = up.back;

                if (up.isFront && up.up == null)
                {
                    up.up = new SkipListNode<TKey, TValue>();
                    head = up.up;
                }

                up = up.up;

                SkipListNode<TKey, TValue> previous = up;
                while ((previous.isFront || ((IComparable)previous.key).CompareTo(node.key) < 0) && previous.forward != null)
                    previous = previous.forward;

                SkipListNode<TKey, TValue> newNode = new SkipListNode<TKey, TValue>(node.keyValue);
                newNode.forward = previous.forward;
                previous.forward = newNode;
                newNode.down = last;
                newNode.down.up = newNode;
                last = newNode;
            }
        }

        private int levels()
        {
            Random generator = new Random();
            int levels = 0;
            while (generator.NextDouble() < 0.5)
                levels++;
            return levels;
        }
    }
}
