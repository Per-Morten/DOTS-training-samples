using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct CSpawnRequest : IComponentData
{
    public int Count;
    public Entity Prefab;
}
