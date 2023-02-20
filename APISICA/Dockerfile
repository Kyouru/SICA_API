#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS="http://+:5000"

ENV TZ=America/Lima
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["APISICA/APISICA.csproj", "APISICA/"]
RUN dotnet restore "APISICA/APISICA.csproj"
COPY . .
WORKDIR "/src/APISICA"
RUN dotnet build "APISICA.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "APISICA.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APISICA.dll"]