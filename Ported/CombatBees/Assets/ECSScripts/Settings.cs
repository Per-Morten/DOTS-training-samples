using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Rendering;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }

    public bool EcsEnabled = true;

    [Header("Field Related")]
    public float3 FieldSize = new float3(100f, 20f, 30f);
    public float Gravity;

    [Header("Bee Related")]
    public float FlightJitter;

    [Range(0f, 1f)]
    public float Damping;
    public Mesh BeeMesh;
    public Material FirstTeamMaterial;
    public Material SecondTeamMaterial;
    public int BeeCount;

    [Range(0f, 1f)]
    public float Aggression;

    public float TeamAttraction;
    public float TeamRepulsion;

    public float ChaseForce;
    public float AttackForce;
    public float AttackDistance;
    public float HitDistance;
    public float SecondsToDeathStartValue;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple settings in scene");

        if (!EcsEnabled || !SceneManager.GetActiveScene().name.Contains("ecs"))
            return;

        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var random = new System.Random(2);

        var firstTeamRequest = buffer.CreateEntity();
        buffer.AddComponent(firstTeamRequest, new Randomizer { Value = new Unity.Mathematics.Random(Unity.Mathematics.math.asuint(random.Next(1, int.MaxValue))) });
        buffer.AddComponent(firstTeamRequest, new SpawnRequest { Count = BeeCount / 2 });
        buffer.AddSharedComponent(firstTeamRequest, new Team { Value = 0 });

        var secondTeamRequest = buffer.CreateEntity();
        buffer.AddComponent(secondTeamRequest, new Randomizer { Value = new Unity.Mathematics.Random(Unity.Mathematics.math.asuint(random.Next(1, int.MaxValue))) });
        buffer.AddComponent(secondTeamRequest, new SpawnRequest { Count = BeeCount / 2 });
        buffer.AddSharedComponent(secondTeamRequest, new Team { Value = 1 });

        buffer.Playback(manager);
        buffer.Dispose();

    }
}
