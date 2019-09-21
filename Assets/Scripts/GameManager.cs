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
    public float elapsedTime = 0f;
    public int score = 0;

    public Tilemap boundsTilemap;

    public GameObject pipePrefab;
    public GameObject positionPipesPrefab;

    public List<Pipe> pipes;

    public List<Packet> packetPrefabs;

    public Text scoreText;

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

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            //pipes[0].SpawnPacket(packetAssets[0]);
            for (int i = 0; i < pipes.Count; i++)
            {
                pipes[i].SpawnPacket(packetAssets[i]);
            }

        }
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= gameDuration)
        {
            // End game
            Debug.LogWarning("Game over");
        }
    }

    public void DeliveredPacket(Pipe pipe, Packet packet)
    {
        if (pipe.packet == packet.packet)
        {
            score += 10;
        }
        else
        {
            score -= 10;
        }

        UpdateScoreText();
        Destroy(packet.gameObject);
    }

    public void PacketCollided(Packet packetOne, Packet packet)
    {
        score -= 10;


        UpdateScoreText();
    }

    public void UpdateScoreText()
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
