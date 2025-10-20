# LiquidCode - Архитектура и структура проекта

## 📁 Структура директорий

```
LiquidCode/
├── Controllers/              # API контроллеры (точка входа для HTTP запросов)
│   ├── AuthenticationController.cs    # Управление аутентификацией
│   ├── MissionsController.cs          # Управление миссиями
│   └── SubmitController.cs            # Управление сабмитами
│
├── Services/                 # Бизнес-логика (Service Layer)
│   ├── AuthService/
│   │   ├── IAuthenticationService.cs
│   │   └── AuthenticationService.cs
│   ├── MissionService/
│   │   ├── IMissionService.cs
│   │   └── MissionService.cs
│   └── SubmitService/
│       ├── ISubmitService.cs
│       └── SubmitService.cs
│
├── Repositories/             # Доступ к данным (Repository Pattern)
│   ├── IRepository.cs                 # Базовый интерфейс
│   ├── Repository.cs                  # Базовая реализация
│   ├── IUserRepository.cs
│   ├── UserRepository.cs
│   ├── IMissionRepository.cs
│   ├── MissionRepository.cs
│   ├── ISubmitRepository.cs
│   └── SubmitRepository.cs
│
├── Models/
│   ├── Database/              # EF Core модели БД
│   │   ├── DbUser.cs
│   │   ├── DbMission.cs
│   │   ├── DbUserSubmit.cs
│   │   └── ...
│   ├── Api/                   # API моделей для контроллеров (старая структура)
│   │   ├── AuthenticationController/
│   │   ├── MissionsController/
│   │   └── SubmitController/
│   ├── Dto/                   # DTO (Data Transfer Objects) - новая структура
│   │   └── CommonResponses.cs
│   └── Constants/             # Константы приложения
│       └── AppConstants.cs
│
├── Extensions/                # Методы расширения
│   └── ClaimsPrincipalExtensions.cs
│
├── Validators/               # Валидаторы данных (FluentValidation)
│   └── [валидаторы для DTOs]
│
├── Middleware/               # Middleware для обработки запросов
│   └── ExceptionHandlerMiddleware.cs
│
├── Tools/                    # Утилиты и вспомогательные функции
│   ├── StringTools.cs
│   └── BuilderExtensions.cs
│
├── Db/                       # EF Core контекст и конфигурация
│   ├── LiquidDbContext.cs
│   ├── ConnectionStringParser.cs
│   └── Migrations/
│
└── Program.cs                # Точка входа приложения
```

## 🏗️ Слои архитектуры

### 1. **Controllers Layer** (Презентационный слой)
- Обрабатывает HTTP запросы
- Валидирует входные данные
- Вызывает Services
- Возвращает HTTP ответы

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
{
    var result = await authService.LoginAsync(model, userAgent, ipAddress, cancellationToken);
    return result == null ? Unauthorized() : Ok(result);
}
```

### 2. **Services Layer** (Бизнес-логика)
- Содержит основную логику приложения
- Использует Repositories для доступа к данным
- Должна быть независима от HTTP контекста
- Тестируется без контроллеров

```csharp
public async Task<AuthTokensModel?> LoginAsync(
    LoginModel model, string userAgent, string ipAddress, CancellationToken cancellationToken)
{
    var user = await _userRepository.FindByUsernameAsync(model.Username, cancellationToken);
    var passwordHash = (model.Password + user.Salt).ComputeSha256();
    if (passwordHash != user.PassHash)
        return null;
    
    var tokens = GenerateTokens(user.Username, user.Id);
    await SaveRefreshTokenAsync(user, tokens.RefreshToken, userAgent, ipAddress, cancellationToken);
    return tokens;
}
```

### 3. **Repositories Layer** (Доступ к данным)
- Инкапсулирует логику доступа к БД
- Работает с EF Core DbContext
- Может использовать кэширование
- Легко заменяются для тестирования

```csharp
public interface IUserRepository : IRepository<DbUser>
{
    Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<int> GetRefreshTokenCountAsync(int userId, CancellationToken cancellationToken = default);
}

