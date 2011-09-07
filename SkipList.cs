using System;
using System.Collections;
using System.Collections.Generic;

namespace SkipList
{
    /** <summary>
     * (C) 2011 Ken Causey <solutions@kencausey.com>
     * Distributed under the MIT license, see LICENSE.txt file for details.
     * 
     * This is an implementation of skiplists, a data structure concept
     * developed by William Pugh.
     * 
     * http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.15.9072
     * 
     * (To aid my understanding I also watched an online lecture which
     * forms the basis for this implementation:
     * 
     * http://videolectures.net/mit6046jf05_demaine_lec12/ )
     * 
     * A skiplist is a mapping data structure which maintains its key/value
     * collection sorted by the keys.  Search is optimized by maintaining
     * 'fast lanes' which allow skipping past many values.  Since this is a
     * general purpose data structure where little is known about the keys
     * or values, the fast lanes are generated on a probabilistic basis.  This
     * results is O(log n) search performance.
     * 
     * Note that because a sorted structure is maintained, the keys must
     * implement the IComparable interface.
     **/
     
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable
    {
        private SkipListNode<TKey, TValue> head;
        private int count;
        /// <summary>
        /// A read-only value representing the current number of items in the
        /// map.
        /// </summary>
        public int Count { get { return count; } }
        /// <summary>
        /// Skiplists are always read/write structures in this implementation.
        /// </summary>
        public bool IsReadOnly { get { return false; } }
        /// <summary>
        /// This implementation supports indexed [] reference for both reading
        /// and writing entries of the map.  Note that if you set the value
        /// for an existing key in the map the current value will be
        /// overwritten.
        /// </summary>
        /// <param name="key">The IComparable key reference</param>
        /// <returns>the value</returns>
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
        /// <summary>
        /// Returns a collection (List) representing all the keys in the map in
        /// key-sorted order.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>(count);
                walkEntries(n => keys.Add(n.key));
                return keys;
            }
        }
        /// <summary>
        /// Returns a collection (List) representing all the value in the map
        /// in key-sorted order.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>(count);
                walkEntries(n => values.Add(n.value));
                return values;
            }
        }

        private struct SkipListKVPair<W, X>
        {
            private W key;
            public W Key
            {
                get { return key; }
            }
            public X Value;

            public SkipListKVPair (W key, X value)
            {
                this.key = key;
                this.Value = value;
            }   
        }

        private class SkipListNode<TNKey, TNValue>
        {
            public SkipListNode<TNKey, TNValue> forward, back, up, down;
            public SkipListKVPair<TNKey, TNValue> keyValue;
            public bool isFront = false;

            public TNKey key
            {
                get { return keyValue.Key; }
            }
            public TNValue value
            {
                get { return keyValue.Value; }
                set { keyValue.Value = value; }
            }

            public SkipListNode()
            {
                this.keyValue = new SkipListKVPair<TNKey, TNValue>(default(TNKey), default(TNValue));
                this.isFront = true;
            }

            public SkipListNode(SkipListKVPair<TNKey, TNValue> keyValue)
            {
                this.keyValue = keyValue;
            }

            public SkipListNode(TNKey key, TNValue value)
            {
                this.keyValue = new SkipListKVPair<TNKey, TNValue>(key, value);
            }
        }

        /// <summary>
        /// Creates and returns a new empty skiplist.
        /// </summary>
        public SkipList()
        {
            this.head = new SkipListNode<TKey, TValue>();
            count = 0;
        }

        /// <summary>
        /// This is an alternative (to indexing) interface to add and modify
        /// existing values in the map.
        /// </summary>
        /// <param name="key">The IComparable key</param>
        /// <param name="value">The new value</param>
        public void Add(TKey key, TValue value)
        {
            // Duh, we have to be able to tell when no key is found from when one is found
            // and if none is found have a reference to the last place searched....  return
            // a bool and use an out value?
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);
            if(found)
                position.value = value;
            else
            {
                // In this scenario position, rather than the value we searched
                // for is the value immediately previous to where it should be inserted.
                SkipListNode<TKey, TValue> newEntry = new SkipListNode<TKey, TValue>((TKey)key, value);
                count++;

                newEntry.back = position;
                if(position.forward != null)
                    newEntry.forward = position.forward;
                position.forward = newEntry;
                promote(newEntry);
            }
        }
        
        /// <summary>
        /// Add an entry using a System.Collections.Generic.KeyValuePair<>.
        /// </summary>
        /// <param name="keyValue">The KeyValuePair<> to add.  The key must be
        /// an IComparable.  If a matching entry already exists the value will
        /// be updated to the value specified in the KeyValuePair.</param>
        public void Add(KeyValuePair<TKey, TValue> keyValue)
        {
            Add(keyValue.Key, keyValue.Value);
        }

        /// <summary>
        /// Empty the skiplist.
        /// </summary>
        public void Clear()
        {
            head = new SkipListNode<TKey, TValue>();
            count = 0;
            // Must more be done to ensure that all references are released?
        }


        /// <summary>
        /// Test for the existence of an entry with the given key.
        /// </summary>
        /// <param name="key">The IComparable key to search for.</param>
        /// <returns>a bool indicating whether the map contains an entry with
        /// the specified key</returns>
        public bool ContainsKey(TKey key)
        {
            SkipListNode<TKey, TValue> notused;
            return search(key, out notused);          
        }

        /// <summary>
        /// Test for the existence of an entry with a matching key from a
        /// System.Collections.Generic.KeyValuePair<>.  Note that the value from
        /// the KeyValuePair is ignored and only the key is used in this test.
        /// </summary>
        /// <param name="keyValue">The KeyValuePair<> for which to search the
        /// map, note that only the IComparable key is used.</param>
        /// <returns>a bool indicating whether or not a matching entry exists
        /// in the map</returns>
        public bool Contains(KeyValuePair<TKey, TValue> keyValue)
        {
            return ContainsKey(keyValue.Key);       
        }

        /// <summary>
        /// Remove an entry in the map matching the specified key.
        /// </summary>
        /// <param name="key">The IComparable key to search for.  If found the
        /// matching entry is removed from the map.</param>
        /// <returns>a bool indicating whether the specified key was found in
        /// the map and the entry removed</returns>
        public bool Remove(TKey key)
        {
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);
            if(!found)
                return false;
            else
            {
                SkipListNode<TKey, TValue> old = position;
                do {
                    old.back.forward = old.forward;
                    if(old.forward != null)
                        old.forward.back = old.back;
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

        /// <summary>
        /// Remove an entry in the map matching the key from the specified
        /// System.Collections.Generic.KeyValuePair<>.  Only the key part of the
        /// KeyValuePair is used in the search.  Note that the value part of
        /// the KeyValuePair is not used.
        /// </summary>
        /// <param name="key">A KeyValuePair<> containing the IComparable key to
        /// search for.  If found the matching entry is removed from the map.</param>
        /// <returns>a bool indicating whether the a matching entry was found
        /// in the map and removed</returns>
        public bool Remove(KeyValuePair<TKey, TValue> keyValue)
        {
            return Remove(keyValue.Key);
        }

        /// <summary>
        /// Allows searching for a matching entry by IComparable key returning
        /// the value, if found as an out value.  Also returns as the standard
        /// return value whether or not a matching entry was found.
        /// </summary>
        /// <param name="key">IComparable key to search for</param>
        /// <param name="value">An out value specifying the value of the entry
        /// if found, otherwise the default is returned.</param>
        /// <returns>a bool indicating whether or not a matching entry was
        /// found</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                value = get(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Copies all entries in the skiplist to the provided System.Array of
        /// System.Collection.Generic.KeyValuePair<>s starting at the given
        /// index.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if the array
        /// provided is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the array is
        /// read-only, or does not have sufficient space after the specified
        /// index for the entries in the skiplist</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the
        /// specified index is less than zero.</exception>
        /// <param name="array">The array of KeyValuePair<>s in which to copy
        /// the skiplist entries.  The array must have sufficient space after
        /// the specified index to hold all entries in the skiplist.</param>
        /// <param name="index">The index of the array at which to start
        /// copying the entries.</param>
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

        /// <summary>
        /// Provides a System.Collections.Generic.IEnumerator<> interface to a
        /// collection of System.Collection.Generic.KeyValuePair<>s
        /// representing the entries in the map in key-sorted order.
        /// NOTE: The enumerator returned enumerates over internally used
        /// values, modifying the value is fine but do not modify the key
        /// because that would invalidate the internal structural assumptions.
        /// </summary>
        /// <returns>An IEnumerator<> of the map entries in key-sorted order</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            SkipListNode<TKey, TValue> position = head;
            while (position.down != null)
                position = position.down;
            while (position.forward != null)
            {
                position = position.forward;
                yield return new KeyValuePair<TKey, TValue>(position.key, position.value);
            }
        }

        /// <summary>
        /// Provides a System.Collections.IEnumerator interface to a collection
        /// of System.Collection.Generic.KeyValuePair<>s representing the
        /// entries in the map in key-sorted order.
        /// NOTE: The enumerator returned enumerates over internally used
        /// values, modifying the value is fine but do not modify the key
        /// because that would invalidate the internal structural assumptions.
        /// </summary>
        /// <returns>An IEnumerator of the map entries in key-sorted order</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        /// <summary>
        /// Retrieve the value from the matching entry in the map to the given
        ///   IComparable key.
        /// </summary>
        /// <param name="key">The IComparable key to search for</param>
        /// <returns>The value found</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// Thrown if no entry is found with the given key</exception>
        private TValue get(TKey key)
        {
            SkipListNode<TKey, TValue> position;
            bool found = search(key, out position);
            if (!found)
                throw new KeyNotFoundException("Unable to find entry with key \"" + key.ToString() + "\"");
            return position.value;
        }

        /// <summary>
        /// Takes an Action that accepts one argument representing a
        /// SkipListNode in the map and performs the given action on every entry
        /// in the map in key-sorted order.
        /// </summary>
        /// <param name="lambda">A System.Action(T) that accepts one parameter
        /// which will be each unique entry as a SkipListNode</param>
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

        /// <summary>
        /// The core search algorithm:  Returns a SkipListPair of SkipListNodes
        /// representing the matching entry with the given IComparable key and
        /// the immediately preceding entry in the map on the fastlane in which
        /// the entry was found.
        /// </summary>
        /// <param name="key">The IComparable key for which to search</param>
        /// <param name="position">Either the matching node if the true is
        /// returned as the return value, or, if false is returned, the value
        /// just before where the new value could be inserted.</param>
        /// <returns>Whether or not the search for value was found.</returns>
        private bool search(TKey key, out SkipListNode<TKey, TValue> position)
        {
            if(key == null)
                throw new ArgumentNullException("key");

            SkipListNode<TKey, TValue> current;
            position = current = head;

            while ((current.isFront || key.CompareTo(current.key) >= 0) && (current.forward != null || current.down != null))
            {
                position = current;
                if (key.CompareTo(current.key) == 0)
                    return true;

                if (current.forward == null || key.CompareTo(current.forward.key) < 0)
                {
                    if (current.down == null)
                        return false;
                    else
                        current = current.down;
                }
                else
                    current = current.forward;
            }
            position = current;

            // If the matching value is found in the last position of the last row, we could end up here with a match.
            if (key.CompareTo(position.key) == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// This algorithm promotes the newly added node on a probabilistic
        /// basis.
        /// </summary>
        /// <param name="node">The root node (initially added node added to the
        /// bottom, primary, row) to consider promoting.</param>
        private void promote(SkipListNode<TKey, TValue> node)
        {
            // up represents our search for the value just prior to the newly
            // added value in the next row to which the newly added value
            // should be promoted.
            // last represents the most recently added node, starting with the
            // newly created node.
            SkipListNode<TKey, TValue> up = node.back;
            SkipListNode<TKey, TValue> last = node;

            for (int levels = this.levels(); levels > 0; levels--)
            {
                // Find the next node back that links to next row up.
                // If we find our way back to the head of the row and there is
                // no link up then that means it is time to create a new row.
                while (up.up == null && !up.isFront)
                    up = up.back;

                if (up.isFront && up.up == null)
                {
                    // As mentioned above is this is the front of the row and
                    // there is no link up then we need to start a new row and
                    // update the head to ensure it always points to the start
                    // of the topmost row.
                    up.up = new SkipListNode<TKey, TValue>();
                    head = up.up;
                }

                up = up.up;

                // At this point up should represent the value in the next row
                // up immediately prior to where the new node should be
                // promoted.  If this node has been promoted to a previously
                // unreached level, then up will be the head of the new row.
                SkipListNode<TKey, TValue> newNode = new SkipListNode<TKey, TValue>(node.keyValue);
                newNode.forward = up.forward;
                up.forward = newNode;
                // Remember last starts as the brand new node but should be
                // updated to always point to the representative node in
                // the previous row.
                newNode.down = last;
                newNode.down.up = newNode;
                last = newNode;
            }
        }

        /// <summary>
        /// The random number of level to promote a newly added node.
        /// </summary>
        /// <returns>the number of levels of promotion</returns>
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
