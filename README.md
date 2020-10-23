# vector-tiles-api

[![Build status](https://build.anyways.eu/app/rest/builds/buildType:(id:anyways_Core_VectorTilesApi)/statusIcon)](https://build.anyways.eu/viewType.html?buildTypeId=anyways_Core_VectorTilesApi)  

Status: **PRODUCTION**

The vector tiles microservice.

This microservice has the responsibility to generate and to serve mapbox vector tiles, reusable in different anyways applications. This API is only to used for relatively static data derived from OSM data or other sources. The prime example is cycling networks - which only update sporadically.

## Architecture

### Anyways.Vectortiles

Only contains a few data structures - nearly empty.

### Anyways.VectorTiles.CycleNetworks

The console project used to generate vector tiles based on a routerdb. For this, a specifications-json is used. (Note that this functionality will move to another project one day). 

To create a vector tile set:

````
cd src/Anyways.VectorTiles.CycleNetworks/
dotnet run <path to routerdb> <path to specification of the vector tile> <output directory>
````

### Anyways.VectorTiles.API

Bland ASP.NET project with an http-server to serve the right vector tiles when queried.

## Deployment

Docker as usual, see [management repo](https://github.com/anyways-open/management/blob/master/infrastructure/HETZNER-EX41-SSD-842855.md) for more info.
