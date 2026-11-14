# VoiceOver — голосовой ввод из панели задач / Voice typing from the taskbar

## Русский

Маленькая бесплатная программа для запуска встроенного голосового ввода Windows кнопкой на панели задач. Возвращает фокус в окно приложения и отправляет сочетание **Win+H**. Использует распознавание речи Windows; собственного окна и фоновой службы нет. Стандартная панель диктовки Windows остаётся видимой.

### Скачать и закрепить

1. Скачайте **VoiceOver-v1.0.0.zip** из [последнего релиза](https://github.com/AlexSoftLTD/VoiceOver/releases/latest).
2. Распакуйте архив в постоянную папку, например `Документы\VoiceOver`.
3. Нажмите правой кнопкой на **VoiceOver.exe** → **Показать дополнительные параметры** → **Закрепить на панели задач**. В некоторых версиях Windows пункт закрепления доступен сразу.
4. Поставьте курсор в текстовое поле и нажмите закреплённую кнопку с микрофоном.

Остановить диктовку можно кнопкой микрофона на системной панели. Повторное нажатие нашей кнопки снова отправляет Win+H; результат зависит от версии Windows. Остановка не отменяет уже введённый текст.

Нужны Windows 10/11, микрофон, интернет и разрешённое распознавание речи Windows. Для приложений, открытых от имени администратора, передача команды может не работать. Не перемещайте программу после закрепления.

### Сборка из исходников

Запустите `build.ps1` в PowerShell. Используется встроенный компилятор .NET Framework; внешние пакеты не нужны. `VoiceOver.exe --self-test` проверяет структуры Win32 без запуска диктовки.

## English

A small free utility that opens Windows voice typing from a taskbar button. It restores focus to the application window and sends **Win+H**. Speech recognition is handled by Windows. The utility has no window or background service; the standard Windows dictation panel remains visible.

### Download and pin

1. Download **VoiceOver-v1.0.0.zip** from the [latest release](https://github.com/AlexSoftLTD/VoiceOver/releases/latest).
2. Extract it to a permanent folder, such as `Documents\VoiceOver`.
3. Right-click **VoiceOver.exe** → **Show more options** → **Pin to taskbar**. Some Windows versions show the pin option directly.
4. Place the caret in a text field and click the pinned microphone button.

Stop dictation using the microphone button on the Windows panel. Clicking our button again sends Win+H again; behavior depends on your Windows version. Stopping dictation does not undo text already entered.

Requires Windows 10/11, a microphone, an internet connection and Windows speech recognition enabled. Sending the command to applications running as administrator may fail. Keep the program in its original location after pinning it.

### Build from source

Run `build.ps1` in PowerShell. It uses the .NET Framework compiler included with Windows; no external packages are required. `VoiceOver.exe --self-test` checks the Win32 structures without starting dictation.

## License / Лицензия

[MIT](LICENSE) — free to use, modify and redistribute, including commercially, with the license notice retained. / Можно бесплатно использовать, изменять и распространять, в том числе коммерчески, сохраняя текст лицензии.

[Windows voice typing — Microsoft Support](https://support.microsoft.com/en-us/accessibility/windows/use-voice-typing-to-talk-instead-of-type-on-your-pc)
