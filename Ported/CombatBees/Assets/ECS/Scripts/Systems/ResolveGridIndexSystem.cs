using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public class ResolveGridIndexSystem : SystemBase
{
    private EntityQuery mGridQuery;
    protected override void OnCreate()
    {
        mGridQuery = GetEntityQuery(ComponentType.ReadOnly<CGridCellCount>());
    }

    protected override void OnUpdate()
    {
        Entity grid;
        using (var tmp = mGridQuery.ToEntityArray(Allocator.TempJob))
            grid = tmp.Length > 0 ? tmp[0] : Entity.Null;

        Debug.Assert(grid != Entity.Null);

        var cellSize = EntityManager.GetComponentData<CGridCellSize>(grid).Value;
        var cellCount = EntityManager.GetComponentData<CGridCellCount>(grid).Value;
        var gridPos = EntityManager.GetComponentData<CGridPos>(grid).Value;

        Entities
            .WithAll<TResource>()
            .ForEach((ref CGridIndex idx, in Translation t) =>
            {
                var pos = t.Value;
                idx.Value = (int2)(math.clamp(math.floor((pos.xz - cellSize * 0.5f - gridPos) / cellSize), float2.zero, cellCount));
            })
            .ScheduleParallel();


    }
}
