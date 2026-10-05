param(
    [string]$Mode = "Installer", # Options: Installer, Unpacked, SingleFile, Standalone, Standard, Commander, CommanderFx, All, Dual, Sequential
    [string]$TargetFramework = "net8.0-windows", # Options: net8.0-windows, net10.0-windows
    [switch]$Net10,
    [switch]$Dual,
    [switch]$Sequential
)

if ($Net10) {
    $TargetFramework = "net10.0-windows"
}

$IsDual = ($Dual -or $Sequential -or ($Mode -eq "Dual") -or ($Mode -eq "Sequential"))

# WinPurify Pro v4.45.0 PowerShell 1-Click Automation Builder
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "   WinPurify Pro v4.45.0 Unified Automation Builder" -ForegroundColor Cyan
Write-Host "   Target: Native C++ Core + .NET 8 / .NET 10 WPF Suite" -ForegroundColor Cyan
if ($IsDual) {
    Write-Host "   Mode: Sequential Dual-TFM Build (.NET 10.0 & .NET 8.0 Isolated Paths)" -ForegroundColor Cyan
} else {
    Write-Host "   Framework: $TargetFramework | Mode: $Mode" -ForegroundColor Cyan
}
Write-Host "=====================================================================" -ForegroundColor Cyan

# 1. Check & Compile C++ Native Core
Write-Host "`n[*] [Step 1/2] Invoking C++ Native Compiler..." -ForegroundColor Yellow
if (Test-Path "build_dll.bat") {
    Start-Process -FilePath "cmd.exe" -ArgumentList "/c build_dll.bat nopause" -NoNewWindow -Wait
}

# 2. Check dotnet CLI
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] dotnet CLI not found in system path." -ForegroundColor Red
    exit 1
}

function Copy-Assets([string]$destDir) {
    if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
    if (Test-Path "PurifyEngineCore.dll") { Copy-Item "PurifyEngineCore.dll" -Destination "$destDir\PurifyEngineCore.dll" -Force }
    if (Test-Path "WinPurify_logo.png") { Copy-Item "WinPurify_logo.png" -Destination "$destDir\WinPurify_logo.png" -Force }
    if (Test-Path "WinPurify_icon.png") { Copy-Item "WinPurify_icon.png" -Destination "$destDir\WinPurify_icon.png" -Force }
    if (Test-Path "WinPurify.ico") { Copy-Item "WinPurify.ico" -Destination "$destDir\WinPurify.ico" -Force }
    if (Test-Path "LICENSE_KO.txt") { Copy-Item "LICENSE_KO.txt" -Destination "$destDir\LICENSE_KO.txt" -Force }
    if (Test-Path "LICENSE_EN.txt") { Copy-Item "LICENSE_EN.txt" -Destination "$destDir\LICENSE_EN.txt" -Force }
}

