# **TG Proxy подбирает рабочий Telegram MTProto‑прокси и сразу отправляет ссылку в Telegram Desktop.**

```powershell
dotnet build "TelegramLauncher.sln"
dotnet publish "TelegramLauncher/TelegramLauncher.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
# EXE: TelegramLauncher/bin/Release/net8.0-windows/win-x64/publish/TelegramLauncher.exe
```

| Что | Как работает |
|-----|--------------|
| Без установки | Single-file self-contained `.exe` для Windows |
| Подбор прокси | Загружает публичные MTProto источники, проверяет доступность с устройства пользователя |
| Интеграция с Telegram | После подбора отправляет `tg://proxy?...` ссылку в Telegram Desktop |
| Позиционирование окна | Приложение следует за окном Telegram и не работает как always-on-top |
| Обновления | Проверяет GitHub Releases (если настроены `GitHubOwner` и `GitHubRepository`) |

## Quick Start

1. Запустите Telegram Desktop.
2. Запустите `TelegramLauncher.exe`.
3. Нажмите `Подобрать прокси`.
4. Дождитесь статуса `Прокси найден`.
5. Нажмите `Подключиться к прокси` (или сразу используйте отправленную ссылку в Telegram).

## Версия

Текущая версия: **1.1.0**.

## Источники proxy

Приложение использует открытые источники (без topic-страниц GitHub) из кода `ProxySourceService`:

| Группа | Источники |
|--------|-----------|
| Основные | `kort0881/telegram-proxy-collector` (`proxy_ru`, `proxy_eu`, `proxy_all`), `SoliSpirit/mtproto`, `mtpro.xyz` |
| Дополнительные | `proxygenerator1/ProxyGenerator`, `hookzof/socks5_list`, `Freedom-Guard/Proxy`, `securemanager/MTPROTO`, `Surfboardv2ray/TGProto`, `klondike0x/mtp4tg-proxies` |
| Запасные | `V2RAYCONFIGSPOOL/TELEGRAM_PROXY_SUB` (no1..no10), `Therealwh/MTPproxyLIST` |

## Лицензия

Проект распространяется по лицензии **[CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/)**.
Подробности: `LICENSE`.

## Сторонний код и лицензии

- В `TelegramLauncher.csproj` нет внешних NuGet-пакетов (`PackageReference` отсутствуют).
- Приложение использует стандартные библиотеки .NET 8 (`System.*`).
- Отдельные правовые замечания по сторонним материалам см. в `THIRD_PARTY_NOTICES.md`.

## Обновления из GitHub

В `TelegramLauncher/AppMetadata.cs` заполните:

```csharp
public const string GitHubOwner = "your-owner";
public const string GitHubRepository = "your-repo";
```

После этого окно `О программе` будет проверять последнюю версию по GitHub Releases.

---

Код подготовлен с помощью Cursor.

Поддержка проекта Донат https://www.donationalerts.com/r/themarfa  
Донат криптой https://nowpayments.io/donation/themarfa
