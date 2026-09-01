<div align="center">

<img src="screenshots/app_icon.png" width="88" height="88" alt="Помощник для РО">

# Помощник для РО

**Оверлей-помощник для сотрудников ФСБ на сервере «Россия Онлайн» — Кутузовский**

Устав, УК, ТК, макросы и настройки — прямо поверх игры, без переключения окон.

[![Версия](https://img.shields.io/badge/версия-0.4.12-3D5AFE?style=flat-square)](../../releases/latest)
[![Платформа](https://img.shields.io/badge/платформа-Windows-3D5AFE?style=flat-square)](#-требования)
[![.NET](https://img.shields.io/badge/.NET-8.0-3D5AFE?style=flat-square)](#-требования)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-0%2F70%2B-3D5AFE?style=flat-square)](https://www.virustotal.com/gui/file/afb2687e8f024d4e205a65a592db63988c5c3453ca982f8b579a8e2fb4d812d6)
[![Скачать](https://img.shields.io/badge/скачать-releases-3D5AFE?style=flat-square)](../../releases/latest)

[**Скачать последнюю версию**](../../releases/latest) · [Сайт проекта](https://helperro.netlify.app/) · [Discord-мейнтейнеры](#-команда)

</div>

---

## О проекте

**Помощник для РО** — компактное тёмное окно, которое можно закрепить поверх игры. Не нужно сворачивать клиент, чтобы свериться со статьёй устава, найти нужный пункт УК/ТК или запустить заготовку отыгровки — всё доступно в один хоткей.

## Возможности

| Модуль | Описание |
|---|---|
| 📖 **Устав ФСБ** | Полный текст устава фракции с быстрым поиском по разделам |
| ⚖️ **УК РО** | Уголовный кодекс сервера — статьи, сроки и санкции без переключения окон |
| 💼 **ТК РО** | Трудовой кодекс сервера — то же самое удобство поиска и навигации |
| 🗂 **Общее** | Миранда, порядок задержания, права задержанного, кодекс этики |
| ⭐ **Избранное** | Закрепляй нужные статьи звёздочкой — быстрый доступ без повторного поиска |
| 🧰 **Полезное** | Готовые макросы (боди-камера, служебные) и справочник воинских званий |
| 📝 **Заметки** | Свои заметки с форматированием, сохраняются локально на этом ПК |
| 🎨 **Настройки** | 9 стилей текста, темы оформления, размер и прозрачность окна, автозапуск, свои горячие клавиши |
| 🔄 **Автообновление** | Приложение само проверяет и предлагает установить новую версию — скачанный файл проверяется по контрольной сумме (SHA-256) перед установкой |

## Скриншоты

<table>
<tr>
<td><img src="screenshots/obshee.png" width="260"><br><sub>Общее</sub></td>
<td><img src="screenshots/ukro.png" width="260"><br><sub>УК РО</sub></td>
<td><img src="screenshots/tkro.png" width="260"><br><sub>ТК РО</sub></td>
</tr>
<tr>
<td><img src="screenshots/ustav.png" width="260"><br><sub>Устав ФСБ</sub></td>
<td><img src="screenshots/izbrannoe.png" width="260"><br><sub>Избранное</sub></td>
<td><img src="screenshots/poleznoe.png" width="260"><br><sub>Полезное</sub></td>
</tr>
<tr>
<td><img src="screenshots/zametki.png" width="260"><br><sub>Заметки</sub></td>
<td><img src="screenshots/nastroiki.png" width="260"><br><sub>Настройки</sub></td>
<td><img src="screenshots/oproekte.png" width="260"><br><sub>О проекте</sub></td>
</tr>
</table>

## Скачать

Готовые сборки — на странице **[Releases](../../releases)**. Файл `HelperGos.exe` — самодостаточный, ничего дополнительно распаковывать не нужно.

Зеркала на случай проблем с доступом к GitHub:

- Яндекс.Диск: https://disk.yandex.ru/d/rcNRU6IoAKc-Hg
- Google Диск: https://drive.google.com/drive/folders/1NLbrBjAYa9duZ_gFSAFlhS7nBRIasveK?usp=sharing

## Требования

- Windows 10/11 (x64)
- .NET 8 Desktop Runtime — если не установлен, приложение предложит поставить его автоматически при первом запуске

## Установка

1. Скачай `HelperGos.exe` из [Releases](../../releases/latest).
2. Запусти файл.
3. Если Windows SmartScreen/браузер покажет предупреждение — это стандартная реакция на новый неподписанный файл без истории репутации, не вирус. Жми «Подробнее» → «Выполнить в любом случае» (или аналог).

Файл версии 0.4.12 [проверен на VirusTotal](https://www.virustotal.com/gui/file/afb2687e8f024d4e205a65a592db63988c5c3453ca982f8b579a8e2fb4d812d6) — 0 из 70+ антивирусов не обнаружили угроз.

## Горячие клавиши

По умолчанию окно вызывается клавишей **Ё** (можно поменять в настройках приложения). Отдельно настраиваются клавиши блокировки окна, быстрого перехода в «Избранное» и в «Заметки».

## Сборка из исходников

```bash
git clone https://github.com/Accapura/Helper_RO_App.git
cd Helper_RO_App
dotnet publish -c Release -r win-x64 --self-contained true
```

Требуется .NET 8 SDK и Windows (используется WPF + WebView2).

## Команда

| Роль | Ник |
|---|---|
| Разработка | **Денис Яров** — Discord: `roste233` |
| Разработка | **Михаил Яровой** — Discord: `accapura` |

## Лицензия

Проект распространяется бесплатно для игроков сервера «Россия Онлайн». Открытый исходный код — для ознакомления и локальной сборки.

---

<div align="center">
<sub>Помощник для РО · Россия Онлайн · Кутузовский</sub>
</div>
