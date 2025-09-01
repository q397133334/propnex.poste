## About this solution

Develop Using Visual Studio 2022
The project Propnex.Poster.WinClient is developed with a CEF-based browser. It is used to log in to https://www.iproperty.com.my and is responsible for content publishing.
The project Propnex.Poster.WebServer is responsible for obtaining the content to be published (the content is provided by other services). This service is in charge of acquiring and integrating the content that needs to be published, and the client will then obtain the processed content again.This is a minimalist, non-layered startup solution with the ABP Framework. All the fundamental ABP modules are already installed.
The application needs to connect to a database. Run the following command in the `Propnex.Poster` directory:
````bash
dotnet run --migrate-database
````