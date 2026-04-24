using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TableType { Simple, Chaining, OpenAddressing }
public enum ProbingType { Linear, Quadratic, DoubleHash }

public class HashTableManager : MonoBehaviour
{
    public HashTableUi hashTable;

    public TMP_Dropdown tableDropdown;
    public TMP_Dropdown probingDropdown;

    public TMP_InputField keyInput;
    public TMP_InputField valueInput;

    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    private int currentCapacity;

    private int key = 0;
    private int value = 0;

    private TableType currentTableType;
    private ProbingType currentProbingType;

    // private SimpleHashTable<string, string> simpleTable;
    // private ChainingHashTable<string, string> chainingTable;
    private OpenAddressingHashTable<int, int> openAddressingTable;

    void Start()
    {
        currentCapacity = 16;

        tableDropdown.onValueChanged.AddListener(OnTableTypeChanged);
        probingDropdown.onValueChanged.AddListener(OnProbingTypeChanged);

        addButton.onClick.AddListener(OnAddClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);
        clearButton.onClick.AddListener(OnClearClicked);

        // 3. 초기 화면 생성
        OnTableTypeChanged(2);
    }

    public void OnTableTypeChanged(int index)
    {
        currentTableType = (TableType)index;

        probingDropdown.interactable = (currentTableType == TableType.OpenAddressing);

        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                openAddressingTable = new OpenAddressingHashTable<int, int>();
                openAddressingTable.OnResize += SetCurrentCapacity;
                break;
                //case TableType.Simple:
                //    currentTable = new SimpleHashTable<int, int>();
                //    break;
                //case TableType.Chaining:
                //    currentTable = new ChainingHashTable<int, int>();
                //    break;
        }

        hashTable.InitializeTable(currentCapacity);

        Debug.Log($"{currentTableType} 테이블로 전환. 크기: {currentCapacity}");
    }

    private void SetCurrentCapacity(int val) {
        currentCapacity = val;
    }

    public void OnProbingTypeChanged(int index)
    {
        currentProbingType = (ProbingType)index;
        Debug.Log($"프로빙 방식 변경: {currentProbingType}");
        openAddressingTable.SetProbingType(currentProbingType);
        UpdateUI();
    }

    private void OnAddClicked()
    {
        if (keyInput != null && !(int.TryParse(keyInput.text, out key) || !int.TryParse(valueInput.text, out value)))
        {
            Debug.Log("Key와 Value에 숫자를 입력하세요.");
            return;
        }

        // [흐름 예시]
        // 1. 팀원 로직 호출: int index = currentTable.GetHashIndex(k);
        // 2. 가상 인덱스 테스트 (예시로 5번 인덱스라고 가정)

        Debug.Log($"현재 테이블 {currentTableType}");

        bool isChaining = (currentTableType == TableType.Chaining);

        //int targetIndex = 0;

        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                openAddressingTable.Add(key, value);
                Debug.Log("ADD");
                PrintTableState();
                //targetIndex = openAddressingTable.CurrRefIndex;
                //hashTable.UpdateSlot(targetIndex, key, value, isChaining);
                break;
            //case TableType.Chaining:
            //    openAddressingTable.Add(key, value);
            //    targetIndex = openAddressingTable.CurrRefIndex;
            //    hashTable.UpdateSlot(targetIndex, key, value, isChaining);
            //    break;
            //case TableType.OpenAddressing:
            //    openAddressingTable.Add(key, value);
            //    targetIndex = openAddressingTable.CurrRefIndex;
            //    hashTable.UpdateSlot(targetIndex, key, value, isChaining);
            //    break;
        }

        UpdateUI();

        keyInput.text = string.Empty;
        valueInput.text = string.Empty;
    }

    private void OnRemoveClicked()
    {
        if (keyInput != null && !(int.TryParse(keyInput.text, out key)))
        {
            Debug.Log("Key와 Value에 숫자를 입력하세요.");
            return;
        }

        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                openAddressingTable.Remove(key);
                //targetIndex = openAddressingTable.CurrRefIndex;
                //hashTable.UpdateSlot(targetIndex, key, value, isChaining);
                break;
                //case TableType.Chaining:
                //    openAddressingTable.Add(key, value);
                //    targetIndex = openAddressingTable.CurrRefIndex;
                //    hashTable.UpdateSlot(targetIndex, key, value, isChaining);
                //    break;
                //case TableType.OpenAddressing:
                //    openAddressingTable.Add(key, value);
                //    targetIndex = openAddressingTable.CurrRefIndex;
                //    hashTable.UpdateSlot(targetIndex, key, value, isChaining);
                //    break;
        }

        UpdateUI();

        Debug.Log("데이터 삭제 시도: " + keyInput.text);
    }

    private void OnClearClicked()
    {
        openAddressingTable.Clear();
        UpdateUI();
        Debug.Log("해시 테이블 초기화");
    }

    private void UpdateUI()
    {
        hashTable.InitializeTable(currentCapacity);

        if (currentTableType == TableType.OpenAddressing && openAddressingTable != null)
        {
            Debug.Log("UI갱신");
            PrintTableState();
            var buckets = openAddressingTable.Table;

            // 예: 테이블의 모든 칸을 돌면서 데이터가 있는 곳만 그리기
            for (int i = 0; i < currentCapacity; i++)
            {
                if (!buckets[i].isEmpty)
                {
                    hashTable.UpdateSlot(i, buckets[i].key, buckets[i].value, false);
                }
                //// 팀원 코드에서 해당 인덱스에 데이터가 있는지 확인하는 메서드가 필요함
                //if (openAddressingTable.HasDataAt(i))
                //{
                //    var entry = openAddressingTable.GetEntryAt(i);
                //    // 바뀐 인덱스(i)에 맞춰 노드를 생성함
                //    hashTable.UpdateSlot(i, entry.Key, entry.Value, false);
                //}
            }
        }
    }

    private void PrintTableState()
    {
        OpenAddressingHashTable<int, int>.OpenBucket[] _buckets = openAddressingTable.Table;
        Debug.Log($"현재 내부 테이블 크기 : {openAddressingTable.Capacity}\n" +
                  $"실제로 저장된 데이터 개수 : {openAddressingTable.Count}\n" +
                  $"내부 테이블 상태 : ");

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _buckets.Length; i++)
        {
            sb.Append($"Index : {i}, Key : {_buckets[i].key}, Value : {_buckets[i].value}\n");
        }

        Debug.Log($"{sb}");
    }
}