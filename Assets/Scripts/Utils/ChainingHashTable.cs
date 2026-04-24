using System;
using System.Collections;
using System.Collections.Generic;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    // public Func<int> OnReSize()2;

    private class Bucket
    {
        public TKey Key;
        public TValue Value;

        public Bucket(TKey key) : this(key, default) { }
        public Bucket(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    private LinkedList<Bucket>[] _hashTable;

    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out TValue value))
            {
                Add(key, value);
            }
            return value;
        }
        set
        {
            int hash = GetHash(key);
            foreach (var bucket in _hashTable[hash])
            {
                if (bucket.Key.Equals(key))
                {
                    bucket.Value = value; // 덮어 쓰기로 약속
                    return;
                }
            }
            _hashTable[hash].AddLast(new Bucket(key, value));
            CurrRefIndex = hash;
            _count++;

            if (_loadFactor) Resize();
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keys = new();
            foreach (var buckets in _hashTable)
            {
                foreach (var item in buckets)
                {
                    keys.Add(item.Key);
                }
            }
            return keys;
        }
    }
    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new();
            foreach (var buckets in _hashTable)
            {
                foreach (var item in buckets)
                {
                    values.Add(item.Value);
                }
            }
            return values;
        }
    }

    public const int k_InitializeSize = 16;
    private int _count;
    public int Count => _count;
    public int Capacity => _hashTable.Length;
    private bool _loadFactor => (float)_count / _hashTable.Length >= 0.75f;

    public int CurrRefIndex = -1;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public ChainingHashTable()
    {
        Initialize();
    }

    private void Initialize()
    {
        _hashTable = new LinkedList<Bucket>[_hashTable?.Length ?? k_InitializeSize];
        for (int i = 0; i < Capacity; i++)
        {
            _hashTable[i] = new();
        }
        _count = 0;
    }

    public int GetHash(TKey key)
    {
        if (key is null) throw new ArgumentNullException();

        int hash = key.GetHashCode();
        return (hash & 0x7fffffff) % _hashTable.Length;
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Add(TKey key, TValue value)
    {
        int hash = GetHash(key);
        foreach (var bucket in _hashTable[hash])
        {
            if (bucket.Key.Equals(key)) throw new ArgumentException();
        }
        _hashTable[hash].AddLast(new Bucket(key, value));
        CurrRefIndex = hash;
        _count++;

        if (_loadFactor) Resize();
    }

    public void Clear() => Initialize();

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!TryGetValue(item.Key, out TValue value)) return false;
        return value.Equals(item.Value);
    }

    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        // TODO: 해시테이블? 복사 구현
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var buckets in _hashTable)
        {
            foreach (var bucket in buckets)
            {
                yield return new(bucket.Key, bucket.Value);
            }
        }
    }

    public IEnumerable GetBuckets(TKey key)
    {
        foreach (var bucket in _hashTable[GetHash(key)])
        {
            yield return bucket;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
    public bool Remove(TKey key)
    {
        var buckets = _hashTable[GetHash(key)];
        foreach (var bucket in buckets)
        {
            if (bucket.Key.Equals(key))
            {
                buckets.Remove(bucket);
                _count--;
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null) throw new ArgumentNullException();

        foreach (var bucket in _hashTable[GetHash(key)])
        {
            if (bucket.Key.Equals(key))
            {
                value = bucket.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private void Resize()
    {
        LinkedList<Bucket>[] newHashTable = new LinkedList<Bucket>[_hashTable.Length * 2];
        for (int i = 0; i < newHashTable.Length; i++)
        {
            newHashTable[i] = new();
        }

        foreach (var buckets in _hashTable)
        {
            foreach (var bucket in buckets)
            {
                newHashTable[GetHash(bucket.Key)].AddLast(bucket);
            }
        }

        _hashTable = newHashTable;
        CurrRefIndex = -1;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}