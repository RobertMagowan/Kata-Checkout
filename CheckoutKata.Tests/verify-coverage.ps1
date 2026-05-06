param(
    [string]$SolutionPath = ".\\CheckoutKata.slnx"
)

dotnet test $SolutionPath /p:EnforceCoverageGate=true
exit $LASTEXITCODE
