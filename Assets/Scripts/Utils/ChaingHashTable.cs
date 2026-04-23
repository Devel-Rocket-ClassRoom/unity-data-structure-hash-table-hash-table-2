using System;
using System.Collections;
using System.Collections.Generic;

public class ChaingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
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
            if (!TryGetValue(key, out TValue value)) throw new KeyNotFoundException();
            return value;
        }
        set
        {
            if (key == null || value == null) throw new ArgumentNullException();

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
            _count++;
        }
    }

    public ICollection<TKey> Keys { get; }
    public ICollection<TValue> Values { get; }

    public const int k_InitializeSize = 16;
    private int _count = 0;
    public int Count => _count;
    public int Capacity => _hashTable.Length;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get; }

    public ChaingHashTable()
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
    }

    public int GetHash(TKey key)
    {
        int hash = key.GetHashCode();
        return (hash & 0x7fffffff) % _hashTable.Length;
    }

    public void Add(KeyValuePair<TKey, TValue> item) => this[item.Key] = item.Value;
    public void Add(TKey key, TValue value) => this[key] = value;

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
        foreach (var item in _hashTable)
        {
            foreach (var bucket in item)
            {
                yield return new(bucket.Key, bucket.Value);
            }
        }
    }

    // public IEnumerable<(int Index, LinkedList<Bucket> Chain)> EnumerateBuckets()
    // {
    //     for (int i = 0; i < _hashTable.Length; i++)
    //     {
    //         yield return (i, _hashTable[i]);
    //     }
    // }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException();

        foreach (var bucket in _hashTable[GetHash(key)])
        {
            if (bucket.Key.Equals(key))
            {
                _hashTable[GetHash(key)].Remove(bucket);
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}