# 🔄 Руководство по обновлению LiquidCode

## 📋 Обзор изменений

Проект был значительно улучшен со следующими критическими обновлениями:

### 1. ✅ Безопасность паролей (BCrypt)
- **Было:** SHA256 (небезопасно!)
- **Стало:** BCrypt с work factor 12
- **Действие:** После обновления все пользователи должны сбросить пароли

### 2. ✅ Global Exception Handler
- Централизованная обработка ошибок
- Структурированные JSON ответы
- Автоматическое скрытие деталей в production

### 3. ✅ Улучшенная структура БД
- ✅ Индексы на всех важных полях
- ✅ Soft Delete для User, Mission, UserSubmit
- ✅ Автоматические CreatedAt/UpdatedAt для всех таблиц
- ✅ Уникальные составные индексы

### 4. ✅ Docker Compose
- PostgreSQL с автоматической настройкой
- MinIO (S3-совместимое хранилище)
- PgAdmin для администрирования (опционально)

---

## 🚀 Быстрый старт (новая установка)

```bash
# 1. Клонировать репозиторий
git clone <your-repo-url>
cd LiquidCode

# 2. Запустить инфраструктуру
docker-compose up -d

# 3. Скопировать переменные окружения
cp .env.example .env.local

# 4. Применить миграции
cd LiquidCode
dotnet run --launch-profile migrate-db

# 5. Запустить приложение
dotnet run --launch-profile http
```

Приложение доступно на: `http://localhost:8081/swagger`

---

## 🔄 Обновление существующего проекта

### ⚠️ ВАЖНО: Создайте backup БД перед обновлением!

```bash
# Создать backup
docker exec liquidcode-postgres pg_dump -U liquidcode liquidcode_dev > backup_$(date +%Y%m%d).sql
```

### Шаг 1: Обновить код

```bash
git pull origin main
cd LiquidCode
dotnet restore
dotnet build
```

### Шаг 2: Применить миграции БД

```bash
# Применить новую миграцию
dotnet run --launch-profile migrate-db
```

### Шаг 3: Запустить Docker инфраструктуру

```bash
# Вернуться в корень проекта
cd ..

# Запустить новые сервисы
docker-compose up -d
```

### Шаг 4: Проверить приложение

```bash
cd LiquidCode
dotnet run --launch-profile http
```

---

## 🗄️ Миграция БД - Детали

### Что изменилось в БД:

#### Таблица `users`
```sql
-- Новые поля
ALTER TABLE users ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;
ALTER TABLE users ADD COLUMN deleted_at TIMESTAMP;
ALTER TABLE users ADD COLUMN created_at TIMESTAMP DEFAULT NOW();
ALTER TABLE users ADD COLUMN updated_at TIMESTAMP DEFAULT NOW();

-- Новые индексы
CREATE UNIQUE INDEX ix_users_username ON users(username);
CREATE INDEX ix_users_email ON users(email);
CREATE INDEX ix_users_is_deleted ON users(is_deleted);
```

#### Таблица `missions`
```sql
-- Новые поля
ALTER TABLE missions ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;
ALTER TABLE missions ADD COLUMN deleted_at TIMESTAMP;

-- Обновленные поля
ALTER TABLE missions ALTER COLUMN created_at TYPE TIMESTAMP;
ALTER TABLE missions ALTER COLUMN updated_at TYPE TIMESTAMP;

-- Новые индексы
CREATE INDEX ix_missions_difficulty ON missions(difficulty);
CREATE INDEX ix_missions_created_at ON missions(created_at);
CREATE INDEX ix_missions_is_deleted ON missions(is_deleted);
```

#### Таблица `user_submits`
```sql
-- Новые поля
ALTER TABLE user_submits ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;
ALTER TABLE user_submits ADD COLUMN deleted_at TIMESTAMP;
ALTER TABLE user_submits ADD COLUMN created_at TIMESTAMP DEFAULT NOW();
ALTER TABLE user_submits ADD COLUMN updated_at TIMESTAMP DEFAULT NOW();

-- Новые индексы
CREATE INDEX ix_user_submits_created_at ON user_submits(created_at);
CREATE INDEX ix_user_submits_is_deleted ON user_submits(is_deleted);
```

#### Все остальные таблицы
```sql
-- Добавлены поля для отслеживания времени
ALTER TABLE <table_name> ADD COLUMN created_at TIMESTAMP DEFAULT NOW();
ALTER TABLE <table_name> ADD COLUMN updated_at TIMESTAMP DEFAULT NOW();
```

