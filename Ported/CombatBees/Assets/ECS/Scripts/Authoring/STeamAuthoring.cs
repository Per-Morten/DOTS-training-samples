using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class STeamAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [System.Serializable]
    public enum AuthoringTeam
    {
        Yellow = 0,
        Purple = 1,
    }

    public AuthoringTeam Team;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new STeam { Value = (int)Team });
        
    }
}
