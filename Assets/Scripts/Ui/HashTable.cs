using UnityEngine;
using System.Collections.Generic;

public class HashTableUi : MonoBehaviour
{
    public Transform contentTransform; 
    public GameObject slotPrefab;      

    private List<HashSlot> uiSlots = new List<HashSlot>();

    // 지정된 크기만큼 슬롯을 초기에 생성
    // [capacity를 받아서 지정된 size만큼 배열이나 리스트를 반환해주는 코드가 있을 것이라고 예상]
    public void InitializeTable(int size)
    {
        // 기존에 생성된 슬롯이 있다면 제거
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        // 테이블 사이즈만큼 슬롯 프리팹 생성
        for (int i = 0; i < size; i++)
        {
            GameObject go = Instantiate(slotPrefab, contentTransform);
            HashSlot slotScript = go.GetComponent<HashSlot>();

            if (slotScript != null)
            {
                slotScript.SetIndex(i);
                uiSlots.Add(slotScript);
            }
        }
    }

    // 특정 인덱스의 슬롯을 찾아 데이터를 갱신할 때 사용
    // [해시값으로 유효한 인덱스를 계산해서 넘겨주는 코드가 있을 것이라고 예상]
    public void UpdateSlot(int index, string key, string value, bool isChaining)
    {
        if (index >= 0 && index < uiSlots.Count)
        {
            uiSlots[index].UpdateData(key, value, isChaining);
        }
    }
}