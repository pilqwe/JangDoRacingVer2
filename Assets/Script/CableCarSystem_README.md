# 🚡 케이블카 시스템 사용 가이드

이 시스템은 Unity 레이싱 게임에서 케이블카가 자동으로 스폰되고 이동하며 사라지는 환경 요소를 제공합니다.

## 📁 포함된 스크립트

1. **CableCarSystem.cs** - 메인 케이블카 관리 시스템
2. **CableCarController.cs** - 개별 케이블카 제어
3. **CableCarUIDisplay.cs** - 디버그 UI 표시
4. **CableCarEffects.cs** - 시각적/청각적 효과

## 🛠️ 설정 방법

### 1단계: 케이블카 프리팹 준비

1. Sketchfab에서 케이블카 모델을 다운로드하여 Unity에 Import
2. 케이블카 모델을 Prefab으로 만들기
3. 필요시 Collider와 Rigidbody 컴포넌트 추가

### 2단계: 케이블카 시스템 설정

1. 빈 GameObject 생성 후 "CableCarSystem"으로 이름 변경
2. `CableCarSystem.cs` 스크립트 추가
3. Inspector에서 다음 설정:
   - **Cable Car Prefab**: 준비한 케이블카 프리팹 할당
   - **Player Transform**: 플레이어 객체 할당 (scooterCtrl 오브젝트)
   - **Move Speed**: 케이블카 이동 속도 (기본 5)
   - **Spawn Interval**: 스폰 간격 초 (기본 10초)
   - **Max Cable Cars**: 최대 동시 케이블카 수 (기본 3)

### 3단계: 웨이포인트 설정

1. 케이블카가 이동할 경로에 빈 GameObject들을 배치
2. 이들을 "Waypoint_1", "Waypoint_2", ... 형태로 이름 지정
3. CableCarSystem의 Waypoints 배열에 순서대로 할당
4. 웨이포인트는 높은 곳에 배치하여 케이블카처럼 보이게 설정

### 4단계: UI 디스플레이 추가 (선택사항)

1. UI Canvas에 빈 GameObject 생성
2. `CableCarUIDisplay.cs` 스크립트 추가
3. 게임 실행 후 F2로 UI 표시/숨김 가능

### 5단계: 이펙트 추가 (선택사항)

1. 케이블카 프리팹에 `CableCarEffects.cs` 스크립트 추가
2. 파티클 시스템, 조명, 오디오 소스 등을 설정

## 🎮 조작법

### 기본 기능

- **자동 스폰**: 설정된 간격으로 케이블카가 자동 생성
- **경로 이동**: 웨이포인트를 따라 부드럽게 이동
- **자동 제거**: 경로 끝 도달 또는 플레이어와 거리가 멀어지면 제거

### 디버그 조작 (CableCarUIDisplay 사용시)

- **F2**: 디버그 UI 표시/숨김
- **F3**: 수동으로 케이블카 스폰
- **F4**: 모든 케이블카 제거

## ⚙️ 상세 설정 옵션

### CableCarSystem 설정

```csharp
[Header("케이블카 설정")]
public GameObject cableCarPrefab;           // 케이블카 프리팹
public Transform[] waypoints;               // 이동 경로
public float moveSpeed = 5f;                // 이동 속도
public float spawnInterval = 10f;           // 스폰 간격
public int maxCableCars = 3;                // 최대 케이블카 수

[Header("스폰 설정")]
public bool autoSpawn = true;               // 자동 스폰 활성화
public float despawnDistance = 50f;         // 제거 거리
public Transform playerTransform;           // 플레이어 Transform

[Header("애니메이션 설정")]
public bool swayAnimation = true;           // 흔들림 애니메이션
public float swayAmount = 0.5f;             // 흔들림 크기
public float swaySpeed = 1f;                // 흔들림 속도
```

### CableCarEffects 설정

```csharp
[Header("파티클 이펙트")]
public ParticleSystem[] smokeEffects;      // 연기 효과
public ParticleSystem[] sparkEffects;     // 스파크 효과

[Header("라이트 이펙트")]
public Light[] cableCarLights;            // 조명
public bool flickerLights = true;         // 깜빡임 효과

[Header("사운드 이펙트")]
public AudioSource movementAudio;         // 이동 소리
public AudioClip[] randomSounds;          // 랜덤 효과음
```

## 💡 사용 팁

1. **웨이포인트 배치**: 실제 케이블카처럼 보이도록 높은 곳에 일정한 간격으로 배치
2. **성능 최적화**: maxCableCars를 적절히 설정하여 성능 관리
3. **사운드 설정**: 3D 사운드로 설정하여 거리감 표현
4. **파티클 효과**: 케이블카 하단에 연기 효과를 추가하여 현실감 증대
5. **조명 효과**: 야간 씬에서 케이블카 조명으로 분위기 연출

## 🔧 문제 해결

### 케이블카가 스폰되지 않는 경우

- 케이블카 프리팹이 할당되었는지 확인
- 웨이포인트가 2개 이상 설정되었는지 확인
- maxCableCars 수가 이미 도달했는지 확인

### 케이블카가 이상하게 움직이는 경우

- 웨이포인트 순서가 올바른지 확인
- moveSpeed 값이 적절한지 확인
- 케이블카 프리팹에 불필요한 물리 컴포넌트가 없는지 확인

### 성능 문제가 있는 경우

- maxCableCars 수를 줄이기
- despawnDistance를 줄여서 더 빨리 제거되도록 설정
- 파티클 효과의 개수나 강도 조절

## 📝 커스터마이징

이 시스템은 쉽게 확장 가능합니다:

1. **새로운 이펙트 추가**: CableCarEffects.cs에 원하는 효과 추가
2. **다양한 케이블카**: 여러 종류의 케이블카 프리팹 사용
3. **인터랙션 추가**: 플레이어와의 상호작용 기능 추가
4. **AI 패턴**: 더 복잡한 이동 패턴 구현

즐거운 게임 개발 되세요! 🎮
