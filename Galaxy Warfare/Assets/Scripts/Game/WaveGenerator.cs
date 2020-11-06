using UnityEngine;

[ExecuteAlways]
public class WaveGenerator : MonoBehaviour
{
    [Header("Spawning Area")]
    [SerializeField] private PlayerShipController playerShip = null;
    [SerializeField] private float distanceFromPlayer = 100f;
    [SerializeField] private Rect rectArea = new Rect(Vector2.zero, new Vector2(16, 9) * 100);

    [Header("Ships")] [Space]
    [SerializeField] private GameObject[] shipPrefabs = null;
    [SerializeField] private RangeInt shipCountRange = new RangeInt(3, 7);
    private Vector3[] spawnPoints;

    // The WAVE
    private Vector3 A, B, C, D;
    private Transform currentWavePlane = null;
    private int currentWaveCount = 1;
    private AIShipController[] spawnedShips = null;

    private void Start()
    {
    }

    private void NewWave()
    {
        ResetSpawns();
        SpawnShips();
    }

    private void Update()
    {
        if (currentWavePlane == null)
        {
            string planeName = $"Wave #{currentWaveCount}";
            GameObject obj = GameObject.Find(planeName);
            if (obj != null)
            {
                currentWavePlane = obj.transform;
                currentWavePlane.parent = transform;
                currentWavePlane.SetAsLastSibling();
            }
            else
            {
                currentWavePlane = Instantiate(new GameObject(planeName), transform).transform;
            }
        }

        // A     B
        // 
        // D     C
        A = playerShip.transform.TransformPoint(rectArea.position) + playerShip.transform.forward * distanceFromPlayer;
        B = A + playerShip.transform.right * rectArea.width;
        C = B + -playerShip.transform.up * rectArea.height;
        D = C + -playerShip.transform.right * rectArea.width;

        currentWavePlane.position = A + (C - A).normalized * Vector3.Distance(A, C) / 2;
        currentWavePlane.LookAt(playerShip.transform);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NewWave();
        }
    }

#if UNITY_EDITOR
    [Header("Editor")] [Space]
    [SerializeField] private bool generateSpawnPoints = false;
    [SerializeField] private bool generateShipObjects = false;
    [SerializeField] private bool reset = false;
    private void LateUpdate()
    {
        if(generateSpawnPoints == true)
        {
            generateSpawnPoints = false;
            GenerateSpawnPoints();
        }
        if(generateShipObjects == true)
        {
            generateShipObjects = false;
            SpawnShips();
        }
        if(reset == true)
        {
            reset = false;
            ResetSpawns();
        }
    }
#endif

    private void GenerateSpawnPoints()
    {
        DestroySpawnedShips();

        spawnPoints = new Vector3[Random.Range(shipCountRange.min, shipCountRange.max + 1)];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            bool safe = true;
            float currDist = 4f;
            do
            {
                currDist -= 0.2f;
                spawnPoints[i] = new Vector3(Random.Range(-rectArea.width / 2, rectArea.width / 2), Random.Range(-rectArea.height / 2, rectArea.height / 2));
                if (currDist >= 0.4f)
                {
                    foreach (Vector3 otherSpawnPoint in spawnPoints)
                    {
                        if (otherSpawnPoint != spawnPoints[i] && Vector3.Distance(otherSpawnPoint, spawnPoints[i]) < currDist)
                        {
                            safe = false;
                            break;
                        }
                    }
                }
                else
                {
                    safe = true;
                }
            } while (!safe);
        }
    }

    private void DestroySpawnedShips()
    {
        if (spawnedShips != null && spawnedShips.Length > 0)
        {
            foreach (AIShipController ship in spawnedShips)
            {
                if (ship != null)
                {
                    if (Application.isPlaying)
                        Destroy(ship.gameObject);
                    else
                        DestroyImmediate(ship.gameObject);
                }
            }
        }
    }

    private void ResetSpawns()
    {
        foreach (Transform transf in currentWavePlane.GetComponentsInChildren<Transform>())
        {
            if (transf != currentWavePlane.transform && transf != null)
            {
                DestroyImmediate(transf.gameObject);
            }
        }
        spawnPoints = null;
        spawnedShips = null;
    }

    private void SpawnShips()
    {
        if(shipPrefabs.Length > 0)
        {
            if (spawnPoints == null || spawnPoints.Length == 0) GenerateSpawnPoints();

            DestroySpawnedShips();

            spawnedShips = new AIShipController[spawnPoints.Length];
            for(int i = 0; i < spawnedShips.Length; i++)
            {
                int randomShipIndex = Random.Range(0, shipPrefabs.Length);
                Vector3 pos = currentWavePlane.TransformPoint(spawnPoints[i]) + currentWavePlane.forward * distanceFromPlayer * 1.25f;
                spawnedShips[i] = Instantiate(shipPrefabs[randomShipIndex], pos, playerShip.transform.rotation, transform).GetComponent<AIShipController>();
                if (spawnedShips[i] == null)
                {
                    throw new System.Exception($"Milord, the prefab you're trying to spawn ({shipPrefabs[randomShipIndex].name}) does not have a {nameof(AIShipController)} component!");
                }
                else
                {
                    spawnedShips[i].name = $"AlienShip_{i}";
                    AIPathFollower shipPathFollower = spawnedShips[i].GetComponent<AIPathFollower>();
                    if(shipPathFollower != null)
                    {
                        shipPathFollower.followingPath = true;
                        shipPathFollower.playerShip = playerShip;
                        shipPathFollower.waveTransform = currentWavePlane;
                        shipPathFollower.waypoints = new Transform[2];
                        for(int j = 0; j < shipPathFollower.waypoints.Length; j++)
                        {
                            shipPathFollower.waypoints[j] = Instantiate(new GameObject($"{spawnedShips[i].name}_waypoint_{j}"), currentWavePlane).transform;
                        }
                        shipPathFollower.waypoints[0].position = currentWavePlane.TransformPoint(spawnPoints[i]) + 
                            -currentWavePlane.transform.forward * Random.Range(1f, 20f) + // Z
                            currentWavePlane.transform.right * Random.Range(-5f, 5f) + // X
                            currentWavePlane.transform.up * Random.Range(-5f, 5f); // Y
                        shipPathFollower.waypoints[1].position = currentWavePlane.TransformPoint(spawnPoints[i]);
                        shipPathFollower.stopOnIndex = 1;
                    }
                }
            }
        }
        else
        {
            throw new System.Exception($"Milord, but there ain't no prefabs set in {nameof(shipPrefabs)} as of yet!");
        }
    }

    private void DrawRect(bool selected)
    {
        if (playerShip != null)
        {
            float alpha = selected ? 1 : 0.35f;
            Gizmos.color = new Color(0, 1, 1, alpha);
            Gizmos.DrawLine(A, B);
            Gizmos.DrawLine(B, C);
            Gizmos.DrawLine(C, D);
            Gizmos.DrawLine(D, A);

            if(spawnPoints != null)
            {
                for(int i = 0; i < spawnPoints.Length; i++)
                {
                    if(spawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(currentWavePlane.TransformPoint(spawnPoints[i]), 2f);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawRect(false);
    }
    private void OnDrawGizmosSelected()
    {
        DrawRect(true);
    }
}
