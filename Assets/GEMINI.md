# Salem Project - Gemini Context

이 파일은 Gemini CLI가 프로젝트의 맥락을 빠르게 파악하고 일관된 코딩 스타일을 유지하기 위한 가이드라인입니다.

## 프로젝트 개요
- **장르**: 마피아/추리 스타일의 게임
- **주요 시스템**: 역할(Role) 기반의 AI 행동 시스템, 대화 시스템(Yarn Spinner), 2D 기반 연출.
- **핵심 기술 스택**:
  - **Engine**: Unity
  - **Scripting**: C#
  - **Dialogue**: Yarn Spinner (`Dialogue/` 폴더 내)
  - **Animation/Tween**: DOTween (`Plugins/Demigiant/` 폴더 내)
  - **UI**: TextMesh Pro
  - **Rendering**: Universal Render Pipeline (URP) 2D

## 주요 폴더 구조
- `Scripts/AI`: 각 직업군(Witch, Thief, Believer 등)의 로직 및 AI 행동 트리.
- `Scripts/Gameplay`: `Character.cs`, `Role.cs` 등 핵심 게임 데이터 및 로직.
- `Scripts/System`: `AIManager`, `RoleManager`, `Timer` 등 전역 시스템 관리.
- `Scripts/UI`: 페이드 효과 및 UI 관리.
- `Prefabs/Characters`: 캐릭터 오브젝트 프리팹.

## 코딩 규칙 및 관습
- Class와 Method는 `PascalCase`, Private Field는 `_camelCase`, Serialize Field는 `camelCase`, Constant는 `UPPER_CASE`, 그 외는 C# 기본 Convention을 따라감.
- **AI 시스템**: `AIAction` 및 `AIContext`를 통한 상태 기반 또는 행동 트리 방식 사용.
- **확장성**: 새로운 직업(Role) 추가 시 `Scripts/AI` 폴더에 관련 클래스 구현.

## Gemini를 위한 팁
- 프로젝트 관련 기획은 `@게임구성.pdf`, `@Salem_ppt.pptx`에 작성되어 있습니다.
- 대화 관련 수정 시 `Test.yarn` 또는 `SalemProject.yarnproject` 또는 `캐릭별 대사.txt`를 참조하십시오.
- AI 로직 수정 시 `AIAction` 클래스와의 호환성을 확인하십시오.
- 객체지향적인, SOLID원칙을 지키는 코드를 짜도록 하십시오.
- 모호한점이 있을경우, 본인이 판단해서 임의의 답변을 작성하는 대신, 구체화를 위한 또다른 질문을 해주십시오.