using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class OpenAddressingTest : MonoBehaviour {
	
	private OpenAddressingHashTable<string, int> _table;
	
	private string randomChars = "abcdefghijklmnopqrstuvwxyz0123456789";

	[Header("=== 단축키 및 값 목록 ===")]
	[Header("=== Add ===")]
	[SerializeField] private string _addKey = "hello";
	[SerializeField] private int _addValue = 123;
	[SerializeField] private KeyCode _addKeyCode = KeyCode.Keypad1;
	[SerializeField] private KeyCode _addRandomKeyCode = KeyCode.Keypad2;

	[Header("=== Remove ===")]
	[SerializeField] private string _removeKey = "hello";
	[SerializeField] private KeyCode _removeKeyCode = KeyCode.Keypad3;

	[Header("=== ProbingMethod ===")]
	[SerializeField] private ProbingMethod _probingMethod = ProbingMethod.Linear;
	[SerializeField] private KeyCode _switchProbingMethodKey = KeyCode.Keypad4;

	// Awake에서 _table 초기화
	private void Awake() {
		_table = new OpenAddressingHashTable<string, int>();
	}


	private void Update() {
		
		// 지정된 키, 값을 삽입
		if (Input.GetKeyDown(_addKeyCode)) {
			_table.Add(_addKey, _addValue);
			Debug.Log($"Add : ({_addKey}, {_addValue}) 삽입");
			
			PrintTableState();
		}
		
		if (Input.GetKeyDown(_addRandomKeyCode)) {
			int keyLength = Random.Range(2, 5);
			StringBuilder key = new StringBuilder();
			for (int i = 0; i < keyLength; i++) {
				key.Append(randomChars[Random.Range(0, randomChars.Length)]);
			}
			int randomVal = Random.Range(0, 100);
			_table.Add(key.ToString(), randomVal);
			Debug.Log($"Add : ({key}, {randomVal}) 삽입");
			
			PrintTableState();
		}

		if (Input.GetKeyDown(_removeKeyCode)) {
			_table.Remove(_removeKey);
			Debug.Log($"Remove : 키가 {_removeKey}인 값 제거");
			
			PrintTableState();
		}
		
		if (Input.GetKeyDown(_switchProbingMethodKey)) {
			_table.SetProbingMethod(_probingMethod);
			Debug.Log($"Switch Method : 빈 해시 탐색 방식 {_probingMethod.ToString()}(으)로 변경");
			
			PrintTableState();
		}
	}
	
	private void PrintTableState() {
		OpenAddressingHashTable<string, int>.OpenBucket[] _buckets = _table.Table;
		Debug.Log($"현재 내부 테이블 크기 : {_table.Capacity}\n" +
		          $"실제로 저장된 데이터 개수 : {_table.Count}\n" +
		          $"내부 테이블 상태 : ");
		
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < _buckets.Length; i++) {
			sb.Append($"Index : {i}, Key : {_buckets[i].key}, Value : {_buckets[i].value}\n");
		}

		Debug.Log($"{sb}");
	}
}