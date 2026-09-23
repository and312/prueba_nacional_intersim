param(
    [string]$mdPath = "c:\Users\DELL XPS\Desktop\INTERSIM\nacional\arq\ARQUITECTURA_BACKEND_NET8.md",
    [string]$pdfPath = "c:\Users\DELL XPS\Desktop\INTERSIM\nacional\arq\ARQUITECTURA_BACKEND_NET8.pdf"
)

Write-Host "Reading Markdown from $mdPath"
$lines = Get-Content -Path $mdPath -Encoding UTF8

# Create Word application COM object
Write-Host "Initializing Word COM Object..."
$word = New-Object -ComObject Word.Application
$word.DisplayAlerts = 0
$word.Visible = $false
$doc = $word.Documents.Add()
$selection = $word.Selection
Write-Host "Word COM Object initialized successfully!"

# Custom helper to write text with formatting
function Add-Paragraph ($text, $fontSize = 11, $bold = $false, $color = "Auto", $spaceAfter = 6, $italic = $false) {
    $selection.Font.Name = "Segoe UI"
    $selection.Font.Size = $fontSize
    $selection.Font.Bold = [int]$bold
    $selection.Font.Italic = [int]$italic
    if ($color -eq "Blue") {
        $selection.Font.Color = 16737843 # Dark blue
    } else {
        $selection.Font.Color = 9999999 # Auto/Default black
    }
    $selection.ParagraphFormat.SpaceAfter = $spaceAfter
    $selection.ParagraphFormat.LineSpacingRule = 0 # Single spacing
    $selection.TypeText($text)
    $selection.TypeParagraph()
}

# Custom helper to render tables
function Render-Table ($rowsData) {
    if ($rowsData.Count -lt 1) { return }
    
    # Parse headers and rows
    $parsedRows = New-Object System.Collections.Generic.List[System.Object]
    foreach ($rowLine in $rowsData) {
        # Skip separator rows like | :---: | :--- |
        if ($rowLine -match '^\|\s*[-:]+\s*\|') { continue }
        
        $cells = $rowLine.Split('|')
        # Clean empty columns at start/end
        $cleanedCells = @()
        for ($i = 1; $i -lt ($cells.Length - 1); $i++) {
            $cleanedCells += $cells[$i].Trim()
        }
        $parsedRows.Add($cleanedCells)
    }

    $rowCount = $parsedRows.Count
    if ($rowCount -lt 1) { return }
    $colCount = $parsedRows[0].Length

    Write-Host "Creating Word table with $rowCount rows and $colCount columns"
    $table = $doc.Tables.Add($selection.Range, $rowCount, $colCount)
    $table.Borders.Enable = $true
    
    # Style the table
    $table.Style = "Table Grid"
    
    for ($r = 0; $r -lt $rowCount; $r++) {
        $rowCells = $parsedRows[$r]
        for ($c = 0; $c -lt $colCount; $c++) {
            $cellText = ""
            if ($c -lt $rowCells.Length) { $cellText = $rowCells[$c] }
            
            $cell = $table.Cell($r + 1, $c + 1)
            $cell.Range.Text = $cellText
            
            # Header formatting
            if ($r -eq 0) {
                $cell.Range.Font.Bold = $true
                $cell.Range.Font.Size = 10
                $cell.Shading.BackgroundPatternColor = 14211288 # Light gray
            } else {
                $cell.Range.Font.Bold = $false
                $cell.Range.Font.Size = 9.5
            }
            $cell.Range.Font.Name = "Segoe UI"
        }
    }
    
    # Move selection past the table
    $selection.Start = $table.Range.End
    $selection.TypeParagraph()
}

# Custom helper to render Mermaid diagrams using Mermaid.ink API
function Render-Mermaid ($codeLines) {
    if ($codeLines.Count -lt 1) { return }
    
    $mermaidCode = $codeLines -join "`n"
    Write-Host "Rendering Mermaid diagram..."
    
    # Create JSON payload
    $jsonObj = @{
        code = $mermaidCode
        mermaid = @{
            theme = "default"
        }
    }
    $jsonString = ConvertTo-Json -InputObject $jsonObj -Compress
    
    # Convert to URL-safe Base64
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($jsonString)
    $base64 = [System.Convert]::ToBase64String($bytes)
    $urlSafeBase64 = $base64.Replace('+', '-').Replace('/', '_').Replace('=', '')
    
    $url = "https://mermaid.ink/img/$urlSafeBase64"
    $tempFile = [System.IO.Path]::GetTempFileName() + ".png"
    
    try {
        Write-Host "Downloading rendered diagram..."
        # Set security protocol to TLS 1.2/1.3 for newer APIs
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13
        Invoke-RestMethod -Uri $url -Method Get -OutFile $tempFile
        
        Write-Host "Inserting diagram image into Word..."
        # Insert image
        $shape = $selection.InlineShapes.AddPicture($tempFile)
        
        # Center the paragraph containing the image
        $selection.ParagraphFormat.Alignment = 1 # wdAlignParagraphCenter
        $selection.TypeParagraph()
        $selection.ParagraphFormat.Alignment = 0 # wdAlignParagraphLeft
    } catch {
        Write-Warning "Failed to render Mermaid diagram locally, embedding raw code: $_"
        # Fallback to plain text inside the doc
        Add-Paragraph "--- [INICIO DIAGRAMA MERMAID] ---" -italic $true -fontSize 9.5
        
        $selection.Font.Name = "Consolas"
        $selection.Font.Size = 9
        $selection.TypeText(($codeLines -join "`n"))
        $selection.TypeParagraph()
        
        $selection.Font.Name = "Segoe UI"
        $selection.Font.Size = 11
        Add-Paragraph "--- [FIN DIAGRAMA MERMAID] ---" -italic $true -fontSize 9.5
    } finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        }
    }
}

