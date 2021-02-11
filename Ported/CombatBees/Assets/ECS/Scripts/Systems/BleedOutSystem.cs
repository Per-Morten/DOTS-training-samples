using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BleedOutSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float dt = Time.DeltaTime;
        Entities
            .WithNone<TDeath>()
            .ForEach((Entity e, int entityInQueryIndex, ref CSecondsToDeath secondsToDeath) =>
            {
                secondsToDeath.Value -= dt;
                if (secondsToDeath.Value <= 0.0f)
                    ecb.AddComponent<TDeath>(entityInQueryIndex, e);
            })
            .WithName("BleedOut")
            .ScheduleParallel();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}