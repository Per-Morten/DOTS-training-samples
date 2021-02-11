using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[UpdateInGroup(typeof(DebugSystemGroup))]
public class RenderResourceToFloorDebugSystem : SystemBase
{
    private EntityQuery mGridQuery;
    protected override void OnCreate()
    {
        mGridQuery = GetEntityQuery(ComponentType.ReadOnly<CGridCellCount>());
        Enabled = false;
    }

    protected override void OnUpdate()
    {
        var grid = Entity.Null;
        {
            var tmp = mGridQuery.ToEntityArray(Allocator.TempJob);
            grid = tmp.Length > 0 ? tmp[0] : Entity.Null;
            tmp.Dispose();
        }

        Debug.Assert(grid != Entity.Null);

        var cellSize = EntityManager.GetComponentData<CGridCellSize>(grid).Value;
        var cellCount = EntityManager.GetComponentData<CGridCellCount>(grid).Value;
        var gridPos = EntityManager.GetComponentData<CGridPos>(grid).Value;
    
        Entities
            .WithAll<TResource>()
            .ForEach((in Translation t, in CGridIndex idx) =>
            {
                DBG.Line(t.Value, new float3(t.Value.xz, -Settings.Instance.FieldSize.y / 2.0f).xzy, Color.blue);
                DBG.Point(new float3(t.Value.xz, -Settings.Instance.FieldSize.y / 2.0f).xzy, Color.green);
                var gp = gridPos + (cellSize * idx.Value) + cellSize * 0.5f;
                DBG.Point(new float3(gp, -Settings.Instance.FieldSize.y / 2.0f).xzy, Color.cyan);
            })
            .WithoutBurst()
            .Run();
    }
}
