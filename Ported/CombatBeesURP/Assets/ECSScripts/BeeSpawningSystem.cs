using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class BeeSpawningSystem : SystemBase
{
    private EntityArchetype mBeeArchetype;
    private BeginSimulationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        mBeeArchetype = EntityManager.CreateArchetype(typeof(Translation), typeof(Rotation),
                                                      typeof(Scale), typeof(LocalToWorld),
                                                      typeof(RenderMesh), typeof(TBeeTag),
                                                      typeof(CVelocity), typeof(STeam),
                                                      typeof(CRandomizer),
                                                      typeof(RenderBounds));
    }

    protected override void OnUpdate()
    {
        var buffer = mCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var archetype = mBeeArchetype;
        Entities
            .ForEach((Entity e, int entityInQueryIndex, ref CSpawnRequest request, ref CRandomizer randomizer) =>
            {
                var count = request.Count;
                var team = request.Team;
                var material = (team & 1) == 0 ? Settings.Instance.FirstTeamMaterial : Settings.Instance.SecondTeamMaterial;
                var spawnPos = UnityEngine.Vector3.right * (-Settings.Instance.FieldSize.x * .4f + Settings.Instance.FieldSize.x * .8f * team);
                for (int i = 0; i < count; i++)
                {
                    var bee = buffer.CreateEntity(entityInQueryIndex, archetype);
                    buffer.SetComponent(entityInQueryIndex, bee, new Translation { Value = spawnPos });
                    buffer.SetComponent(entityInQueryIndex, bee, new Rotation { Value = quaternion.identity });
                    buffer.SetComponent(entityInQueryIndex, bee, new Scale { Value = 1.0f });
                    buffer.SetComponent(entityInQueryIndex, bee, new LocalToWorld { Value = float4x4.identity });
                    buffer.SetComponent(entityInQueryIndex, bee, new CVelocity { Value = new float3(0.0f, 0.0f, 0.0f) });
                    buffer.SetComponent(entityInQueryIndex, bee, new CRandomizer { Value = new Random(math.asuint(randomizer.Value.NextInt(1, int.MaxValue))) });
                    buffer.SetComponent(entityInQueryIndex, bee, new RenderBounds { Value = new AABB { Center = new float3(0.0f), Extents = new float3(1.0f) } });
                    buffer.SetSharedComponent(entityInQueryIndex, bee, new RenderMesh { castShadows = UnityEngine.Rendering.ShadowCastingMode.On, layer = 0, material = material, mesh = Settings.Instance.BeeMesh, receiveShadows = true });
                    buffer.SetSharedComponent(entityInQueryIndex, bee, new STeam { Value = team });
                }
                buffer.DestroyEntity(entityInQueryIndex, e);
            })
            .WithoutBurst()
            .ScheduleParallel();
        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}