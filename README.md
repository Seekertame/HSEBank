# Запуск
Запустите Program.cs
# Архитектура

## Проектная структура

```
src/
  HSEBank.Domain/                # Доменный слой (ядро системы)
    Entities/
      BankAccount.cs             # Счёт: Id, Name, Balance; Apply/ RecomputeBalance
      Category.cs                # Категория: Id, Type (Income/Expense), Name
      Operation.cs               # Операция: Id, Type, AccountId, CategoryId, Amount, Date, Description; Update(...)
    ValueObjects/
      Money.cs                   # Денежный тип (арифметика, форматирование)
      Description.cs             # Описание (нормализация, длина)
      Ids.cs                     # AccountId, CategoryId, OperationId (Guid)
      Types.cs                   # enum OperationType, CategoryType
    Repositories/
      Interfaces.cs              # IBankAccountRepository, ICategoryRepository, IOperationRepository
    Factories/
      OperationFactory.cs        # Создание валидных операций в одном месте
    Services/
      AnalyticsService.cs        # Сальдо и группировка по категориям

  HSEBank.Application/           # Сценарии (use-cases) над доменом
    Facades/
      AccountsFacade.cs          # CRUD счётов (+ оркестрация)
      CategoriesFacade.cs        # CRUD категорий
      OperationsFacade.cs        # Add/Edit/Remove операций + пересчёт баланса
      AnalyticsFacade.cs         # Обёртка над AnalyticsService
    Commands/
      ICommand.cs                # Контракт команды
      AddOperationCommand.cs     # Добавление операции как объект-команда
      TimedCommand.cs            # Декоратор: измеряет время выполнения

  HSEBank.Infrastructure/        # Технические детали (хранение, импорт/экспорт)
    Repositories/
      InMemoryBankAccountRepository.cs
      InMemoryCategoryRepository.cs
      InMemoryOperationRepository.cs
      CachedBankAccountRepository.cs   # Proxy над счётами (write-through кэш)
    Import/
      ImporterBase.cs                 # Template Method
      CsvOperationImporter.cs         # Импорт операций из CSV
    Export/
      IExportVisitor.cs               # Контракт посетителя
      JsonOperationExportVisitor.cs   # Экспорт операций в JSON

  HSEBank.Presentation.Console/   # Консольный UI
    Program.cs                    # Composition Root: DI, запуск Shell
    Shell/
      AppShell.cs                 # Главный цикл и навигация
    Screens/
      AccountsScreen.cs           # Счета: список/создать/выбрать/переименовать/удалить
      CategoriesScreen.cs         # Категории: список/создать/переименовать/удалить
      OperationsScreen.cs         # Операции: список/добавить/удалить/редактировать
      AnalyticsScreen.cs          # Аналитика: сальдо и агрегирование по категориям
      ImportExportScreen.cs       # Экспорт JSON / импорт CSV
    UI/
      ConsoleUi.cs                # Общие утилиты ввода-вывода
```

## Связи между слоями

```
Presentation.Console  ─────→  Application  ─────→  Domain
        │                         │
        └────────────── DI ───────┘
                        │
                Infrastructure  ─────→  Domain
```

- **Presentation** зависит только от **Application** (и DI-контейнера).
    
- **Application** зависит только от **Domain** (интерфейсы репозиториев).
    
- **Infrastructure** реализует эти интерфейсы и подключается через DI в `Program.cs`.
    
- **Domain** не зависит ни от чего.
    

## Зоны ответственности

- **Domain** — правила предметной области:
    
    - `Entities` держат инварианты (например, `BankAccount.RecomputeBalance` пересчитывает баланс из операций).
        
    - `ValueObjects` инкапсулируют формат/валидацию (`Money`, `Description`, `Id`-тип).
        
    - `Repositories/Interfaces` — только контракты для доступа к данным.
        
    - `OperationFactory` — централизованное создание `Operation`.
        
    - `AnalyticsService` — доменные вычисления (сальдо и группы).
        
- **Application** — координация сценариев:
    
    - `*Facade` — «контроллеры» use-case’ов (CRUD, аналитика), обращаются к репозиториям и сущностям.
        
    - `Commands` — сценарии как объекты (`AddOperationCommand`) + **декоратор** `TimedCommand` для метрик.
        
