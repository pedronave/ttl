using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance;

    public float packetsPerSecond = 1f;
    public float ticksPerSecond = 2f;

    [SerializeField]
    private float packetsIncreaseGranularity = 0.25f;
    [SerializeField]
    private float ticksIncreaseGranularity = 0.25f;

    [SerializeField]
    private float easyPPS = .5f;
    [SerializeField]
    private float easyTPS = 1f;

    [SerializeField]
    private float mediumPPS = 1f;
    [SerializeField]
    private float mediumTPS = 2f;

    [SerializeField]
    private float hardPPS = 2f;
    [SerializeField]
    private float hardTPS = 4f;

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(gameObject);
        }else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEasy()
    {
        packetsPerSecond = easyPPS;
        ticksPerSecond = easyTPS;
    }

    public void SetMedium()
    {
        packetsPerSecond = mediumPPS;
        ticksPerSecond = mediumTPS;
    }

    public void SetHard()
    {
        packetsPerSecond = hardPPS;
        ticksPerSecond = hardTPS;
    }

    public void IncreasePacketsPerSecond()
    { 
        packetsPerSecond += packetsIncreaseGranularity;
        
    }
    public void DecreasePacketsPerSecond()
    {
        if (packetsPerSecond > packetsIncreaseGranularity)
        {
            packetsPerSecond -= packetsIncreaseGranularity;
        }
    }

    public void IncreaseTicksPerSecond()
    {
        ticksPerSecond += ticksIncreaseGranularity;
    }

    public void DecreaseTicksPerSecond()
    {
        if (ticksPerSecond > ticksIncreaseGranularity)
        {
            ticksPerSecond -= ticksIncreaseGranularity;
        }
    }
}
