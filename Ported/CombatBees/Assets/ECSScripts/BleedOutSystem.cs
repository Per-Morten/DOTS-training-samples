using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BleedOutSystem : JobComponentSystem
{
    [BurstCompile][ExcludeComponent(typeof(DeathTag))][RequireComponentTag(typeof(BeeTag))]
    struct BleedOutSystemJob : IJobForEachWithEntity<SecondsToDeath>
    {
        public float DeltaTime;
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, ref SecondsToDeath secondsToDeath)
        {
            secondsToDeath.Value -= DeltaTime;
            if (secondsToDeath.Value <= 0.0f)
            {
                CommandBuffer.AddComponent<DeathTag>(index, entity);
            }
        }
    }

    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new BleedOutSystemJob();
        job.DeltaTime = Time.DeltaTime;
        job.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var handle = job.Schedule(this, inputDependencies);
        mCommandBufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }
}