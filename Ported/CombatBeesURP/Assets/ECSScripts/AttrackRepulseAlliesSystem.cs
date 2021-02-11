#define NEW
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

public class AttrackRepulseAlliesSystem
#if NEW
    : SystemBase
#else
    : JobComponentSystem
#endif
{
#if NEW
    [BurstCompile]
    public struct AttrackRepulseAlliesJob : IJobChunk
    {
        public ComponentTypeHandle<CRandomizer> RandomizerType;
        public ComponentTypeHandle<CVelocity> VelocityType;

        [ReadOnly]
        public ComponentTypeHandle<Translation> TranslationType;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> Translations;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> Allies;

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float TeamAttraction;
        [ReadOnly] public float TeamRepulsion;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            if (Allies.Length == 0)
                return;

            var randomizers = chunk.GetNativeArray(RandomizerType).Reinterpret<Random>();
            var translations = chunk.GetNativeArray(TranslationType).Reinterpret<float3>();
            var velocities = chunk.GetNativeArray(VelocityType).Reinterpret<float3>();

            for (int i = 0; i < chunk.Count; i++)
            {
                var random = randomizers[i];
                var attractive = random.NextInt(0, Allies.Length);
                var repulsive = random.NextInt(0, Allies.Length);
                randomizers[i] = random;
                {
                    var delta = Translations[Allies[attractive]].Value - translations[i];
                    var distance = math.length(delta);
                    // max to avoid div by zero
                    // step(y, x) == x >= y ? 1 : 0
                    // Meaning that 1 - step(y, x) == x < y == y > x
                    velocities[i] += (1f - math.step(distance, 0f)) * delta * (TeamAttraction * DeltaTime / math.max(0.001f, distance));
                }

                {
                    var delta = Translations[Allies[repulsive]].Value - translations[i];
                    var distance = math.length(delta);
                    // max to avoid div by zero
                    velocities[i] -= (1f - math.step(distance, 0f)) * delta * (TeamAttraction * DeltaTime / math.max(0.001f, distance));
                }
            }
        }
    }

    private EntityQuery mFirstTeamQuery;
    private EntityQuery mSecondTeamQuery;

    protected override void OnCreate()
    {
        var firstQuery = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(CSecondsToDeath) },
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<CVelocity>(),
                ComponentType.ReadWrite<CRandomizer>(),
                ComponentType.ReadOnly<STeam>(),
                ComponentType.ReadOnly<TBeeTag>()
            }
        };

        var secondQuery = Utility.CopyEntityQueryDesc(firstQuery);

        mFirstTeamQuery = GetEntityQuery(firstQuery);
        mFirstTeamQuery.SetSharedComponentFilter(new STeam { Value = 0 });

        mSecondTeamQuery = GetEntityQuery(secondQuery);
        mSecondTeamQuery.SetSharedComponentFilter(new STeam { Value = 1 });
    }

    protected override void OnUpdate()
    {
        var baseJob = new AttrackRepulseAlliesJob
        {
            DeltaTime = Time.DeltaTime,
            Translations = GetComponentDataFromEntity<Translation>(true),
            TeamAttraction = Settings.Instance.TeamAttraction,
            TeamRepulsion = Settings.Instance.TeamRepulsion,
            TranslationType = GetComponentTypeHandle<Translation>(true),
            VelocityType = GetComponentTypeHandle<CVelocity>(false),
            RandomizerType = GetComponentTypeHandle<CRandomizer>(false),
        };

        var firstTeamJob = baseJob;
        firstTeamJob.Allies = mFirstTeamQuery.ToEntityArray(Allocator.TempJob);
        Dependency = firstTeamJob.ScheduleParallel(mFirstTeamQuery, Dependency);

        var secondTeamJob = baseJob;
        secondTeamJob.Allies = mSecondTeamQuery.ToEntityArray(Allocator.TempJob);
        Dependency = secondTeamJob.Schedule(mSecondTeamQuery, Dependency);
    }

#else
    [BurstCompile]
    public struct AttrackRepulseAlliesJob : IJobChunk
    {
        public ArchetypeChunkComponentType<Randomizer> RandomizerType;
        public ArchetypeChunkComponentType<Velocity> VelocityType;

        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> Translations;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> Allies;

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float TeamAttraction;
        [ReadOnly] public float TeamRepulsion;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            if (Allies.Length == 0)
                return;

            var randomizers = chunk.GetNativeArray(RandomizerType).Reinterpret<Random>();
            var translations = chunk.GetNativeArray(TranslationType).Reinterpret<float3>();
            var velocities = chunk.GetNativeArray(VelocityType).Reinterpret<float3>();

            for (int i = 0; i < chunk.Count; i++)
            {
                var random = randomizers[i];
                var attractive = random.NextInt(0, Allies.Length);
                var repulsive = random.NextInt(0, Allies.Length);
                randomizers[i] = random;
                {
                    var delta = Translations[Allies[attractive]].Value - translations[i];
                    var distance = math.length(delta);
                    // max to avoid div by zero
                    // step(y, x) == x >= y ? 1 : 0
                    // Meaning that 1 - step(y, x) == x < y == y > x
                    velocities[i] += (1f - math.step(distance, 0f)) * delta * (TeamAttraction * DeltaTime / math.max(0.001f, distance));
                }

                {
                    var delta = Translations[Allies[repulsive]].Value - translations[i];
                    var distance = math.length(delta);
                    // max to avoid div by zero
                    velocities[i] -= (1f - math.step(distance, 0f)) * delta * (TeamAttraction * DeltaTime / math.max(0.001f, distance));
                }
            }
        }
    }

    private EntityQuery mFirstTeamQuery;
    private EntityQuery mSecondTeamQuery;

    protected override void OnCreate()
    {
        var firstQuery = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(SecondsToDeath) },
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<Velocity>(),
                ComponentType.ReadWrite<Randomizer>(),
                ComponentType.ReadOnly<Team>(),
                ComponentType.ReadOnly<BeeTag>()
            }
        };

        var secondQuery = Utility.CopyEntityQueryDesc(firstQuery);

        mFirstTeamQuery = GetEntityQuery(firstQuery);
        mFirstTeamQuery.SetSharedComponentFilter(new Team { Value = 0 });

        mSecondTeamQuery = GetEntityQuery(secondQuery);
        mSecondTeamQuery.SetSharedComponentFilter(new Team { Value = 1 });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var baseJob = new AttrackRepulseAlliesJob
        {
            DeltaTime = Time.DeltaTime,
            Translations = GetComponentDataFromEntity<Translation>(true),
            TeamAttraction = Settings.Instance.TeamAttraction,
            TeamRepulsion = Settings.Instance.TeamRepulsion,
            TranslationType = GetArchetypeChunkComponentType<Translation>(true),
            VelocityType = GetArchetypeChunkComponentType<Velocity>(false),
            RandomizerType = GetArchetypeChunkComponentType<Randomizer>(false),
        };

        var firstTeamJob = baseJob;
        firstTeamJob.Allies = mFirstTeamQuery.ToEntityArray(Allocator.TempJob);
        var firstHandle = firstTeamJob.Schedule(mFirstTeamQuery, inputDeps);

        var secondTeamJob = baseJob;
        secondTeamJob.Allies = mSecondTeamQuery.ToEntityArray(Allocator.TempJob);
        var secondHandle = secondTeamJob.Schedule(mSecondTeamQuery, firstHandle);

        //return JobHandle.CombineDependencies(firstHandle, secondHandle);

        return secondHandle;
    }
#endif
}
