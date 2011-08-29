using System;

namespace SkipList
{
    public class SkipList<T, U>
    {

        private SkipListNode<IComparable<T>,U> head;

        private class SkipListNode<T, U>
        {
            public SkipListNode<IComparable<T>,U> forward, back, up, down;
            public Tuple<IComparable<T>,U> keyValue;
            public IComparable<T> key
            {
                get { return keyValue.Item1; }
            }
            public U value
            {
                get { return keyValue.Item2; }
                set { keyValue = new Tuple<IComparable<T>,U>(keyValue.Item1, value); }
            }

            public SkipListNode()
            {
            }

            public SkipListNode(Tuple<IComparable<T>,U> keyValue)
            {
                this.keyValue = keyValue;
            }

            public SkipListNode(IComparable<T> key, U value)
            {
                this.keyValue = new Tuple<IComparable<T>,U>(key, value);
            }
        }

        public SkipList()
        {
            this.head = new SkipListNode<IComparable<T>,U>();
        }

        public void add(IComparable<T> key, U value)
        {
            Tuple<SkipListNode<IComparable<T>,U>, SkipListNode<IComparable<T>,U>> position = search(key);
            if(position.Item2 != null)
                position.Item2.value = value;
            else
            {
                SkipListNode<IComparable<T>,U> newEntry = new SkipListNode<IComparable<T>,U>(key, value);
                newEntry.back = position.Item1;
                if(position.Item1.forward != null)
                    newEntry.forward = position.Item1.forward;
                position.Item1.forward = newEntry;
                promote(newEntry);
            }
        }

        public U get(IComparable<T>key)
        {
            Tuple<SkipListNode<IComparable<T>,U>, SkipListNode<IComparable<T>,U>> position = search(key);
            if (position.Item2 == null)
                return null;
            return position.Item2.value;
        }

        private Tuple<SkipListNode<IComparable<T>,U>, SkipListNode<IComparable<T>,U>> search(IComparable<T> key)
        {
            SkipListNode<IComparable<T>,U> current, previous;
            current = this.head;

            while ((current.key == null || current.key.CompareTo(key) < 0) && (current.forward != null && current.down != null))
            {
                previous = current;
                if (current.forward == null || current.forward.key.CompareTo(key) > 0)
                {
                    if (current.down == null)
                        return new Tuple<SkipListNode<IComparable<T>, U>, SkipListNode<IComparable<T>, U>>(previous, null);
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }

            if (current.key == key)
                return new Tuple<SkipListNode<IComparable<T>, U>, SkipListNode<IComparable<T>, U>>(previous, current);
            else
                return new Tuple<SkipListNode<IComparable<T>, U>, SkipListNode<IComparable<T>, U>>(previous, null);
        }

        private void promote(SkipListNode<IComparable<T>,U> node)
        {
            SkipListNode<IComparable<T>, U> up = node.back;
            SkipListNode<IComparable<T>, U> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                while (up.up == null && up.back != null)
                    up = up.back;

                if (up.back == null)
                    up.up = new SkipListNode<IComparable<T>, U>();
                up = up.up;

                SkipListNode<IComparable<T>, U> previous = up;
                while (previous.key.CompareTo(node.key) < 0 && previous.forward != null)
                    previous = previous.forward;

                SkipListNode<IComparable<T>, U> newNode = new SkipListNode<IComparable<T>, U>(node.keyValue);
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