# Custom helper to render regular code blocks in monospace format
function Render-CodeBlock ($codeLines, $type) {
    if ($codeLines.Count -lt 1) { return }
    
    # Save current font settings
    $oldFontName = $selection.Font.Name
    $oldFontSize = $selection.Font.Size
    
    # Set to Consolas/monospace for code blocks
    $selection.Font.Name = "Consolas"
    $selection.Font.Size = 9
    $selection.Font.Bold = 0
    $selection.Font.Italic = 0
    $selection.Font.Color = 4210752 # Charcoal / Dark gray
    
    # Left indent for code block representation
    $selection.ParagraphFormat.LeftIndent = 18
    
    # Write code lines in a single COM call for speed
    $codeText = $codeLines -join "`n"
    $selection.TypeText($codeText)
    $selection.TypeParagraph()
    
    # Reset layout and fonts
    $selection.ParagraphFormat.LeftIndent = 0
    $selection.Font.Name = $oldFontName
    $selection.Font.Size = $oldFontSize
    $selection.Font.Color = 9999999 # Auto/Default
}

$tableLines = New-Object System.Collections.Generic.List[string]
$inTable = $false
$inCodeBlock = $false
$codeBlockLines = New-Object System.Collections.Generic.List[string]
$codeBlockType = ""

foreach ($line in $lines) {
    $lineTrimmed = $line.Trim()
    
    # Handle Code Blocks
    if ($lineTrimmed -like "```*") {
        if (-not $inCodeBlock) {
            $inCodeBlock = $true
            $codeBlockType = ($lineTrimmed -replace '^```', '').Trim().ToLower()
            $codeBlockLines.Clear()
            
            # If we were building a table, close it
            if ($inTable) {
                Render-Table $tableLines
                $tableLines.Clear()
                $inTable = $false
            }
            continue
        } else {
            $inCodeBlock = $false
            if ($codeBlockType -eq "mermaid") {
                Render-Mermaid $codeBlockLines
            } else {
                Render-CodeBlock $codeBlockLines $codeBlockType
            }
            continue
        }
    }
    
    # If we are inside a code block, collect lines as-is
    if ($inCodeBlock) {
        $codeBlockLines.Add($line)
        continue
    }
    
    # Handle Tables
    if ($lineTrimmed -like "|*") {
        $inTable = $true
        $tableLines.Add($lineTrimmed)
        continue
    } else {
        if ($inTable) {
            # End of table, render it
            Render-Table $tableLines
            $tableLines.Clear()
            $inTable = $false
        }
    }
    
    # Handle Headers
    if ($lineTrimmed -match '^#\s+(.*)') {
        $headerText = $Matches[1]
        Add-Paragraph $headerText -fontSize 18 -bold $true -color "Blue" -spaceAfter 12
    }
    elseif ($lineTrimmed -match '^##\s+(.*)') {
        $headerText = $Matches[1]
        Add-Paragraph $headerText -fontSize 14 -bold $true -color "Blue" -spaceAfter 8
    }
    elseif ($lineTrimmed -match '^###\s+(.*)') {
        $headerText = $Matches[1]
        Add-Paragraph $headerText -fontSize 11.5 -bold $true -spaceAfter 6
    }
    # Handle Bullet Points
    elseif ($lineTrimmed -match '^[\*\-]\s+(.*)') {
        $bulletText = "• " + $Matches[1]
        Add-Paragraph ($bulletText -replace '\*\*', '') -fontSize 10.5 -spaceAfter 4
    }
    # Handle Horizontal Rules
    elseif ($lineTrimmed -eq "---") {
        $selection.TypeParagraph()
    }
    # Handle Normal Paragraphs
    elseif ($lineTrimmed -ne "") {
        $cleanLine = $lineTrimmed -replace '\*\*', ""
        Add-Paragraph $cleanLine -fontSize 10.5 -spaceAfter 6
    }
}

# Final flush for table or code block in case markdown ends with one of them
if ($inTable) {
    Render-Table $tableLines
}
if ($inCodeBlock) {
    if ($codeBlockType -eq "mermaid") {
        Render-Mermaid $codeBlockLines
    } else {
        Render-CodeBlock $codeBlockLines $codeBlockType
    }
}

# Save and Close Word document as PDF
Write-Host "Saving document to $pdfPath as PDF"
# 17 is wdFormatPDF
$doc.SaveAs([ref]$pdfPath, [ref]17)
$doc.Close([ref]0) # wdDoNotSaveChanges
$word.Quit()

Write-Host "PDF document created successfully!"
