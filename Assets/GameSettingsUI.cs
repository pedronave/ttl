using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSettingsUI : MonoBehaviour
{
    [InspectorName("Ticks Per Second Text")]
    public TextMeshProUGUI tpsText;

    [InspectorName("Packets Per Second Text")]
    public TextMeshProUGUI ppsText;

    public GameConfig gameConfig;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tpsText.text = gameConfig.ticksPerSecond.ToString();
        ppsText.text = gameConfig.packetsPerSecond.ToString();
    }
}
