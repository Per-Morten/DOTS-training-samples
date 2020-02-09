using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class BeeSpawningSystem : ComponentSystem
{
    private EntityArchetype mBeeArchetype;

    protected override void OnCreate()
    {
        mBeeArchetype = EntityManager.CreateArchetype(typeof(Translation), typeof(Rotation),
                                                      typeof(Scale), typeof(LocalToWorld),
                                                      typeof(RenderMesh), typeof(BeeTag),
                                                      typeof(Velocity), typeof(Team),
                                                      typeof(Randomizer));
    }

    protected override void OnUpdate()
    {
        var buffer = PostUpdateCommands;
        Entities.ForEach((Entity e, ref SpawnRequest request, ref Randomizer randomizer) =>
        {
            var team = EntityManager.GetSharedComponentData<Team>(e);
            var material = (team.Value & 1) == 0 ? Settings.Instance.FirstTeamMaterial : Settings.Instance.SecondTeamMaterial;
            var spawnPos = UnityEngine.Vector3.right * (-Settings.Instance.FieldSize.x * .4f + Settings.Instance.FieldSize.x * .8f * team.Value);
            for (int i = 0; i < request.Count; i++)
            {
                var bee = buffer.CreateEntity(mBeeArchetype);
                buffer.SetComponent(bee, new Translation { Value = spawnPos });
                buffer.SetComponent(bee, new Rotation { Value = quaternion.identity });
                buffer.SetComponent(bee, new Scale { Value = 1.0f });
                buffer.SetComponent(bee, new LocalToWorld { Value = float4x4.identity });
                buffer.SetComponent(bee, new Velocity { Value = new float3(0.0f, 0.0f, 0.0f) });
                buffer.SetComponent(bee, new Randomizer { Value = new Random(math.asuint(randomizer.Value.NextInt(1, int.MaxValue))) });
                buffer.SetSharedComponent(bee, new RenderMesh { castShadows = UnityEngine.Rendering.ShadowCastingMode.On, layer = 0, material = material, mesh = Settings.Instance.BeeMesh, receiveShadows = true });
                buffer.SetSharedComponent(bee, team);
            }
            PostUpdateCommands.DestroyEntity(e);
        });
    }
}