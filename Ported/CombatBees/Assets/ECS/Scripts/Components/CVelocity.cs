using Unity.Mathematics;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CVelocity : IComponentData
{
    public float3 Value;
}
