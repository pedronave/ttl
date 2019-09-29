using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Vector3Int cell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    public PacketAsset packet;
    private GameManager gm;

    [SerializeField]
    private Sprite[] sprites = new Sprite[1];

    public GameObject packetPrefab;

    private int spriteIndex = 0;
    private PacketAsset packetToSpawn;

    public bool Available => packetToSpawn == null;

    public GameObject label;
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        sr = GetComponent<SpriteRenderer>();

        gm.tickElapsed += Tick;

        float zRotation = transform.rotation.eulerAngles.z;
        if (cell.Equals(new Vector3Int(int.MinValue, int.MinValue, int.MinValue)))
        {
            
        }
        
        label.transform.rotation = Quaternion.identity;
        label.GetComponent<SpriteRenderer>().sprite = packet.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!cell.Equals(new Vector3Int(int.MinValue, int.MinValue, int.MinValue)))
        {
            transform.position = cell + new Vector3(.5f, -.5f);
        }
    }

    public bool SpawnPacket(PacketAsset packetAsset)
    {
        if (Available)
        {
            packetToSpawn = packetAsset;
            return true;
        }
        

        return false;
    }

    private void Tick()
    {
        if (!Available)
        {
            spriteIndex++;
            if (spriteIndex >= sprites.Length)
            {
                spriteIndex = 0;
                
                GameObject packetGo = Instantiate(packetPrefab, transform.position - transform.up, Quaternion.identity);

                Packet spawnedPacket = packetGo.GetComponent<Packet>();
                spawnedPacket.packet = packetToSpawn;

                gm.AddSpawnedPacket(spawnedPacket);

                packetToSpawn = null;
            }
            sr.sprite = sprites[spriteIndex];
        }
    } 

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Packet packet = collision.GetComponent<Packet>();

        if (packet != null)
        {
            gm.DeliveredPacket(this, packet);
        }
    }
}
