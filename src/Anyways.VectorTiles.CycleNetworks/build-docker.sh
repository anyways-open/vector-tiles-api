dotnet publish -c Release --runtime linux-x64 
sudo docker build --tag anyways-vector-tiles-generator:0.0.1 . 
sudo docker run --rm -v `pwd`:/var/app/source/ -v `pwd`/outputtiles/:/var/app/tiles/ anyways-vector-tiles-generator:0.0.1

