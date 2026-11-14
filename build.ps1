$ErrorActionPreference = 'Stop'
$taskRoot = $PSScriptRoot
Add-Type -AssemblyName System.Drawing
$bitmap = [System.Drawing.Bitmap]::new(64,64)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)
$blue = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(0,115,210))
$white = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$pen = [System.Drawing.Pen]::new([System.Drawing.Color]::White,4)
$graphics.FillEllipse($blue,1,1,62,62)
$graphics.FillEllipse($white,25,11,14,14)
$graphics.FillRectangle($white,25,18,14,19)
$graphics.FillEllipse($white,25,30,14,14)
$graphics.DrawArc($pen,18,23,28,28,0,180)
$graphics.DrawLine($pen,32,51,32,56)
$graphics.DrawLine($pen,24,56,40,56)
$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
$stream = [System.IO.File]::Create((Join-Path $taskRoot 'VoiceOver.ico'))
try { $icon.Save($stream) } finally { $stream.Dispose(); $icon.Dispose(); $pen.Dispose(); $white.Dispose(); $blue.Dispose(); $graphics.Dispose(); $bitmap.Dispose() }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /reference:System.Windows.Forms.dll "/win32icon:$taskRoot\VoiceOver.ico" "/out:$taskRoot\VoiceOver.exe" "$taskRoot\VoiceOver.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed' }
$check = Start-Process -FilePath (Join-Path $taskRoot 'VoiceOver.exe') -ArgumentList '--self-test' -PassThru -Wait -WindowStyle Hidden
if ($check.ExitCode -ne 0) { throw 'Native input layout self-test failed' }
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut((Join-Path $taskRoot 'Голосовой ввод.lnk'))
$shortcut.TargetPath = Join-Path $taskRoot 'VoiceOver.exe'
$shortcut.WorkingDirectory = $taskRoot
$shortcut.Description = 'Голосовой ввод Windows — Win+H'
$shortcut.IconLocation = "$taskRoot\VoiceOver.ico,0"
$shortcut.Save()
Write-Output 'Built VoiceOver.exe; native input layout self-test passed; shortcut created.'