function Build-FrameworkTarget([string]$tfm, [string]$targetMode) {
    $tfmDir = if ($tfm -match "10") { "net10.0" } else { "net8.0" }
    $pubBase = "publish\$tfmDir"
    
    Write-Host "`n=====================================================================" -ForegroundColor Cyan
    Write-Host "   Target Framework: $tfm (Isolated Output: $pubBase\)" -ForegroundColor Cyan
    Write-Host "=====================================================================" -ForegroundColor Cyan

    Write-Host "[*] Restoring NuGet dependencies for $tfm..." -ForegroundColor DarkGray
    dotnet restore WinPurifyPro.csproj -p:TargetFramework=$tfm --verbosity quiet
    dotnet restore WinPurifyCommander.csproj -p:TargetFramework=$tfm --verbosity quiet

    if ($targetMode -eq "Unpacked" -or $targetMode -eq "Installer" -or $targetMode -eq "All") {
        Write-Host "`n[*] Publishing Unpacked Multi-File Application Directory ($pubBase\App)..." -ForegroundColor Cyan
        Write-Host "    Mode: Self-Contained Unpacked (Slim R2R-Off, ~75MB) [$tfm]" -ForegroundColor DarkCyan
        if (Test-Path "$pubBase\App") {
            Write-Host "[*] Cleaning previous unpacked application directory..." -ForegroundColor DarkGray
            Remove-Item "$pubBase\App" -Recurse -Force -ErrorAction SilentlyContinue
        }
        Write-Host "[+] Building WinPurifyPro.exe (Unpacked Multi-File Assemblies, Slim)..." -ForegroundColor Yellow
        dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=false -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "$pubBase\App"

        Write-Host "[+] Building WinPurifyCommander.exe for $pubBase\App..." -ForegroundColor Yellow
        if (Test-Path "publish\TempAppCommander") { Remove-Item "publish\TempAppCommander" -Recurse -Force -ErrorAction SilentlyContinue }
        dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempAppCommander"
        if (Test-Path "publish\TempAppCommander\WinPurifyCommander.exe") {
            Copy-Item "publish\TempAppCommander\WinPurifyCommander.exe" -Destination "$pubBase\App\WinPurifyCommander.exe" -Force
            if (-not (Test-Path "$pubBase\Commander")) { New-Item -ItemType Directory -Path "$pubBase\Commander" -Force | Out-Null }
            Copy-Item "publish\TempAppCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander.exe" -Force
        }
        Remove-Item "publish\TempAppCommander" -Recurse -Force -ErrorAction SilentlyContinue

        Copy-Assets "$pubBase\App"
        if (Test-Path "installed.tag") { Copy-Item "installed.tag" -Destination "$pubBase\App\installed.tag" -Force }
        Write-Host "[SUCCESS] Unpacked Multi-File Publish (~75MB Slim): $pubBase\App\ (WinPurifyPro.exe & WinPurifyCommander.exe)" -ForegroundColor Green
    }

    if ($targetMode -eq "Installer" -or $targetMode -eq "All") {
        Write-Host "`n[*] Compiling Inno Setup 6 Installer (installer.iss) for $tfm..." -ForegroundColor Cyan
        $iscc = $null
        if (Get-Command iscc.exe -ErrorAction SilentlyContinue) {
            $iscc = "iscc.exe"
        } else {
            $candidatePaths = @(
                "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
                "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
                "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
                "C:\Program Files\Inno Setup 6\ISCC.exe",
                "D:\Program Files (x86)\Inno Setup 6\ISCC.exe",
                "D:\Program Files\Inno Setup 6\ISCC.exe",
                "C:\Inno Setup 6\ISCC.exe",
                "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
            )
            foreach ($p in $candidatePaths) {
                if (Test-Path $p) {
                    $iscc = $p
                    break
                }
            }
        }

        if ($iscc -and (Test-Path "installer.iss")) {
            Write-Host "[+] Running ISCC: $iscc with Isolated Paths" -ForegroundColor Yellow
            $setupFilename = "WinPurifyPro_v4.45.0_${tfmDir}_Setup"
            $isccArgs = @(
                "/DAppSourceDir=$pubBase\App",
                "/DOutputDir=$pubBase\Installer",
                "/DOutputBaseFilename=$setupFilename",
                "installer.iss"
            )
            Start-Process -FilePath $iscc -ArgumentList $isccArgs -NoNewWindow -Wait
            Write-Host "[SUCCESS] Inno Setup 6 Installer created: $pubBase\Installer\$setupFilename.exe" -ForegroundColor Green
        } else {
            Write-Host "[INFO] Inno Setup 6 (ISCC.exe) not found. Download from: https://jrsoftware.org/isdl.php" -ForegroundColor DarkYellow
        }
    }

    if ($targetMode -eq "SingleFile" -or $targetMode -eq "All") {
        Write-Host "`n[*] Publishing Single-File Portable Executables ($pubBase\SingleFile)..." -ForegroundColor Cyan
        Write-Host "[+] Building WinPurifyPro.exe (Main Client)..." -ForegroundColor Yellow
        dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained false -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "$pubBase\SingleFile"
        
        Write-Host "[+] Building WinPurifyCommander.exe (Central Commander Console, Self-Contained)..." -ForegroundColor Yellow
        if (Test-Path "publish\TempSingleCommander") { Remove-Item "publish\TempSingleCommander" -Recurse -Force -ErrorAction SilentlyContinue }
        dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempSingleCommander"
        if (Test-Path "publish\TempSingleCommander\WinPurifyCommander.exe") {
            Copy-Item "publish\TempSingleCommander\WinPurifyCommander.exe" -Destination "$pubBase\SingleFile\WinPurifyCommander.exe" -Force
            if (-not (Test-Path "$pubBase\Commander")) { New-Item -ItemType Directory -Path "$pubBase\Commander" -Force | Out-Null }
            Copy-Item "publish\TempSingleCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander.exe" -Force
            Copy-Item "publish\TempSingleCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander_Standalone.exe" -Force
        }
        Remove-Item "publish\TempSingleCommander" -Recurse -Force -ErrorAction SilentlyContinue
        
        Copy-Assets "$pubBase\SingleFile"
        Copy-Assets "$pubBase\Commander"
        Write-Host "[SUCCESS] Single-File Publish: $pubBase\SingleFile\WinPurifyPro.exe & WinPurifyCommander.exe" -ForegroundColor Green
    }

    if ($targetMode -eq "Commander" -or $targetMode -eq "All") {
        Write-Host "`n[*] Publishing Central Commander Executables ($pubBase\Commander)..." -ForegroundColor Cyan
        Write-Host "[+] Building WinPurifyCommander.exe (Self-Contained Standalone Slim)..." -ForegroundColor Yellow
        if (Test-Path "publish\TempStandaloneCommander") { Remove-Item "publish\TempStandaloneCommander" -Recurse -Force -ErrorAction SilentlyContinue }
        dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempStandaloneCommander"
        if (Test-Path "publish\TempStandaloneCommander\WinPurifyCommander.exe") {
            if (-not (Test-Path "$pubBase\Commander")) { New-Item -ItemType Directory -Path "$pubBase\Commander" -Force | Out-Null }
            Copy-Item "publish\TempStandaloneCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander.exe" -Force
            Copy-Item "publish\TempStandaloneCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander_Standalone.exe" -Force
        }
        Remove-Item "publish\TempStandaloneCommander" -Recurse -Force -ErrorAction SilentlyContinue

        Copy-Assets "$pubBase\Commander"
        Write-Host "[SUCCESS] Commander Publish: $pubBase\Commander\WinPurifyCommander.exe & WinPurifyCommander_Standalone.exe" -ForegroundColor Green
    }

    if ($targetMode -eq "CommanderFx" -or $targetMode -eq "All") {
        Write-Host "`n[*] Publishing Central Commander Executables (Framework-Dependent Slim, ~3MB)..." -ForegroundColor Cyan
        Write-Host "[+] Building WinPurifyCommander.exe (Framework-Dependent Slim)..." -ForegroundColor Yellow
        if (Test-Path "publish\TempFxCommander") { Remove-Item "publish\TempFxCommander" -Recurse -Force -ErrorAction SilentlyContinue }
        dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempFxCommander"
        if (Test-Path "publish\TempFxCommander\WinPurifyCommander.exe") {
            if (-not (Test-Path "$pubBase\Commander")) { New-Item -ItemType Directory -Path "$pubBase\Commander" -Force | Out-Null }
            Copy-Item "publish\TempFxCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander.exe" -Force
            Copy-Item "publish\TempFxCommander\WinPurifyCommander.exe" -Destination "$pubBase\Commander\WinPurifyCommander_Framework.exe" -Force
        }
        Remove-Item "publish\TempFxCommander" -Recurse -Force -ErrorAction SilentlyContinue

        Copy-Assets "$pubBase\Commander"
        Write-Host "[SUCCESS] Framework-Dependent Commander Publish: $pubBase\Commander\WinPurifyCommander.exe (~3MB)" -ForegroundColor Green
    }

    if ($targetMode -eq "Standalone" -or $targetMode -eq "All") {
        Write-Host "`n[*] Publishing Standalone Single-File (.NET Runtime Embedded)..." -ForegroundColor Cyan
        Write-Host "[+] Building Standalone WinPurifyPro.exe..." -ForegroundColor Yellow
        dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "$pubBase\Standalone"
        
        Write-Host "[+] Building Standalone WinPurifyCommander.exe..." -ForegroundColor Yellow
        if (Test-Path "publish\TempStandaloneCommander") { Remove-Item "publish\TempStandaloneCommander" -Recurse -Force -ErrorAction SilentlyContinue }
        dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f $tfm -p:TargetFramework=$tfm -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempStandaloneCommander"
        if (Test-Path "publish\TempStandaloneCommander\WinPurifyCommander.exe") {
            Copy-Item "publish\TempStandaloneCommander\WinPurifyCommander.exe" -Destination "$pubBase\Standalone\WinPurifyCommander.exe" -Force
        }
        Remove-Item "publish\TempStandaloneCommander" -Recurse -Force -ErrorAction SilentlyContinue

        Copy-Assets "$pubBase\Standalone"
        Write-Host "[SUCCESS] Standalone Single-File Publish: $pubBase\Standalone\WinPurifyPro.exe & WinPurifyCommander.exe (~70MB)" -ForegroundColor Green
    }

    if ($targetMode -eq "Standard" -or $targetMode -eq "All") {
        Write-Host "`n[*] Building Standard Framework-Dependent WPF Application & Commander..." -ForegroundColor Cyan
        Write-Host "[+] Building WinPurifyPro.exe (Framework-Dependent)..." -ForegroundColor Yellow
        dotnet build WinPurifyPro.csproj -c Release -r win-x64 --self-contained false -f $tfm -p:TargetFramework=$tfm -o "$pubBase\Standard"
        
        Write-Host "[+] Building WinPurifyCommander.exe (Framework-Dependent)..." -ForegroundColor Yellow
        dotnet build WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f $tfm -p:TargetFramework=$tfm -o "$pubBase\Standard"
        
        Copy-Assets "$pubBase\Standard"
        Write-Host "[SUCCESS] Standard Build: $pubBase\Standard\WinPurifyPro.exe & WinPurifyCommander.exe" -ForegroundColor Green
    }
}

