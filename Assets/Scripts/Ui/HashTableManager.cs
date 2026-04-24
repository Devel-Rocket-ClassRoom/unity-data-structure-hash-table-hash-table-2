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
    public Button randomButton;

    private int currentCapacity;

    private int key = 0;
    private int value = 0;

    private TableType currentTableType;
    private ProbingType currentProbingType;

    private SimpleHashTable<int, int> simpleTable;
    private ChainingHashTable<int, int> chainingTable;
    private OpenAddressingHashTable<int, int> openAddressingTable;

    void Start()
    {
        currentCapacity = 16;

        tableDropdown.onValueChanged.AddListener(OnTableTypeChanged);
        probingDropdown.onValueChanged.AddListener(OnProbingTypeChanged);

        addButton.onClick.AddListener(OnAddClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);
        clearButton.onClick.AddListener(OnClearClicked);
        randomButton.onClick.AddListener(OnRandomClicked);

        OnTableTypeChanged(0);
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
            case TableType.Simple:
                simpleTable = new SimpleHashTable<int, int>();
                break;
            case TableType.Chaining:
                chainingTable = new ChainingHashTable<int, int>();
                chainingTable.OnReSize += SetCurrentCapacity;
                break;
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
        if (keyInput != null && !(int.TryParse(keyInput.text, out key) && int.TryParse(valueInput.text, out value)))
        {
            Debug.Log("Key와 Value에 숫자를 입력하세요.");
            return;
        }

        Debug.Log($"현재 테이블 {currentTableType}");

        bool isChaining = (currentTableType == TableType.Chaining);

        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                openAddressingTable.Add(key, value);
                break;
            case TableType.Chaining:
                chainingTable.Add(key, value);
                break;
            case TableType.Simple:
                simpleTable.Add(key, value);
                break;
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
                break;
            case TableType.Chaining:
                chainingTable.Remove(key);
                break;
            case TableType.Simple:
                simpleTable.Remove(key);
                break;
        }

        UpdateUI();
        Debug.Log("데이터 삭제 시도: " + keyInput.text);
    }

    private void OnClearClicked()
    {
        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                openAddressingTable.Clear();
                break;
            case TableType.Chaining:
                chainingTable.Clear();
                break;
            case TableType.Simple:
                simpleTable.Clear();
                break;
        }
        UpdateUI();
        Debug.Log("해시 테이블 초기화");
    }

    private void OnRandomClicked()
    {
        int key = Random.Range(0, 1000);
        int value = Random.Range(0, 1000);

        keyInput.text = key.ToString();
        valueInput.text = value.ToString();

        OnAddClicked();
    }

    private void UpdateUI()
    {
        hashTable.InitializeTable(currentCapacity);

        switch (currentTableType)
        {
            case TableType.OpenAddressing:
                var addressBuckets = openAddressingTable.Table;

                for (int i = 0; i < currentCapacity; i++)
                {
                    if (!addressBuckets[i].isEmpty)
                    {
                        hashTable.UpdateSlot(i, addressBuckets[i].key, addressBuckets[i].value, false);
                    }
                }

                break;
            case TableType.Chaining:
                for (int i = 0; i < chainingTable.Capacity; i++)
                {
                    var chainingBuckets = chainingTable.GetBuckets(i);

                    foreach (var bucket in chainingBuckets)
                    {
                        hashTable.UpdateSlot(i, bucket.Key, bucket.Value, true);
                    }
                }
                break;
            case TableType.Simple:
                var buckets = simpleTable.Buckets;

                for (int i = 0; i < currentCapacity; i++)
                {
                    if (buckets[i].isOccupied)
                    {
                        hashTable.UpdateSlot(i, buckets[i].Key, buckets[i].Value, false);
                    }
                }
                break;
        }
    }
}