#! /bin/bash

# Updates the docker image from dockerhub
# Builds vector tiles with the latest routerdb

echo "Running vector tile generation"


docker pull anywaysopen/vector-tiles-generator:latest
docker run --rm -v /var/data/mapdata/:/var/app/source/ -v /var/services/vector-tiles-api/data/cyclenetworks-test/:/var/app/tiles/ anyways-open/vector-tiles-generator:latest
