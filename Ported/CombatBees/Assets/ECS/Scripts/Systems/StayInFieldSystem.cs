using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class StayInFieldSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float3 ScaledFieldSize = Settings.Instance.FieldSize * 0.5f;
        Entities
            .ForEach((ref Translation pos, ref CVelocity vel) =>
            {
                if (math.abs(pos.Value.x) > ScaledFieldSize.x)
                {
                    pos.Value.x = (ScaledFieldSize.x) * math.sign(pos.Value.x);
                    vel.Value *= new float3(-0.5f, 0.8f, 0.8f);
                }

                if (math.abs(pos.Value.z) > ScaledFieldSize.z)
                {
                    pos.Value.z = (ScaledFieldSize.z) * math.sign(pos.Value.z);
                    vel.Value *= new float3(-0.5f, 0.8f, 0.8f);
                }

                // TODO: Deal with resources
                if (math.abs(pos.Value.y) > ScaledFieldSize.y)
                {
                    pos.Value.y = (ScaledFieldSize.y) * math.sign(pos.Value.y);
                    vel.Value *= new float3(-0.5f, 0.8f, 0.8f);
                }
            })
            .WithName("StayInField")
            .ScheduleParallel();
    }
}
