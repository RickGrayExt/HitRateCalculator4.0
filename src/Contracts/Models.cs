using System;
using System.Collections.Generic;

namespace Contracts;

public record RunParams(
    bool UseIntelligentStationAllocation,
    int MaxStationsOpen,
    int MaxSkusPerRack,
    int MaxSkusPerStation,
    bool PrioritizeHitRate,
    int OrderBatchSize,
    int MaxOrdersPerBatch,
    int MaxRacks
);

public record SkuDemand(string SkuId, int TotalUnits, int OrderCount, double Velocity, bool Seasonal, string Category);
public record SkuGroup(string Category, List<string> Skus);
public record ShelfLocation(string SkuId, int RackId, int ShelfNumber, int Position);
public record Rack(int RackId, int Capacity, List<string> SkuIds);
public record Batch(Guid BatchId, List<string> OrderIds, List<string> Skus);
public record StationAssignment(int StationId, List<Guid> BatchIds);
public record HitRateResult(Guid RunId, double HitRate, int TotalOrders, int SingleStationOrders);

public record StartRunCommand(Guid RunId, string DatasetPath, string Mode, RunParams Params);
public record SalesPatternsIdentified(Guid RunId, List<SkuDemand> Demand, RunParams Params);
public record SkuGroupsCreated(Guid RunId, List<SkuGroup> Groups, List<SkuDemand> Demand, RunParams Params);
public record ShelfLocationsAssigned(Guid RunId, List<ShelfLocation> Locations, List<SkuDemand> Demand, RunParams Params);
public record RackLayoutCalculated(Guid RunId, List<Rack> Racks, List<ShelfLocation> Locations, List<SkuDemand> Demand, RunParams Params);
public record BatchesCreated(Guid RunId, List<Batch> Batches, string Mode, RunParams Params);
public record StationsAllocated(Guid RunId, List<StationAssignment> Assignments, RunParams Params);
public record HitRateCalculated(Guid RunId, HitRateResult Result);