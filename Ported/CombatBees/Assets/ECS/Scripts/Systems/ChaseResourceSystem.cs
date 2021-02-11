using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Debug = UnityEngine.Debug;

public class ChaseResourceSystem : SystemBase
{
    private struct ChaseResourceJob : IJobChunk
    {
        public float DeltaTime;
        public float ChaseForce;

        public EntityCommandBuffer CommandBuffer;

        [ReadOnly]
        public ComponentTypeHandle<Translation> ChunkTranslationType;
        public ComponentTypeHandle<CVelocity> ChunkVelocityType;
        public ComponentTypeHandle<CTarget> ChunkTargetType;

        public Entity Grid;

        [ReadOnly]
        public BufferFromEntity<BStackHeights> GridStackHeights;

        [ReadOnly]
        public ComponentDataFromEntity<CGridCellCount> GridCellCount;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> ResourceTranslations;



        // Retargeting logic.
        // Possible take:
        // Create HashMap of all entities in other team
        // If Has Owner && Owner is in HashMap of other team -> Retarget

        // Actually a good question for Dale, what's a good way to do this?
        // Write about it tomorrow night.
        [ReadOnly]
        public ComponentDataFromEntity<CHeldBy> ResourceHolders;

        [ReadOnly]
        public ComponentDataFromEntity<CStackHeight> ResourceStackHeight;

        [ReadOnly]
        public NativeHashMap<Entity, EmptyStruct> OtherTeam;

        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {

        }
    }

    private BeginInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {

    }

    protected override void OnUpdate()
    {
        return;
        //var buffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        //Entities
        //    .WithAll<TResourceTarget>()
        //    .ForEach((Entity e, int entityInQueryIndex, ref CVelocity vel, ref CTarget target, in Translation pos) =>
        //    {
                
        //    })
        //    .ScheduleParallel();

        //mCommandBufferSystem.AddJobHandleForProducer(Dependency);
            
    }
}