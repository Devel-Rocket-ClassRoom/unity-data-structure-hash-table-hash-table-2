메서드/프로퍼티	핵심 동작
Add(TKey, TValue)   로드팩터 → Resize → 충돌 시 예외 → 저장
Resize()	2배 배열 생성 → 전체 재해싱
TryGetValue	단일 인덱스 탐색, bool 반환
ContainsKey	TryGetValue에 위임
this[key] setter	같은 키면 값 업데이트, 없으면 Add 위임
Keys / Values	버킷 순회 후 점유된 항목만 리스트로 반환
Remove(TKey)	인덱스 탐색 → isOccupied = false
Remove(KVP)	키 + 값 둘 다 일치할 때만 제거
Contains(KVP)	키 + 값 둘 다 일치 확인
Add(KVP)	Add(key, value)에 위임
Clear()	Array.Clear + _count = 0
CopyTo	점유 항목을 외부 배열에 복사
GetEnumerator	yield return으로 점유 항목 순회