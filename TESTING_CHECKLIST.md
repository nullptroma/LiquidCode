# ✅ Checklist для проверки изменений

## 🚀 Быстрая проверка (5 минут)

### 1. Проверить сборку проекта
```bash
cd /home/nullptr/Documents/Gitea/LiquidCode/LiquidCode
dotnet build
# Ожидаемый результат: Build succeeded in X.Xs
```
**Статус:** ✅ Успешно

---

### 2. Запустить Docker инфраструктуру
```bash
cd /home/nullptr/Documents/Gitea/LiquidCode
docker-compose up -d
```

**Проверить:**
```bash
docker-compose ps
# Должны быть запущены:
# - liquidcode-postgres (healthy)
# - liquidcode-minio (healthy)
# - liquidcode-minio-setup (exited 0)
```

**Доступ к сервисам:**
- PostgreSQL: `localhost:5432`
- MinIO Console: http://localhost:9001 (minioadmin/minioadmin)
- MinIO API: http://localhost:9000

---

### 3. Применить миграции
```bash
cd LiquidCode
dotnet run --launch-profile migrate-db
# Ожидаемый результат: миграции применены успешно
```

**Проверка:**
```bash
# Подключиться к БД
docker exec -it liquidcode-postgres psql -U liquidcode -d liquidcode_dev

# Проверить таблицы
\dt

# Проверить структуру users
\d users
# Должны быть: is_deleted, deleted_at, created_at, updated_at

# Проверить индексы
\di
# Должно быть 13+ индексов
```

---

### 4. Запустить приложение
```bash
dotnet run --launch-profile http
# Приложение запустится на http://localhost:8081
```

**Открыть Swagger:** http://localhost:8081/swagger

---

### 5. Тест BCrypt (регистрация и вход)

**5.1 Регистрация:**
```bash
curl -X POST http://localhost:8081/authentication/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

**Ожидаемый ответ:**
```json
{
  "accessToken": "eyJhbGciOiJ...",
  "refreshToken": "..."
}
```

**5.2 Проверка пароля в БД:**
```bash
docker exec -it liquidcode-postgres psql -U liquidcode -d liquidcode_dev \
  -c "SELECT username, pass_hash FROM users WHERE username='testuser';"
```

**Проверить:**
- ✅ `pass_hash` начинается с `$2a$` или `$2b$` (BCrypt)
- ✅ НЕ SHA256 хеш (не 64 символа)

**5.3 Вход:**
```bash
curl -X POST http://localhost:8081/authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "TestPassword123!"
  }'
```

**Ожидаемый ответ:**
```json
{
  "accessToken": "eyJhbGciOiJ...",
  "refreshToken": "..."
}
```

**5.4 Проверка неверного пароля:**
```bash
curl -X POST http://localhost:8081/authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "WrongPassword"
  }'
```

**Ожидаемый ответ:** `401 Unauthorized`

---

### 6. Тест Exception Handling

**6.1 Вызвать ошибку (whoami без токена):**
```bash
curl -X GET http://localhost:8081/authentication/whoami
```

**Ожидаемый ответ:**
```json
{
  "statusCode": 401,
  "message": "Unauthorized",
  "timestamp": "2025-10-20T..."
}
```

**Проверить:**
- ✅ Структурированный JSON ответ
- ✅ Правильный HTTP статус код
- ✅ Timestamp присутствует

---

### 7. Тест Soft Delete

**7.1 Создать пользователя:**
```sql
-- В psql
INSERT INTO users (username, email, pass_hash, salt, is_deleted, created_at, updated_at)
VALUES ('deleteme', 'delete@test.com', 'hash', '', false, NOW(), NOW());
```

**7.2 Проверить, что виден:**
```sql
SELECT * FROM users WHERE username='deleteme';
-- Должен вернуть запись
```

**7.3 Мягко удалить:**
```sql
UPDATE users 
SET is_deleted = true, deleted_at = NOW() 
WHERE username='deleteme';
```

**7.4 Проверить, что скрыт (через EF Core):**
Через API или C# код:
```csharp
var user = await dbContext.Users
    .FirstOrDefaultAsync(u => u.Username == "deleteme");
