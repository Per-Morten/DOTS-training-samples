using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class JitterAndDampingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var flightJitter = Settings.Instance.FlightJitter;
        var deltaTime = Time.DeltaTime;
        var damping = Settings.Instance.Damping;

        Entities
            .WithAll<TBeeTag>()
            .ForEach((ref CVelocity vel, ref CRandomizer rand) =>
            {
                vel.Value += rand.Value.NextFloat3Direction() * flightJitter * deltaTime;
                vel.Value *= (1f - damping);
            })
            .WithName("JittarAndDamping")
            .WithBurst()
            .ScheduleParallel();
    }
}
