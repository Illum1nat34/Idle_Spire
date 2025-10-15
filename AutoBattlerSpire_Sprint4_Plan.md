
# Auto-Battler Spire — Sprint 4 Plan (based on Sprint 3 snapshot)

> Этот документ самодостаточный. Скопируй его в Cursor как `design_spec.md` и работай по разделам (T1…T7). Он описывает текущее состояние проекта (конец Sprint 3), цели Sprint 4, точные изменения в коде, пути файлов, критерии приёмки и диагностику.

---

## 0) Snapshot после Sprint 3

**Жанр/петля.** ПК (Unity 2022.3 LTS). Карточный автобаттлер: за ход тянется 3 карты последовательно в 3 слота (S1/S2/S3) со стоимостью 1/2/3. Карта играет, если `card.cost ≤ slot.cost` или она адаптивная для слота; иначе уходит в сброс. Игрок и враг ходят по очереди.

**Блок.** Блок у владельца **сбрасывается в начале его хода** (“новый ход — новый блок”). Урон сначала сжигает блок, остаток идёт по HP.

**Слои кода (в Sprint 3):**
- `Assets/Scripts/Data/` — `CardData`, `HeroData`, `EnemyData`, `EffectKind{ Damage, Block }`.
- `Assets/Scripts/Combat/` — `Fighter`, `Deck`, `CardInstance`, `CombatRunner`.
- `Assets/Scripts/Core/` — `Rng`, `Slot` (+ `Cost()`), `CombatContext`, `EffectExecutor`.
- `Assets/Scripts/UI/` — `CombatUI` (имена, полосы HP, Block, кнопка End Turn, лог).
- **В HUD нет** просмотра/сдвига (мы это убрали к концу Sprint 3).

**Сцена (Combat):**  
`CombatRunner` (ссылки на SO героя/врага или fallback-карты) + `Canvas/CombatUI` (UI) + кнопка End Turn.

---

## 1) Цели Sprint 4

1) **Ручной Просмотр как эффект карты** — новый `EffectKind.ScryManual`.  
   При розыгрыше такой карты у **игрока** открывается модалка с X верхними картами колоды: можно менять порядок (↑/↓), отмечать «вниз колоды»; по “Принять” порядок применяется. У врага пока игнорируем (без модалки).

2) **Editor-утилита** — меню **Tools → Build → Add Scry Modal**, которое создаёт модалку под текущим `Canvas` и автоматически прописывает её в `CombatUI.ScryModal`.

3) **FX-прослойка (опционально)** — `CombatFx` с методами для плавного апдейта HP/Block (если DOTween установлен — твиним, иначе мгновенно). Внедряем по минимуму.

4) **HUD-минимум** — UI остаётся простым: End Turn, полосы, лог. Кнопок “Просмотр/Сдвиг” в HUD **нет**.

---

## 2) Изменения по классам (контракты и места)

> Ниже — **API и места правок**. Реализацию генерируй в Cursor, соблюдая сигнатуры и пути.  

### 2.1. `Assets/Scripts/Data/EffectKind.cs`
Добавить значение:
```csharp
public enum EffectKind
{
    None = 0,
    Damage = 1,
    Block = 2,
    ScryManual = 100 // NEW
}
```

### 2.2. `Assets/Scripts/Combat/Deck.cs`
Добавить два метода (публичные):
```csharp
// Срез верхних X карт (без снятия с колоды)
public List<CardInstance> PeekTop(int x);

// Применение результата просмотра
// toBottom: индексы из исходного среза, которые отправить в низ колоды,
// orderedTop: индексы (из того же среза), которые вернуть наверх в указанном порядке.
public void ApplyScry(List<int> toBottom, List<int> orderedTop);
```
**Алгоритм ApplyScry (строго):**
1) Снять первые `N = min(orderedTop.Count + toBottom.Count, draw.Count)` карт в буфер **в исходном порядке**.  
2) Вернуть наверх карты из `orderedTop` **в указанном порядке**.  
3) Добавить в низ карты из `toBottom` **в порядке перечисления**.

### 2.3. `Assets/Scripts/Combat/CombatRunner.cs`
В месте применения эффектов сыгранной карты (когда `canPlay == true`), до отправки карты в сброс, вставить обработку Scry:
```csharp
if (eff.Kind == EffectKind.ScryManual && _playerTurn && UI != null && UI.ScryModal != null)
{
    bool done = false;
    UI.ScryModal.Open(self.Deck, Mathf.Max(1, eff.V1), ok => { done = true; });
    yield return new WaitUntil(() => done);
    continue; // сам эффект не исполняется в EffectExecutor
}
```
Остальные эффекты — как раньше (через `EffectExecutor.Execute`).

### 2.4. `Assets/Scripts/UI/CombatUI.cs`
Добавить поле:
```csharp
[Header("Optional")]
public ScryModal ScryModal; // ссылка на модалку
```

### 2.5. Новый файл `Assets/Scripts/UI/ScryModal.cs`
Публичный API:
```csharp
public class ScryModal : MonoBehaviour
{
    [Header("Refs")]
    public GameObject PanelRoot;
    public Transform ListRoot;
    public Button ButtonConfirm;
    public Button ButtonCancel;
    public TextMeshProUGUI Header;

    [Header("Item Template")]
    public GameObject ItemPrefab; // имеет компонент CardChip

    public bool IsOpen { get; }
    public void Open(Deck deck, int x, Action<bool> onClosed);
}
```
**Поведение:**
- `Open(deck, x, cb)`:  
  `_snapshot = deck.PeekTop(x)` → создать дочерние `ItemPrefab` в `ListRoot`,  
  `Header.text = $"Просмотр {x}"`, `PanelRoot.SetActive(true)`.