# 3. Execute Builds
if ($IsDual) {
    Write-Host "`n[*] Starting Sequential Dual-TFM Build (Stage 1/2: .NET 10.0 SDK)..." -ForegroundColor Magenta
    Build-FrameworkTarget "net10.0-windows" "All"

    Write-Host "`n[*] Starting Sequential Dual-TFM Build (Stage 2/2: .NET 8.0 LTS)..." -ForegroundColor Magenta
    Build-FrameworkTarget "net8.0-windows" "All"

    Write-Host "`n=====================================================================" -ForegroundColor Cyan
    Write-Host "   SEQUENTIAL DUAL-FRAMEWORK BUILD COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "   - .NET 10.0 Artifacts: publish\net10.0\ (Zero file collision)" -ForegroundColor Green
    Write-Host "   - .NET 8.0  Artifacts: publish\net8.0\  (Zero file collision)" -ForegroundColor Green
    Write-Host "=====================================================================" -ForegroundColor Cyan
} else {
    Build-FrameworkTarget $TargetFramework $Mode
    Write-Host "`n=====================================================================" -ForegroundColor Cyan
    Write-Host "   BUILD OPERATION COMPLETED FOR $TargetFramework!" -ForegroundColor Green
    Write-Host "=====================================================================" -ForegroundColor Cyan
}
