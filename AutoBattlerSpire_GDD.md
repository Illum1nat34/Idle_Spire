
# Auto‑Battler Spire — Game Design Document (GDD)

_Last updated: Sprint 4 planning snapshot_

## 1) High Concept
Card‑based roguelike autobattler inspired by **Slay the Spire**. The player ascends floors, chooses paths and events, and builds a deck. **Combat is automated**: each turn, 3 cards are drawn sequentially into **slots S1/S2/S3** with fixed slot costs **1 / 2 / 3**. A card **plays automatically** if `card.cost ≤ slot.cost` (or the card is **Adaptive** to that slot); otherwise it is **folded to discard**. The buildcraft (deck, upgrades, relics) is the main decision space.

**Target platform:** PC (Steam) first. **Mobile** (iOS/Android) is a later decision.  
**Engine:** Unity **2022.3.60f1** (LTS).

---

## 2) Core Loop
1. Pick a route node on the floor map (combat, elite, event, shop, rest).
2. Resolve encounter: **auto‑combat 1v1** driven by deck + relics.
3. Post‑combat: choose 1 of 3 card rewards (or skip), get gold, chance for relics.
4. Progress to next node; after boss → ascend to next floor/act.
5. Repeat until victory or death. Roguelike runs with meta‑progression unlocks.

---

## 3) Combat Design

### 3.1 Turn Structure
- Start of a **combat turn**: draw **exactly 3** cards one by one into **S1, S2, S3**.
- For each slot in order: try to **play** the drawn card.
  - **Play** if `card.cost ≤ slot.cost` or `card.isAdaptiveFor(slot)`.
  - Else **Fold**: send to discard immediately (no effect).
- After 3 slots resolved → turn ends. Opponent takes a turn next.

### 3.2 Block & Damage
- **Block resets** at the **start of the owner’s turn** (fresh shield each turn).
- Damage reduces current Block; any excess damages HP. HP cannot drop below 0.

### 3.3 Card Anatomy
- **Cost**: base 0/1/2/3 (0 rare/special).  
- **Type**: Attack / Skill / Power (cosmetic now, helps later for relics/events).  
- **Adaptive** (mask over S1/S2/S3): card may play in specific slots even if `cost > slot`. When played in an allowed slot, the card can use **slot multipliers** (e.g., +% value when landing in S3).  
- **Upgrade**: some cards upgrade into Adaptive variants or increase values.

**Effects Set (initial):**
- `Damage(V1)`, `Block(V1)`, and **ScryManual(X)** (manual reorder/ship‑to‑bottom of top X deck cards; player only).  
- Future: Status effects (Vulnerable/Weak/Frailty), Draw/Discard manipulation, Scaling (Strength/Dexterity), Exert/Exhaust, Ethereal, etc.

### 3.4 Enemy Design
- Each enemy has its **own deck** (data‑driven) and optional **traits** (passives) analogous to player relics.  
- **Telegraphing**: no intent icons (unneeded due to card‑slot randomness). Difficulty/clarity tuned via enemy deck composition and traits.

---

## 4) Progression & Meta

### 4.1 Floors/Acts
- A run comprises multiple floors per act; acts end with a boss.  
- Node types: Combat, Elite, Event, Shop, Rest, Treasure.  
- Pathing: standard branching map.

### 4.2 Events
- Lightweight narrative choices with card/relic/gold tradeoffs; some events add/remove/upgrade cards.

### 4.3 Relics
- Passive modifiers that shape builds (e.g., +1 Block on S1 plays; add Strength if S3 overcharge, etc.).  
- Rarity: Common / Uncommon / Rare / Boss / Shop.

### 4.4 Meta‑Progression
- Unlockable cards/relics, starter decks, cosmetics. Persistent profile & seed history.

---

## 5) Content Guidelines (v0 scope)
- **Cards**: ~60 base cards across 1 starting class (player), 30+ enemy cards.
- **Relics**: 25–30.
- **Enemies**: 15 base + 5 elites + 3 bosses.
- **Events**: 20.
- **Runs**: 3 acts baseline.

---

## 6) Technical Design

### 6.1 Project Structure (runtime)
- `Scripts/Data/` — ScriptableObjects for `CardData`, `HeroData`, `EnemyData`, `RelicData` (later), `EffectKind`.
- `Scripts/Combat/` — `CardInstance`, `Deck`, `Fighter`, `CombatRunner`, enemy AI hooks.
- `Scripts/Core/` — `Rng`, `Slot` (+`Cost()`), `CombatContext`, `EffectExecutor`, `Save/Profiles` (later).
- `Scripts/UI/` — `CombatUI`, `ScryModal`, map/event UIs (later).
- `Scripts/FX/` — presentation layer (tween helpers & VFX triggers).

### 6.2 Data‑Driven
- Cards, heroes, enemies are **SO‑driven**; decks are lists of SO references with counts.  
- Balance via SO values; no hardcoding in logic.

### 6.3 Tooling / Packages
- **TextMeshPro** — text/UI.
- **DOTween (Demigiant)** — tweens & simple combat/UI animations.  
- **Odin Inspector** (optional but recommended) — faster authoring of complex SOs and custom inspectors.  
- **Addressables** — later for content packaging (optional for Prototype).  
- **Unity Input System** — basic controls.  
- **URP** — lightweight rendering.  
- **FMOD** (or Unity audio) — optional audio middleware.
- **Localization** — later (Unity Localization).

