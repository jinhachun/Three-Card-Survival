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
- 게임 데이터 → ScriptableObject
- 씬 동작 → MonoBehaviour
- 씬 오브젝트 참조는 ScriptableObject에 넣지 않는다 — MonoBehaviour에 둔다.
- 에디터 전용 코드는 반드시 `#if UNITY_EDITOR`로 감싼다.

### URP / 렌더링
- 이 프로젝트는 **URP + 2D Renderer** 사용.
- 스프라이트는 **Sprite-Lit-Default** 머티리얼 사용.
- LineRenderer도 URP 호환 머티리얼 필요.

### Odin Inspector
- 인스펙터 UI는 Odin 사용: `[ShowIf]`, `[BoxGroup]`, `[EnumToggleButtons]`, `[Button]`, `[Required]`, `[ReadOnly]` 등.
- Odin 버튼 라벨은 한국어로 작성 가능.

### 코딩 스타일
- 클래스명 / 메서드명 / 변수명: **영어**
- 코드 주석, Odin 버튼 라벨: 한국어 가능
- 주석은 WHY가 비명백할 때만 작성.

### 인게임 텍스트 ← 절대 규칙
- **인게임에 표시되는 모든 문자열은 영어로만 작성한다.**
- 카드 설명, 효과 텍스트, HUD 표시, 팝업 메시지, 플로팅 텍스트 전부 해당.
- 프로젝트에 한국어 폰트가 없으므로 한글이 화면에 표시되면 깨진다.
- 유일한 예외: `Debug.Log` 등 에디터 전용 로그 메시지.

### 기획 참조
- 새 기능 구현 전 `ClaudeRules/기획서.txt` 확인 필수.
- 기획서와 충돌하는 코드 작성 금지.
- 진행 현황은 `ClaudeRules/TODO.md` 참조.

