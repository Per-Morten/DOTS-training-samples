using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(DebugSystemGroup))]
public class RenderGridDebugSystem : SystemBase
{
    protected override void OnCreate()
    {
        Enabled = false;
    }

    protected override void OnUpdate()
    {
        Entities
            .ForEach((in CGridPos pos, in CGridCellCount count, in CGridCellSize size) =>
            {
                var floor = -(Settings.Instance.FieldSize / 2.0f).y;
                DBG.Point(new float3(pos.Value, floor).xzy, Color.green);
                for (int x = 0; x < count.Value.x; x++)
                {
                    for (int y = 0; y < count.Value.y; y++)
                    {
                        var min = pos.Value + size.Value * new float2(x, y);
                        var max = pos.Value + size.Value * (new float2(x, y) + 1.0f);
                        DBG.RectXZ(min, max, floor, Color.red);
                    }
                }
            })
            .WithoutBurst()
            .Run();
    }
}
