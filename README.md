
# Tanks Merge: Arena Battles

Мобильная гибридная казуальная игра, в которой игрок собирает боевой танк из запчастей и сражается на нем на арене с ботами.

<img src="demo_screens.jpg" width="640">

## Ссылки

### ▶️ [Демо на YouTube](https://youtube.com/shorts/9-a418QvZb4)
### 🎮 [Google Play](https://play.google.com/store/apps/details?id=gamecream.tankmerge) 
### 🎮 [App Store](https://apps.apple.com/us/app/id6761921730)

---

## Стек и зависимости

- `Unity 6000.0.59f2`
- `VContainer`
- `UniTask`
- `R3`
- `MessagePipe`
- `DOTween`
- `Cinemachine`
- `Addressables`
- `Unity NavMesh`
- `Unity Localization Package`
- `Unity Test Framework`

---

## Архитектура

- Зависимости собираются через VContainer:
  - `ProjectLTS` - корневой скоуп (Singleton). Регистрирует все глобальные сервисы. 
  - `HomeLTS` / `BattleLTS` / `TutorialBattleLTS` - дочерние скоупы (Scoped),
    поднимаются при загрузке соответствующей сцены и регистрируют
    сервисы, вью и точки входа своего контекста.
- `Bootstrap` обрабатывает инициализацию игры.
- `AppStateMachine` управляет состояниями игры: `HomeState`, `BattleState`, `TutorialBattleState`. Каждое состояние загружает свои сцены и управляет показом loading/transition экранов.

---

## Экономика и конфиги

Балансная конфигурация хранится в `ScriptableObject`-конфигах.

- `EconomyConfigSO` - параметры баланса экономики
- `MergeConfigSO` - параметры merge-сессии
- `BattleConfigSO` - длительность боя, количество ботов, задержка респауна, радиус автоприцела, префабы танков
- `BotDifficultyConfigSO` - таблица тиров DDA: распределение ботов по весовым категориям и смещение их уровня снаряжения относительно игрока
- `BotBehaviorConfigSO` - два профиля поведения (`Normal` / `Expert`) для бота
- `TankPartStatsCatalogSO` -  боевые статы запчастей танка по уровням
- `ArenaCatalogSO` - список арен с Addressable-ссылкой на префаб и флагом активности

`EconomyService` - единственная точка с формулами баланса, покрыта unit-тестами.

---

## Сохранения

Каждый data-сервис реализует `ISaveModule` и регистрируется параллельно со своим бизнес-интерфейсом. `SaveService` собирает все модули через DI, сериализует через Newtonsoft.Json и сохраняет в одном файле.

`AutoSaveService` вызывает сохранение с заданным интервалом, а также при потере фокуса (`Application.focusChanged`) и при выходе (`Application.quitting`).

Модули, сохраняющие состояние: `CurrencyService`, `EquipmentDataService`, `MergeDataService`, `IdleDataService`, `OfflineIncomeDataService`, `TutorialDataService`, `BattleStatsDataService`, `AudioService`.

---


## Battle Flow

Бой разбит на изолированные фазы вместо одного толстого контроллера. Контракт - `IBattleFlow` с единственным методом `RunAsync`.

Текущая реализация `FfaBattleFlow` (free-for-all) выстраивает пять фаз в нужном порядке и обрабатывает развилку «досрочный выход vs. естественный конец боя»:

`Init ⮕ Countdown ⮕ Active ⮕ End ⮕ Result`

Каждая фаза - отдельный класс со своей зоной ответственности. Добавление нового режима (Team, Boss, Survival) = новая реализация `IBattleFlow`.

---

## AI ботов

`BotBrain` - это `MonoBehaviour` компонент, который отвечает за поведение танка бота.

Внутри `BotBrain` - стейт-машина на четырёх состояниях:

- `Patrol` - бот перемещается по арене в поисках цели
- `Chase` - обнаружил противника, преследует его
- `Attack` - в зоне атаки, открывает огонь
- `Retreat` - HP упало ниже порога, уходит на безопасную дистанцию

Всё поведение: радиус патруля, дистанция атаки, время реакции, порог отступления - настраивается через `BotBehaviorConfigSO` без правки кода. 

Поскольку `BotBrain` - просто компонент на префабе танка, для босса или особого врага достаточно заменить компонент BotBrain на другую реализацию `ITankInput`.

---

## Dynamic Difficulty Adjustment

Сложность ботов адаптируется по двум осям:

- **Количество боёв** на текущем снаряжении повышает тир.
- **Количество неудачных финишей** снижает тир на одну ступень каждые N провалов; любой успешный финиш обнуляет счётчик.

Оба счётчика сбрасываются при экипировке детали нового максимального уровня. Логика изолирована в `BattleDifficultyService` и покрыта unit-тестами.

---

## Merge

Построен по слоям:

- **Model** (`MergeModel`) - единственный источник истины: состояние ячеек, экипировка танка.
- **Services** - вся доменная логика: правила и валидация слияния, мутации сетки, экипировка деталей, ценообразование покупки/продажи.
- **Controllers** - точки входа для действий и жизненного цикла. `MergeSessionController` восстанавливает состояние из сохранения и инициализирует views при старте; `MergeDragController`, `MergePurchaseController`, `MergeExpansionController` обрабатывают пользовательские действия и делегируют в сервисы.
- **Views** - реактивно отображают состояние модели.

---

## Туториал

Обучение разбито на два независимых этапа:

- **Home tutorial** (`HomeTutorialController`) - знакомит с merge и idle механиками.
- **Battle tutorial** (`TutorialBattleController`) - скриптованный сценарий для обучения бою.

Используется однонаправленная зависимость: туториал использует код игры, но игровой код ничего о туториале не знает. Прогресс сохраняется после каждого шага - рестарт посреди обучения продолжит с последней завершённой точки.


---

## Реклама

За общим интерфейсом `IRawAdsProvider` скрыты два провайдера: `AdMobRawAdsProvider` и `MockRawAdsProvider`. Переключение через `AdsConfigSO.ProviderMode` в инспекторе - без перекомпиляции, можно работать в редакторе без реального SDK.

Поверх провайдера стоит `AdsService`, который ограничивает частоту интерстишиал-рекламы и отправляет рекламные события в аналитику.

---

## Аналитика

`IAnalyticsService` реализована через `AppMetricaAnalyticsService` в продакшене и `MockAnalyticsService` в dev-билдах. Плейсменты и названия событий - константы в `AnalyticsEvents`.

---

---

## Тесты

EditMode unit-тесты (Unity Test Framework) покрывают критичные изолированные системы: `EconomyService` (все формулы баланса), `BattleDifficultyService` (DDA логика), `MergeModel` / `MergeRuleService` (правила слияния), `CostFormatter` (форматирование валюты).

---

## Контакты

- **Email**: alex8rrr@gmail.com
- **Telegram**: @a_dev_99













