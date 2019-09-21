using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool randomPipes = false;
    public int numberOfPipes = 10;
    public List<PacketAsset> packetAssets;

    public float gameDuration = 180f;
    private float elapsedTime = 0f;
    public int score = 0;

    public float ticksPerSecond = 2f;
    public float packetsPerSecond = 0.5f;

    public int packetDeliveryPoints = 10;
    public int wrongPacketDeliveryPenalty = 10;
    public int packetCollisionPenalty = 10;

    public Tilemap boundsTilemap;

    public GameObject pipePrefab;
    public GameObject positionPipesPrefab;

    public List<Pipe> pipes;

    public List<Packet> packetPrefabs;
    public List<Packet> spawnedPackets = new List<Packet>();

    public Text scoreText;
    public Text timeText;
    public AudioManager audioManager;

    private Coroutine tickCoroutine;
    private Coroutine packetSpawnCoroutine;
    

    public delegate void OnTickHandler();
    public event OnTickHandler tickElapsed = delegate { };

    // Start is called before the first frame update
    void Start()
    {
        if (randomPipes)
        {
            pipes = new List<Pipe>();
            GeneratePipes();
        }
        else
        {
            GameObject pipesParent = Instantiate(positionPipesPrefab, Vector3.zero, Quaternion.identity);
            pipes = new List<Pipe>(pipesParent.GetComponentsInChildren<Pipe>());
        }

        UpdateScoreText();
        tickCoroutine = StartCoroutine(TickCoroutine());
        packetSpawnCoroutine = StartCoroutine(PacketSpawnCorouting());
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsedTime >= gameDuration)
        {
            
            // End game
            Debug.LogWarning("Game over");
            StopCoroutine(tickCoroutine);
            StopCoroutine(packetSpawnCoroutine);
        }else
        {
            elapsedTime += Time.deltaTime;
            UpdateTimeText();
        }
    }

    public IEnumerator TickCoroutine()
    {
        while (elapsedTime < gameDuration)
        {
            yield return new WaitForSeconds(1/ticksPerSecond);

            tickElapsed();
            audioManager.PlayTick();

            // Check collisions
            try
            {
                List<Packet> packetsToRemove = new List<Packet>();

                foreach (Packet packet in spawnedPackets)
                {
                    List<Packet> collidingPackets = spawnedPackets.FindAll(p => p.Cell == packet.Cell && p != packet);

                    if (collidingPackets.Count > 0)
                    {
                        packetsToRemove.AddRange(collidingPackets);
                    }
                }

                if (packetsToRemove.Count > 0)
                {
                    audioManager.PlayCollision();

                    foreach (Packet packet in packetsToRemove)
                    {
                        if (spawnedPackets.Contains(packet))
                        {
                            spawnedPackets.Remove(packet);
                            Destroy(packet.gameObject);
                            score -= packetCollisionPenalty;
                            UpdateScoreText();
                        }
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError("Caught exception while checking for packet collisions. " + e.Message);
                spawnedPackets.RemoveAll(item => item == null);
            }
            

        }
    }

    public IEnumerator PacketSpawnCorouting()
    {
        while (elapsedTime < gameDuration)
        {
            int pipeIndex;

            // Pick a packet from a random pipe
            int packetIndex;

            do
            {
                pipeIndex = Random.Range(0, pipes.Count);
                packetIndex = Random.Range(0, pipes.Count);
            } while (pipeIndex == packetIndex);

            GameObject spawnedGo = pipes[pipeIndex].SpawnPacket(pipes[packetIndex].packet);

            if (spawnedGo != null)
            {
                Packet spawnedPacket = spawnedGo.GetComponent<Packet>();
                if (spawnedPacket != null)
                {
                    spawnedPackets.Add(spawnedPacket);

                }

            }

            yield return new WaitForSeconds(1 / packetsPerSecond);
        }
    }

    public void DeliveredPacket(Pipe pipe, Packet packet)
    {
        if (pipe.packet == packet.packet)
        {
            score += packetDeliveryPoints;
        }
        else
        {
            score -= wrongPacketDeliveryPenalty;
        }
        spawnedPackets.Remove(packet);
        UpdateScoreText();
        Destroy(packet.gameObject);
    }

    public void PacketCollided(Packet packetOne, Packet packet)
    {
        score -= 10;


        UpdateScoreText();
    }

    private void UpdateTimeText()
    {
        float timeLeft = gameDuration - elapsedTime;
        timeText.text = string.Format("{0}:{1:00}", (int)(timeLeft / 60f), (int)(timeLeft % 60f));
    }

    private void UpdateScoreText()
    {
        scoreText.text = string.Format("Score: {0}", score);
    }

    public void GeneratePipes()
    {
        List<Vector3Int> pipePositions = new List<Vector3Int>(numberOfPipes);
        numberOfPipes = Mathf.Min(numberOfPipes, packetAssets.Count);
        int tries = 0;

        for (int i = 0; pipePositions.Count < numberOfPipes && tries < 10; i++)
        {
            float rng = Random.Range(0f, 1f);
            int x, y = 0;
            Quaternion pipeRotation = Quaternion.identity;

            if (rng > 0.5)
            {
                // Spawn on vertical walls
                rng = Random.Range(0f, 1f);
                // Pick which wall. If <= 0.5 choose left wall, otherwise right wall
                if (rng <= 0.5f)
                {
                    // Left wall
                    x = boundsTilemap.cellBounds.xMin + 1;
                    pipeRotation = Quaternion.Euler(0, 0, 90);

                }
                else
                {
                    x = boundsTilemap.cellBounds.xMax - 2;
                    pipeRotation = Quaternion.Euler(0, 0, -90);
                }


                // Pick random point in chosen wall
                y = Random.Range(boundsTilemap.cellBounds.yMin + 3, boundsTilemap.cellBounds.yMax - 1);


            }
            else
            {
                // Spawn on horizontal walls
                rng = Random.Range(0f, 1f);
                // Pick which wall. If <= 0.5 choose bottom wall, otherwise top wall
                if (rng <= 0.5f)
                {
                    y = boundsTilemap.cellBounds.yMin + 2;
                    pipeRotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    y = boundsTilemap.cellBounds.yMax - 1;
                }


                // Pick random point in chosen wall
                x = Random.Range(boundsTilemap.cellBounds.xMin + 2, boundsTilemap.cellBounds.xMax - 2);

            }
            Vector3Int position = new Vector3Int(x, y, 0);

            if (!pipePositions.Contains(position))
            {
                //Debug.Log(position);
                pipePositions.Add(position);

                GameObject pipeGo = Instantiate(pipePrefab, position, pipeRotation);
                Pipe pipe = pipeGo.GetComponent<Pipe>();
                pipe.cell = position;
                pipe.packet = packetAssets[0];
                tries = 0;
            }
            else
            {
                Debug.LogWarning("Pipe Collision");
                tries++;
            }


        }
    }

    private void SpawnPipe(int x, int y, Quaternion pipeRotation)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        GameObject pipeGo = Instantiate(pipePrefab, position, pipeRotation);
        Pipe pipe = pipeGo.GetComponent<Pipe>();
        pipe.cell = position;
        pipe.packet = packetAssets[0];
    }

    private void SpawnTestPipes()
    {
        int x, y = 0;
        Quaternion pipeRotation = Quaternion.identity;

        // Top Left 
        x = boundsTilemap.cellBounds.xMin + 1;
        y = boundsTilemap.cellBounds.yMax - 1;
        
        SpawnPipe(x, y, pipeRotation); // Top wall
        pipeRotation = Quaternion.Euler(0,0, 90);
        SpawnPipe(x, y, pipeRotation); // Left wall

        // Top Right
        x = boundsTilemap.cellBounds.xMax - 2;
        y = boundsTilemap.cellBounds.yMax - 1;
        pipeRotation = Quaternion.identity;
        SpawnPipe(x, y, pipeRotation); // Top wall
        pipeRotation = Quaternion.Euler(0, 0, -90);
        SpawnPipe(x, y, pipeRotation); // Right wall

        // Bottom Right
        x = boundsTilemap.cellBounds.xMax - 2;
        y = boundsTilemap.cellBounds.yMin + 2;
        pipeRotation = Quaternion.Euler(0,0, 180);
        SpawnPipe(x, y, pipeRotation); // Bottom wall
        pipeRotation = Quaternion.Euler(0, 0, -90);
        SpawnPipe(x, y, pipeRotation); // Right wall

        // Bottom Left
        x = boundsTilemap.cellBounds.xMin + 1;
        y = boundsTilemap.cellBounds.yMin + 2;
        pipeRotation = Quaternion.Euler(0, 0, 180); // Bottom wall
        SpawnPipe(x, y, pipeRotation);
        pipeRotation = Quaternion.Euler(0, 0, 90);
        SpawnPipe(x, y, pipeRotation); // Left wall
    }

    
}
