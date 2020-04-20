#define NEW
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RemoveDeadSystem
#if NEW
    : SystemBase
#else
    : JobComponentSystem
#endif
{
    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

#if NEW
    protected override void OnUpdate()
    {
        var ecb = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        Entities
            .WithAll<DeathTag>()
            .ForEach((Entity e, int entityInQueryIndex) =>
            {
                ecb.DestroyEntity(entityInQueryIndex, e);
            })
            .WithName("RemoveDead")
            .ScheduleParallel();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

#else
    [BurstCompile]
    public struct RemoveDeadJob : IJobForEachWithEntity<DeathTag>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref DeathTag _)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RemoveDeadJob
        {
            CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        mCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
#endif
}