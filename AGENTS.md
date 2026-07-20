# AGENTS.md — правила для AI-ассистентов

## Зависимости

Это .NET 8 WinForms-проект без npm. Управление пакетами — через NuGet / `dotnet`.

### Проверка уязвимостей

```bash
dotnet list TelegramLauncher/TelegramLauncher.csproj package --vulnerable --include-transitive
```

Сборка должна падать при критических находках (см. CI в `.github/workflows/security.yml`).

### Проверка устаревших пакетов

```bash
dotnet list TelegramLauncher/TelegramLauncher.csproj package --outdated --include-transitive
```

### Правила добавления зависимостей

1. **Новые NuGet-пакеты** — только с явной версией, без «угадывания по памяти». Предпочитать последнюю стабильную: `dotnet add package <Name>` (без пиннинга устаревшей версии).
2. **Сразу после установки** — запустить `dotnet list package --vulnerable --include-transitive` и убедиться, что уязвимостей нет.
3. **Не добавлять пакеты без необходимости** — проект намеренно без внешних зависимостей.
4. Если в проект добавится Node.js-часть: `npm install <pkg>@latest`, затем `npm audit`.

## Секреты

- Никогда не коммитить `.env`, ключи, токены, пароли.
- Перед коммитом: `gitleaks detect --source . --verbose` (или pre-commit hook).
- Секреты прокси MTProto — это данные пользователя/источников, не хранить в репозитории.

### Git hooks

```bash
git config core.hooksPath .githooks
```

**pre-commit** — сканирование секретов через gitleaks. На Windows hook ищет `gitleaks` в PATH или `%LOCALAPPDATA%\gitleaks\gitleaks.exe`.

**post-commit** — удаляет артефакты сборки старше 7 дней (`bin/`, `obj/`, `.vs/`, `release/`, `*.user`, `*.pdb`, `*.cache`, `*.tmp`). Свежие артефакты (например, только что собранный `publish/`) не трогаются.

Ручная очистка:

```bash
.githooks/clean-build-artifacts.sh
```

## Безопасность кода

- Не загружать и не исполнять код с удалённых URL без проверки.
- URL обновлений и прокси-источников — только HTTPS.
- `Process.Start` с `UseShellExecute = true` — только для доверенных `tg://` и GitHub release URL.
