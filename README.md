# 🎯 LiquidCode Platform

> Современная платформа для создания, публикации и решения программистских задач

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🚀 Быстрый старт

```bash
# 1. Клонировать репозиторий
git clone <your-repo-url>
cd LiquidCode

# 2. Запустить инфраструктуру (PostgreSQL + MinIO)
docker-compose up -d

# 3. Скопировать переменные окружения
cp .env.example .env.local

# 4. Применить миграции БД
cd LiquidCode
dotnet run --launch-profile migrate-db

# 5. Запустить приложение
dotnet run --launch-profile http
```

**Приложение доступно на:** `http://localhost:8081/swagger` 🎉

---

## 📋 Возможности

- ✅ **Регистрация и авторизация** через JWT токены с BCrypt хешированием
- ✅ **Загрузка задач** в виде ZIP архивов с описанием на разных языках
- ✅ **Просмотр задач** с полной информацией и фильтрацией
- ✅ **Отправка решений** на проверку в тестирующий модуль
- ✅ **Получение результатов** тестирования в реальном времени
- ✅ **S3-совместимое хранилище** для файлов задач (MinIO)
- ✅ **Soft Delete** для безопасного удаления данных
- ✅ **Автоматические временные метки** для всех сущностей

---

## 🏗️ Архитектура

```
┌─────────────────────────────────┐
│   Controllers (API Layer)       │ ← HTTP запросы
├─────────────────────────────────┤
│   Services (Business Logic)     │ ← Бизнес-логика
├─────────────────────────────────┤
│   Repositories (Data Access)    │ ← Доступ к БД
├─────────────────────────────────┤
│   EF Core + PostgreSQL          │ ← База данных
└─────────────────────────────────┘
```

**Паттерны:** Repository Pattern, Dependency Injection, Middleware Pipeline

**Подробнее:** [LiquidCode/ARCHITECTURE.md](./LiquidCode/ARCHITECTURE.md)

---

## 🐳 Docker сервисы

| Сервис | Порт | Описание |
|--------|------|----------|
| **PostgreSQL** | `5432` | База данных |
| **MinIO API** | `9000` | S3-совместимое хранилище |
| **MinIO Console** | `9001` | Веб-интерфейс MinIO |
| **PgAdmin** | `5050` | Управление БД (опционально) |

**Подробнее:** [DOCKER_GUIDE.md](./DOCKER_GUIDE.md)

---

## 🔐 Безопасность

### ✅ Реализовано

- **BCrypt** для хеширования паролей (work factor 12)
- **JWT** токены с истечением срока действия
- **Refresh tokens** с автоматической очисткой старых
- **Global exception handler** скрывает детали в production
- **Soft delete** вместо физического удаления данных

### 🔜 Планируется

- Rate limiting для защиты от брутфорса
- HTTPS/SSL в production
- CSRF protection
- Валидация загружаемых файлов

---

## 📊 Структура БД

### Основные таблицы

- **Users** - пользователи с BCrypt хешированием паролей
- **RefreshTokens** - токены для обновления JWT
- **Missions** - задачи/миссии с метаданными
- **MissionsTextData** - переводы описаний задач
- **UserSubmits** - попытки решения пользователей
- **Solutions** - результаты тестирования

**Улучшения:**
- 13+ индексов для оптимизации запросов
- Soft delete для User, Mission, UserSubmit
- Автоматические CreatedAt/UpdatedAt для всех таблиц

---

## 📚 Документация

| Файл | Описание |
|------|----------|
| [README.md](./LiquidCode/README.md) | Основная документация API |
| [ARCHITECTURE.md](./LiquidCode/ARCHITECTURE.md) | Детали архитектуры проекта |
| [DOCKER_GUIDE.md](./DOCKER_GUIDE.md) | Работа с Docker инфраструктурой |
| [UPGRADE_GUIDE.md](./UPGRADE_GUIDE.md) | Обновление существующего проекта |
| [IMPROVEMENTS_SUMMARY.md](./IMPROVEMENTS_SUMMARY.md) | Обзор всех улучшений v2.1.0 |

---

## 🛠️ Технологии

