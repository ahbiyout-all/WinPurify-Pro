<#
.SYNOPSIS
    WinPurify Pro - Pre-Build XAML ResourceDictionary Key (MC3023) & Syntax Validator
.DESCRIPTION
    Recursively scans all .xaml files in the project for ResourceDictionary key syntax errors (MC3023),
    mismatched or unclosed tags, and XML well-formedness using .NET XML parsing.
    Flags invalid non-Dictionary tag usage and duplicate keys to halt the build pipeline
    before MSVC C++ and MSBuild/.NET compilation begins.
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$TargetDir = ""
)

# Resolve Target Directory
if ([string]::IsNullOrWhiteSpace($TargetDir)) {
    $TargetDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
}

$resolvedDir = Resolve-Path -Path $TargetDir -ErrorAction SilentlyContinue
if (-not $resolvedDir) {
    Write-Host "[ERROR] Target directory not found: $TargetDir" -ForegroundColor Red
    exit 1
}

Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "   WinPurify Pro XAML Validator (MC3023 ResourceDictionary Engine)" -ForegroundColor Cyan
Write-Host "   Root Directory: $($resolvedDir.Path)" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Cyan

# Recursively locate all .xaml files, excluding output and temporary directories
$excludePattern = "node_modules|obj|bin|publish|\.git|dist-web|TempAppCommander|TempFxCommander"
$xamlFiles = Get-ChildItem -Path $resolvedDir.Path -Recurse -Filter "*.xaml" -File -ErrorAction SilentlyContinue | Where-Object {
    $_.FullName -notmatch $excludePattern
}

if (-not $xamlFiles -or $xamlFiles.Count -eq 0) {
    Write-Host "[INFO] No XAML files found to validate in $($resolvedDir.Path)." -ForegroundColor Yellow
    exit 0
}

Write-Host "[*] Found $($xamlFiles.Count) XAML file(s). Beginning recursive XML structure analysis...`n" -ForegroundColor DarkCyan

$totalErrors = 0
$totalFilesChecked = 0
$failedFiles = @()

# Function to determine if a tag is a valid Dictionary container in WPF
function Test-IsDictionaryContainer([string]$tagName) {
    if ([string]::IsNullOrWhiteSpace($tagName)) { return $false }
    $cleanTag = if ($tagName -match ":") { $tagName.Split(":")[-1] } else { $tagName }
    
    # Valid WPF dictionary containers:
    # 1. <ResourceDictionary> (or custom ResourceDictionary subclasses)
    # 2. Any property element ending with .Resources (e.g. <Window.Resources>, <Application.Resources>, <Grid.Resources>)
    # 3. Any type ending with 'Dictionary'
    return ($cleanTag -eq "ResourceDictionary" -or 
            $cleanTag.EndsWith(".Resources", [System.StringComparison]::OrdinalIgnoreCase) -or 
            $cleanTag.EndsWith("Dictionary", [System.StringComparison]::OrdinalIgnoreCase))
}

