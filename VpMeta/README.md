# VpMeta
Demo project of .NET 8 data upload, validation & storage microservice

## Configuation
VpMeta supports following options provided through *appsettings.json* or as command line arguments.

| Option |Type|Default value|Description|
| ------ | ------ | ------ | ------ |
| Migration | bool | false | Perform DB migration and exit |
| EnableSwagger | bool | true | Enable Swagger endpoint (/swagger/) |
| MaxUploadLimit | long | 50000 | Limit document size for upload (bytes) |
| Db:Type | enum | sqlite | Select DB driver engine (sqlite or mssql) |
| Db:ConnectionString | string | Data Source=/Db/ctm.db | DB connection string |

## Deploying & Running (docker+SqLite)
After cloning the repository run following command in its folder.

1. Build the docker image 
```
docker build -t vpmeta .
```
2. Create a DB docker volume
```
docker volume create vpdb
```
3. Perform an initial DB migration
```
docker run --rm -v vpdb:/Db -p 8080:8080 vpmeta Migration=true
```
4. Run dockerized app
```
docker run --rm -v vpdb:/Db -p 8080:8080 -d vpmeta
```
5. SwaggerUI is at http://localhost:8080/swagger/

## Deploying & Running (docker+MSSQL)
After cloning the repository run following command in its folder.

1. Build the docker image 
```
docker build -t vpmeta .
```
2. Perform an initial DB migration
```
docker run --rm -p 8080:8080 vpmeta Db:Type=mssql "Db:ConnectionString=<Your DB connection string here>" Migration=true
```
3. Run dockerized app
```
docker run --rm -p 8080:8080 -d vpmeta Db:Type=mssql "Db:ConnectionString=<Your DB connection string here>"
```
4. SwaggerUI is at http://localhost:8080/swagger/