public class UserRepository : Repository<DbUser>, IUserRepository
{
    public async Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
```

### 4. **Models Layer** (Модели данных)
- **Database Models** (DbUser, DbMission, etc.) - модели для EF Core
- **API Models** - старые моделей API для контроллеров
- **DTOs** - объекты передачи данных для нового API
- **Constants** - константы приложения

## 🔑 Ключевые паттерны

### Repository Pattern
```csharp
// Вместо прямого доступа к DbContext в контроллере
// Используем Repository

// ❌ Плохо
public class MissionsController(LiquidDbContext db)
{
    var mission = db.Missions.Find(id);
}

// ✅ Хорошо
public class MissionsController(IMissionRepository repository)
{
    var mission = await repository.FindByIdAsync(id);
}
```

### Dependency Injection
```csharp
// В Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// В контроллере - автоматическое внедрение зависимостей
public class AuthenticationController(IAuthenticationService authService)
{
    // authService будет внедрен автоматически
}
```

### Constants Management
```csharp
// Константы вынесены в отдельный класс
public static class AppConstants
{
    public const int MaxRefreshTokensPerUser = 50;
    public const int JwtExpirationMinutes = 2;
    public const int RefreshTokenExpirationDays = 7;
}

// Использование
if (tokenCount >= AppConstants.MaxRefreshTokensPerUser)
{
    // очищаем старые токены
}
```

### Extension Methods
```csharp
// Удобное извлечение user ID из claims
public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
{
    var claim = user.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out userId);
}

// Использование в контроллере
if (!User.TryGetUserId(out var userId))
    return Unauthorized();
```

## 🔄 Поток выполнения

```
HTTP Request
    ↓
Controller (валидация + parsing)
    ↓
Service Layer (бизнес-логика)
    ↓
Repository Layer (CRUD операции)
    ↓
EF Core DbContext
    ↓
PostgreSQL Database
    ↓
... обратно вверх
    ↓
HTTP Response
```

## 📋 Конфигурация и Constants

**Старая структура (Deprecated):**
```csharp
using LiquidCode;
var key = configuration[ConfigurationStrings.JwtSigningKey]; // ❌ Deprecated
```

**Новая структура:**
```csharp
using LiquidCode.Models.Constants;
var key = configuration[ConfigurationKeys.JwtSigningKey]; // ✅ Правильно

// Константы приложения
var maxTokens = AppConstants.MaxRefreshTokensPerUser;
var jwtExpiration = AppConstants.JwtExpirationMinutes;
```

## 🧪 Тестирование

Благодаря Repository Pattern, Services легко тестируются:

```csharp
[TestClass]
public class AuthenticationServiceTests
{
    [TestMethod]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DbUser { Username = "test", PassHash = hash, Salt = salt });
        
        var service = new AuthenticationService(_config, mockRepository.Object, _logger);
        
        // Act
        var result = await service.LoginAsync(new LoginModel("test", "password"), "", "", CancellationToken.None);
        
        // Assert
        Assert.IsNotNull(result);
    }
}
```

## 🚀 Следующие улучшения

1. **Validators** - Добавить FluentValidation для валидации DTOs
2. **Exception Middleware** - Глобальная обработка ошибок
3. **Logging** - Добавить Serilog
4. **Caching** - IMemoryCache/IDistributedCache в Repositories
5. **API Documentation** - XML комментарии и Swagger
6. **Unit Tests** - Тесты для Services и Repositories
7. **AutoMapper** - Маппинг между моделями
8. **Soft Delete** - Мягкое удаление сущностей

## 📚 Ссылки на принципы

- **SOLID принципы** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Clean Code** - Читаемость, поддерживаемость, тестируемость
- **Design Patterns** - Repository, Factory, Dependency Injection, Singleton
