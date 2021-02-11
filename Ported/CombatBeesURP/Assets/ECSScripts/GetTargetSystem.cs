using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class GetTargetSystem : SystemBase
{
    [BurstCompile]
    public struct GetTargetJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle EntityType;
        public ComponentTypeHandle<CRandomizer> RandomizerType;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

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
                CommandBuffer.AddComponent(chunkIndex, entities[i], new CTarget { Value = target });
                CommandBuffer.AddSharedComponent(chunkIndex, entities[i], new STargetType { Value = TargetTypes.Enemy });
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
            None = new ComponentType[] { typeof(CSecondsToDeath), typeof(CTarget) },
            All = new ComponentType[]
            {
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
        var baseJob = new GetTargetJob
        {
            Aggression = Settings.Instance.Aggression,
            RandomizerType = GetComponentTypeHandle<CRandomizer>(false),
            EntityType = GetEntityTypeHandle(),
        };

        var firstTeamJob = baseJob;
        firstTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        firstTeamJob.Enemies = mSecondTeamQuery.ToEntityArray(Allocator.TempJob);
        Dependency = firstTeamJob.ScheduleParallel(mFirstTeamQuery, Dependency);

        var secondTeamJob = baseJob;
        secondTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        secondTeamJob.Enemies = mFirstTeamQuery.ToEntityArray(Allocator.TempJob);
        Dependency = secondTeamJob.ScheduleParallel(mSecondTeamQuery, Dependency);
    }
}