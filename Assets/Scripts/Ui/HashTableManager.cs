using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TableType { Simple, Chaining, OpenAddressing }
public enum ProbingType { Linear, Quadratic, DoubleHash }

public class HashTableManager : MonoBehaviour
{
    public HashTableUi uiVisualizer;

    public TMP_Dropdown tableDropdown;
    public TMP_Dropdown probingDropdown;

    public TMP_InputField keyInput;
    public TMP_InputField valueInput;

    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    private int currentCapacity = 16;
    private TableType currentTableType;
    private ProbingType currentProbingType;

    // private SimpleHashTable<string, string> simpleTable;
    // private ChainingHashTable<string, string> chainingTable;
    // private OpenAddressingHashTable<string, string> openAddressingTable;

    void Start()
    {
        // 1. 드롭다운 초기값 설정 및 이벤트 연결
        tableDropdown.onValueChanged.AddListener(OnTableTypeChanged);
        probingDropdown.onValueChanged.AddListener(OnProbingTypeChanged);

        // 2. 버튼 이벤트 연결
        addButton.onClick.AddListener(OnAddClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);
        clearButton.onClick.AddListener(OnClearClicked);

        // 3. 초기 화면 생성
        OnTableTypeChanged(0);
    }

    // 드롭다운 1: 테이블 종류 선택
    public void OnTableTypeChanged(int index)
    {
        currentTableType = (TableType)index;

        // OpenAddressing일 때만 Probing 드롭다운 활성화
        probingDropdown.interactable = (currentTableType == TableType.OpenAddressing);

        // 테이블 종류를 바꿔도 currentCapacity는 유지하며 UI 초기화
        uiVisualizer.InitializeTable(currentCapacity);

        // TODO: 팀원의 해시 테이블 클래스도 새롭게 인스턴스화 (currentCapacity 전달)
        Debug.Log($"{currentTableType} 테이블로 전환. 크기: {currentCapacity}");
    }

    // 드롭다운 2: 프로빙 방식 선택
    public void OnProbingTypeChanged(int index)
    {
        currentProbingType = (ProbingType)index;
        Debug.Log($"프로빙 방식 변경: {currentProbingType}");
    }

    private void OnAddClicked()
    {
        string k = keyInput.text;
        string v = valueInput.text;

        if (string.IsNullOrEmpty(k)) return;

        // [흐름 예시]
        // 1. 팀원 로직 호출: int index = currentTable.GetHashIndex(k);
        // 2. 가상 인덱스 테스트 (예시로 5번 인덱스라고 가정)
        int targetIndex = Random.Range(0, currentCapacity);
        bool isChaining = (currentTableType == TableType.Chaining);

        uiVisualizer.UpdateSlot(targetIndex, k, v, isChaining);

        Debug.Log($"데이터 추가 시도: {k}:{v} -> Index: {targetIndex}");
    }

    private void OnRemoveClicked()
    {
        // 삭제 로직 구현 (팀원 함수 호출 후 UI 갱신)
        Debug.Log("데이터 삭제 시도: " + keyInput.text);
    }

    private void OnClearClicked()
    {
        // 전체 클리어
        uiVisualizer.InitializeTable(currentCapacity);
        Debug.Log("해시 테이블 초기화");
    }

}