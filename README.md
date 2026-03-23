# Bitstore

Платформа для продажи/покупки музыкальных битов, площадка-посредник между авторами музыки и покупателями

![Статус разработки](https://img.shields.io/badge/status-active-brightgreen)
![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net)
![Architecture](https://img.shields.io/badge/architecture-clean-blue)

## Содержание
- [Технологии](#технологии)
- [Использование](#использование)
- [Разработка](#разработка)
- [Тестирование](#тестирование)
- [Deploy и CI/CD](#deploy-и-cicd)
- [FAQ](#faq)
- [To do](#to-do)
- [Команда проекта](#команда-проекта)

## Технологии

- **.NET 10** — основная платформа разработки
- **C#** — язык программирования
- **Entity Framework Core** — ORM для работы с базой данных
- **PostgreSQL** — реляционная база данных
- **ASP.NET Core Web API** — построение REST API
- **JWT Bearer** — аутентификация и авторизация
- **Clean Architecture** — архитектурный подход с разделением на слои (Core, Application, DataAccess, Infrastructure)

## Использование

### Предварительные требования

Для запуска проекта необходимы:
- .NET 10 SDK
- PostgreSQL (локально или в контейнере)

### Установка и запуск

1. **Клонируйте репозиторий:**
```bash
git clone https://github.com/sgc433/bitstore.git
cd bitstore
```

2. **Настройте базу данных:**
   
   Обновите строку подключения в `Bitstore/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=bitstore_db;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Примените миграции:**
```bash
dotnet ef database update --project Bitstore.DataAccess --startup-project Bitstore
```

4. **Запустите приложение:**
```bash
dotnet run --project Bitstore
```

После запуска API со swagger будет доступно по адресу `http://localhost:5167/swagger/index.html`.

**Пример запроса для получения списка битов:**

```http
GET /api/Beat/allbeats
Host: localhost:5167
Authorization: Bearer <ваш_jwt_токен>
```

**Регистрация нового пользователя:**

```http
POST /api/Auth/register
Content-Type: application/json
{
  "username": "someusername",
  "email": "user@example.com",
  "password": "securePassword123"
}
```

## Разработка

### Требования

Для локальной разработки необходимы:
- .NET 10 SDK
- PostgreSQL (локально или в контейнере)
- Visual Studio 2022 / Rider / VS Code

### Установка зависимостей

Восстановите NuGet-пакеты:

```bash
dotnet restore
```

### Структура проекта

Проект построен на принципах Clean Architecture:

```
Bitstore.sln
├── Bitstore/                    # Api слой
│   ├── Controllers/             # Контроллеры
|   ├── appsettings.json/        # Конфигурация приложения
│   ├── Validators/              # Валидаторы DTO
│   └── Program.cs/              # Основной скрипт запуска (Middleware, DI container и builder)
|
├── Bitstore.Application/        # Слой приложения
│   ├── DTOs/                    # Объекты передачи данных
│   ├── Services/                # Сервисы приложения
│   └── Interfaces/              # Интерфейсы сервисов
│
├── Bitstore.Core/               # Доменный слой
│   ├── Models/                  # Доменные модели
│   ├── Abstractions/            # Доменные интерфейсы
│   
├── Bitstore.DataAccess/         # Слой доступа к данным
│   ├── Repositories/            # Репозитории для работы с сущностями в бд
│   ├── Configurations/          # Конфигурация сущностей Entity Framework Core
│   └── Entities                 # Сущности в бд
└── Bitstore.Infrastructure/     # Инфраструктурный слой
    ├── JwtOptions.cs/           # Настройки JWT
    ├── JwtProvider.cs/          # Создание JWT токена
    └── PasswordHasher           # Хеширование пароля
```

### Применение миграций

Для создания новой миграции:

```bash
dotnet ef migrations add MigrationName --project Bitstore.DataAccess --startup-project Bitstore
```

## Тестирование

В проекте планируется покрытие модульными и интеграционными тестами с использованием xUnit и Moq.

Для запуска тестов (после их добавления):

```bash
dotnet test
```

## Deploy и CI/CD

### Развертывание на сервере

1. **Сборка проекта:**
```bash
dotnet publish --configuration Release --output ./publish
```

2. **Настройка базы данных:**
   - Создайте базу данных PostgreSQL
   - Примените миграции на production-окружении

3. **Запуск приложения:**
```bash
dotnet ./publish/Bitstore.dll
```

### Планируемый CI/CD пайплайн (GitHub Actions)
- Сборка Docker-образа
- Публикация в контейнерный реестр (Docker Hub / GitHub Container Registry)
- Деплой на staging/production окружение

## FAQ

### Зачем вы разработали этот проект?

Для портфолио и практики в построении backend-приложений на .NET с использованием современных подходов: чистая архитектура, репозитории, DTO, JWT аутентификация. Проект демонстрирует навыки, необходимые для разработки масштабируемых e-commerce решений.

### Какие возможности API доступны?

- CRUD операции с битами (требует прав администратора)
- Управление корзиной покупок (в разработке)
- Создание и просмотр музыкальных битов
- Аутентификация и регистрация пользователей (JWT)
- Просмотр истории заказов (в разработке)

### Как работает система авторизации?

API использует JWT (JSON Web Tokens). При успешном входе пользователь получает токен, который необходимо передавать в заголовке `Authorization: Bearer <token>` для доступа к защищенным эндпоинтам.

### Можно ли использовать другую базу данных?

Да, Entity Framework Core позволяет использовать любую поддерживаемую БД (SQL Server, SQLite, MySQL и др.). Для этого потребуется:
1. Установить соответствующий NuGet-пакет
2. Изменить строку подключения в `appsettings.json`
3. Обновить конфигурацию в `Bitstore.DataAccess`

### Как добавить нового администратора?

Администраторские права можно назначить через базу данных, установив соответствующую роль в таблице пользователей, либо через отдельный эндпоинт (в будующем).

## To do

- [ ] Написать модульные и интеграционные тесты (xUnit + Moq)
- [ ] Добавить Redis для кэширования товаров и корзины
- [ ] Реализовать пагинацию и фильтрацию для списка товаров
- [ ] Настроить CI/CD через GitHub Actions
- [ ] Добавить обработку платежей (интеграция с платежными системами)
- [ ] Реализовать email-уведомления о статусе заказа
- [ ] Контейнеризировать проект с помощью Docker

## Команда проекта

- **sgc433** - backend-разработчик, проектирование архитектуры, реализация бизнес-логики
- **mitchell-do** - frontend-разработчик, UX/UI дизайнер
