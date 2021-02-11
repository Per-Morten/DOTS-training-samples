using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CGridIndex : IComponentData
{
    public int2 Value;
}
