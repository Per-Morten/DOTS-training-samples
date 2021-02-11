using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Debug = UnityEngine.Debug;

public class ResolveResourceStacksSystem : SystemBase
{
    private EntityQuery mGridQuery;
    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
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
        var stackHeights = EntityManager.GetBuffer<BStackHeights>(grid);
        var floorStart = -Settings.Instance.FieldSize.y * 0.5f;
        var resourcesSize = Settings.Instance.ResourceSize;

        var ecb = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<TResource>()
            .WithNone<CHeldBy, CStackHeight>()
            .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref CVelocity velocity, in CGridIndex idx) =>
            {
                var cellStackIdx = idx.Value.y * cellCount.x + idx.Value.x;
                var stackHeight = stackHeights[cellStackIdx].Value;
                var floorHeight = floorStart + (stackHeight + 0.5f) * resourcesSize;

                if (translation.Value.y < floorHeight)
                {
                    if (floorHeight + resourcesSize < -floorStart)
                    {
                        translation.Value = new float3(translation.Value.xz, floorHeight).xzy;
                        velocity.Value = float3.zero;
                        ecb.AddComponent(entityInQueryIndex, e, new CStackHeight { Value = stackHeight++ });
                        stackHeights[cellStackIdx] = new BStackHeights { Value = stackHeight };
                    }
                    else
                    {
                        ecb.DestroyEntity(entityInQueryIndex, e);
                    }
                }
            })
            .Schedule();
        mCommandBufferSystem.AddJobHandleForProducer(Dependency);

    }
}
