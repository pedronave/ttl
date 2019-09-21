using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Packet", menuName = "Packet")]
public class PacketAsset : ScriptableObject
{
    public string packetName;

    public Sprite sprite;
}
