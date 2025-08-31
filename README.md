### TopicSearcherAI

Консольное приложение для автоматического поиска информации по заданной теме. Результаты поиска записываются в Word документ и сохраняются на рабочий стол пользователя.

Поиск и генерация информации осуществляется моделью GigaChat.

**Не является инструментом для полноценной генерации документов**. Требуется ручная доработка, поскольку:
- Плохо работает с математическими темами (например, отображение формул в LaTeX).
- Нет готовых шаблонов оформления: списков, стилей, нумерации.
- Отсутствие возможности вставки: рисунков, графиков, других мультимедиа.
- Иногда могут оставаться отступы между абзацами или некоторые символы markdown разметки.
- Другие проблемы, связанные с LLM генерациями.

### Быстрый старт
1) Установить API ключ от GigaChat API через команду `--key set <your_key>`. 

2) Установить тему `--theme set <your_theme>`.

3) Выполнить поиск всех подтем и сгенерировать отчет `--build`.

<img width="750" height="500" alt="preview_quick_start" src="https://github.com/user-attachments/assets/8b406009-464f-4385-b34c-f384bceace14" />

Подробнее о командах быстрого старта:
- `--key set <your_key>` устанавливает ключ для соединения с GigaChat моделью.
- При установке темы `--theme set <your_theme>` происходит разбиение значения theme на подтемы в формате 1, 1.1, ... (подробнее в `--list`).
- Команда `--build` запускает алгоритм обхода массива подтем и алгоритм генерации информации по каждой подтеме.
  Все результаты объединяются друг с другом. Итоговый ответ записывается в Word документ, который сохраняется на рабочем столе пользователя (подробнее о параметрах модели и документа в `--detail`).

### Сертификация и API ключ
При первом запуске приложения всплывает окно установки сертификата от НУЦ Минцифры ([самостоятельная установка](https://www.gosuslugi.ru/crt)). Без этого сертификата подключение к GigaChat, даже при наличии ключа, будет приводить к ошибке SSL сертификации.
Сертификат устанавливается в корневое хранилище сертификатов пользователя.

Для удаления сертификата:
- Оснастка certmgr
  - Win + R =>
  - certmgr.msc =>
  - Доверенные корневые центры сертификации =>
  - Сертификаты =>
  - Russian Trusted Root CA =>
  - ПКМ по сертификату =>
  - Удалить.
- PowerShell
  - Ввод `Get-ChildItem Cert:\CurrentUser\Root | Where-Object { $_.Subject -like "*Russian Trusted Root CA*" } | Format-List Subject, Thumbprint` =>
  - Ввод `Get-ChildItem Cert:\CurrentUser\Root | Where-Object { $_.Thumbprint -eq "your_trumbprint" } | Remove-Item`.

API ключ к модели GigaChat генерируется в настройках вашего [API проекта](https://developers.sber.ru/portal/gigachat-and-api) на оф. странице.
Установленный ключ `--key set <your_key>` сохраняется в переменные среды пользователя.

Для добавление/удаления ключа вручную:
- Апплет sysdm
  - Win + R =>
  - sysdm.cpl =>
  - Дополнительно =>
  - Переменные среды =>
  - Создать/Изменить/Удалить для окна "Переменные среды пользователя".
- CMD
  - Ввод для установки `setx KEY_BUILDER "your_key"`.
  - Ввод для удаления `reg delete HKCU\Environment /v KEY_BUILDER /f`.
- PowerShell
  - Ввод для установки `[System.Environment]::SetEnvironmentVariable("KEY_BUILDER", "your_key", "User")`.
  - Ввод для удаления `[System.Environment]::SetEnvironmentVariable("KEY_BUILDER", $null, "User")`.

### Команды

Accessor в списках команд заменяется на ключ. слова `get` или `set` (получить или установить свойство).

- Основное
  - `--help` - список команд
  - `--detail` - список переменных
  - `--key set <value>` - API ключ к GigaChat
  - `--theme accessor <value>` - искомая тема
  - `--list` - список декомпозиции темы
  - `--take accessor <value>`  - значение переменной охвата
  - `--build` - поиск информации и сформирование отчета
  - `--default` - возврат к заводским настройкам
- GigaChat модель
  - `--model accessor <value>` - название AI модели
  - `--temperature accessor <value>` - температура ответа
  - `--topP accessor <value>` - topP параметр
  - `--maxTokens accessor <value>` - количество токенов
  - `--prompt accesssor <value>` - промпт для поиска данных
  - `--promptL accessor <value>` - промтп для декомпозиции темы
- Word параметры
  - `--doc.format accessor <value>` - формат документа
  - `--doc.margin accessor <left; top; right; bottom>` - отступы от краев листа
  - `--doc.aligment accessor <value>` - формат выравнивания
  - `--doc.firstLine accessor <value>` -  отступ первой строки
  - `--doc.spacing accessor <value>` - межстрочный интервал
  - `--doc.family accessor <value>` - шрифт документа
  - `--doc.size accessor <value>` - размер шрифта
