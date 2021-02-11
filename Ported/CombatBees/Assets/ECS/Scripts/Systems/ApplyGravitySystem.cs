using Unity.Entities;
using Unity.Mathematics;

public class ApplyGravitySystem : SystemBase
{
    protected override void OnUpdate()
    {
        var gravity = Settings.Instance.Gravity;
        var dt = Time.DeltaTime;
        Entities 
            .WithAny<CSecondsToDeath, TResource>()
            .WithNone<CStackHeight, CHeldBy>()
            .ForEach((ref CVelocity vel) =>
            {
                vel.Value += -math.up() * gravity * dt;
            })
            .ScheduleParallel();
    }
}
