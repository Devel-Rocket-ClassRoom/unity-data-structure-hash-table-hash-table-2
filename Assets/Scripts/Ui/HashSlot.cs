using UnityEngine;
using TMPro;

public class HashSlot : MonoBehaviour
{
    public TMP_Text indexText;
    public Transform Nodes; 
    public GameObject nodePrefab;

    // 당장은 쓸 일이 없을 것 같은데 혹시나 해서 만들어둠
    private int myIndex;

    // 인덱스 설정
    public void SetIndex(int index)
    {
        myIndex = index;
        indexText.text = $"I: {myIndex}";
    }

    // 데이터를 추가
    // [각자의 HashTable에서 이미 유효한 인덱스를 계산해준다고 가정]
    // [Chaining은 슬롯에 여러 노드를 연결해야됨, 다른 HashTable에서는 필요 없음]
    public void UpdateData(string key, string value, bool isChaining)
    {
        // Chaining이 아니면 기존 노드를 클리어함 (같은 키에 새로운 값을 넣는 경우도 생각함)
        if (!isChaining)
        {
            ClearNodes();
        }

        // 새 데이터 노드 생성 및 값 설정
        GameObject newNode = Instantiate(nodePrefab, Nodes);
        TMP_Text nodeTxt = newNode.GetComponentInChildren<TMP_Text>();
        if (nodeTxt != null)
        {
            nodeTxt.text = $"{key}:{value}";
        }
    }

    // 슬롯 비우기
    // [리사이징 해주는 코드가 있을 것이라고 예상하고 그냥 비움]
    public void ClearNodes()
    {
        foreach (Transform child in Nodes)
        {
            Destroy(child.gameObject);
        }
    }
}