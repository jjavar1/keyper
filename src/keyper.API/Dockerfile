FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh
EXPOSE 5000
EXPOSE 5001
ENV DatabaseProvider=postgresql
ENV ConnectionStrings__DefaultConnection="Host=keyper-db;Database=keyper;Username=admin;Password=securepass"
ENTRYPOINT ["./wait-for-it.sh", "keyper-db", "5432", "--", "dotnet", "keyper.API.dll"]
