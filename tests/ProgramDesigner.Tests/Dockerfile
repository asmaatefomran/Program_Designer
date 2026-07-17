FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

COPY . .

RUN dotnet restore tests/ProgramDesigner.Tests/ProgramDesigner.Tests.csproj

CMD ["dotnet", "test", "tests/ProgramDesigner.Tests/ProgramDesigner.Tests.csproj", "--no-restore", "--logger:trx"]