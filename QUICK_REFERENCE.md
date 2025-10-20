# 🚀 Быстрая справка команд

## 🛠️ Сборка и запуск

```bash
# Перейти в проект
cd LiquidCode/LiquidCode

# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build

# Запустить приложение с Swagger
dotnet run --launch-profile http

# Запустить с HTTPS
dotnet run --launch-profile https

# Применить миграции БД
dotnet run --launch-profile migrate-db

# Очистить БД (внимание!)
dotnet run --launch-profile drop-db
```

## 🗄️ База данных

```bash
# Запустить PostgreSQL в Docker (используя скрипт)
bash run-pgsql-docker.sh

# Проверить что Docker запущен
docker ps | grep postgres

# Подключиться к БД
psql postgresql://postgres:password@localhost:5432/liquidcode

# Остановить PostgreSQL
docker stop liquidcode-db
```

## 🧪 Тестирование

```bash
# Запустить все тесты
dotnet test

# Запустить с покрытием
dotnet test --collect:"XPlat Code Coverage"

# Запустить тесты конкретного проекта
dotnet test Tests/Services.Tests.csproj
```

## 📦 Публикация

```bash
# Создать Release build
dotnet publish -c Release -o ./publish

# Запустить опубликованный проект
./publish/LiquidCode

# Создать Docker образ
docker build -t liquidcode:latest .

# Запустить Docker образ
docker run -p 8081:8081 liquidcode:latest
```

## 🔧 IDE команды

### Visual Studio
```
Debug → Start Debugging (F5)
Build → Build Solution (Ctrl+Shift+B)
Tools → NuGet Package Manager
```

### VS Code
```
Terminal → New Terminal (Ctrl+`)
Debug → Start Debugging (F5)
View → Command Palette (Ctrl+Shift+P)
```

## 📚 Документация

```bash
# Читать основные документы
cat ARCHITECTURE.md          # Архитектура проекта
cat MIGRATION.md             # План миграции
cat README.md                # Описание проекта
cat GETTING_STARTED.md       # Инструкция по запуску
cat REFACTORING_SUMMARY.md   # Итоги рефакторинга
```

## 🔍 Отладка

```bash
# Запустить с подробным логированием
dotnet run --launch-profile http -- --verbose

# Просмотреть логи
tail -f bin/Debug/net8.0/logs.txt

# Отладка в VS Code
# F5 → Выбрать ".NET Core Launch (web)" → F5 для запуска
```

## 🐛 Решение проблем

```bash
# Очистить build кэш
dotnet clean

# Полная пересборка
dotnet clean && dotnet build

# Обновить пакеты
dotnet package update

# Восстановить зависимости заново
rm -rf bin obj
dotnet restore

# Проверить версию .NET
dotnet --version
```

## 📝 Git команды

```bash
# Просмотреть изменения
git status

# Добавить все изменения
git add .

# Коммит
git commit -m "Describe changes"

# Отправить на сервер
git push

# Получить обновления
git pull

# Просмотреть историю
git log --oneline -10
```

## 🌐 API тестирование

```bash
# Регистрация
curl -X POST "http://localhost:8081/authentication/register" \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Pass123"}'

# Логин
curl -X POST "http://localhost:8081/authentication/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"Pass123"}'

# WhoAmI (требует JWT токен)
curl -X GET "http://localhost:8081/authentication/whoami" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Список миссий
curl "http://localhost:8081/missions/get-missions-list?pageSize=10&page=0"

# Swagger UI
# Откройте в браузере: http://localhost:8081/swagger
```

## 🎯 Работа с контроллерами

### AuthenticationController
```
POST   /authentication/register  - Регистрация
POST   /authentication/login     - Вход
POST   /authentication/refresh   - Обновить токен
GET    /authentication/whoami    - Кто я
```

### MissionsController
```
POST   /missions/upload                        - Загрузить миссию
GET    /missions/get-missions-list             - Список миссий
GET    /missions/get-mission-texts            - Текст миссии
GET    /missions/get-mission-download-link    - Ссылка на скачивание
```

### SubmitController
```
POST   /submit/submit          - Отправить решение
GET    /submit/get-submission  - Получить сабмит
GET    /submit/get-results     - Результаты
```

## 📊 Переменные окружения

```bash
# Установить через .env файл
export JWT_ISSUER=LiquidCode
export JWT_AUDIENCE=LiquidCodeClient
export JWT_SINGING_KEY=your_secret_key_here
export PG_URI=postgresql://postgres:password@localhost:5432/liquidcode

# Или через User Secrets
dotnet user-secrets set "JWT_SINGING_KEY" "your_key"
dotnet user-secrets set "PG_URI" "postgresql://..."
```

## 🚨 Важные файлы

| Файл | Назначение |
|------|-----------|
| `Program.cs` | Конфигурация приложения |
| `appsettings.json` | Настройки |
| `launchSettings.json` | Профили запуска |
| `LiquidCode.csproj` | Конфигурация проекта |
| `Db/LiquidDbContext.cs` | EF Core контекст |
| `Models/Constants/AppConstants.cs` | Константы |

## 💾 Резервные копии

```bash
# Создать дамп БД
pg_dump postgresql://postgres:password@localhost/liquidcode > backup.sql

# Восстановить из дампа
psql postgresql://postgres:password@localhost/liquidcode < backup.sql

# Архивировать проект
zip -r liquidcode-backup.zip LiquidCode/
```

---

**Справка актуальна для версии 2.0.0 (20 октября 2025)**
