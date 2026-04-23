using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private struct Entry
    {
        public TKey Key;
        public TValue Value;
        public bool isOccupied;
    }

    private Entry[] _buckets;
    private int _count;

    private const int DefaultCapacity = 16;
    private const float LoadFactorThreshold = 0.75f;

    public SimpleHashTable() : this(DefaultCapacity)
    {
        
    }

    public SimpleHashTable(int capacity = 16)
    {
        _buckets = new Entry[capacity];
    }

    // 핵심 헬퍼 메서드
    private int GetHash(TKey key)
    {
        int hash = key.GetHashCode();
        // size == _buckets.Length;
        return (hash & 0x7fffffff) % _buckets.Length;
    }

    public TValue this[TKey key] 
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException($"키 {key} 찾을 수 없음.");
            //throw new System.NotImplementedException(); 
        } 
        set
        {
            int index = GetHash(key);

            if (_buckets[index].isOccupied && EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, key))
            {
                Entry entry = _buckets[index];
                entry.Value = value;
                _buckets[index] = entry;
                return;
            }

            Add (key, value);
            //throw new System.NotImplementedException(); 
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keys = new List<TKey>(_count);
            foreach (Entry e in _buckets)
                if (e.isOccupied) keys.Add(e.Key);
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new List<TValue>(_count);
            foreach (Entry e in _buckets)
                if (e.isOccupied) values.Add(e.Value);
            return values;
        }
    }

    public int Count => _count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        // 로드 팩터 체크 : 요소 추가시 75%가 넘는지
        // float 명시적 변환 적용해서 비교 해야 0.75 비교 가능
        if ((float)(_count + 1) / _buckets.Length > LoadFactorThreshold)
        {
            // 배열 크기 늘리는 리사이징 함수 호출
            Resize();
        }
        // 현재 키가 들어갈 배열 인덱스 계산
        int index = GetHash(key);
        // 해당하는 자리가 차 있는지 확인
        if (_buckets[index].isOccupied)
        {
            // 같은 키 들어왔을 시 중복 키 예오 던지기
            if (EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, key))
            {
                throw new ArgumentException($"키 {key}는 이미 존재합니두~.", nameof(key)); 
            }
            // 다른 키이지만 같은 자리일 시 해시 충돌 예외 처리
            throw new InvalidOperationException($"해시 충돌: {key} -> 인덱스 {index}는 {_buckets[index].Key}가 점유 중.");
        }
        
        // 자리가 비어있을 시 저장
        _buckets[index] = new Entry {Key = key, Value = value, isOccupied = true};
        _count++;
        //throw new System.NotImplementedException();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        // item.Key / item.Value를 분해해서 기존 Add에 위임
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        // 모든 버킷을 default(Entry)로 초기화하고 카운트 리셋
        Array.Clear(_buckets, 0, _buckets.Length);
        _count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = GetHash(item.Key);

        // 키와 값 둘 다 일치해야 true (Remove(KVP)와 동일한 기준)
        return _buckets[index].isOccupied
            && EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, item.Key)
            && EqualityComparer<TValue>.Default.Equals(_buckets[index].Value, item.Value);
    }

    public bool ContainsKey(TKey key)
    {
        // TryGetValue가 탐색 로직을 전부 가지고 있으므로 위임
        // out _ : 값은 필요 없으므로 버림 (C# discard)
        return TryGetValue(key, out _);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _count) throw new ArgumentException("대상 배열이 너무 작습니다.");

        foreach (Entry e in _buckets)
            if (e.isOccupied)
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(e.Key, e.Value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        // yield return : 버킷을 순회하며 점유된 항목만 하나씩 반환
        foreach (Entry e in _buckets)
            if (e.isOccupied)
                yield return new KeyValuePair<TKey, TValue>(e.Key, e.Value);
    }

    public bool Remove(TKey key)
    {
        int index = GetHash(key);

        if (_buckets[index].isOccupied && EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, key))
        {
            _buckets[index].isOccupied = false;
            _count--;
            return true;
        }

        return false;

        //throw new System.NotImplementedException();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int index = GetHash(item.Key);

        if (_buckets[index].isOccupied && EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, item.Key) &&
            EqualityComparer<TValue>.Default.Equals(_buckets[index].Value, item.Value))
        {
            _buckets[index] = default;
            _count--;
            return true;
        }

        return false;

        //throw new System.NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = GetHash(key);

        if (_buckets[index].isOccupied && EqualityComparer<TKey>.Default.Equals(_buckets[index].Key, key))
        {
            value = _buckets[index].Value;
            return true;
        }

        value = default;
        return false;

        //throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Resize()
    {
        Entry[] old = _buckets;

        _buckets = new Entry[old.Length * 2];
        _count = 0;

        foreach (Entry e in old)
        {
            if (!e.isOccupied)
            {
                continue;
            }

            int newIndex = GetHash(e.Key);
            if (_buckets[newIndex].isOccupied)
            {
                throw new InvalidOperationException ($"해시 충돌 : {e.Key} -> 인덱스 {newIndex}는 {_buckets[newIndex].Key}가 점유 중.");
            }

            _buckets[newIndex] = e;
            _count++;
        }
    }
}
