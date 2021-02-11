using Unity.Mathematics;
using Unity.Entities;

public struct CVelocity : IComponentData
{
    public float3 Value;
}

public struct CRandomizer : IComponentData
{
    public Random Value;
}

public struct CDirection : IComponentData
{
    public float3 Value;
}

public struct CSecondsToDeath : IComponentData
{
    public float Value;
}

public struct CTarget : IComponentData
{
    public Entity Value;
}

public struct STeam : ISharedComponentData
{
    public int Value;
}

public struct CSpawnRequest : IComponentData
{
    public int Count;
    public int Team;
}

public enum TargetTypes
{
    Resource,
    Enemy
}

public struct STargetType : ISharedComponentData
{
    public TargetTypes Value;
}

public struct TBeeTag : IComponentData
{

}

public struct TDeathTag : IComponentData
{ 

}
