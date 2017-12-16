
function updateVersion($path){

    [Xml]$csproj = Get-Content $path
    $csproj.Project.PropertyGroup.PackageVersion = "1.0.1"
    $csproj.Save($path)

    dotnet pack $path --output "$PSScriptRoot\Dist" --include-symbols --include-source

}

updateVersion "$PSScriptRoot\Source\IGOEnchi.SmartGameLib\IGOEnchi.SmartGameLib.csproj"
updateVersion "$PSScriptRoot\Source\IGOEnchi.GoGameLib\IGOEnchi.GoGameLogic.csproj"
updateVersion "$PSScriptRoot\Source\IGOEnchi.GoGameSgf\IGOEnchi.GoGameSgf.csproj"




