param([int]$Number,[int[]]$Taps)
. "$PSScriptRoot/batch36-harness.ps1" -InputFile "$PSScriptRoot/batch36-candidates.json" -OutputFile "$PSScriptRoot/batch36-analysis.json"
$entry=(Get-Content "$PSScriptRoot/batch36-candidates.json" -Raw | ConvertFrom-Json) | Where-Object n -eq $Number
$l=[Shift.Game.LevelData]::new();$placements=[System.Collections.Generic.List[Shift.Game.PiecePlacement]]::new()
foreach($p in $entry.pieces){$placements.Add([Shift.Game.PiecePlacement]::new($p.type,$p.color,$p.direction,[Shift.Game.GridPosition]::new($p.x,$p.y),$p.channel,$p.initialOpen))}
$l.Configure($entry.width,$entry.height,$entry.budget,[Shift.Game.PieceColor]::Red,$placements)
$l.ConfigureTargetColors([Shift.Game.PieceColor[]]$entry.targets)
$b=[Shift.Game.BoardManager]::new();$b.Load($l)
$route=if($Taps){for($i=0;$i -lt $Taps.Length;$i+=2){[Shift.Game.GridPosition]::new($Taps[$i],$Taps[$i+1])}}else{([Shift.Game.Editor.PuzzleBatchAnalysis]::new($l)).Analyze($null).shortest}
foreach($tap in $route){$accepted=$b.RequestMove($tap);$b.CompleteResolution();"Tap $tap depth $($b.ReactionDepth) state $($b.State)";$b.Pieces | Where-Object {$_.Active -and $_.Type -in 'Normal','PushBlock'} | ForEach-Object {"$($_.Color) $($_.Type) $($_.Position) $($_.Direction)"}}