---

## 🔐 Миграция паролей пользователей

### ⚠️ КРИТИЧНО: BCrypt несовместим с SHA256

После обновления **все существующие пароли станут невалидными**!

### Варианты решения:

#### Вариант 1: Сброс всех паролей (рекомендуется для dev)
```bash
# Очистить БД и начать заново
dotnet run --launch-profile drop-db
dotnet run --launch-profile migrate-db
```

#### Вариант 2: Миграция паролей существующих пользователей

Если у вас есть активные пользователи, создайте временный endpoint для сброса пароля или миграционный скрипт:

```csharp
// Пример: Временный endpoint для обновления пароля
[HttpPost("migrate-password")]
public async Task<IActionResult> MigratePassword([FromBody] MigratePasswordModel model)
{
    var user = await _userRepository.FindByUsernameAsync(model.Username);
    if (user == null) return NotFound();
    
    // Проверка старого пароля (SHA256)
    var oldHash = (model.OldPassword + user.Salt).ComputeSha256();
    if (oldHash != user.PassHash) return Unauthorized();
    
    // Установка нового пароля (BCrypt)
    var newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
    user.PassHash = newHash;
    user.Salt = ""; // BCrypt управляет salt внутренне
    
    await _userRepository.UpdateAsync(user);
    return Ok("Password migrated successfully");
}
```

#### Вариант 3: Массовая рассылка писем для сброса пароля

---

## 🐳 Docker Services

### Доступ к сервисам после запуска:

| Сервис | URL | Credentials |
|--------|-----|-------------|
| PostgreSQL | `localhost:5432` | `liquidcode` / `liquidcode_dev_password` |
| MinIO Console | `http://localhost:9001` | `minioadmin` / `minioadmin` |
| MinIO API | `http://localhost:9000` | `minioadmin` / `minioadmin` |
| PgAdmin | `http://localhost:5050` | `admin@liquidcode.local` / `admin` |

### Команды Docker:

```bash
# Запуск
docker-compose up -d

# Остановка
docker-compose down

# Логи
docker-compose logs -f

# Перезапуск
docker-compose restart

# Запуск с PgAdmin
docker-compose --profile tools up -d
```

---

## 📝 Проверка после обновления

### 1. Проверка БД

```bash
# Подключиться к PostgreSQL
docker exec -it liquidcode-postgres psql -U liquidcode -d liquidcode_dev

# Проверить структуру таблицы users
\d users

# Проверить индексы
\di
```

### 2. Проверка API

```bash
# Проверка health (если добавлен endpoint)
curl http://localhost:8081/health

# Проверка Swagger
открыть http://localhost:8081/swagger
```

### 3. Тестирование функционала

1. Регистрация нового пользователя
2. Вход в систему
3. Создание миссии
4. Отправка решения

---

## 🔧 Откат изменений

Если что-то пошло не так:

### 1. Откат миграции БД

```bash
cd LiquidCode
dotnet ef database update Initial
```

### 2. Восстановление из backup

```bash
# Остановить приложение
docker-compose down

# Восстановить БД
docker exec -i liquidcode-postgres psql -U liquidcode liquidcode_dev < backup_20251020.sql

# Перезапустить
docker-compose up -d
```

### 3. Откат кода

```bash
git revert HEAD
# или
git checkout <previous-commit>
```

---

## 📚 Дополнительная документация

- [DOCKER_GUIDE.md](./DOCKER_GUIDE.md) - Подробное руководство по Docker
- [ARCHITECTURE.md](./LiquidCode/ARCHITECTURE.md) - Архитектура проекта
- [README.md](./LiquidCode/README.md) - Основная документация

---

## ❓ FAQ

### Q: Нужно ли пересоздавать БД?
A: Для существующих БД - нет, миграции применятся автоматически. Для новых установок - создастся автоматически.

### Q: Что делать с существующими пользователями?
A: См. раздел "Миграция паролей пользователей" выше.

### Q: Можно ли использовать без Docker?
A: Да, установите PostgreSQL и MinIO вручную и настройте переменные окружения.

### Q: Где хранятся данные Docker?
A: В Docker volumes: `postgres_data`, `minio_data`, `pgadmin_data`

---

**Дата обновления:** 20 октября 2025  
**Версия:** 2.1.0
