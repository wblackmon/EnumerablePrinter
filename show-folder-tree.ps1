function Show-FolderTree {
    param(
        [string]$Path = ".",
        [int]$Indent = 0
    )

    # Folders to skip
    $skip = @("bin", "obj", ".vs", "TestResults", "packages", "node_modules")

    # Get items, skip unwanted folders
    $items = Get-ChildItem $Path | Sort-Object Name |
             Where-Object { $skip -notcontains $_.Name }

    foreach ($item in $items) {
        $prefix = " " * $Indent

        if ($item.PSIsContainer) {
            Write-Host "$prefix├── 📁 $($item.Name)" -ForegroundColor Cyan
            Show-FolderTree -Path $item.FullName -Indent ($Indent + 4)
        }
        else {
            Write-Host "$prefix└── 📄 $($item.Name)" -ForegroundColor Gray
        }
    }
}

Show-FolderTree
