using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class SpawnerSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var buffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Entities
            .ForEach((Entity e, int entityInQueryIndex, ref CSpawnRequest request, ref Translation pos) =>
            {
                var count = request.Count;
                for (int i = 0; i < count; i++)
                {
                    var instance = buffer.Instantiate(entityInQueryIndex, request.Prefab);
                    buffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = pos.Value });
                }
                

                buffer.DestroyEntity(entityInQueryIndex, e);
            })
            .WithoutBurst()
            .ScheduleParallel();
        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
