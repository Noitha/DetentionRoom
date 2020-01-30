using Bolt;
using DetentionRoom.Networking.States.Player;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Time;

public class ObjectiveArea : GlobalEventListener
{
    public Slider progressSlider;

    [SerializeField] private float currentProgress;
    private BoltEntity _player;

    [SerializeField] private bool finished;
    [SerializeField] private bool isStudent;
    [SerializeField] private bool error;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
        {
            return;
        }
            
        _player = other.GetComponent<Player>().entity;

        IPlayer playerState = _player.GetState<IPlayer>();
        
        if (playerState.Team != "Student") return;
        isStudent = true;
        error = false;
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (finished)
        {
            GameController.winner = "Students";
            progressSlider.value = 100;
            error = true;
        }
        if (!isStudent)
        {
            error = true;
        }

        if (error)
        {
            return;
        }
            
        currentProgress += deltaTime * 28 / 48;
            
        progressSlider.value = (int) currentProgress;

        if (!(currentProgress >= 100)) return;
        finished = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() == null)
        {
            return;
        }
        ResetProgress();
    }



    private void ResetProgress()
    {
        isStudent = false;
        finished = false;
        error = false;
            
        currentProgress = 0;
            
        progressSlider.minValue = 0;
        progressSlider.maxValue = 100;
        progressSlider.value = 0;
    }
    
}
