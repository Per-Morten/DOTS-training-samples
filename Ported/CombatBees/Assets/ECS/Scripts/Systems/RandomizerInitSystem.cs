using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RandomizerInitSystem : SystemBase
{
    private EndInitializationEntityCommandBufferSystem mCommandBufferSystem;

    protected override void OnCreate()
    {
        mCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var buffer = mCommandBufferSystem.CreateCommandBuffer();
        Entities
            .WithAll<TInitRandomizer>()
            .ForEach((Entity e) =>
            {
                var randomizer = new CRandomizer { Value = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue)) };
                buffer.AddComponent(e, randomizer);
                buffer.RemoveComponent<TInitRandomizer>(e);
            })
            .Run();

        mCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
