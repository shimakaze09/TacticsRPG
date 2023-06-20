using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Utils
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        [SerializeField] [HideInInspector] private int[] _Buckets;
        [SerializeField] [HideInInspector] private int[] _HashCodes;
        [SerializeField] [HideInInspector] private int[] _Next;
        [SerializeField] [HideInInspector] private int _Count;
        [SerializeField] [HideInInspector] private int _Version;
        [SerializeField] [HideInInspector] private int _FreeList;
        [SerializeField] [HideInInspector] private int _FreeCount;
        [SerializeField] [HideInInspector] private TKey[] _Keys;
        [SerializeField] [HideInInspector] private TValue[] _Values;

        private readonly IEqualityComparer<TKey> _Comparer;

        // Mainly for debugging purposes - to get the key-value pairs display
        public Dictionary<TKey, TValue> AsDictionary => new(this);

        public int Count => _Count - _FreeCount;

        public TValue this[TKey key, TValue defaultValue]
        {
            get
            {
                var index = FindIndex(key);
                return index >= 0 ? _Values[index] : defaultValue;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var index = FindIndex(key);
                if (index >= 0)
                    return _Values[index];
                throw new KeyNotFoundException(key.ToString());
            }

            set => Insert(key, value, false);
        }

        public SerializableDictionary()
            : this(0, null)
        {
        }

        public SerializableDictionary(int capacity)
            : this(capacity, null)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Initialize(capacity);

            _Comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(dictionary?.Count ?? 0, comparer)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            foreach (var current in dictionary)
                Add(current.Key, current.Value);
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (var i = 0; i < _Count; i++)
                    if (_HashCodes[i] >= 0 && _Values[i] == null)
                        return true;
            }
            else
            {
                var defaultComparer = EqualityComparer<TValue>.Default;
                for (var i = 0; i < _Count; i++)
                    if (_HashCodes[i] >= 0 && defaultComparer.Equals(_Values[i], value))
                        return true;
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return FindIndex(key) >= 0;
        }

        public void Clear()
        {
            if (_Count <= 0)
                return;

            for (var i = 0; i < _Buckets.Length; i++)
                _Buckets[i] = -1;

            Array.Clear(_Keys, 0, _Count);
            Array.Clear(_Values, 0, _Count);
            Array.Clear(_HashCodes, 0, _Count);
            Array.Clear(_Next, 0, _Count);

            _FreeList = -1;
            _Count = 0;
            _FreeCount = 0;
            _Version++;
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            var bucketsCopy = new int[newSize];
            for (var i = 0; i < bucketsCopy.Length; i++)
                bucketsCopy[i] = -1;

            var keysCopy = new TKey[newSize];
            var valuesCopy = new TValue[newSize];
            var hashCodesCopy = new int[newSize];
            var nextCopy = new int[newSize];

            Array.Copy(_Values, 0, valuesCopy, 0, _Count);
            Array.Copy(_Keys, 0, keysCopy, 0, _Count);
            Array.Copy(_HashCodes, 0, hashCodesCopy, 0, _Count);
            Array.Copy(_Next, 0, nextCopy, 0, _Count);

            if (forceNewHashCodes)
                for (var i = 0; i < _Count; i++)
                    if (hashCodesCopy[i] != -1)
                        hashCodesCopy[i] = _Comparer.GetHashCode(keysCopy[i]) & 2147483647;

            for (var i = 0; i < _Count; i++)
            {
                var index = hashCodesCopy[i] % newSize;
                nextCopy[i] = bucketsCopy[index];
                bucketsCopy[index] = i;
            }

            _Buckets = bucketsCopy;
            _Keys = keysCopy;
            _Values = valuesCopy;
            _HashCodes = hashCodesCopy;
            _Next = nextCopy;
        }

        private void Resize()
        {
            Resize(PrimeHelper.ExpandPrime(_Count), false);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var hash = _Comparer.GetHashCode(key) & 2147483647;
            var index = hash % _Buckets.Length;
            var num = -1;
            for (var i = _Buckets[index]; i >= 0; i = _Next[i])
            {
                if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
                {
                    if (num < 0)
                        _Buckets[index] = _Next[i];
                    else
                        _Next[num] = _Next[i];

                    _HashCodes[i] = -1;
                    _Next[i] = _FreeList;
                    _Keys[i] = default;
                    _Values[i] = default;
                    _FreeList = i;
                    _FreeCount++;
                    _Version++;
                    return true;
                }

                num = i;
            }

            return false;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_Buckets == null)
                Initialize(0);

            var hash = _Comparer.GetHashCode(key) & 2147483647;
            var index = hash % _Buckets.Length;
            var num1 = 0;
            for (var i = _Buckets[index]; i >= 0; i = _Next[i])
            {
                if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
                {
                    if (add)
                        throw new ArgumentException("Key already exists: " + key);

                    _Values[i] = value;
                    _Version++;
                    return;
                }

                num1++;
            }

            int num2;
            if (_FreeCount > 0)
            {
                num2 = _FreeList;
                _FreeList = _Next[num2];
                _FreeCount--;
            }
            else
            {
                if (_Count == _Keys.Length)
                {
                    Resize();
                    index = hash % _Buckets.Length;
                }

                num2 = _Count;
                _Count++;
            }

            _HashCodes[num2] = hash;
            _Next[num2] = _Buckets[index];
            _Keys[num2] = key;
            _Values[num2] = value;
            _Buckets[index] = num2;
            _Version++;

            //if (num3 > 100 && HashHelpers.IsWellKnownEqualityComparer(comparer))
            //{
            //    comparer = (IEqualityComparer<TKey>)HashHelpers.GetRandomizedEqualityComparer(comparer);
            //    Resize(entries.Length, true);
            //}
        }

        private void Initialize(int capacity)
        {
            var prime = PrimeHelper.GetPrime(capacity);

            _Buckets = new int[prime];
            for (var i = 0; i < _Buckets.Length; i++)
                _Buckets[i] = -1;

            _Keys = new TKey[prime];
            _Values = new TValue[prime];
            _HashCodes = new int[prime];
            _Next = new int[prime];

            _FreeList = -1;
        }

        private int FindIndex(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_Buckets != null)
            {
                var hash = _Comparer.GetHashCode(key) & 2147483647;
                for (var i = _Buckets[hash % _Buckets.Length]; i >= 0; i = _Next[i])
                    if (_HashCodes[i] == hash && _Comparer.Equals(_Keys[i], key))
                        return i;
            }

            return -1;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = FindIndex(key);
            if (index >= 0)
            {
                value = _Values[index];
                return true;
            }

            value = default;
            return false;
        }

        private static class PrimeHelper
        {
            public static readonly int[] Primes =
            {
                3,
                7,
                11,
                17,
                23,
                29,
                37,
                47,
                59,
                71,
                89,
                107,
                131,
                163,
                197,
                239,
                293,
                353,
                431,
                521,
                631,
                761,
                919,
                1103,
                1327,
                1597,
                1931,
                2333,
                2801,
                3371,
                4049,
                4861,
                5839,
                7013,
                8419,
                10103,
                12143,
                14591,
                17519,
                21023,
                25229,
                30293,
                36353,
                43627,
                52361,
                62851,
                75431,
                90523,
                108631,
                130363,
                156437,
                187751,
                225307,
                270371,
                324449,
                389357,
                467237,
                560689,
                672827,
                807403,
                968897,
                1162687,
                1395263,
                1674319,
                2009191,
                2411033,
                2893249,
                3471899,
                4166287,
                4999559,
                5999471,
                7199369
            };

            public static bool IsPrime(int candidate)
            {
                if ((candidate & 1) != 0)
                {
                    var num = (int)Math.Sqrt((double)candidate);
                    for (var i = 3; i <= num; i += 2)
                        if (candidate % i == 0)
                            return false;
                    return true;
                }

                return candidate == 2;
            }

            public static int GetPrime(int min)
            {
                if (min < 0)
                    throw new ArgumentException("min < 0");

                foreach (var prime in Primes)
                    if (prime >= min)
                        return prime;

                for (var i = min | 1; i < 2147483647; i += 2)
                    if (IsPrime(i) && (i - 1) % 101 != 0)
                        return i;
                return min;
            }

            public static int ExpandPrime(int oldSize)
            {
                var num = 2 * oldSize;
                if (num > 2146435069 && 2146435069 > oldSize) return 2146435069;
                return GetPrime(num);
            }
        }

        public ICollection<TKey> Keys => _Keys.Take(Count).ToArray();

        public ICollection<TValue> Values => _Values.Take(Count).ToArray();

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var index = FindIndex(item.Key);
            return index >= 0 &&
                   EqualityComparer<TValue>.Default.Equals(_Values[index], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException($"index = {index} array.Length = {array.Length}");

            if (array.Length - index < Count)
                throw new ArgumentException(
                    $"The number of elements in the dictionary ({Count}) is greater than the available space from index to the end of the destination array {array.Length}.");

            for (var i = 0; i < _Count; i++)
                if (_HashCodes[i] >= 0)
                    array[index++] = new KeyValuePair<TKey, TValue>(_Keys[i], _Values[i]);
        }

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly SerializableDictionary<TKey, TValue> _Dictionary;
            private int _Version;
            private int _Index;
            private KeyValuePair<TKey, TValue> _Current;

            public KeyValuePair<TKey, TValue> Current => _Current;

            internal Enumerator(SerializableDictionary<TKey, TValue> dictionary)
            {
                _Dictionary = dictionary;
                _Version = dictionary._Version;
                _Current = default;
                _Index = 0;
            }

            public bool MoveNext()
            {
                if (_Version != _Dictionary._Version)
                    throw new InvalidOperationException(
                        $"Enumerator version {_Version} != Dictionary version {_Dictionary._Version}");

                while (_Index < _Dictionary._Count)
                {
                    if (_Dictionary._HashCodes[_Index] >= 0)
                    {
                        _Current = new KeyValuePair<TKey, TValue>(_Dictionary._Keys[_Index],
                            _Dictionary._Values[_Index]);
                        _Index++;
                        return true;
                    }

                    _Index++;
                }

                _Index = _Dictionary._Count + 1;
                _Current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_Version != _Dictionary._Version)
                    throw new InvalidOperationException(
                        $"Enumerator version {_Version} != Dictionary version {_Dictionary._Version}");

                _Index = 0;
                _Current = default;
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}