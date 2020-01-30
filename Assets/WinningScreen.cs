using System.Collections;
using TMPro;
using UnityEngine;

public class WinningScreen : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
        
    private void Awake()
    {
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        textMeshPro.text = "You is winner " + GameController.winner;

        yield return new WaitForSeconds(5);
        GameController.winner = null;
        GameController.ShutGameDown();
    }
}