- **Принять**: собрать `toBottom` (где `chip.SendToBottom`), `orderedTop` (в текущем порядке остальных), вызвать `deck.ApplyScry(...)`, закрыть (inactive), `cb(true)`.
- **Отмена**: просто закрыть, `cb(false)`.

**Вложенный компонент** `CardChip` (в том же файле или отдельном):
```csharp
public class CardChip : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Order;
    public Toggle ToggleBottom;
    public Button BtnUp;
    public Button BtnDown;

    public bool SendToBottom => ToggleBottom != null && ToggleBottom.isOn;

    public void Bind(string title, int orderIndex, ScryModal modal);
    public void SetOrder(int displayOrder);
}
```
Кнопки ↑/↓ меняют порядок элементов `_chips` в модалке и обновляют номера.

### 2.6. Новый файл `Assets/Editor/Build_AddScryModal.cs`
Меню: **Tools → Build → Add Scry Modal**  
Действия:
1) Найти `Canvas` и `CombatUI` на сцене.  
2) Создать `ScryModal` с компонентом и вложенными объектами:
   - `PanelRoot` (Image, full-screen, цвет с альфой 0.6, inactive по умолчанию),
   - `Window` (Image, 700×500),
   - `Header` (TMPUGUI, крупный),
   - `List` (RectTransform область под элементы),
   - `Confirm`/`Cancel` (Button + TMPUGUI),
   - `ItemPrefab` (inactive, содержит CardChip: Title, Order, Toggle, Up/Down).
3) Прописать `ui.ScryModal = modal`.

### 2.7. (Опционально) `Assets/Scripts/FX/CombatFx.cs`
Интерфейс:
```csharp
public static class CombatFx
{
    public static void TweenHp(Slider slider, float to, float dur = 0.25f);
    public static void TweenBlock(TextMeshProUGUI label, int value);
}
```
Если есть DOTween — использовать твины, иначе присваивать напрямую. Вызывать из `CombatUI.UpdateBars(...)` по желанию.

---

## 3) Acceptance (критерии приёмки)

- [ ] Меню **Tools → Build → Add Scry Modal** присутствует. Нажатие создаёт объект `ScryModal` под текущим `Canvas` и заполняет `CombatUI.ScryModal`.  
- [ ] Карта с `EffectKind = ScryManual` и `V1=2` вызывает модалку **на ходу игрока**.  
- [ ] В модалке можно:
  - [ ] переставлять карты ↑/↓;
  - [ ] помечать галочками «вниз»;
  - [ ] по **Принять** порядок применяется (верхние — по заданному порядку, отмеченные — в низ колоды);
  - [ ] по **Отмена** изменений в колоде нет.
- [ ] После закрытия модалки ход продолжается (нет зависаний).  
- [ ] Ход врага не открывает модалку.

---

## 4) Минимальный тестовый набор (через SO)

Создай 10 `CardData` (Create → ABS → Card):

1. Искра — Attack, cost 1, Damage 6  
2. Удар в прыжке — Attack, cost 2, Damage 10  
3. Огненный взрыв — Attack, cost 3, Damage 14  
4. Пластина — Skill, cost 2, Block 8  
5. Железная воля — Skill, cost 3, Block 12  
6. Гибкий толчок — Attack, cost 2, Damage 7  
7. Парирующий удар — Attack, cost 2, Damage 6 + Block 4  
8. Эхо клинка — Attack, cost 2, Damage 5 + Damage 5  
9. Око — Skill, cost 1, **ScryManual V1=2**  
10. Щитовой удар — Attack, cost 3, Damage 8 + Block 6

`HeroData` (MaxHp 60) — положи туда 8–10 любых карт (включая «Око»).  
`EnemyData` (MaxHp 40) — собери из атак/дефенсов (без ScryManual).  
В `CombatRunner` проставь `PlayerHero`/`EnemyDef`.

---

## 5) Порядок задач для Cursor

- **S4-T1** — Добавить `ScryManual` в `EffectKind`.  
- **S4-T2** — Реализовать `Deck.PeekTop(int)` и `Deck.ApplyScry(...)`.  
- **S4-T3** — `CombatRunner`: обработка `ScryManual` (корутина, ожидание модалки на ходу игрока).  
- **S4-T4** — `ScryModal` + `CardChip` (UI, API из раздела 2.5).  
- **S4-T5** — `CombatUI`: добавить публичное поле `ScryModal` и использовать его в Runner.  
- **S4-T6** — `Editor/Build_AddScryModal.cs`: кнопка строит модалку и линкует её в UI.  
- **S4-T7 (опц.)** — `CombatFx` + плавные бары.

---

## 6) Диагностика

- **CS0101: enum/class уже объявлен** — в `Assets/` осталась вторая копия файла (например, второй `EffectKind.cs`). Должен быть **ровно один**.  
- **Missing partial / duplicate members** — дубликаты `CombatUI`, `ScryModal`, `CombatRunner`. Поиск `t:Script ClassName` и удалить лишние.  
- **Меню Tools не появляется** — есть compile-ошибки. Сначала починить ошибки.  
- **Модалка не открывается** — у карты не `ScryManual` или `V1 ≤ 0`, или `CombatUI.ScryModal` не задан, или карта не сыгралась (стоимость не подошла слоту).  
- **Зависание** — убедись, что кнопки модалки вызывают `Close(true/false)` и `onClosed` точно вызывается.

---

## 7) Замечания по стилю/архитектуре

- Просмотр — это **эффект карты**, не кнопка в HUD.  
- `ApplyScry` **не** перемешивает колоду целиком — только перекладывает срез.  
- Враг в Sprint 4 модалку не вызывает (поведение уточним позднее).  
- FX необязательны для приёмки Sprint 4.