foreach ($file in $xamlFiles) {
    $totalFilesChecked++
    $relPath = Resolve-Path -Path $file.FullName -Relative
    $fileErrors = @()
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8

    try {
        $settings = New-Object System.Xml.XmlReaderSettings
        $settings.DtdProcessing = [System.Xml.DtdProcessing]::Ignore
        $settings.IgnoreWhitespace = $false
        $settings.IgnoreComments = $true
        
        $stringReader = New-Object System.IO.StringReader($content)
        $xmlReader = [System.Xml.XmlReader]::Create($stringReader, $settings)
        
        # Stack tracks active XML element hierarchy: { Tag, Line, Col, IsDictionary, Keys: Hashtable }
        $elementStack = New-Object System.Collections.Generic.Stack[PSObject]

        while ($xmlReader.Read()) {
            if ($xmlReader.NodeType -eq [System.Xml.XmlNodeType]::Element) {
                $rawTag = $xmlReader.Name
                $curLine = $xmlReader.LineNumber
                $curCol = $xmlReader.LinePosition
                $isDict = Test-IsDictionaryContainer $rawTag
                $isEmpty = $xmlReader.IsEmptyElement

                # Inspect attributes for x:Key or Key
                $hasKey = $false
                $keyValue = ""
                
                if ($xmlReader.HasAttributes) {
                    if ($xmlReader.MoveToFirstAttribute()) {
                        do {
                            $attrName = $xmlReader.Name
                            $attrLocal = $xmlReader.LocalName
                            
                            if ($attrLocal -eq "Key" -or $attrName -eq "x:Key") {
                                $hasKey = $true
                                $keyValue = $xmlReader.Value
                                break
                            }
                        } while ($xmlReader.MoveToNextAttribute())
                        
                        $xmlReader.MoveToElement() | Out-Null
                    }
                }

                # Validate x:Key constraints
                if ($hasKey) {
                    # 1. Non-empty key check
                    if ([string]::IsNullOrWhiteSpace($keyValue)) {
                        $fileErrors += "$relPath($curLine,$curCol): [Invalid Key] Tag '<$rawTag>' defines an empty or whitespace-only Key. ResourceDictionary keys must be non-empty."
                    }

                    # 2. MC3023 Violation Check: Key attribute on non-Dictionary container
                    if ($elementStack.Count -gt 0) {
                        $parent = $elementStack.Peek()
                        
                        if (-not $parent.IsDictionary) {
                            $err = "$relPath($curLine,$curCol): error MC3023: Key attribute can only be used on a tag contained in a Dictionary (such as a ResourceDictionary). " +
                                   "Tag '<$rawTag>' defines Key=`"$keyValue`" but is nested inside non-Dictionary tag '<$($parent.Tag)>' (opened at Line $($parent.Line), Col $($parent.Col)). " +
                                   "Hint: Check for unclosed preceding tags (e.g., missing </Setter>, </Style>, or </ControlTemplate>) right above this tag."
                            $fileErrors += $err
                        } else {
                            # 3. Duplicate Key Check within current ResourceDictionary scope
                            if ($parent.Keys.ContainsKey($keyValue)) {
                                $prevLine = $parent.Keys[$keyValue]
                                $fileErrors += "$relPath($curLine,$curCol): [Duplicate Key] Duplicate Key=`"$keyValue`" detected on '<$rawTag>'. Already defined at Line $prevLine in this dictionary scope. Duplicate keys will cause runtime initialization failure."
                            } else {
                                $parent.Keys[$keyValue] = $curLine
                            }
                        }
                    } else {
                        # Key on root element without dictionary container
                        if (-not $isDict) {
                            $fileErrors += "$relPath($curLine,$curCol): error MC3023: Root tag '<$rawTag>' defines Key=`"$keyValue`" outside of any ResourceDictionary."
                        }
                    }
                }

                if (-not $isEmpty) {
                    $elementStack.Push([PSCustomObject]@{
                        Tag = $rawTag
                        Line = $curLine
                        Col = $curCol
                        IsDictionary = $isDict
                        Keys = @{}
                    })
                }
            }
            elseif ($xmlReader.NodeType -eq [System.Xml.XmlNodeType]::EndElement) {
                if ($elementStack.Count -gt 0) {
                    $top = $elementStack.Pop()
                    if ($top.Tag -ne $xmlReader.Name) {
                        $closeLine = $xmlReader.LineNumber
                        $closeCol = $xmlReader.LinePosition
                        $fileErrors += "$relPath($closeLine,$closeCol): [Tag Mismatch] Expected closing tag </$($top.Tag)> (opened at Line $($top.Line), Col $($top.Col)), but found </$($xmlReader.Name)>."
                    }
                } else {
                    $closeLine = $xmlReader.LineNumber
                    $closeCol = $xmlReader.LinePosition
                    $fileErrors += "$relPath($closeLine,$closeCol): [Unexpected Close] Closing tag </$($xmlReader.Name)> without matching opening tag."
                }
            }
        }
        
        $xmlReader.Close()
        
        # Check for unclosed tags remaining on the stack
        while ($elementStack.Count -gt 0) {
            $unclosed = $elementStack.Pop()
            $fileErrors += "$relPath($($unclosed.Line),$($unclosed.Col)): [Unclosed Tag] Tag '<$($unclosed.Tag)>' was opened but never closed before end-of-file."
        }
        
    } catch [System.Xml.XmlException] {
        $fileErrors += "$relPath($($_.Exception.LineNumber),$($_.Exception.LinePosition)): [XML Parse Error] $($_.Exception.Message)"
    } catch {
        $fileErrors += "$relPath(1,1): [System Error] $($_.Exception.Message)"
    }

    if ($fileErrors.Count -eq 0) {
        $lineCount = ($content -split "`r?`n").Length
        Write-Host "  [PASS] $relPath ($lineCount lines) - Validated" -ForegroundColor Green
    } else {
        $totalErrors += $fileErrors.Count
        $failedFiles += $relPath
        Write-Host "`n  [FAIL] $relPath ($($fileErrors.Count) error(s) detected):" -ForegroundColor Red
        foreach ($err in $fileErrors) {
            Write-Host "    --> $err" -ForegroundColor Yellow
        }
        Write-Host ""
    }
}

Write-Host "---------------------------------------------------------------------" -ForegroundColor Cyan
if ($totalErrors -eq 0) {
    Write-Host "[SUCCESS] All $totalFilesChecked XAML file(s) passed MC3023 ResourceDictionary & XML syntax validation!" -ForegroundColor Green
    Write-Host "          Pipeline ready to proceed to MSVC C++ & MSBuild/.NET compilation.`n" -ForegroundColor DarkGreen
    exit 0
} else {
    Write-Host "[FATAL] Pre-Build XAML Validation failed with $totalErrors error(s) across $($failedFiles.Count) file(s):" -ForegroundColor Red
    foreach ($f in $failedFiles) {
        Write-Host "  - $f" -ForegroundColor Yellow
    }
    Write-Host "`nHALTING BUILD PIPELINE before MSVC/MSBuild starts to prevent wasted compilation time." -ForegroundColor Red
    Write-Host "Please resolve the MC3023 / syntax errors above and re-run the build script.`n" -ForegroundColor Red
    exit 1
}
