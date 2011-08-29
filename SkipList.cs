using System;

namespace SkipList
{
    public class SkipList<T, U>
    {

        private SkipListNode<T,U> head;

        private class SkipListNode<T,U>
        {
            public SkipListNode<T,U> forward, back, up, down;
            public Tuple<T,U> keyValue;
            public T key
            {
                get { return keyValue.Item1; }
            }
            public U value
            {
                get { return keyValue.Item2; }
                set { keyValue = new Tuple<T,U>(keyValue.Item1, value); }
            }

            public SkipListNode()
            {
            }

            public SkipListNode(Tuple<T,U> keyValue)
            {
                this.keyValue = keyValue;
            }

            public SkipListNode(T key, U value)
            {
                this.keyValue = new Tuple<T,U>(key, value);
            }
        }

        public SkipList()
        {
            this.head = new SkipListNode<T,U>();
        }

        public void add(IComparable<T> key, U value)
        {
            Tuple<SkipListNode<T,U>, SkipListNode<T,U>> position = search(key);
            if(position.Item2 != null)
                position.Item2.value = value;
            else
            {
                SkipListNode<T,U> newEntry = new SkipListNode<T,U>((T) key, value);
                newEntry.back = position.Item1;
                if(position.Item1.forward != null)
                    newEntry.forward = position.Item1.forward;
                position.Item1.forward = newEntry;
                promote(newEntry);
            }
        }

        public Nullable<U> get(IComparable<T> key)
        {
            Tuple<SkipListNode<T,U>, SkipListNode<T,U>> position = search(key);
            if (position.Item2 == null)
                return null;
            return position.Item2.value;
        }

        private Tuple<SkipListNode<T,U>, SkipListNode<T,U>> search(IComparable<T> key)
        {
            SkipListNode<T,U> current, previous;
            previous = current = this.head;  // Problem to set previous here?  Eliminates error below, but recheck logic.

            while ((current.key == null || key.CompareTo(current.key) >= 0) && (current.forward != null && current.down != null))
            {
                previous = current;
                if (current.forward == null || key.CompareTo(current.key) <= 0)
                {
                    if (current.down == null)
                        return new Tuple<SkipListNode<T, U>, SkipListNode<T, U>>(previous, null);
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }

            if (key.CompareTo(current.key) == 0)
                return new Tuple<SkipListNode<T, U>, SkipListNode<T, U>>(previous, current);
            else
                return new Tuple<SkipListNode<T, U>, SkipListNode<T, U>>(previous, null);
        }

        private void promote(SkipListNode<T,U> node)
        {
            SkipListNode<T, U> up = node.back;
            SkipListNode<T, U> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                while (up.up == null && up.back != null)
                    up = up.back;

                if (up.back == null)
                    up.up = new SkipListNode<T, U>();
                up = up.up;

                SkipListNode<T, U> previous = up;
                while (((IComparable) previous.key).CompareTo(node.key) < 0 && previous.forward != null)
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
