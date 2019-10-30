﻿using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

// JobComponentSystems can run on worker threads.
// However, creating and removing Entities can only be done on the main thread to prevent race conditions.
// The system uses an EntityCommandBuffer to defer tasks that can't be done inside the Job.

// ReSharper disable once InconsistentNaming
//[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SpawnerSystem : JobComponentSystem
{
    // BeginInitializationEntityCommandBufferSystem is used to create a command buffer which will then be played back
    // when that barrier system executes.
    // Though the instantiation command is recorded in the SpawnJob, it's not actually processed (or "played back")
    // until the corresponding EntityCommandBufferSystem is updated. To ensure that the transform system has a chance
    // to run on the newly-spawned entities before they're rendered for the first time, the SpawnerSystem_FromEntity
    // will use the BeginSimulationEntityCommandBufferSystem to play back its commands. This introduces a one-frame lag
    // between recording the commands and instantiating the entities, but in practice this is usually not noticeable.
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    private EntityQuery query;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        EntityQueryDesc queryDescription = new EntityQueryDesc();
        queryDescription.All = new[] {ComponentType.ReadOnly<GridTile>()};
        query = GetEntityQuery(queryDescription);
    }

    struct SpawnJob : IJobForEachWithEntity_EBCCC<GridTile, ResourcesComponent, FarmerDataComponent, GridComponent>
    { 
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, DynamicBuffer<GridTile> gridTileBuffer,
            ref ResourcesComponent resourcesComponent, [ReadOnly] ref FarmerDataComponent farmerDataComponent,
            [ReadOnly] ref GridComponent gridComponent)
        {
            var entityIndex = 0;
            var gridIndex = 0;

            while (resourcesComponent.MoneyForFarmers >= 10)
            {
                for (int i = gridIndex; i < gridComponent.Size * gridComponent.Size; i++)
                {
                    if (gridTileBuffer[i].IsShop())
                    {
                        var instance = CommandBuffer.Instantiate(entityIndex, farmerDataComponent.farmerEntity);
                        var x = i % gridComponent.Size;
                        var y = i / gridComponent.Size;
                        
                        // Place the instantiated in a grid with some noise
                        var position = new float3(x,0,y);
                        CommandBuffer.SetComponent(index, instance, new Translation {Value = position});
                        CommandBuffer.SetComponent(index, instance, new PositionComponent() {position = new Vector2(x,y)});
                        CommandBuffer.SetComponent(index, instance, new GoalPositionComponent() {position = new Vector2(20,20)});
                        resourcesComponent.MoneyForFarmers -= 10;
                        entityIndex++;
                        gridIndex = i;
                        CommandBuffer.DestroyEntity(index, entity);
                        break;
                    }
                }

                if (gridIndex == (gridComponent.Size * gridComponent.Size) - 1)
                {
                    gridIndex = 0;
                }
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //Instead of performing structural changes directly, a Job can add a command to an EntityCommandBuffer to perform such changes on the main thread after the Job has finished.
        //Command buffers allow you to perform any, potentially costly, calculations on a worker thread, while queuing up the actual insertions and deletions for later.

        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        var job = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);


        // SpawnJob runs in parallel with no sync point until the barrier system executes.
        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
