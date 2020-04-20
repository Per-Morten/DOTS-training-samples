using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class JitterAndDampingSystem : JobComponentSystem
{
    EntityQuery mQuery;

    protected override void OnCreate()
    {
        mQuery = GetEntityQuery(new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(SecondsToDeath) },
            All = new ComponentType[]
            {
                ComponentType.ReadWrite<Velocity>(),
                ComponentType.ReadWrite<Randomizer>(),
                ComponentType.ReadOnly<BeeTag>(),
            }
        });
    }

    [BurstCompile]
    public struct JitterAndDampingJob : IJobChunk
    {
        public ArchetypeChunkComponentType<Velocity> VelocityType;
        public ArchetypeChunkComponentType<Randomizer> RandomizerType;

        public float DeltaTime;
        public float Damping;
        public float FlightJitter;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var velocities = chunk.GetNativeArray(VelocityType).Reinterpret<float3>();
            var randoms = chunk.GetNativeArray(RandomizerType).Reinterpret<Random>();
            for (int i = 0; i < chunk.Count; i++)
            {
                var random = randoms[i];
                velocities[i] += random.NextFloat3Direction() * FlightJitter * DeltaTime;
                velocities[i] *= (1f - Damping);
                randoms[i] = random;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new JitterAndDampingJob();
        job.DeltaTime = Time.DeltaTime;
        job.FlightJitter = Settings.Instance.FlightJitter;
        job.Damping = Settings.Instance.Damping;
        job.VelocityType = GetArchetypeChunkComponentType<Velocity>(false);
        job.RandomizerType = GetArchetypeChunkComponentType<Randomizer>(false);
        return job.Schedule(mQuery, inputDeps);
    }
}
