🖱️ The Clicker Hero
"마을을 위협하는 던전의 발생, 클릭으로 세상을 구하는 영웅의 연대기" > The Clicker Hero는 데이터 기반의 확장성과 정교한 타격감을 목표로 하는 2D 픽셀 아트 액션 클릭커 RPG입니다.

🎮 Project Overview
목표: 2027년 초 스팀(Steam) 글로벌 출시

장르: 2D Clicker, Action RPG, Roguelike Lite

핵심 루프: 자원 채집(Click) → 영웅 및 장비 강화 → 던전 공략(Boss Battle) → 유물 수집 및 스토리 확장

🛠️ Tech Stack & Architecture
Core
Engine: Unity 2022.3+ (2D)

Language: C#

Version Control: Git (GitHub)

Art: Aseprite (Hand-drawn Pixel Art)

Key Architecture
Data-Driven Design: ScriptableObject를 활용하여 무기, 몬스터, 스테이지 데이터를 코드와 분리, 밸런싱 효율 극대화.

Optimization: Object Pooling을 통한 런타임 가비지 컬렉션 최소화 및 대규모 적 스폰 성능 확보.

Localization: CSV 기반의 다국어 시스템 구축을 통해 글로벌 서비스 확장성 고려.

Save System: JSON 직렬화를 통한 사용자 데이터 영속성 관리.

📅 Milestones (Development Progress)
✅ Phase 1: Core Mechanics (Completed)
[x] 기본 클릭 기반 재화 획득 시스템 구축

[x] 가중치 랜덤 알고리즘 기반의 전략적 몬스터 스폰 시스템

[x] 적군 기본 AI 및 다중 피격 판정(Melee/Aura) 로직 구현

[x] 다국어 지원 시스템(LanguageManager) 개발

🏃 Phase 2: Content & Juice (In Progress)
[ ] Aseprite 기반 플레이어/적군 애니메이션 리소스 제작

[ ] 시각적 피드백 강화 (Hit-stop, Camera Shake, White Flash)

[ ] 던전 시스템 및 스테이지 진행 로직 고도화

🔜 Phase 3: Steam Integration & Polishing
[ ] Steamworks API 연동 (도전과제 및 클라우드 저장)

[ ] 사운드 이펙트(SFX) 및 배경음악(BGM) 최적화

👾 Developer's Note
이 프로젝트는 단순한 클릭 게임을 넘어, '압도적인 타격감'과 '수치적 성장의 즐거움'을 전달하는 데 집중하고 있습니다. 특히 모든 리소스(코드, 아트)를 1인 개발하며 프로젝트의 유기적인 결합을 시도하고 있습니다.

## 📅 현재 진행 상황
- [x] 프로젝트 생성
- [x] 기본 클릭 메커니즘 구현 
- [ ] 스팀 API 연동
