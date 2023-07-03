FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DatabaseBranchSwitcherService/DatabaseBranchSwitcherService.csproj", "DatabaseBranchSwitcherService/"]
RUN dotnet restore "DatabaseBranchSwitcherService/DatabaseBranchSwitcherService.csproj"
COPY . .
WORKDIR "/src/DatabaseBranchSwitcherService"
RUN dotnet build "DatabaseBranchSwitcherService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatabaseBranchSwitcherService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseBranchSwitcherService.dll"]
