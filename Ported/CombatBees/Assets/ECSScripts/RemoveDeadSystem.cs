using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RemoveDeadSystem : JobComponentSystem
{
    [BurstCompile]
    public struct RemoveDeadJob : IJobForEachWithEntity<DeathTag>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref DeathTag _)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }

    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
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
}