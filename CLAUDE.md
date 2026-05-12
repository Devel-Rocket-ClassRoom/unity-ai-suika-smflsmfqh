# 수박게임 (Suika Game)

2D 수박게임. 같은 레벨 과일을 충돌시켜 더 큰 과일로 합치는 물리 퍼즐. Unity 6.3 URP-2D.

---

## Tech Stack

- **Unity** 6000.3.15f1 / URP-2D 템플릿
- **C#** — CSharpier 포맷 적용 (트레일링 콤마, 인자별 라인 분할)
- Physics 2D (Rigidbody2D, CircleCollider2D)
- TextMeshPro UI / Screen Space Overlay Canvas

---

## Project Structure

```
Assets/
  Scripts/
    GameManager.cs        싱글턴, 11종 FruitConfig 정적 배열, 원형 Sprite 런타임 생성
    Fruit.cs              과일 컴포넌트 (물리/충돌/머지), 정적 ActiveFruits 리스트
    FruitSpawner.cs       Queue 오브젝트 풀링, 마우스 조준, 클릭/Space 드롭
    UIManager.cs          점수 / 다음 과일 / 게임오버 패널 / 재시작 버튼
    GameOverDetector.cs   ActiveFruits 스캔으로 위험선 체류 시간 감지
  Scenes/
    Main.unity            메인 씬 (유일한 게임 씬)
  Prefabs/
    Fruit.prefab              Rigidbody2D + CircleCollider2D + SpriteRenderer + Fruit.cs
    GameManager_GO.prefab
    FruitSpawner_GO.prefab    fruitPrefab 슬롯에 Fruit.prefab 연결됨
    UIManager_GO.prefab
    GameOverDetector_GO.prefab
    Canvas.prefab             Screen Space Overlay — ScoreText / NextLabel / GameOverPanel / RestartButton
    Container.prefab          좌·우·바닥 BoxCollider2D (플레이 필드 벽)
    Background.prefab
  Settings/               URP-2D 렌더러 설정
  TextMesh Pro/           TMP 기본 리소스
```

---

## Core Architecture

### 싱글턴 명명 규칙

일반적인 `Instance` 대신 **클래스명과 동일한 소문자 정적 프로퍼티**를 사용한다. 의도된 컨벤션이므로 새 코드에서도 유지한다.

| 클래스 | 정적 접근자 |
|--------|------------|
| `GameManager` | `GameManager.gameManager` |
| `FruitSpawner` | `FruitSpawner.fruitSpawner` |
| `UIManager` | `UIManager.uiManager` |

### 데이터 위치

- 11종 과일 설정(이름·반지름·색·점수)은 `GameManager.Configs` **정적 배열**에 인라인 정의. ScriptableObject로 분리되어 있지 않다.
- 원형 스프라이트는 `GameManager.CreateCircleSprite()`로 런타임 생성 (`Awake` 1회).

### 과일 생명주기

1. `FruitSpawner.PrepareNext()` — 풀에서 Fruit 꺼내 `Init(level)` → 스폰 라인 대기 (`IsActive=false`, Kinematic)
2. 클릭 또는 Space → `Fruit.Drop()` — `IsActive=true`, Dynamic 전환, `ActiveFruits` 등록
3. `OnCollisionEnter2D` — 같은 레벨 + 둘 다 `IsActive` + 머지 플래그 없음 → `FruitSpawner.SpawnAt(level+1, mid)`, 두 과일 `Release`
4. `Release` → `Fruit.Deactivate()` → `gameObject.SetActive(false)` → 풀 반환

> **풀링 우회 금지**: Fruit 생성/소멸은 반드시 `GetFromPool` / `Release` 경유.

### 충돌 시 미세 반발

머지가 아닌 일반 충돌에서 `Fruit.bounceSpeedRef / minBounceForce / maxBounceForce`로 상대속도 비례 Impulse를 가해 끼임을 방지한다.

### 게임오버 판정

`GameOverDetector`가 매 프레임 `Fruit.ActiveFruits`를 순회해 `dangerY(3.8)` 위에 과일이 있는 동안 `dangerTimer`를 누적한다. `gracePeriod(3s)` 초과 시 `GameManager.TriggerGameOver()` 호출. 씬 로드 직후 `startDelay(2s)` 동안은 검사 건너뜀.

### 이벤트 흐름

```
AddScore()          → GameManager.OnScoreChanged(int)  → UIManager.UpdateScore()
TriggerGameOver()   → GameManager.OnGameOver()         → UIManager.ShowGameOver()
```

`UIManager.Start()`에서 두 이벤트를 구독한다.

---

## Inspector 연결 (씬 사전 조건)

`Main.unity`에서 다음 참조가 인스펙터로 연결되어 있어야 정상 동작한다:

- `FruitSpawner.fruitPrefab` → `Prefabs/Fruit.prefab`
- `UIManager.scoreText` / `nextFruitLabel` / `gameOverPanel` / `finalScoreText` / `restartButton` → Canvas 하위 위젯
- `RestartButton` OnClick은 코드(`AddListener`)로 바인딩되므로 인스펙터 설정 불필요

---

## Conventions

- **포맷**: CSharpier 스타일 유지 (트레일링 콤마, 인자별 라인 분할). 새 스크립트도 동일하게 작성.
- **언어**: 식별자는 영문, 주석·문서는 한국어 허용.
- **새 과일 추가**: `GameManager.Configs` 배열 끝에 `FruitConfig` 항목 추가.
- **싱글턴**: 위 표의 소문자 컨벤션 유지.

---

## 실행 / 테스트

1. Unity 6000.3.15f1로 프로젝트 열기
2. `Assets/Scenes/Main.unity` 열기
3. Hierarchy에 매니저 GO 4개 + Canvas + Container + Background 확인
4. Play → 마우스 좌우 조준, 클릭 또는 Space로 드롭 → 같은 과일 충돌 시 머지
5. 상단 `dangerY` 위에 과일이 3초 이상 머무르면 게임오버 패널 표시


