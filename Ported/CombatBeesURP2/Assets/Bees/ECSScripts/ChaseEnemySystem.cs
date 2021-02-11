using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ChaseEnemySystem
    : SystemBase
{
    [BurstCompile]
    struct ChaseEnemySystemJob : IJobChunk
    {
        [ReadOnly]
        public ComponentDataFromEntity<Translation> AllTranslations;

        [ReadOnly]
        public ComponentDataFromEntity<CSecondsToDeath> AllSecondsToDeath;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;

        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;
        public ArchetypeChunkComponentType<CVelocity> VelocityType;
        public ArchetypeChunkComponentType<CTarget> TargetType;

        public float DeltaTime;
        public float ChaseForce;
        public float AttackForce;
        public float AttackDistance;
        public float HitDistance;

        public float SecondsToDeathStartValue;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var enemies = chunk.GetNativeArray(TargetType).Reinterpret<Entity>();
            var velocities = chunk.GetNativeArray(VelocityType).Reinterpret<float3>();
            var translations = chunk.GetNativeArray(TranslationType).Reinterpret<float3>();
            var entities = chunk.GetNativeArray(EntityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Debug.Assert(AllTranslations.Exists(enemies[i]));

                if (AllSecondsToDeath.Exists(enemies[i]))
                {
                    CommandBuffer.RemoveComponent<CTarget>(chunkIndex, entities[i]);
                    continue;
                }

                var enemyPos = AllTranslations[enemies[i]].Value;
                var delta = enemyPos - translations[i];

                float force = ChaseForce;
                var deltaLengthSq = math.lengthsq(delta);
                if (deltaLengthSq <= AttackDistance * AttackDistance)
                {
                    force = AttackForce;
                    if (deltaLengthSq <= HitDistance * HitDistance)
                    {
                        CommandBuffer.RemoveComponent<CTarget>(chunkIndex, entities[i]);
                        CommandBuffer.AddComponent(chunkIndex, enemies[i], new CSecondsToDeath { Value = SecondsToDeathStartValue });
                    }
                }

                velocities[i] += delta * (force * DeltaTime / math.length(delta));
            }
        }
    }

    private EntityQuery mEnemyQuery;
    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        var enemyQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Translation>(), ComponentType.ReadWrite<CVelocity>(), ComponentType.ReadWrite<CTarget>() },
            None = new ComponentType[] { typeof(CSecondsToDeath) }
        };

        mEnemyQuery = GetEntityQuery(enemyQuery);

        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Dependency = new ChaseEnemySystemJob
        {
            AllTranslations = GetComponentDataFromEntity<Translation>(true),
            AllSecondsToDeath = GetComponentDataFromEntity<CSecondsToDeath>(true),
            CommandBuffer = mCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            EntityType = GetArchetypeChunkEntityType(),
            TranslationType = GetArchetypeChunkComponentType<Translation>(true),
            VelocityType = GetArchetypeChunkComponentType<CVelocity>(false),
            TargetType = GetArchetypeChunkComponentType<CTarget>(false),
            DeltaTime = Time.DeltaTime,
            ChaseForce = Settings.Instance.ChaseForce,
            AttackForce = Settings.Instance.AttackForce,
            AttackDistance = Settings.Instance.AttackDistance,
            HitDistance = Settings.Instance.HitDistance,
            SecondsToDeathStartValue = Settings.Instance.SecondsToDeathStartValue,
        }
        .ScheduleParallel(mEnemyQuery, Dependency);
    }
}