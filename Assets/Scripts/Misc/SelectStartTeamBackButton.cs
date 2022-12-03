using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SelectStartTeamBackButton : MonoBehaviour
{
    [Header("Scripts")] 
    [SerializeField] private Manager _manager;

    [Space(10)] [Header("Unity Event")] 
    public UnityEvent onPlayingAgainstAI;
    public UnityEvent onPlayingAgainstPlayer;
    
    public void CheckBackButton()
    {
        switch (_manager.gameMode)
        {
            case GameMode.vsAi:
                onPlayingAgainstAI?.Invoke();
                break;
            case GameMode.vsPlayer:
                onPlayingAgainstPlayer?.Invoke();
                break;
        }
    }
}
