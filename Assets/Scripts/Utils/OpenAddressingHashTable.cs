using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenAddressingHashTable<TKey, TValue> : IDictionary<TKey, TValue> {
    public Action<int> OnResize;

    public struct OpenBucket
    {
        public bool isEmpty;
        public bool isDeleted;
        public TKey key;
        public TValue value;

        public void SetValue(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
            isEmpty = false;
        }

        public void DeleteValue()
        {
            key = default;
            value = default;
            isDeleted = true;
            isEmpty = true;
        }

        public void ClearBucket()
        {
            key = default;
            value = default;
            isEmpty = true;
            isDeleted = false;
        }
    }

    private const int k_InitialSize = 16;
    private int _count;
    private OpenBucket[] _table;
    private ProbingType _probingType;

    public int CurrRefIndex { get; private set; }

    // 정렬 방식 바꾸면 리사이징처럼 버킷 재계산
    private ProbingType ProbingType
    {
        set
        {
            _probingType = value;

            // 새 해시테이블 생성
            OpenBucket[] newBucket = new OpenBucket[Capacity];
            for (int i = 0; i < newBucket.Length; i++) { newBucket[i].ClearBucket(); }

            // 이미 있는 값을 새 테이블로 이전
            for (int i = 0; i < _table.Length; i++)
            {
                if (_table[i].isEmpty) { continue; }
                AddWithCollision(_table[i].key, _table[i].value, newBucket);
            }

            // 새 테이블 적용
            _table = newBucket;
        }
    }

    // 현재 내부 배열 크기 (size)
    public int Capacity
    {
        get => _table.Length;
    }

    // 현재 버킷 내에 몇 개의 값이 존재?
    public int Count
    {
        get => _count;
        private set => _count = value;
    }

    private float LoadFactor => (float)Count / Capacity;
    private bool NeedsResizing => LoadFactor >= 0.6f;

    // 기본 Probing 방식은 Linear
    public OpenAddressingHashTable() : this(ProbingType.Linear) { }

    public OpenAddressingHashTable(ProbingType type)
    {
        _count = 0;
        _table = new OpenBucket[k_InitialSize];
        _probingType = type;
        
        Clear();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var entry in _table)
        {
            if (!entry.isEmpty)
            {
                yield return new KeyValuePair<TKey, TValue>(entry.key, entry.value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(TKey key, TValue value)
    {
        // 지금 내 테이블에 값 Add
        AddWithCollision(key, value, _table);

        // 로드팩터 검사 후 필요하다면 리사이징
        if (NeedsResizing)
        {
            Resize();
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    private void Resize() {
        // Capacity가 2배로 증가된 새 해시테이블 생성
        OpenBucket[] newBucket = new OpenBucket[Capacity * 2];
        for (int i = 0; i < newBucket.Length; i++) { newBucket[i].ClearBucket(); }

        // 이미 있는 값을 새 테이블로 이전
        for (int i = 0; i < _table.Length; i++)
        {
            if (_table[i].isEmpty) { continue; }
            AddWithCollision(_table[i].key, _table[i].value, newBucket);
        }

        // 새 테이블 적용
        _table = newBucket;

        // 리사이징 함수 호출
        OnResize?.Invoke(Capacity);
    }

    public int GetHash(TKey key, int tryCount = 1)
    {
        if (key == null)
        {
            throw new ArgumentNullException($"키 {key}가 null입니다.");
        }

        int hash = (key.GetHashCode() & 0x7fffffff) % Capacity;

        return tryCount switch
        {
            // 0번째 시도라면 기본 해시 반환
            < 1 => hash,

            // 한번 충돌이 발생했다면, 방법에 따라 다음 Hash 반환
            _ => _probingType switch
            {
                ProbingType.Linear => (hash + tryCount) % Capacity,
                ProbingType.Quadratic => (hash + (int)Mathf.Pow(tryCount, 2)) % Capacity,
                ProbingType.DoubleHash => (hash + tryCount).GetHashCode() % Capacity,
                _ => throw new InvalidOperationException()
            }
        };
    }

    public void Clear()
    {
        for (int i = 0; i < Capacity; i++) { _table[i].ClearBucket(); }
        Count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out _);
    }

    /// <summary>
    /// array의 arrayIndex부터 내가 가진 모든 원소를 저장
    /// </summary>  
    /// <param name="array">저장될 배열</param>
    /// <param name="arrayIndex">저장을 시작할 index</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        for (int i = 0; i < _table.Length; i++)
        {
            if (!_table[i].isEmpty)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(_table[i].key, _table[i].value);
            }
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public bool Remove(TKey key)
    {
        // 나의 데이터 찾기
        int tryCount = 0;
        do
        {
            int hash = GetHash(key, tryCount++);

            // 버킷이 비어있고, 삭제된 값도 아니라면 들어온 적 없는 값이다. false 반환
            if (_table[hash].isEmpty && !_table[hash].isDeleted)
            {
                return false;
            }

            // 버킷만 비어있다면, 값이 사라진 것이니 바로 다음으로 이동
            if (_table[hash].isEmpty) { continue; }

            // 값이 있다면, 비교하고 삭제 결정
            if (_table[hash].key.Equals(key))
            {
                _table[hash].DeleteValue();
                Count--;
                CurrRefIndex = hash;
                return true;
            }
        } while (tryCount < Capacity);
        
        return false;
    }

    public bool IsReadOnly => false;

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        // 나의 데이터 찾기
        int tryCount = 0;
        do
        {
            int hash = GetHash(key, tryCount++);

            // 버킷이 비어있고, 삭제된 값도 아니라면 들어온 적 없는 값이다. false 반환
            if (_table[hash].isEmpty && !_table[hash].isDeleted)
            {
                value = default;
                return false;
            }

            // 버킷만 비어있다면, 값이 사라진 것이니 바로 다음으로 이동
            if (_table[hash].isEmpty) { continue; }

            // 값이 있다면, 비교하고 반환 여부 결정
            if (_table[hash].key.Equals(key))
            {
                value = _table[hash].value;
                return true;
            }
        } while (tryCount < Capacity);
        
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue val))
            {
                // 값이 있다면, 그대로 반환
                return val;
            }
            
            // 값이 없다면, Exception
            throw new KeyNotFoundException();
        }
        set
        {
            AddWithoutCollision(key, value, _table);

            // 로드팩터 검사 후 필요하다면 리사이징
            if (NeedsResizing)
            {
                Resize();
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keys = new List<TKey>();

            for (int i = 0; i < _table.Length; i++)
            {
                if (!_table[i].isEmpty) { keys.Add(_table[i].key); }
            }

            return keys;
        }
    }
    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new List<TValue>();

            for (int i = 0; i < _table.Length; i++)
            {
                if (!_table[i].isEmpty) { values.Add(_table[i].value); }
            }

            return values;
        }
    }

    // @@@@@@@@@@@@ 테스트용 함수 @@@@@@@@@@@@@
    public OpenBucket[] Table => _table;
    public void SetProbingType(ProbingType type)
    {
        ProbingType = type;
    }


    /// <summary>
    /// 특정 테이블에 값을 넣는 함수. Add 및 Resizing시 사용
    /// </summary>
    /// <param name="key">들어갈 키</param>
    /// <param name="value">들어갈 값</param>
    /// <param name="table">넣고자 하는 해시테이블</param>
    private void AddWithCollision(TKey key, TValue value, OpenBucket[] table)
    {
        int tryCount = 0;
        int firstTombstone = -1;
        bool addCountFlag = table == _table;

        do
        {
            // 해시값 가져오기
            int hash = GetHash(key, tryCount++);

            // 툼스톤 자리를 찾았다면, 일단 넘어가기
            if (_table[hash].isEmpty && _table[hash].isDeleted)
            {
                // 첫 툼스톤 자리만 저장함.
                if (firstTombstone == -1)
                {
                    firstTombstone = hash;
                }
                continue;
            }

            // 빈 자리를 찾았다면, 삽입 후 종료
            if (table[hash].isEmpty)
            {
                // 빈 툼스톤 자리가 있었다면, 거기다 저장
                if (firstTombstone != -1)
                {
                    table[firstTombstone].SetValue(key, value);
                    CurrRefIndex = firstTombstone;
                }
                else
                {
                    CurrRefIndex = hash;
                    table[hash].SetValue(key, value);
                }

                // 현재 내 테이블에 추가했다면, Count 증가
                if (addCountFlag) { Count++; }

                break;
            }

            // 이미 존재하는 키였다면, Exception
            if (!table[hash].isEmpty && table[hash].key.Equals(key))
            {
                throw new ArgumentException($"키 {key}는 이미 존재하는 값입니다.");
            }

        } while (true);
    }

    /// <summary>
    /// 특정 테이블에 값을 넣는 함수. Add 및 Resizing시 사용
    /// </summary>
    /// <param name="key">들어갈 키</param>
    /// <param name="value">들어갈 값</param>
    /// <param name="table">넣고자 하는 해시테이블</param>
    private void AddWithoutCollision(TKey key, TValue value, OpenBucket[] table)
    {
        int tryCount = 0;
        int firstTombstone = -1;
        bool addCountFlag = table == _table;

        do
        {
            // 해시값 가져오기
            int hash = GetHash(key, tryCount++);

            // 툼스톤 자리를 찾았다면, 일단 넘어가기
            if (table[hash].isEmpty && table[hash].isDeleted)
            {
                // 첫 툼스톤 자리만 저장함.
                if (firstTombstone == -1)
                {
                    firstTombstone = hash;
                }
                continue;
            }

            // 빈 자리를 찾았다면, 삽입 후 종료
            if (table[hash].isEmpty)
            {
                // 빈 툼스톤 자리가 있었다면, 거기다 저장
                if (firstTombstone != -1)
                {
                    CurrRefIndex = firstTombstone;
                    table[firstTombstone].SetValue(key, value);
                }
                else
                {
                    CurrRefIndex = hash;
                    table[hash].SetValue(key, value);
                }

                // 현재 내 테이블에 추가했다면, Count 증가
                if (addCountFlag) { Count++; }

                break;
            }

            // 이미 존재하는 키였다면, 값 갱신
            if (!table[hash].isEmpty && table[hash].key.Equals(key))
            {
                table[hash].SetValue(key, value);
                CurrRefIndex = hash;
            }

        } while (true);
    }
}
