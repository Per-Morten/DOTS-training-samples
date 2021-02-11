using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BleedOutSystem : SystemBase
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
            .WithNone<TDeathTag>()
            .ForEach((Entity e, int entityInQueryIndex, ref CSecondsToDeath secondsToDeath) =>
            {
                secondsToDeath.Value -= dt;
                if (secondsToDeath.Value <= 0.0f)
                    ecb.AddComponent<TDeathTag>(entityInQueryIndex, e);
            })
            .WithName("BleedOut")
            .ScheduleParallel();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}