// user должен быть null (благодаря global query filter)
```

**7.5 Проверить, что физически существует:**
```sql
SELECT * FROM users WHERE username='deleteme';
-- Должен вернуть запись с is_deleted=true
```

---

### 8. Тест Timestamps

**8.1 Создать миссию через API:**
(Требуется JWT токен от предыдущих шагов)

**8.2 Проверить timestamps:**
```sql
SELECT id, name, created_at, updated_at 
FROM missions 
ORDER BY id DESC LIMIT 1;
```

**Проверить:**
- ✅ `created_at` установлен
- ✅ `updated_at` равен `created_at`

**8.3 Обновить миссию:**
```sql
UPDATE missions SET name = 'Updated Name' WHERE id = <last_id>;
```

**8.4 Проверить:**
```sql
SELECT id, name, created_at, updated_at 
FROM missions WHERE id = <last_id>;
```

**Проверить:**
- ✅ `created_at` не изменился
- ✅ `updated_at` обновился автоматически

---

### 9. Тест MinIO

**9.1 Открыть MinIO Console:**
http://localhost:9001 (minioadmin/minioadmin)

**9.2 Проверить бакеты:**
- ✅ `problems` существует
- ✅ `problems-public` существует

**9.3 Загрузить тестовый файл:**
```bash
echo "test content" > test.txt
docker exec -it liquidcode-minio \
  mc cp /tmp/test.txt minio/problems/test.txt
```

**9.4 Проверить через API:**
```bash
curl http://localhost:9000/problems/test.txt
# Должен вернуть ошибку (приватный бакет)

curl http://localhost:9000/problems-public/test.txt
# Должен вернуть содержимое (если файл там есть)
```

---

## 📊 Итоговый Checklist

- [ ] ✅ Проект собирается без ошибок
- [ ] ✅ Docker контейнеры запущены и здоровы
- [ ] ✅ Миграции применились успешно
- [ ] ✅ Swagger UI доступен
- [ ] ✅ Регистрация работает с BCrypt
- [ ] ✅ Вход работает с BCrypt
- [ ] ✅ Exception Handler возвращает структурированные ответы
- [ ] ✅ Soft Delete фильтрует удалённые записи
- [ ] ✅ Timestamps обновляются автоматически
- [ ] ✅ MinIO бакеты созданы и доступны
- [ ] ✅ Индексы созданы в БД

---

## 🐛 Troubleshooting

### Проблема: Docker контейнеры не запускаются
```bash
# Проверить логи
docker-compose logs

# Перезапустить
docker-compose down
docker-compose up -d
```

### Проблема: Миграции не применяются
```bash
# Проверить строку подключения
echo $PG_URI

# Проверить, что PostgreSQL запущен
docker-compose ps postgres

# Попробовать drop и пересоздать
dotnet run --launch-profile drop-db
dotnet run --launch-profile migrate-db
```

### Проблема: BCrypt не работает
```bash
# Проверить, что пакет установлен
dotnet list package | grep BCrypt

# Переустановить
dotnet remove package BCrypt.Net-Next
dotnet add package BCrypt.Net-Next
```

### Проблема: MinIO бакеты не созданы
```bash
# Проверить логи minio-setup
docker-compose logs minio-setup

# Пересоздать вручную
docker exec -it liquidcode-minio sh
mc alias set myminio http://localhost:9000 minioadmin minioadmin
mc mb myminio/problems
mc mb myminio/problems-public
mc anonymous set download myminio/problems-public
```

---

## 🎉 Всё работает!

Если все пункты выполнены успешно, проект полностью готов к разработке!

**Следующие шаги:**
1. Ознакомиться с [ARCHITECTURE.md](LiquidCode/ARCHITECTURE.md)
2. Изучить [DOCKER_GUIDE.md](DOCKER_GUIDE.md)
3. Начать разработку новых фич!

**Удачи! 🚀**
