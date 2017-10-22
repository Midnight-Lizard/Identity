#===========================================#
#				PROJECT	BUILD				#
#===========================================#
FROM microsoft/aspnetcore-build:2 as dotnet-build
COPY *.csproj /build/
WORKDIR /build
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o ./results

#===========================================#
#				IMAGE	BUILD				#
#===========================================#
FROM microsoft/aspnetcore:2 as image
WORKDIR /app
EXPOSE 80
COPY --from="dotnet-build" /build/results .
ENTRYPOINT ["dotnet", "MidnightLizard.Web.Identity.dll"]
