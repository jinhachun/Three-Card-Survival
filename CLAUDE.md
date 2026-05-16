# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:

- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them — don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:

- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it — don't delete it.

When your changes create orphans:

- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:

- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:

```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

## 5. Unity / 프로젝트 규칙

### 아키텍처

- 게임 데이터 → ScriptableObject (예: NodeData, SkillData)
- 씬 동작 → MonoBehaviour (예: NodeView, MapManager)
- 씬 오브젝트 참조(NodeView target 등)는 ScriptableObject에 넣지 않는다 — MonoBehaviour에 둔다.
- 에디터 전용 코드(자동 수집 버튼 등)는 반드시 `#if UNITY_EDITOR`로 감싼다.

### URP / 렌더링

- 이 프로젝트는 **URP + 2D Renderer** 사용.
- 스프라이트는 **Sprite-Lit-Default** 머티리얼 사용 — `Sprites/Default`는 2D 라이팅을 무시하므로 금지.
- LineRenderer(엣지 시각화)도 URP 호환 머티리얼 필요.
- 포그 오브 워는 2D 라이트(Spot Light 2D / Global Light 2D)로 구현.

### Odin Inspector

- 인스펙터 UI는 Odin 사용: `[ShowIf]`, `[BoxGroup]`, `[EnumToggleButtons]`, `[Button]`, `[Required]`, `[ReadOnly]` 등.
- Odin 버튼 라벨은 한국어로 작성.

### 코딩 스타일

- 클래스명 / 메서드명 / 변수명: **영어**
- Odin 버튼 라벨, 디자인 의도 설명 주석: **한국어**
- 주석은 WHY가 비명백할 때만 작성. WHAT 설명 주석은 쓰지 않는다.

### 기획 참조

- 새 기능 구현 전 `ClaudeRules/기획서.txt` 확인 필수.
- 기획서와 충돌하는 코드 작성 금지.
- 진행 현황은 `ClaudeRules/TODO.md` 참조.
