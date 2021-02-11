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
    public float Gravity = -20.0f;

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

    [Header("Resource Related")]
    public Mesh ResourceMesh;
    public Material ResourceMaterial;
    public float ResourceSize = 0.75f;
    public float SnapStiffness = 2.0f;
    public float CarryStiffness = 15.0f;
    public float SpawnRate = 10.0f;
    public int BeesPerResource = 8;
    public int StartResourceCount = 100;

    public GameObject YellowTeamGoPrefab;
    public GameObject PurpleTeamGoPrefab;
    public GameObject ResourceGoPrefab;

    [HideInInspector]
    public Entity YellowTeamPrefab;
    public Entity PurpleTeamPrefab;
    public Entity ResourcePrefab;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple settings in scene");

        if (!EcsEnabled || !SceneManager.GetActiveScene().name.Contains("ecs"))
            return;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        YellowTeamPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(YellowTeamGoPrefab, settings);
        PurpleTeamPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(PurpleTeamGoPrefab, settings);
        ResourcePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(ResourceGoPrefab, settings);


        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // Setup Teams
        {
            var firstTeam = buffer.CreateEntity();
            buffer.AddComponent(firstTeam, new CSpawnRequest { Count = BeeCount / 2, Prefab = YellowTeamPrefab });
            buffer.AddComponent(firstTeam, new Translation { Value = UnityEngine.Vector3.right * (-Settings.Instance.FieldSize.x * .4f + Settings.Instance.FieldSize.x * .8f * 0.0f) });

            var secondTeam = buffer.CreateEntity();
            buffer.AddComponent(secondTeam, new CSpawnRequest { Count = BeeCount / 2, Prefab = PurpleTeamPrefab });
            buffer.AddComponent(secondTeam, new Translation { Value = UnityEngine.Vector3.right * (-Settings.Instance.FieldSize.x * .4f + Settings.Instance.FieldSize.x * .8f * 1.0f) });
        }

        // Setup Grid
        {
            var grid = buffer.CreateEntity();
            var counts = (int2)math.round(FieldSize.xz / ResourceSize);
            buffer.AddComponent(grid, new CGridCellCount { Value = counts });
            var size = FieldSize.xz / counts;
            buffer.AddComponent(grid, new CGridCellSize { Value = size });
            var pos = ((float2)counts) * -.5f * size;
            buffer.AddComponent(grid, new CGridPos { Value = pos });
            var heights = buffer.AddBuffer<BStackHeights>(grid);
            for (int i = 0; i < counts.x * counts.y; i++)
                heights.Add(new BStackHeights { Value = 0 });
        }


        // Setup Resources
        {
            var resource = buffer.CreateEntity();
            buffer.AddComponent(resource, new CSpawnRequest { Count = StartResourceCount, Prefab = ResourcePrefab });
            buffer.AddComponent(resource, new Translation { Value = UnityEngine.Vector3.right * (-Settings.Instance.FieldSize.x * .4f + Settings.Instance.FieldSize.x * .8f * 0.5f) });
        }



        buffer.Playback(manager);
        buffer.Dispose();

    }
}
