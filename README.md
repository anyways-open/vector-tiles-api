# vector-tiles-api

[![Build status](https://build.anyways.eu/app/rest/builds/buildType:(id:anyways_Core_VectorTilesApi)/statusIcon)](https://build.anyways.eu/viewType.html?buildTypeId=anyways_Core_VectorTilesApi)  

Status: **PRODUCTION**

The vector tiles microservice.

This microservice has the responsability to serve mapbox vector tiles reusable in different anyways applications. This API is only to used for relatively static data derived from OSM data or other sources.

The included console project can be used to extract vector tiles from a router db, using a specifications-json. (Note that this functionality will move to IDP one day). 

To create a vector tile set:

````
cd src/Anyways.VectorTiles.CycleNetworks/
dotnet run <path to routerdb> <path to specification of the vector tile> <output directory>
````

## Deployment

Docker as usual, see [management repo](https://github.com/anyways-open/management/blob/master/infrastructure/HETZNER-EX41-SSD-842855.md) for more info.
