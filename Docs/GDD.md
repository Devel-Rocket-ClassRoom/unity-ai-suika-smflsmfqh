# 수박게임 GDD

> Unity 6000.3.15f1 / URP-2D
> 이 문서는 현재 구현된 상태를 기준으로 작성된 설계 문서입니다.

---

## 1. 게임 개요

**한 줄 요약**
같은 과일끼리 부딪히면 한 단계 큰 과일로 합쳐지는 물리 퍼즐. 통이 넘치기 전에 최대 점수를 노린다.

**코어 루프**
1. 상단에서 다음 과일을 좌우로 조준 → 클릭 또는 Space 키로 떨어뜨림
2. 물리로 낙하 → 같은 단계 과일끼리 충돌 시 머지(합체) → 점수 획득
3. 통 상단 라인을 일정 시간 초과해 점유하면 게임 오버 → 점수 표시 → 재시작

**플레이타임**
1세션 2~5분. 캐주얼.

---

## 2. 과일 단계 (11단계)

| Lv | 이름 | 반지름 (world unit) | 직경 | 점수 (머지 시) |
|----|------|---------------------|------|----------------|
| 0  | Cherry      | 0.25 | 0.50 | 1  |
| 1  | Strawberry  | 0.35 | 0.70 | 3  |
| 2  | Grape       | 0.45 | 0.90 | 6  |
| 3  | Tangerine   | 0.55 | 1.10 | 10 |
| 4  | Peach       | 0.65 | 1.30 | 15 |
| 5  | Apple       | 0.75 | 1.50 | 21 |
| 6  | Pair        | 0.90 | 1.80 | 28 |
| 7  | Peach       | 1.05 | 2.10 | 36 |
| 8  | Pineapple   | 1.20 | 2.40 | 45 |
| 9  | Melon       | 1.40 | 2.80 | 55 |
| 10 | Watermelon  | 1.60 | 3.20 | 66 |

- 드로퍼에서 스폰되는 과일은 **Lv 0~4** 중 랜덤
- 최대 단계(Lv 10) 과일끼리 머지해도 추가 승급 없음 (합체 시도 자체를 건너뜀)
- 모든 과일 데이터는 `GameManager.Configs[]` 정적 배열에 인라인 정의

---

## 3. 핵심 메커닉

### 3.1 드롭 (Drop)

- 화면 상단에 다음에 떨어질 과일 미리보기 (`UIManager.ShowNextFruit`)
- 마우스 X좌표를 따라 과일이 스폰 라인 위에서 좌우 이동
- 클램프 범위: `minX = -2.2`, `maxX = 2.2`
- 스폰 Y 위치: `spawnY = 5.0`
- 클릭 또는 Space 키 → `Fruit.Drop()` 호출 → Dynamic 전환, 중력 활성화
- 드롭 쿨다운: **1.0초** (다음 과일 준비까지 대기)
- 드롭 전 과일: `Kinematic`, `CollisionEnabled=false` (조준 중 다른 과일과 충돌 없음)

### 3.2 물리 (Physics)

- 모든 과일: `Rigidbody2D` + `CircleCollider2D`
- Gravity Scale: **2.0**
- Collider radius: `0.5` (localScale으로 크기 조절)
- 머지가 아닌 일반 충돌 시 상대속도 비례 미세 반발력 적용:
  - `bounceSpeedRef = 6f` — 최대 힘에 도달하는 기준 속도
  - `minBounceForce = 0.2f`
  - `maxBounceForce = 2.0f`
  - 목적: 과일끼리 끼임 현상 방지
- 통(Container): 좌/우/바닥 `BoxCollider2D` 3개

### 3.3 머지 (Merge)

1. `OnCollisionEnter2D` 진입
2. 상대방이 같은 Level + 둘 다 `IsActive=true` + 둘 다 `isMerging=false` 조건 확인
3. 두 과일 모두 `isMerging=true` 플래그 설정 (이중 머지 방지)
4. 중점 `(A.pos + B.pos) / 2` 에 `Level+1` 과일 스폰
5. 두 과일 모두 `Release` (풀 반환)
6. 점수 추가: 결과 단계의 `score` 값

### 3.4 게임 오버 (Game Over)

- `GameOverDetector`가 매 프레임 `Fruit.ActiveFruits` 순회
- 위험선 Y: `dangerY`, Celling Transform의 위치
- `dangerY` 이상에 과일이 존재하면 `dangerTimer` 누적
- 과일이 없으면 `dangerTimer = 0` 리셋
- 누적 시간이 **3초** (`gracePeriod`) 초과 시 `GameManager.TriggerGameOver()` 호출
- 씬 로드 직후 **2초** (`startDelay`) 동안은 검사 건너뜀

---

## 4. 점수

- 머지 발생 시: 결과 단계의 `score` 값 즉시 누적
- 누적 점수는 `GameManager.Score` (int)
- 변경 시 `OnScoreChanged(int)` 이벤트 발행 → UIManager가 구독해 화면 갱신
- 하이스코어 저장 없음 (세션 내 점수만 표시)

---

## 5. 화면 · UI 구성

