
# HitRate Microservices (Clean Build)

Run:
```
docker compose build
docker compose up
```
UI: http://localhost:8081
Gateway: http://localhost:8080/health

Place your CSV at `./data/DataSetClean.csv` with columns:
`sku_id,units,orders,velocity,seasonal,category`
