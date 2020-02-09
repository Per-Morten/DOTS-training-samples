using Unity.Mathematics;
using Unity.Entities;

public struct Velocity : IComponentData
{
    public float3 Value;
}

public struct Randomizer : IComponentData
{
    public Random Value;
}

public struct Direction : IComponentData
{
    public float3 Value;
}

public struct SecondsToDeath : IComponentData
{
    public float Value;
}

public struct Target : IComponentData
{
    public Entity Value;
}

public struct Team : ISharedComponentData
{
    public int Value;
}

public struct SpawnRequest : IComponentData
{
    public int Count;
}

public enum TargetTypes
{
    Resource,
    Enemy
}

public struct TargetType : ISharedComponentData
{
    public TargetTypes Value;
}

public struct BeeTag : IComponentData
{

}

public struct DeathTag : IComponentData
{ 

}
