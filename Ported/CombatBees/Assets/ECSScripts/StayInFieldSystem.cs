using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class StayInFieldSystem : JobComponentSystem
{
    [BurstCompile][RequireComponentTag(typeof(BeeTag))]
    public struct StayInFieldJob : IJobForEach<Translation, Velocity>
    {
        [ReadOnly] public float3 ScaledFieldSize;

        public void Execute(ref Translation pos, ref Velocity vel)
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
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new StayInFieldJob { ScaledFieldSize = Settings.Instance.FieldSize * .5f }.Schedule(this, inputDeps);
    }
}
