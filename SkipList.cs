using System;

namespace SkipList
{
    public class SkipList<T, U> where T: IComparable
    {

        private SkipListNode<T,U> head;

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

        private class SkipListNode<T,U>
        {
            public SkipListNode<T,U> forward, back, up, down;
            public SkipListPair<T,U> keyValue;
            public bool isFront = false;

            public T key
            {
                get { return keyValue.first; }
            }
            public U value
            {
                get { return keyValue.second; }
                set { keyValue = new SkipListPair<T,U>(keyValue.first, value); }
            }

            public SkipListNode()
            {
                this.keyValue = new SkipListPair<T, U>(default(T), default(U));
                this.isFront = true;
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
            if(position.first != null)
                position.first.value = value;
            else
            {
                SkipListNode<T,U> newEntry = new SkipListNode<T,U>((T)key, value);
                newEntry.back = position.second;
                if(position.second.forward != null)
                    newEntry.forward = position.second.forward;
                position.second.forward = newEntry;
                promote(newEntry);
            }
        }

        public U get(IComparable<T> key)
        {
            SkipListPair<SkipListNode<T,U>, SkipListNode<T,U>> position = search(key);
            if (position.first == null)
                return default(U);
            return position.first.value;
        }

        private SkipListPair<SkipListNode<T,U>, SkipListNode<T,U>> search(IComparable<T> key)
        {
            SkipListNode<T,U> current, previous;
            previous = current = this.head;

            while ((current.isFront || key.CompareTo(current.key) >= 0) && (current.forward != null || current.down != null))
            {
                previous = current;
                if (current.forward == null || key.CompareTo(current.key) <= 0)
                {
                    if (current.down == null)
                        return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(previous, null);
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }

            if (key.CompareTo(current.key) == 0)
                return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(current, previous);
            else
                return new SkipListPair<SkipListNode<T, U>, SkipListNode<T, U>>(null, previous);
        }

        private void promote(SkipListNode<T,U> node)
        {
            SkipListNode<T,U> up = node.back;
            SkipListNode<T,U> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                while (up.up == null && !up.isFront)
                    up = up.back;

                if (up.isFront)
                    up.up = new SkipListNode<T, U>();

                up = up.up;

                SkipListNode<T, U> previous = up;
                while ((previous.isFront || ((IComparable)previous.key).CompareTo(node.key) < 0) && previous.forward != null)
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
