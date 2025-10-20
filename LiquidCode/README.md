# 🎯 LiquidCode - Платформа для решения программистских задач

## 📋 Описание проекта

LiquidCode - это веб-платформа для создания, публикации и решения программистских задач. Пользователи могут:
- **Регистрироваться и авторизоваться** через JWT токены
- **Загружать задачи** в виде ZIP архивов с описанием на разных языках
- **Просматривать задачи** с полной информацией
- **Отправлять решения** на проверку
- **Получать результаты** тестирования

## 🏗️ Архитектура

Проект использует **трехслойную архитектуру** (3-Tier Architecture):

```
┌─────────────────────────────────┐
│   Controllers (API Layer)       │ ← HTTP запросы
├─────────────────────────────────┤
│   Services (Business Logic)     │ ← Бизнес-логика
├─────────────────────────────────┤
│   Repositories (Data Access)    │ ← Доступ к БД
├─────────────────────────────────┤
│   EF Core + PostgreSQL          │ ← БД
└─────────────────────────────────┘
```

### Компоненты

| Папка | Описание |
|-------|---------|
| `Controllers/` | API endpoints для HTTP запросов |
| `Services/` | Бизнес-логика (аутентификация, миссии, сабмиты) |
| `Repositories/` | Доступ к данным, Repository Pattern |
| `Models/` | Модели БД, DTOs, константы |
| `Extensions/` | Методы расширения (extension methods) |
| `Middleware/` | Middleware для обработки запросов |
| `Validators/` | Валидаторы для входных данных |
| `Db/` | EF Core DbContext и миграции |
| `Tools/` | Утилиты и вспомогательные функции |

## 🚀 Быстрый старт

### Требования
- .NET 8.0
- PostgreSQL 12+
- Docker (опционально для БД)

### Установка и запуск

1. **Клонировать репозиторий**
```bash
git clone https://gitea.example.com/repo/LiquidCode.git
cd LiquidCode/LiquidCode
```

2. **Установить зависимости**
```bash
dotnet restore
```

3. **Запустить PostgreSQL (Docker)**
```bash
bash run-pgsql-docker.sh
```

4. **Применить миграции БД**
```bash
dotnet run --launch-profile migrate-db
```

5. **Запустить приложение**
```bash
dotnet run --launch-profile http
```

Приложение будет доступно на `http://localhost:8081`

Swagger UI: `http://localhost:8081/swagger`

## 📚 API Endpoints

### Аутентификация

```http
POST /authentication/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "securePassword123"
}
```

```http
POST /authentication/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "securePassword123"
}
```

```http
POST /authentication/refresh
Content-Type: application/json

{
  "refreshToken": "token_string_here"
}
```

```http
GET /authentication/whoami
Authorization: Bearer eyJhbGc...
```

### Миссии

```http
POST /missions/upload
Authorization: Bearer eyJhbGc...
Content-Type: multipart/form-data

file: missions.zip
name: "Сортировка массива"
difficulty: 3
```

```http
GET /missions/get-missions-list?pageSize=10&page=0
```

```http
GET /missions/get-mission-texts?id=1&language=russian
```

```http
GET /missions/get-mission-download-link?id=1
```

### Сабмиты

```http
POST /submit/submit
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "missionId": 1,
  "sourceCode": "...",
  "language": "cpp"
}
```

## 🔧 Конфигурация

### Переменные окружения

```bash
# JWT конфигурация
JWT_ISSUER=LiquidCode
JWT_AUDIENCE=LiquidCodeClient
JWT_SINGING_KEY=your_very_long_secret_key_at_least_256_bits_long

# БД
PG_URI=postgresql://user:password@localhost:5432/liquidcode

# S3 (Minio или AWS)
S3_ACCESS_KEY=minioadmin
S3_SECRET_KEY=minioadmin
S3_ENDPOINT=http://localhost:9000
S3_PUBLIC_BUCKET=problems-public
S3_PRIVATE_BUCKET=problems

# Тестирующий модуль
TESTING_MODULE_URL=http://localhost:5000

# Флаги запуска
MIGRATE_ONLY=0
DROP_DATABASE=0
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### User Secrets (Development)

```bash
# Установить секреты
dotnet user-secrets init
dotnet user-secrets set "JWT_SINGING_KEY" "your_secret_key_here"
dotnet user-secrets set "PG_URI" "postgresql://..."
```

## 🗄️ Структура БД

### Таблицы

| Таблица | Описание |
|---------|---------|
| `Users` | Пользователи |
| `RefreshTokens` | Refresh токены для авторизации |
| `Missions` | Задачи/миссии |
| `MissionsTextData` | Текстовое описание миссий на разных языках |
| `UserSubmits` | Сабмиты пользователей (попытки решения) |
| `Solutions` | Результаты тестирования сабмитов |

### Связи

```
Users (1) ──────→ (M) RefreshTokens
Users (1) ──────→ (M) Missions (как автор)
Users (1) ──────→ (M) UserSubmits
Missions (1) ────→ (M) MissionsTextData
Missions (1) ────→ (M) UserSubmits
UserSubmits (1) ─→ (1) Solutions
```

## 🧪 Тестирование

### Запуск unit тестов
```bash
dotnet test
```

### Запуск с покрытием
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Безопасность

### Что уже реализовано
- ✅ Хеширование паролей с солью (SHA256)
- ✅ JWT токены для аутентификации
- ✅ Refresh токены с истечением
- ✅ Ограничение количества активных токенов (50 на пользователя)
- ✅ CORS политика

### Что нужно добавить
- ⏳ Rate limiting
- ⏳ HTTPS/SSL в продакшене
- ⏳ CSRF protection
- ⏳ Валидация файлов при загрузке
- ⏳ Скан на вредоносный код

## 📈 Производительность

### Оптимизации
- ✅ Асинхронные операции везде
- ✅ Pagination для больших списков
- ⏳ Кэширование часто используемых данных
- ⏳ Индексы в БД на часто запрашиваемые поля
- ⏳ Lazy loading для связанных сущностей

## 📝 Логирование

Логирование настроено в каждом Service:

```csharp
_logger.LogInformation("User registered: {Username}", username);
_logger.LogWarning("Login failed: {Username}", username);
_logger.LogError(ex, "Database error");
```

Логи выводятся в консоль, можно настроить Serilog для сохранения в файлы.

## 🤝 Contribution

Перед разработкой прочитайте:
- [`ARCHITECTURE.md`](./ARCHITECTURE.md) - Принципы архитектуры
- [`MIGRATION.md`](./MIGRATION.md) - План миграции

### Соглашения о кодировании
- Используйте `async/await` для асинхронных операций
- Внедряйте зависимости через конструктор (DI)
- Логируйте важные события
- Пишите XML комментарии к публичным методам
- Используйте `CancellationToken` для долгих операций

## 📚 Полезные ресурсы

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

## 📞 Контакты

- **Автор**: [Your Name]
- **Email**: your.email@example.com
- **GitHub**: [Your GitHub Profile]

## 📄 Лицензия

MIT License - смотрите файл LICENSE для подробностей.

---

**Последнее обновление**: 20 октября 2025
**Версия**: 2.0.0 (Рефакторинг архитектуры)
