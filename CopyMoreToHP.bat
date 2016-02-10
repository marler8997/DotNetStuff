

.\CopyProject\bin\Debug\CopyProject.exe -n More:HP.Libraries -t dst:HP.Libraries.template     More\More.csproj         ..\esmodeling\Source\HP\Libraries\HP.Libraries.csproj
REM .\CopyProject\bin\Debug\CopyProject.exe -n More:HP.Libraries -t dst:HP.Libraries.CE.template  More\More.CE.csproj      ..\esmodeling\Source\HP\Libraries\HP.Libraries.CE.csproj

.\CopyProject\bin\Debug\CopyProject.exe -n More:HP.Libraries -t dst:HP.Libraries.Npc.template More\Npc\More.Npc.csproj ..\esmodeling\Source\HP\Libraries\Npc\HP.Libraries.Npc.csproj

.\CopyProject\bin\Debug\CopyProject.exe -n More:HP.Libraries -t dst:NpcClientGenerator.template More\Npc\NpcClientGenerator\NpcClientGenerator.csproj ..\esmodeling\Source\HP\Libraries\Npc\NpcClientGenerator\NpcClientGenerator.csproj

REM .\CopyProject\bin\Debug\CopyProject.exe -n More:HP.Libraries -t dst:HP.Libraries.Lfd.template More\Lfd\More.Lfd.csproj ..\esmodeling\Source\HP\Libraries\Lfd\HP.Libraries.Lfd.csproj