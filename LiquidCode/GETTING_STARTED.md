# 🚀 Инструкция по запуску LiquidCode

## Системные требования

- **ОС**: Windows, macOS, Linux (в примерах используется зш для Linux/macOS)
- **.NET**: 8.0 SDK
- **PostgreSQL**: 12+ (или Docker)
- **RAM**: минимум 2GB
- **Disk**: минимум 1GB свободного места

## ⚙️ Этап 1: Подготовка окружения

### 1.1 Установить .NET 8 SDK
```bash
# macOS (homebrew)
brew install dotnet

# Ubuntu/Debian
sudo apt-get install dotnet-sdk-8.0

# Windows
# Скачать с https://dotnet.microsoft.com/download/dotnet/8.0
```

### 1.2 Проверить установку
```bash
dotnet --version
# Должно вывести 8.x.x
```

### 1.3 Установить PostgreSQL (опция 1: Docker - рекомендуется)
```bash
bash run-pgsql-docker.sh
```

### 1.3 Альтернатива: Установить PostgreSQL локально
```bash
# macOS
brew install postgresql@15
brew services start postgresql@15

# Ubuntu/Debian
sudo apt-get install postgresql postgresql-contrib
sudo systemctl start postgresql

# Windows
# Скачать установщик с https://www.postgresql.org/download/windows/
```

## 📝 Этап 2: Конфигурация

### 2.1 Установить переменные окружения

**Способ 1: Через .env файл (рекомендуется для development)**

Создать файл `.env` в корне проекта:
```bash
cd /home/nullptr/Documents/Gitea/LiquidCode/LiquidCode
nano .env
```

Добавить:
```env
# JWT конфигурация
JWT_ISSUER=LiquidCode
JWT_AUDIENCE=LiquidCodeClient
JWT_SINGING_KEY=aVeryLongSecretKeyAtLeast256BitsLongForSecurityPurposesAreMetWithThisLengthRequirement

# База данных
PG_URI=postgresql://postgres:password@localhost:5432/liquidcode

# S3 (если используется Docker Minio - см. run-pgsql-docker.sh)
S3_ACCESS_KEY=minioadmin
S3_SECRET_KEY=minioadmin
S3_ENDPOINT=http://localhost:9000
S3_PUBLIC_BUCKET=problems-public
S3_PRIVATE_BUCKET=problems

# Тестирующий модуль (если используется)
TESTING_MODULE_URL=http://localhost:5000

# Флаги запуска
MIGRATE_ONLY=0
DROP_DATABASE=0
```

**Способ 2: Через User Secrets (безопаснее для production)**
```bash
cd LiquidCode
dotnet user-secrets init
dotnet user-secrets set "JWT_ISSUER" "LiquidCode"
dotnet user-secrets set "JWT_AUDIENCE" "LiquidCodeClient"
dotnet user-secrets set "JWT_SINGING_KEY" "your_secret_key_here"
dotnet user-secrets set "PG_URI" "postgresql://postgres:password@localhost:5432/liquidcode"
```

### 2.2 Проверить подключение к БД

```bash
# Проверить подключение PostgreSQL
psql postgresql://postgres:password@localhost:5432/liquidcode

# Должно подключиться, если нет - проверить БД
```

## 🔨 Этап 3: Подготовка БД

### 3.1 Применить миграции

```bash
cd LiquidCode

# Применить миграции (создать таблицы)
dotnet run --launch-profile migrate-db

# Должно вывести: "Migration is complete!"
```

### 3.2 Опционально: Очистить БД (для тестирования)
```bash
# Удалить все таблицы (ОСТОРОЖНО!)
dotnet run --launch-profile drop-db

# Потом снова применить миграции
dotnet run --launch-profile migrate-db
```

## ▶️ Этап 4: Запуск приложения

### 4.1 Запуск с Swagger UI
```bash
cd LiquidCode
dotnet run --launch-profile http

# Приложение будет доступно на http://localhost:8081
# Swagger UI: http://localhost:8081/swagger
```

