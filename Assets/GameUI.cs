using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameObject EndGamePanel;
    public TextMeshProUGUI pointsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowEndGamePanel(int points)
    {
        EndGamePanel.SetActive(true);
        pointsText.text = string.Format("{0} points", points);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }
}
