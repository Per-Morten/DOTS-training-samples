using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem : JobComponentSystem
{
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
}