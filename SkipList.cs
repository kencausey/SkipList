using System;

namespace SkipList
{
    public class SkipList<T, U>
    {

        private SkipListNode<T,U> head;

        private class SkipListPair<W, X>
        {
            public W? first;
            public X? second;

            public SkipListPair (W? first, X? second)
            {
                this.first = first;
                this.second = second;
            }   
        }

        private class SkipListNode<T,U>
        {
            public SkipListNode<T,U>? forward, back, up, down;
            public SkipListPair<T,U> keyValue;
            public T? key
            {
                get { return keyValue.first; }
            }
            public U? value
            {
                get { return keyValue.second; }
                set { keyValue = new SkipListPair<T,U>(keyValue.first, value); }
            }

            public SkipListNode()
            {
            }

            public SkipListNode(SkipListPair<T,U> keyValue)
            {
                this.keyValue = keyValue;
            }

            public SkipListNode(T key, U value)
            {
                this.keyValue = new SkipListPair<T,U>(key, value);
            }
        }

        public SkipList()
        {
            this.head = new SkipListNode<T,U>();
        }

        public void add(IComparable<T> key, U value)
        {
            SkipListPair<SkipListNode<T,U>, SkipListNode<T,U>> position = search(key);
            if(position.first.HasValue)
                position.second.value = value;
            else
            {
                SkipListNode<T,U> newEntry = new SkipListNode<T,U>((T) key, value);
                newEntry.back = position.first;
                if(position.first.forward.HasValue)
                    newEntry.forward = position.first.forward;
                position.first.forward = newEntry;
                promote(newEntry);
            }
        }

        public Nullable<U> get(IComparable<T> key)
        {
            SkipListPair<SkipListNode<T,U>, SkipListNode<T,U>> position = search(key);
            if (!position.second.HasValue)
                return null;
            return position.second.value;
        }

        private SkipListPair<SkipListNode<T,U>, SkipListNode<T,U>> search(IComparable<T> key)
        {
            SkipListNode<T,U> current, previous;
            previous = current = this.head;  // Problem to set previous here?  Eliminates error below, but recheck logic.

            while ((!current.key.HasValue || key.CompareTo(current.key) >= 0) && (current.forward.HasValue && current.down.HasValue))
            {
                previous = current;
                if (!current.forward.HasValue || key.CompareTo(current.key) <= 0)
                {
                    if (!current.down.HasValue)
                        return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(previous, null);
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }

            if (key.CompareTo(current.key) == 0)
                return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(previous, current);
            else
                return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(previous, null);
        }

        private void promote(SkipListNode<T,U> node)
        {
            SkipListNode<T,U> up = node.back;
            SkipListNode<T,U> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                while (!up.up.HasValue && up.back.HasValue)
                    up = up.back;

                if (!up.back.HasValue)
                    up.up = new SkipListNode<T, U>();
                up = up.up;

                SkipListNode<T, U> previous = up;
                while (((IComparable) previous.key).CompareTo(node.key) < 0 && previous.forward.HasValue)
                    previous = previous.forward;

                SkipListNode<T, U> newNode = new SkipListNode<T, U>(node.keyValue);
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
