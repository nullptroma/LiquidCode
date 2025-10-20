# 🚀 LiquidCode - Docker Development Guide

## 📦 Быстрый старт с Docker

### 1. Запуск инфраструктуры

```bash
# Запустить PostgreSQL и MinIO
docker-compose up -d

# Проверить статус
docker-compose ps

# Просмотреть логи
docker-compose logs -f
```

### 2. Остановка и очистка

```bash
# Остановить контейнеры
docker-compose down

# Остановить и удалить данные (осторожно!)
docker-compose down -v
```

### 3. Запуск с PgAdmin (опционально)

```bash
# Запустить с инструментами администрирования
docker-compose --profile tools up -d
```

## 🔧 Доступ к сервисам

| Сервис | URL | Логин | Пароль |
|--------|-----|-------|--------|
| PostgreSQL | `localhost:5432` | `liquidcode` | `liquidcode_dev_password` |
| MinIO API | `http://localhost:9000` | `minioadmin` | `minioadmin` |
| MinIO Console | `http://localhost:9001` | `minioadmin` | `minioadmin` |
| PgAdmin | `http://localhost:5050` | `admin@liquidcode.local` | `admin` |

## 📋 Настройка приложения

### 1. Создать файл с переменными окружения

```bash
cp .env.example .env.local
```

### 2. Отредактировать `.env.local` (при необходимости)

Основные параметры уже настроены для локальной разработки.

### 3. Применить миграции БД

```bash
cd LiquidCode
dotnet run --launch-profile migrate-db
```

### 4. Запустить приложение

```bash
dotnet run --launch-profile http
```

Приложение будет доступно на `http://localhost:8081/swagger`

## 🗄️ Подключение к PostgreSQL

### Через psql
```bash
psql -h localhost -U liquidcode -d liquidcode_dev
# Пароль: liquidcode_dev_password
```

### Через PgAdmin
1. Открыть `http://localhost:5050`
2. Войти: `admin@liquidcode.local` / `admin`
3. Добавить сервер:
   - Host: `postgres` (если внутри Docker) или `localhost` (если снаружи)
   - Port: `5432`
   - Username: `liquidcode`
   - Password: `liquidcode_dev_password`

## 🪣 Работа с MinIO

### Через веб-консоль
1. Открыть `http://localhost:9001`
2. Войти: `minioadmin` / `minioadmin`
3. Бакеты `problems` и `problems-public` уже созданы

### Через MinIO Client (mc)
```bash
# Настроить alias
mc alias set local http://localhost:9000 minioadmin minioadmin

# Просмотреть бакеты
mc ls local

# Загрузить файл
mc cp myfile.zip local/problems/

# Скачать файл
mc cp local/problems/myfile.zip ./
```

## 🔄 Миграции базы данных

### Создать новую миграцию
```bash
cd LiquidCode
dotnet ef migrations add MigrationName
```

### Применить миграции
```bash
dotnet run --launch-profile migrate-db
```

### Откатить миграцию
```bash
dotnet ef database update PreviousMigrationName
```

### Удалить БД и пересоздать
```bash
dotnet run --launch-profile drop-db
dotnet run --launch-profile migrate-db
```

## 🐛 Отладка

### Просмотр логов контейнеров
```bash
# Все сервисы
docker-compose logs -f

# Конкретный сервис
docker-compose logs -f postgres
docker-compose logs -f minio
```

### Подключение к контейнеру
```bash
# PostgreSQL
docker exec -it liquidcode-postgres psql -U liquidcode -d liquidcode_dev

# MinIO
docker exec -it liquidcode-minio sh
```

### Проверка здоровья сервисов
```bash
# Статус всех контейнеров
docker-compose ps

# Здоровье конкретного сервиса
docker inspect liquidcode-postgres | grep Health -A 10
```

## 📊 Полезные команды

### Очистить неиспользуемые ресурсы Docker
```bash
docker system prune -a
docker volume prune
```

### Пересоздать контейнеры
```bash
docker-compose up -d --force-recreate
```

### Экспорт/импорт данных PostgreSQL
```bash
# Экспорт
docker exec liquidcode-postgres pg_dump -U liquidcode liquidcode_dev > backup.sql

# Импорт
docker exec -i liquidcode-postgres psql -U liquidcode liquidcode_dev < backup.sql
```

## 🎯 Production замечания

**⚠️ ВАЖНО:** Текущая конфигурация предназначена **только для разработки**!

Для production необходимо:
1. ✅ Изменить все пароли
2. ✅ Использовать Docker secrets или .env файлы
3. ✅ Настроить SSL/TLS для PostgreSQL
4. ✅ Настроить HTTPS для MinIO
5. ✅ Использовать внешние volume для данных
6. ✅ Настроить backup стратегию
7. ✅ Ограничить доступ к портам через firewall

---

**Счастливой разработки! 🚀**
