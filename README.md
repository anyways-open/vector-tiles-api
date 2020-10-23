# vector-tiles-api

[![Build status](https://build.anyways.eu/app/rest/builds/buildType:(id:anyways_Core_VectorTilesApi)/statusIcon)](https://build.anyways.eu/viewType.html?buildTypeId=anyways_Core_VectorTilesApi)  

Status: **PRODUCTION**

The vector tiles microservice.

This microservice has the responsibility to generate and to serve [Mapbox Vector Tiles (.mvt)](https://github.com/mapbox/vector-tile-spec/). This API is only to used for relatively static data derived from OSM data or other sources. 

## What?

The service does two main things:
- Serve the `*.mvt` tiles.
- Serve the `mvt.json` files with the correct URL for the tiles using headers in the request.

## Screenshots

<img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot01.png" width="200"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot02.png" width="400"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot03.png" width="400"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot04.png" width="400"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot05.png" width="400"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot06.png" width="400"/> <img src="https://github.com/anyways-open/vector-tiles-api/raw/develop/docs/screenshots/screenshot07.png" width="200"/> 