### 4.2 Запуск с HTTPS
```bash
dotnet run --launch-profile https

# HTTPS: https://localhost:7066
# HTTP: http://localhost:5034
```

### 4.3 Запуск в production режиме
```bash
dotnet run -c Release

# По умолчанию на http://localhost:5000
```

## 🧪 Этап 5: Тестирование

### 5.1 Проверить здоровье приложения
```bash
curl http://localhost:8081/health
```

### 5.2 Проверить Swagger
Откройте в браузере: http://localhost:8081/swagger

### 5.3 Тестовый запрос (Register)
```bash
curl -X POST "http://localhost:8081/authentication/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPassword123"
  }'

# Ожидаемый ответ:
# {
#   "accessToken": "eyJhbGc...",
#   "refreshToken": "aBcDeF..."
# }
```

### 5.4 Тестовый запрос (Login)
```bash
curl -X POST "http://localhost:8081/authentication/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "TestPassword123"
  }'
```

### 5.5 Тестовый запрос (WhoAmI)
```bash
# Замените ACCESS_TOKEN на токен из ответа login
curl -X GET "http://localhost:8081/authentication/whoami" \
  -H "Authorization: Bearer ACCESS_TOKEN"

# Ожидаемый ответ:
# {"username":"testuser"}
```

## 📊 Мониторинг и отладка

### Логирование
Логи выводятся в консоль по умолчанию. Уровни:
- **Information** - обычные события (login, register)
- **Warning** - потенциальные проблемы (failed login)
- **Error** - ошибки (исключения в БД)

### Профилирование
```bash
# Запустить с профилировкой
dotnet run --launch-profile http --verbose
```

### Отладка в VS Code
1. Откройте Debug панель (Ctrl+Shift+D)
2. Выберите ".NET Core Launch (web)"
3. Нажмите F5

## ❌ Решение проблем

### Проблема: "Connection refused" к БД
```
Решение:
1. Проверить, запущена ли PostgreSQL: docker ps
2. Проверить переменную PG_URI
3. Переподключиться к БД
```

### Проблема: "Certificate error"
```
Решение (для development):
Добавить в appsettings.Development.json:
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "path/to/cert.pfx",
        "Password": "password"
      }
    }
  }
}
```

### Проблема: "No such table"
```
Решение:
Применить миграции:
dotnet run --launch-profile migrate-db
```

### Проблема: "Port 8081 is already in use"
```
Решение:
1. Найти процесс: lsof -i :8081
2. Убить процесс: kill -9 <PID>
3. ИЛИ изменить порт в launchSettings.json
```

## 📦 Публикация (Deployment)

### Создать Release build
```bash
cd LiquidCode
dotnet publish -c Release -o ./publish

# Архив для развертывания
cd publish
zip -r ../liquidcode-release.zip .
```

### Развертывание на Linux сервер
```bash
# На сервере
wget https://your-repo/liquidcode-release.zip
unzip liquidcode-release.zip
chmod +x LiquidCode

# Запуск
./LiquidCode
# или через systemd
sudo systemctl start liquidcode
```

## 🔐 Безопасность (Production)

1. **Изменить JWT ключ** - используйте длинный случайный ключ
2. **Изменить пароли БД** - не используйте 'password'
3. **Настроить HTTPS** - получить сертификат (Let's Encrypt)
4. **Настроить CORS** - ограничить доступ по доменам
5. **Включить Rate Limiting** - защита от DDoS
6. **Использовать Secrets Manager** - AWS Secrets Manager, Azure Key Vault

## 📚 Полезные команды

```bash
# Проверить версию .NET
dotnet --version

# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build

# Запустить тесты
dotnet test

# Очистить build артефакты
dotnet clean

# Получить информацию о проекте
dotnet list package

# Обновить NuGet пакеты
dotnet package update
```

## 🆘 Поддержка

Если у вас есть проблемы:
1. Проверьте логи консоли
2. Прочитайте [`README.md`](./README.md)
3. Посмотрите [`ARCHITECTURE.md`](./ARCHITECTURE.md)
4. Создайте issue на GitHub/Gitea

---

**Готово! Приложение должно работать на http://localhost:8081** 🎉
