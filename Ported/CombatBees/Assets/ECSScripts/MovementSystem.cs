#define NEW
using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem
#if NEW
    : SystemBase
#else
    : JobComponentSystem
#endif
{
#if NEW
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        Entities
            .ForEach((ref Translation pos, ref Velocity vel) =>
            {
                pos.Value += vel.Value * dt;
            })
            .WithName("Move")
            .ScheduleParallel();
    }
#else
    [BurstCompile]
    public struct MovementJob : IJobForEach<Translation, Velocity>
    {
        public float DeltaTime;

        public void Execute(ref Translation pos, ref Velocity vel)
        {
            pos.Value += vel.Value * DeltaTime;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MovementJob();
        job.DeltaTime = Time.DeltaTime;
        return job.Schedule(this, inputDeps);
    }
#endif

}