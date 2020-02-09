using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class GetTargetSystem : JobComponentSystem
{
    [BurstCompile]
    public struct GetTargetJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        public ArchetypeChunkComponentType<Randomizer> RandomizerType;
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> Enemies;

        public float Aggression;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            if (Enemies.Length == 0)
                return;

            var randomizers = chunk.GetNativeArray(RandomizerType).Reinterpret<Random>();
            var entities = chunk.GetNativeArray(EntityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (randomizers[i].NextFloat(0f, 1f) >= Aggression)
                    continue;

                var target = Enemies[randomizers[i].NextInt(0, Enemies.Length)];
                CommandBuffer.AddComponent(chunkIndex, entities[i], new Target { Value = target });
                CommandBuffer.AddSharedComponent(chunkIndex, entities[i], new TargetType { Value = TargetTypes.Enemy });
            }
        }
    }

    private EntityQuery mFirstTeamQuery;
    private EntityQuery mSecondTeamQuery;
    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        var firstQuery = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(SecondsToDeath), typeof(Target) },
            All = new ComponentType[]
            {
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
        var baseJob = new GetTargetJob
        {
            Aggression = Settings.Instance.Aggression,
            RandomizerType = GetArchetypeChunkComponentType<Randomizer>(false),
            EntityType = GetArchetypeChunkEntityType(),
        };

        var firstTeamJob = baseJob;
        firstTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        firstTeamJob.Enemies = mSecondTeamQuery.ToEntityArray(Allocator.TempJob);
        var firstHandle = firstTeamJob.Schedule(mFirstTeamQuery, inputDeps);

        var secondTeamJob = baseJob;
        secondTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        secondTeamJob.Enemies = mFirstTeamQuery.ToEntityArray(Allocator.TempJob);
        var secondHandle = secondTeamJob.Schedule(mSecondTeamQuery, firstHandle);
        return secondHandle;
    }
}