#EC2 Image Must be ami-05d0ec393acfeec85
<powershell>
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

choco install git -params '"/GitAndUnixToolsOnPath"' -y

cd \
refreshenv
New-Alias -Name git -Value "$Env:ProgramFiles\Git\bin\git.exe"
git clone https://github.com/slipmp/forro.git
cd forro
cd Forro.Admin
choco install nodejs.install -y
refreshenv
dotnet publish --configuration Release #> donetpublish-logs.txt

Import-Module WebAdministration 

Set-ItemProperty 'IIS:\Sites\Default Web Site' -Name physicalPath -Value C:\forro\Forro.Admin\bin\Release\netcoreapp2.1\publish
</powershell>