using UnityEngine;

public class HashTableTest : MonoBehaviour
{
    [Header("연결할 HashTable 스크립트")]
    public HashTableUi hashTable; // Left 오브젝트에 있는 HashTable을 드래그 앤 드롭

    void Start()
    {
        // 1단계: 테이블 초기화 테스트 (10개의 슬롯 생성)
        Test_Initialize();

        // 2단계: 데이터 삽입 테스트 (단일 삽입 - Simple/OpenAddressing 방식 가정)
        Test_SingleInsert();

        // 3단계: 체이닝 삽입 테스트 (한 슬롯에 여러 데이터 삽입)
        Test_ChainingInsert();
    }

    // 10개의 빈 슬롯이 생기는지 확인
    void Test_Initialize()
    {
        if (hashTable != null)
        {
            hashTable.InitializeTable(10);
            Debug.Log("테스트: 10개의 슬롯 초기화 완료");
        }
    }

    // 특정 인덱스에 데이터가 하나만 들어가는지 확인
    void Test_SingleInsert()
    {
        // 2번 인덱스에 'A' 삽입
        hashTable.UpdateSlot(2, "Apple", "Red", false);

        // 2번 인덱스에 'B'를 다시 삽입했을 때 기존 'A'가 지워지고 'B'만 남는지 확인
        hashTable.UpdateSlot(2, "Banana", "Yellow", false);

        Debug.Log("테스트: 2번 슬롯에 단일 데이터(Banana) 삽입 완료");
    }

    // 특정 인덱스에 데이터가 누적(Chaining)되는지 확인
    void Test_ChainingInsert()
    {
        // 5번 인덱스에 데이터 3개 연속 삽입 (isChaining = true)
        hashTable.UpdateSlot(5, "Monster_A", "Level 1", true);
        hashTable.UpdateSlot(5, "Monster_B", "Level 10", true);
        hashTable.UpdateSlot(5, "Monster_C", "Level 99", true);

        Debug.Log("테스트: 5번 슬롯에 3개의 데이터 체이닝 완료");
    }

    // 인스펙터에서 우클릭으로 언제든 다시 테스트 가능
    [ContextMenu("다시 테스트하기")]
    public void ReRunTest()
    {
        Start();
    }
}