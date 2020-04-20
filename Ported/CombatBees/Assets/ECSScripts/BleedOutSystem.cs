#define NEW
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BleedOutSystem
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

    protected override void OnUpdate()
    {
        var ecb = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        float dt = Time.DeltaTime;
        Entities
            .WithNone<DeathTag>()
            .ForEach((Entity e, int entityInQueryIndex, ref SecondsToDeath secondsToDeath) =>
            {
                secondsToDeath.Value -= dt;
                if (secondsToDeath.Value <= 0.0f)
                    ecb.AddComponent<DeathTag>(entityInQueryIndex, e);
            })
            .WithName("BleedOut")
            .ScheduleParallel();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

#if NEW
#else
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

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new BleedOutSystemJob();
        job.DeltaTime = Time.DeltaTime;
        job.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var handle = job.Schedule(this, inputDependencies);
        mCommandBufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }
#endif
}