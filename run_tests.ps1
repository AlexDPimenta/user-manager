# 1. clean older results
if (Test-Path "coveragereport") { Remove-Item -Recurse -Force "coveragereport" }

# 2. run and generate coverage
dotnet test --settings coverlet.runsettings --no-restore

# 3. transform coverage xml o html
reportgenerator -reports:"tests/UserManager.UnitTests/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# 4. open the report
Start-Process "coveragereport/index.html"