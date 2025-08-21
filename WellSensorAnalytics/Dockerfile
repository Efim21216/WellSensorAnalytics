FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /source

COPY --link *.csproj .
RUN dotnet restore -a $TARGETARCH

COPY --link . .
RUN dotnet publish -a $TARGETARCH -c Release -o /app


FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "WellSensorAnalytics.dll"]
