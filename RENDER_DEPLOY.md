# DripCube - Render Deployment

## Файлы для деплоя

✅ **Dockerfile** - multi-stage build для .NET 8
✅ **Program.cs** - обновлен для поддержки переменных окружения

## Настройка на Render

### 1. Создайте Web Service

1. Перейдите на [render.com](https://render.com)
2. **New** → **Web Service**
3. Подключите ваш GitHub/GitLab репозиторий

### 2. Настройки сервиса

| Параметр | Значение |
|----------|----------|
| **Name** | dripcube (или любое имя) |
| **Environment** | Docker |
| **Region** | Oregon (или ближайший) |
| **Instance Type** | Free или Starter |

### 3. Переменные окружения (Environment Variables)

В разделе **Environment** → **Environment Variables** добавьте:

```
DATABASE_URL=postgresql://user:NBWohmR0QCiyPLqFd2uGR2HWMNvm9GnA@dpg-d4g6db8dl3ps73da19pg-a/abcn
```

> ⚠️ **Важно**: Render автоматически устанавливает переменную `PORT`, её добавлять не нужно.

### 4. Запуск

Нажмите **Create Web Service** и дождитесь завершения деплоя.

## После деплоя

- **Swagger UI**: `https://your-app.onrender.com/swagger`
- **API**: `https://your-app.onrender.com/api/...`
- **Admin**: Login: `admin` / Password: `admin123`

## Диагностика

Если приложение не запускается, проверьте логи в разделе **Logs** на Render.
