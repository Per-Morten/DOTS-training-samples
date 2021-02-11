using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        Entities
            .ForEach((ref Translation pos, ref CVelocity vel) =>
            {
                pos.Value += vel.Value * dt;
            })
            .WithName("Movement")
            .ScheduleParallel();
    }
}