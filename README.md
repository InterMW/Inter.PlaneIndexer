# Inter.PlaneIndexer

## Summary
This microservice is in charge of taking compiled plane frames and exposing them on a per-plane
and per-minute basis.

# Overview
A brief description of each process

## Ingest

This process listens for plane frame messages and stores them in redis.

## Indexer

This process, triggered once a minute, reviews the last minute of plane data and stores it to couchbase as a document linked to the previous minute each plane was sighted.

## Access

This process is a webapi which decides which datastore is relevant and then exposes the plane info as a chain of a linked list.

# How to run

Clone this repository and run with `dotnet run --project Application/Application.csproj`

## General information

This project requires dotnet 6 sdk to run (install link [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)).

When running locally, I have the rabbit password replaced using the dotnet user-secrets tool. 
Please follow Microsoft's [guide](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux) to set the value of rabbit_pass to your configured rabbit account's password for the Applications.csproj.

## Required Infrastructure
|Product|Details|Database Install Link|
|-|-|-|
|Redis| Update the PlaneHistoryContext value of the ConnectionStrings section of [appsettings.json](Application/appsettings.json).| Docker installation guide for redis [here](https://github.com/bitnami/containers/blob/main/bitnami/redis/README.md).|
|RabbitMQ| The code will create the exchanges, queues, and bindings for you, just update the Rabbit:ClientDeclarations:Connections:0 details in [appsettings.json](Application/appsettings.json). Note that this will not give you access to the incoming data stream, please reach out to me, if interested.  To trigger the Indexer, send a message to the Clock exchange following the binding as described by the appsettings.json file.| Docker installation guide for RabbitMQ [here](https://hub.docker.com/_/rabbitmq).|
|Couchbase| Update the Couchbase section with your url and a user that has Full Admin rights.| Docker installation guide for Couchbase [here](https://docs.couchbase.com/server/current/install/getting-started-docker.html)|
