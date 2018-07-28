#===========================================#
#				PROJECT	BUILD				#
#===========================================#
FROM microsoft/dotnet:2.1-sdk as dotnet-build
ARG DOTNET_CONFIG=Release
WORKDIR /build
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c ${DOTNET_CONFIG} -o ./results

#===========================================#
#				IMAGE	BUILD				#
#===========================================#
FROM microsoft/dotnet:2.1-aspnetcore-runtime as image
WORKDIR /app
EXPOSE 80
COPY --from=dotnet-build /build/results .
ENTRYPOINT ["dotnet", "MidnightLizard.Web.Identity.dll"]
