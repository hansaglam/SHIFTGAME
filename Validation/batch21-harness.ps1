param([string]$InputFile = "$PSScriptRoot/batch21-candidates.json", [string]$OutputFile = "$PSScriptRoot/batch21-analysis.json")
$ErrorActionPreference='Stop'
# Authoring aid: compile the actual BoardManager, LevelData and analyzer sources. The tiny
# Unity stubs below only supply attributes/ScriptableObject; no simulation code is replaced.
$gameRoot = Join-Path $PSScriptRoot '../Assets/_Game'
$shim = @'
namespace UnityEngine {
 public class ScriptableObject { public string name; }
 public class SerializeField : System.Attribute {}
 public class RangeAttribute : System.Attribute { public RangeAttribute(int a,int b){} }
 public class MinAttribute : System.Attribute { public MinAttribute(int a){} }
 public class TextAreaAttribute : System.Attribute {}
 public class TooltipAttribute : System.Attribute { public TooltipAttribute(string a){} }
 public class CreateAssetMenuAttribute : System.Attribute { public string fileName,menuName; }
}
namespace Shift.Game {
 public class LevelDesign {}
 public class LevelDesignCatalog { public static LevelDesignCatalog Current => null; public LevelDesign Find(LevelData l)=>null; }
}
'@
$paths=@('Scripts/Core/GameEnums.cs','Scripts/Core/GridPosition.cs','Scripts/Core/DirectionUtility.cs','Scripts/Board/BoardAction.cs','Scripts/Board/BoardManager.cs','Scripts/Levels/LevelData.cs','Scripts/Levels/VerifiedOptimality.cs','Editor/PuzzleBatchAnalysis.cs')
# Compile each file as its own syntax unit so using directives retain their scope.
$shimPath=Join-Path $PSScriptRoot 'batch21-unity-stubs.cs'
Set-Content -LiteralPath $shimPath -Value $shim
Add-Type -CompilerOptions '/nowarn:0649' -Path (@($shimPath)+@($paths | ForEach-Object { Join-Path $gameRoot $_ }))
$results=@()
foreach($entry in (Get-Content -LiteralPath $InputFile -Raw | ConvertFrom-Json)) {
 $level=[Shift.Game.LevelData]::new();$level.name=$entry.name
 $pieces=[System.Collections.Generic.List[Shift.Game.PiecePlacement]]::new()
 foreach($p in $entry.pieces) { $pieces.Add([Shift.Game.PiecePlacement]::new($p.type,$p.color,$p.direction,[Shift.Game.GridPosition]::new($p.x,$p.y),$p.channel,$p.initialOpen)) }
 $width=if($entry.width){$entry.width}else{$entry.size};$height=if($entry.height){$entry.height}else{$entry.size}
 $level.Configure($width,$height,$entry.budget,[Shift.Game.PieceColor]::Red,$pieces)
 $analyzer=[Shift.Game.Editor.PuzzleBatchAnalysis]::new($level)
 $result=$analyzer.Analyze($null)
 $results+= [pscustomobject]@{number=$entry.n;name=$entry.name;fingerprint=[Shift.Game.VerifiedOptimality]::Fingerprint($level);analysis=$result}
}
$results | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $OutputFile
$results | ForEach-Object { [pscustomobject]@{level=$_.number;min=$_.analysis.minimum;paths=$_.analysis.solutionPaths;prob=$_.analysis.randomSolveProbability;decisions=$_.analysis.meaningfulDecisions;depth=$_.analysis.finalDepth;states=$_.analysis.states;route=($_.analysis.shortest | ForEach-Object { "($($_.x),$($_.y))" }) -join ' '} } | Format-Table -AutoSize