### 6.4 Save / Settings
- JSON profile with unlocked pools, runs, options. (Sprint 8+)

### 6.5 Telemetry (optional)
- Simple CSV/JSON run logs for tuning (deck size, card play rates, DPS/mitigation).

---

## 7) Visual Targets & UX
- Clean, readable UI: 3 slot pads at bottom, center board for combat feedback, left/top log for dev builds.  
- Card visuals are **placeholder** until art drop; all UI texts via TMP.  
- FX priorities: play/fold feedback, damage/heal popups, block pulse, card fly‑in/out (DOTween).

---

## 8) Sprint Roadmap (plan)

> Спринты по 1–2 недели. Каждый закрывается **сборкой без ошибок**, коротким тест‑планом и чек‑листом.

### Sprint 1 — Core Sim (завершён)
- Базовые типы: `Fighter`, `Deck`, `CardInstance`, `CombatContext`, `EffectExecutor`.
- Эффекты: `Damage`, `Block`.  
- Авто‑слоты 1/2/3, цикл боя в лог.  
**Deliverable:** консольный бой Player vs Enemy.

### Sprint 2 — Data & SO
- `CardData`, `HeroData`, `EnemyData` + загрузка колод из SO.  
- Адаптивные карты (флаги слотов), апгрейды (минимальный каркас).  
**Deliverable:** бой на SO‑данных, стартовые колоды из `HeroData`/`EnemyData`.

### Sprint 3 — Minimal Combat UI (завершён)
- `CombatUI`: имена, HP, Block, кнопка **End Turn**, dev‑лог на Canvas.  
- Игрок/враг ход чередуются, блок сбрасывается в начале **своего** хода.  
**Deliverable:** боевой прототип с кнопкой **End Turn** и понятными числовыми индикаторами.

### Sprint 4 — Scry (текущий)
- **Новый эффект** `ScryManual(X)` → модалка ручного просмотра X верхних карт (перестановка, отправка вниз).  
- `Tools → Build → Add Scry Modal` — редакторная кнопка создаёт модалку и линкует в `CombatUI`.  
- FX‑минимум: мягкий апдейт HP/Block (через DOTween, если установлен).  
**Deliverable:** карта с `ScryManual` корректно открывает модалку и меняет порядок колоды.

### Sprint 5 — Узловая карта и события
- Генерация карты акта (узлы: бой/элит/событие/магазин/костёр).  
- UI выбора пути, простые события (SO), базовый магазин.  
**Deliverable:** навигация по акту с входом в бой/события.

### Sprint 6 — Реликвии и билды
- `RelicData` + система пассивов/хуков (до/после слота, при Play/Fold, при Start/End Turn).  
- 15–20 первых реликвий.  
**Deliverable:** реликвии реально меняют бой.

### Sprint 7 — Пул врагов и баланс
- Набор обычных/элитных/боссов; редактор **EnemyDeckBuilder** (если нужно).  
- Passives для врагов (а‑ля мини‑реликвии).  
**Deliverable:** 1 акт полный, базовый баланс.

### Sprint 8 — Метапрогресс и сохранения
- Профиль игрока, разблокировки, сейвы/лоады.  
**Deliverable:** стабильные ран‑циклы с прогрессией.

### Sprint 9 — Визуал/аудио интеграция
- Подключение готовых ассетов: UI, персонажи, VFX, SFX.  
- Анимации карт/ударов (DOTween sequences), экранные шейки, пост‑эффекты.  
**Deliverable:** вертикальный срез с целостным визуалом.

### Sprint 10 — Полиш/демо (Steam)
- UX‑полировка, туториал‑шаги, настройки, производительность.  
- Демоверсия (1 акт) и страница в Steam.  
**Deliverable:** Demo build + marketing assets.

---

## 9) Risks & Mitigation
- **Дубли/конфликты скриптов** — строго один `EffectKind.cs`, избегаем partial/копий. Решение: Editor‑скрипты для поиска дублей, чистка `Library/` при залипаниях.
- **UI сложность** — ограничиваемся uGUI + TMP + DOTween, чёткая иерархия Canvas.
- **Баланс/рандом** — детерминированный RNG по seed; логирование для анализа.
- **Мобайл порт** — интерфейс и перф. закладывать заранее: без тяжёлых материалов, Addressables.

---

## 10) Tools / Packages (мы используем)
- **TextMeshPro** — стандартный текст.  
- **DOTween Pro/Free** (Demigiant) — анимации UI/карт/чисел.  
- **Odin Inspector** (optional) — ускорение редактора SO и отладочных окон.  
- **Addressables** (later) — развоз ресурсов.  
- **Unity Input System**, **URP**.  
- **Version Control** — Git (минимум .gitignore для Unity).  
- **FMOD/Unity Audio** — TBD (зависит от звукового пайплайна).  
- **Unity Localization** — позже.

---

## 11) Test Plan (per sprint)
- Smoke (без ошибок компиляции, меню Tools доступно).  
- 5–10 автосимуляций боя (лог без исключений).  
- Проверка карты с `ScryManual` (открытие модалки, изменение порядка, отсутствие зависаний).  
- Набор unit‑проверок простых (Deck.ApplyScry, Block reset).

---

## 12) Glossary
- **Slot S1/S2/S3** — позиции действий за ход (cost 1/2/3).  
- **Adaptive** — карта, разрешённая для конкретных слотов, даже если её cost выше.  
- **ScryManual** — ручной просмотр X верхних карт с перестановкой и отправкой вниз.
