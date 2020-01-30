using System.Collections.Generic;
using DetentionRoom.Networking;
using DetentionRoom.Networking.States.Player;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static string winner;
    public GameObject winningScreen;

    private void Update()
    {
        if (MenuController.PlayerLives == 0)
        {
            winner = "Teachers";
        }

        if (winner == null)
        {
            return;
        }
        AwakeWinnerScreen();
    }

    private void AwakeWinnerScreen()
    {
        winningScreen.SetActive(true);
    }

    public static void ShutGameDown()
    {
        List<Player> objects = GetAllPlayers();
        foreach (var variable in objects)
        {
            variable.GetComponent<Player>().EndGame();
        }
    }

    private static List<Player> GetAllPlayers()
    {
        List<Player> allPlayers = new List<Player>();

        foreach (var boltEntity in BoltNetwork.Entities)
        {
            if (boltEntity.GetState<IPlayer>() == null)
            {
                continue;
            }

            allPlayers.Add(boltEntity.GetComponent<Player>());
        }

        return allPlayers;
    }
}
