using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Vector3Int cell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    public PacketAsset packet;
    private GameManager gm;


    public GameObject packetPrefab;

    public GameObject label;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
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

    public GameObject SpawnPacket(PacketAsset packetAsset)
    {
        GameObject packetGo = Instantiate(packetPrefab, transform.position - transform.up, Quaternion.identity);
        packetGo.GetComponent<Packet>().packet = packetAsset;

        return packetGo;
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