```
┌──────────────────────────────────────┐
│  Score: 1250        Next: Grape      │  ← Top HUD
│  ──────── 위험선 (dangerY)-------──── │
│                                      │
│         [통 영역]                    │
│                                      │
│       Apple  Peach                   │
│   Cherry       Grape  Strawberry     │
│  ────────────────────────────────    │
└──────────────────────────────────────┘
         ▲ 조준 과일 (마우스 따라감)
```

**Canvas (Screen Space Overlay)**
- `ScoreText` (TMP): 좌측 상단 — `Score: {N:N0}`
- `NextLabel` (TMP): 우측 상단 — `Next: {fruitName}`
- `GameOverPanel`: 게임 오버 시 표시
  - `FinalScoreText` (TMP): `Final Score\n{score:N0}`
  - `RestartButton`: 클릭 시 `GameManager.RestartGame()` 호출 (씬 리로드)

---

## 6. 오브젝트 풀링

- `FruitSpawner`가 `Queue<Fruit>` 풀 관리
- 과일 생성: 풀에서 꺼내 `Init(level)` → `gameObject.SetActive(true)`
- 과일 소멸: `Fruit.Deactivate()` → `gameObject.SetActive(false)` → 풀에 반환
- 초기 풀 크기: 0 (요청 시 `Instantiate`, 이후 재사용)
- 풀 우회 금지: 과일 생성/소멸은 반드시 `GetFromPool` / `Release` 경유

---

## 7. 기술 스택 / 구조

- **Unity**: 6000.3.15f1, URP-2D 템플릿
- **언어**: C# (CSharpier 포맷 — 트레일링 콤마, 인자별 라인 분할)
- **물리**: Built-in Physics 2D

**씬**: `Assets/Scenes/Main.unity` (단일 씬)

**스크립트 구조**
```
Assets/Scripts/
  GameManager.cs        싱글턴, FruitConfig 정적 배열, 원형 Sprite 런타임 생성
  Fruit.cs              과일 컴포넌트 (물리/충돌/머지), ActiveFruits 정적 리스트
  FruitSpawner.cs       Queue 풀링, 마우스 조준, 클릭/Space 드롭
  UIManager.cs          점수 / 다음 과일 / 게임오버 패널 / 재시작 버튼
  GameOverDetector.cs   ActiveFruits 스캔으로 위험선 체류 시간 감지
```

**프리팹 구성**
```
Assets/Prefabs/
  Fruit.prefab              Rigidbody2D + CircleCollider2D + SpriteRenderer + Fruit.cs
  GameManager_GO.prefab
  FruitSpawner_GO.prefab    fruitPrefab 슬롯에 Fruit.prefab 연결
  UIManager_GO.prefab
  GameOverDetector_GO.prefab
  Canvas.prefab             ScoreText / NextLabel / GameOverPanel / RestartButton
  Container.prefab          좌·우·바닥 BoxCollider2D
  Background.prefab
```

**싱글턴 접근자**

| 클래스 | 정적 접근자 |
|--------|------------|
| `GameManager` | `GameManager.gameManager` |
| `FruitSpawner` | `FruitSpawner.fruitSpawner` |
| `UIManager` | `UIManager.uiManager` |

---

## 8. 튜닝 파라미터 (Inspector 노출)

| 파라미터 | 컴포넌트 | 현재 값 | 설명 |
|----------|----------|---------|------|
| `spawnY` | FruitSpawner | 5.0 | 과일 생성 Y 위치 |
| `minX / maxX` | FruitSpawner | -2.2 / 2.2 | 조준 범위 클램프 |
| `dropCooldown` | FruitSpawner | 1.0s | 드롭 후 다음 과일까지 대기 |
| `dangerY` | GameOverDetector | 3.8 | 게임오버 위험선 Y |
| `gracePeriod` | GameOverDetector | 3.0s | 위험선 초과 허용 시간 |
| `startDelay` | GameOverDetector | 2.0s | 씬 시작 후 검사 면제 시간 |
| `bounceSpeedRef` | Fruit | 6.0 | 반발력 보간 기준 속도 |
| `minBounceForce` | Fruit | 0.2 | 최소 반발 Impulse |
| `maxBounceForce` | Fruit | 2.0 | 최대 반발 Impulse |
| Gravity Scale | Fruit.Drop() | 2.0 | 드롭 후 중력 배율 |

---

## 9. Inspector 연결 조건

`Main.unity` 씬에서 다음 참조가 연결되어 있어야 정상 동작한다:

- `FruitSpawner.fruitPrefab` → `Prefabs/Fruit.prefab`
- `UIManager.scoreText` → Canvas/ScoreText
- `UIManager.nextFruitLabel` → Canvas/NextLabel
- `UIManager.gameOverPanel` → Canvas/GameOverPanel
- `UIManager.finalScoreText` → Canvas/GameOverPanel/FinalScoreText
- `UIManager.restartButton` → Canvas/GameOverPanel/RestartButton
- `RestartButton.OnClick` 바인딩은 코드(`AddListener`)로 처리 — 인스펙터 설정 불필요
