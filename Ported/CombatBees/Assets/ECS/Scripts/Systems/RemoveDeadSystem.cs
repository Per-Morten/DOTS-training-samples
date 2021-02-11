using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RemoveDeadSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithAll<TDeath>()
            .ForEach((Entity e, int entityInQueryIndex) =>
            {
                ecb.DestroyEntity(entityInQueryIndex, e);
            })
            .WithName("RemoveDead")
            .ScheduleParallel();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}