### Backend
- **.NET 8.0** - основной фреймворк
- **ASP.NET Core** - веб API
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL 16** - реляционная БД
- **JWT Bearer** - аутентификация
- **BCrypt.Net** - хеширование паролей

### Инфраструктура
- **Docker Compose** - оркестрация контейнеров
- **MinIO** - S3-совместимое хранилище
- **PgAdmin** - управление PostgreSQL

### Инструменты разработки
- **Swagger/OpenAPI** - документация API
- **EF Core Migrations** - версионирование схемы БД
- **User Secrets** - безопасное хранение секретов

---

## 🔧 Разработка

### Требования
- .NET 8.0 SDK
- Docker и Docker Compose
- PostgreSQL 16+ (или через Docker)

### Команды

```bash
# Сборка
dotnet build

# Запуск
dotnet run --launch-profile http

# Миграции
dotnet ef migrations add MigrationName
dotnet run --launch-profile migrate-db

# Тесты (когда будут добавлены)
dotnet test
```

### Переменные окружения

См. файл `.env.example` для полного списка.

Основные:
- `PG_URI` - строка подключения к PostgreSQL
- `JWT_SINGING_KEY` - ключ для подписи JWT
- `S3_ENDPOINT` - URL MinIO сервера

---

## 📈 Roadmap

### v2.2.0 (Ближайшее будущее)
- [ ] FluentValidation для валидации DTO
- [ ] Unit тесты для Services и Repositories
- [ ] Serilog для структурированного логирования
- [ ] Health Checks endpoint
- [ ] Rate Limiting middleware

### v2.3.0 (Планируется)
- [ ] AutoMapper для маппинга моделей
- [ ] CI/CD pipeline (GitHub Actions / Gitea Actions)
- [ ] Integration тесты
- [ ] Performance benchmarks
- [ ] Кэширование (Redis)

### v3.0.0 (Будущее)
- [ ] GraphQL API
- [ ] WebSockets для real-time обновлений
- [ ] Микросервисная архитектура
- [ ] Kubernetes deployment
- [ ] Мониторинг (Prometheus + Grafana)

---

## 🤝 Contribution

Мы приветствуем вклад в проект!

### Как начать?
1. Fork репозитория
2. Создать feature branch (`git checkout -b feature/amazing-feature`)
3. Commit изменений (`git commit -m 'Add amazing feature'`)
4. Push в branch (`git push origin feature/amazing-feature`)
5. Открыть Pull Request

### Соглашения
- Используйте async/await для асинхронных операций
- Внедряйте зависимости через конструктор (DI)
- Пишите XML комментарии к публичным методам
- Логируйте важные события
- Следуйте существующему стилю кода

---

## 📝 История версий

### v2.1.0 (20 октября 2025) - Критические улучшения
- ✅ BCrypt для хеширования паролей (замена SHA256)
- ✅ Global Exception Handler middleware
- ✅ Улучшенная структура БД (индексы, soft delete, timestamps)
- ✅ Docker Compose для локальной разработки
- ✅ Расширенная документация

### v2.0.0 (Ранее) - Рефакторинг архитектуры
- ✅ Repository Pattern
- ✅ Service Layer
- ✅ Dependency Injection
- ✅ Улучшенная структура проекта

### v1.0.0 (Начало)
- ✅ Базовый функционал
- ✅ JWT аутентификация
- ✅ CRUD операции

**Полная история:** см. [IMPROVEMENTS_SUMMARY.md](./IMPROVEMENTS_SUMMARY.md)

---

## 📞 Контакты

- **Issues:** [Создать issue](../../issues)
- **Pull Requests:** [Открыть PR](../../pulls)
- **Документация:** См. папку `/LiquidCode/`

---

## 📄 Лицензия

Этот проект лицензирован под MIT License - см. файл [LICENSE](LICENSE) для деталей.

---

<div align="center">

**Сделано с ❤️ командой LiquidCode**

[![GitHub stars](https://img.shields.io/github/stars/yourusername/liquidcode?style=social)](https://github.com/yourusername/liquidcode)
[![GitHub forks](https://img.shields.io/github/forks/yourusername/liquidcode?style=social)](https://github.com/yourusername/liquidcode/fork)

</div>
