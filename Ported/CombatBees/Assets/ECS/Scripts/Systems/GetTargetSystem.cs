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
        public float Aggression;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> Enemies;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> Resources;

        [ReadOnly] 
        public ComponentDataFromEntity<CStackHeight> ResourceStackHeight;
        [ReadOnly] 
        public ComponentDataFromEntity<CGridIndex> ResourceGridIndex;

        [ReadOnly]
        public Entity GridEntity;

        [ReadOnly]
        public ComponentDataFromEntity<CGridCellCount> GridCellCount;
        [ReadOnly]
        public BufferFromEntity<BStackHeights> GridStackHeights;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var randomizers = chunk.GetNativeArray(RandomizerType).Reinterpret<Random>();
            var entities = chunk.GetNativeArray(EntityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (randomizers[i].NextFloat(0f, 1f) < Aggression && Enemies.Length > 0)
                {
                    var target = Enemies[randomizers[i].NextInt(0, Enemies.Length)];
                    CommandBuffer.AddComponent(chunkIndex, entities[i], new CTarget { Value = target });
                    CommandBuffer.AddComponent(chunkIndex, entities[i], new TEnemyTarget { });
                }
                else if (Resources.Length > 0)
                {
                    var target = Resources[randomizers[i].NextInt(0, Resources.Length)];
                    var targetIdx2D = ResourceGridIndex[target].Value;
                    var targetIdx = targetIdx2D.y * GridCellCount[GridEntity].Value.x + targetIdx2D.x;
                    if (ResourceStackHeight[target].Value == GridStackHeights[GridEntity][targetIdx].Value - 1)
                    {
                        CommandBuffer.AddComponent(chunkIndex, entities[i], new CTarget { Value = target });
                        CommandBuffer.AddComponent(chunkIndex, entities[i], new TResourceTarget { });
                    }
                }
            }
        }
    }

    private EntityQuery mFirstTeamQuery;
    private EntityQuery mSecondTeamQuery;
    private EntityQuery mFirstTeamAsEnemyQuery;
    private EntityQuery mSecondTeamAsEnemyQuery;
    private EntityQuery mResourceQuery;
    private EntityQuery mGridQuery;
    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        // TODO: We need two different queries, one which just goes after bees (no matter whether or not they have targets)
        //       And the one that we currently have.
        var asActorQuery = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(CSecondsToDeath), typeof(CTarget) },
            All = new ComponentType[]
            {
                ComponentType.ReadWrite<CRandomizer>(),
                ComponentType.ReadOnly<STeam>(),
                ComponentType.ReadOnly<TBee>()
            }
        };
        
        mSecondTeamQuery = GetEntityQuery(Utility.CopyEntityQueryDesc(asActorQuery));
        mSecondTeamQuery.SetSharedComponentFilter(new STeam { Value = 1 });

        mFirstTeamQuery = GetEntityQuery(asActorQuery);
        mFirstTeamQuery.SetSharedComponentFilter(new STeam { Value = 0 });


        var asEnemyQuery = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(CSecondsToDeath) },
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<STeam>(),
                ComponentType.ReadOnly<TBee>()
            }
        };

        mSecondTeamAsEnemyQuery = GetEntityQuery(Utility.CopyEntityQueryDesc(asEnemyQuery));
        mSecondTeamAsEnemyQuery.SetSharedComponentFilter(new STeam { Value = 1 });

        mFirstTeamAsEnemyQuery = GetEntityQuery(asEnemyQuery);
        mFirstTeamAsEnemyQuery.SetSharedComponentFilter(new STeam { Value = 0 });

        mResourceQuery = GetEntityQuery(ComponentType.ReadOnly<TResource>(), ComponentType.ReadOnly<CGridIndex>(), ComponentType.ReadOnly<CStackHeight>());

        mGridQuery = GetEntityQuery(ComponentType.ReadOnly<CGridCellCount>());
    }

    protected override void OnUpdate()
    {
        Entity grid;
        using (var tmp = mGridQuery.ToEntityArray(Allocator.TempJob))
            grid = tmp.Length > 0 ? tmp[0] : Entity.Null;
        
        var baseJob = new GetTargetJob
        {
            Aggression = Settings.Instance.Aggression,
            RandomizerType = GetComponentTypeHandle<CRandomizer>(false),
            EntityType = GetEntityTypeHandle(),
            ResourceGridIndex = GetComponentDataFromEntity<CGridIndex>(true),
            ResourceStackHeight = GetComponentDataFromEntity<CStackHeight>(true),
            GridEntity = grid,
            GridCellCount = GetComponentDataFromEntity<CGridCellCount>(true),
            GridStackHeights = GetBufferFromEntity<BStackHeights>(true),
        };
        
        var firstTeamJob = baseJob;
        firstTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        firstTeamJob.Enemies = mSecondTeamAsEnemyQuery.ToEntityArray(Allocator.TempJob);
        firstTeamJob.Resources = mResourceQuery.ToEntityArray(Allocator.TempJob);
        Dependency = firstTeamJob.ScheduleParallel(mFirstTeamQuery, Dependency);

        var secondTeamJob = baseJob;
        secondTeamJob.CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        secondTeamJob.Enemies = mFirstTeamAsEnemyQuery.ToEntityArray(Allocator.TempJob);
        secondTeamJob.Resources = mResourceQuery.ToEntityArray(Allocator.TempJob);
        Dependency = secondTeamJob.ScheduleParallel(mSecondTeamQuery, Dependency);
        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}