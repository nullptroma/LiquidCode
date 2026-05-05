# LiquidCode

Backend API на ASP.NET Core для платформы с миссиями, сабмитами, группами, конкурсами и статьями.

## Технологии

- .NET 8 (`ASP.NET Core Web API`)
- PostgreSQL + `EntityFramework Core`
- S3-совместимое хранилище (MinIO/AWS S3)
- JWT-аутентификация
- Swagger/OpenAPI
- xUnit + Testcontainers (интеграционные тесты)

## Структура репозитория

- `LiquidCode` - основной Web API проект
- `LiquidCode.IntegrationTests` - интеграционные тесты
- `docker-compose.yml` - локальные сервисы (PostgreSQL, MinIO, опционально PgAdmin)

## Требования

- `dotnet` SDK 8.0+
- Docker + Docker Compose

## Быстрый старт (локально)

1. Поднять инфраструктуру:

```bash
docker compose up -d postgres minio minio-setup
```

2. Установить переменные окружения (минимальный набор для локального запуска):

```bash
export PG_URI="postgresql://postgres:qwef@localhost:5432/dev-db"
export JWT_ISSUER="liquid"
export JWT_AUDIENCE="audience"
export JWT_SINGING_KEY="supersecretkey_supersecretkey_supersecretkey_supersecretkey"

export S3_ACCESS_KEY="minioadmin"
export S3_SECRET_KEY="minioadmin"
export S3_ENDPOINT="http://localhost:9000"
export S3_PRIVATE_BUCKET="problems"
export S3_PUBLIC_BUCKET="problems-public"

export SERVICE_BASE_URL="http://localhost:8081"
export TESTING_MODULE_URL="http://localhost:8080/"
export SUBMIT_CALLBACK_SECRET="dev-submit-secret"
```

3. Применить миграции:

```bash
MIGRATE_ONLY=1 dotnet run --project LiquidCode
```

4. Запустить API:

```bash
dotnet run --project LiquidCode
```

5. Swagger UI будет доступен по адресу:

`http://localhost:8081/swagger`

## Работа с БД

- Применить миграции: `MIGRATE_ONLY=1 dotnet run --project LiquidCode`
- Удалить БД: `DROP_DATABASE=1 dotnet run --project LiquidCode`

## Интеграционные тесты

```bash
dotnet test LiquidCode.IntegrationTests
```

Тесты поднимают временный PostgreSQL контейнер через Testcontainers.

## Полезные команды

- Восстановить зависимости: `dotnet restore`
- Собрать решение: `dotnet build`
- Запустить все сервисы из compose: `docker compose up -d`
- Запустить PgAdmin (опционально): `docker compose --profile tools up -d pgadmin`

## Примечания

- Конфигурация читается из переменных окружения.
- В `appsettings.Development.json` есть дефолтные значения для разработки, но для стабильного локального окружения удобнее задавать их явно через env-переменные.
- Для локального MinIO имена бакетов должны соответствовать тем, что создаются в `minio-setup` (`problems`, `problems-public`).