- **Infrastructure** — детали реализации:
    
    - `InMemory*Repository` — оперативное хранилище.
        
    - `CachedBankAccountRepository` — **Proxy**: кэширование чтения, запись сквозь в базовый репозиторий.
        
    - `ImporterBase`/**CsvOperationImporter** — **Template Method** для импорта.
        
    - `IExportVisitor`/**JsonOperationExportVisitor** — **Visitor** для экспорта.
        
- **Presentation.Console** — взаимодействие с пользователем:
    
    - `AppShell` — главный цикл и выбор экрана.
        
    - `Screens/*` — функции экранов, без бизнес-логики (только вызовы фасадов).
        
    - `ConsoleUi` — унифицированный ввод/вывод.
        

## Потоки выполнения (ключевые сценарии)

1. **Создание счёта**
    

- `AccountsScreen → AccountsFacade.Create(...) → IBankAccountRepository.Add(...)`.
    
- UI при желании делает новый счёт «текущим».
    

2. **Добавление операции**
    

- `OperationsScreen` собирает данные → создаёт `AddOperationCommand`.
    
- `TimedCommand( AddOperationCommand ).Execute()` — логирует время.
    
- Внутри `OperationsFacade.Add(...)`:
    
    - `OperationFactory.Create(...)` → `IOperationRepository.Add(...)`;
        
    - `BankAccount.RecomputeBalance(ops.GetByAccount(...))` → `IBankAccountRepository.Update(...)`.
        

3. **Редактирование операции**
    

- `OperationsScreen` выбирает операцию и новые значения.
    
- `OperationsFacade.Edit(...)`:
    
    - `IOperationRepository.Get(...)` → `Operation.Update(...)` → `IOperationRepository.Update(...)`;
        
    - `BankAccount.RecomputeBalance(...)` → `IBankAccountRepository.Update(...)`.
        

4. **Удаление операции**
    

- `OperationsFacade.Remove(id)`:
    
    - `IOperationRepository.Get(id)` → `Remove(id)`;
        
    - `BankAccount.RecomputeBalance(...)` → `Update(...)`.
        

5. **Аналитика**
    

- `AnalyticsScreen` → `AnalyticsFacade.IncomeMinusExpense(...)`, `GroupByCategory(...)`.
    
- Сервис берёт операции из репозитория и считает итоги.
    

6. **Импорт/Экспорт**
    

- Импорт: `ImportExportScreen` → `CsvOperationImporter.Import(stream)`  
    (каркас **Template Method**: `ReadAll → Parse → Persist`).
    
- Экспорт: `ImportExportScreen` обходит операции текущего счёта, вызывает `IExportVisitor.Visit(op)` → `GetResult()`/**WriteTo(stream)**  
    (паттерн **Visitor**).
    

## Сквозные политики

- **Идентификаторы** — value-объекты на `Guid` (`AccountId.New() => Guid.NewGuid()`).
    
- **Валидация** — на границах: `Description` нормализует `null/пробелы/длину`, `Operation.Update`/`Factory` контролируют сумму/поля.
    
- **Консистентность баланса** — счёт хранит `Balance`, но источник истины — список операций; при любой мутации операций выполняется `RecomputeBalance`.
    
- **DI и расширяемость** — все зависимости связываются в `Program.cs`, замена хранилища/логирования/импорта не требует правок домена/приложения.

---

# Применённые паттерны GoF (с привязкой к классам)

| Паттерн                      | Зачем (коротко)                                                                           | Классы/файлы                                                                                                | Где используется                                                                                    |
| ---------------------------- | ----------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **Facade**                   | Упростить вызовы сценариев, скрыть детали репозиториев и оркестрацию                      | `Application/Facades/AccountsFacade.cs`, `CategoriesFacade.cs`, `OperationsFacade.cs`, `AnalyticsFacade.cs` | Все экраны консоли (`Screens/*`) обращаются только к фасадам                                        |
| **Factory** (Simple Factory) | Централизованно создавать валидные операции, единая политика ID/валидации                 | `Domain/Factories/OperationFactory.cs`                                                                      | `OperationsFacade.Add(...)` вызывает `OperationFactory.Create(...)`                                 |
| **Command**                  | Представить пользовательский сценарий как объект (легко логировать/декорировать)          | `Application/Commands/ICommand.cs`, `AddOperationCommand.cs`                                                | `OperationsScreen` создаёт `AddOperationCommand` для добавления операции                            |
| **Decorator**                | Добавить нефункциональные срезы (тайминг/логирование) без правок команды                  | `Application/Commands/TimedCommand.cs`                                                                      | Обёртка вокруг любой команды: `new TimedCommand(cmd, logger).Execute()`                             |
| **Template Method**          | Общий каркас импорта: `ReadAll → Parse → Persist`, конкретные форматы переопределяют шаги | `Infrastructure/Import/ImporterBase.cs`, `CsvOperationImporter.cs`                                          | `ImportExportScreen` вызывает `CsvOperationImporter.Import(stream)`                                 |
| **Visitor**                  | Отделить логику обхода/экспорта данных от самих структур                                  | `Infrastructure/Export/IExportVisitor.cs`, `JsonOperationExportVisitor.cs`                                  | `ImportExportScreen` обходит операции текущего счёта: `visitor.Visit(op)` → `GetResult()/WriteTo()` |
| **Proxy**                    | Прозрачный кэш поверх репозитория счетов, write-through запись                            | `Infrastructure/Repositories/CachedBankAccountRepository.cs`                                                | В DI (`Program.cs`): `IBankAccountRepository` = `CachedBankAccountRepository(InMemory...)`          |

### Пояснения и связи

- **Facade + Command + Decorator**: экраны формируют команды (например, добавить операцию), фасады исполняют бизнес-логику, `TimedCommand` добавляет метрики выполнения.
    
- **Factory** гарантирует создание **валидной** `Operation` в одном месте — проще расширять правила.
    
- **Template Method** даёт одинаковый pipeline импорта для любых форматов (CSV/JSON/YAML) — добавляется только парсер и `Persist`.
    
- **Visitor** фиксирует формат экспорта независимо от доменных классов (можно добавить ещё посетителей для CSV/YAML).
    
- **Proxy** позволяет менять источник данных (in-memory → БД) и при этом держать быстрые чтения без изменений в приложении.

# SOLID / GRASP — компактно и с привязкой к классам

## SOLID

- **S — Single Responsibility**
    
    - `BankAccount` — инварианты счёта и пересчёт баланса (`RecomputeBalance`).
        
    - `Operation` — состояние операции и её валидное изменение (`Update`).
        
    - `OperationFactory` — единая точка создания `Operation`.
        
    - `AnalyticsService` — только расчёты (сальдо, группировка).
        
    - `*Facade` — оркестрация сценариев (CRUD/аналитика).
        
    - `CsvOperationImporter` / `JsonOperationExportVisitor` — только импорт/экспорт.
        
- **O — Open/Closed**
    
    - Новые импортеры/экспортёры добавляются без правок каркаса:  
        `ImporterBase` (Template Method), `IExportVisitor` (Visitor).
        
    - Новые команды/сценарии — через реализацию `ICommand`, `TimedCommand` уже готов их декорировать.
        
- **L — Liskov Substitution**
    
    - Любая реализация `I*Repository` (InMemory/Cached/в будущем EF Core) взаимозаменяема для фасадов/сервисов.
        
    - `IBankAccountRepository` ⇄ `InMemoryBankAccountRepository` ⇄ `CachedBankAccountRepository`.
        
- **I — Interface Segregation**
    
    - Раздельные контракты: `IBankAccountRepository`, `ICategoryRepository`, `IOperationRepository` — без «толстого» общего интерфейса.
        
- **D — Dependency Inversion**
    
    - `Application` зависит от **абстракций** (`I*Repository`, `OperationFactory`), реализации подключаются в `Program.cs` через DI (`Microsoft.Extensions.DependencyInjection`).
        

## GRASP

- **High Cohesion (высокая связность)**
    
    - `AnalyticsService` занимается только аналитикой;
        
    - `ConsoleUi` — только ввод/вывод;
        
    - `Money`, `Description`, `Id`-типы — локализуют правила форматирования/валидации.
        
- **Low Coupling (слабая связность)**
    
    - UI (`Screens/*`) знает только фасады;
        
    - Фасады знают только репозитории (через интерфейсы) и доменные сущности;
        
    - `Domain` не зависит от инфраструктуры/UI.
        
- **Controller**
    
    - `AccountsFacade`, `CategoriesFacade`, `OperationsFacade`, `AnalyticsFacade` — контроллеры прикладных сценариев.
        
- **Information Expert**
    
    - `BankAccount.RecomputeBalance` — баланс считает «владелец» данных;
        
    - `AnalyticsService.GroupByCategory` — агрегирует там, где есть знание предметной логики.
        
- **Creator**
    
    - `OperationFactory` создаёт `Operation` (знает её зависимости и правила).
        
- **Polymorphism/Indirection (опционально, но уместно отметить)**
    
    - Полиморфизм по репозиториям (`I*Repository`) и по импортёрам/экспортёрам (`ImporterBase`, `IExportVisitor`) снижает сцепление и упрощает расширение.
        

