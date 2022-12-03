using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Manager : MonoBehaviour
{
    [Header("Game Settings")] 
    [ShowOnly] public GameMode gameMode;
    [ShowOnly] public Team playerTeam;  
    
    [Space(10)] [Header("Repository Data")] 
    public RepositoryAudioFiles RepositoryAudioFiles;
    
    [Space(10)] [Header("Unity Event")] 
    public UnityEvent onIntroDone;
    public UnityEvent onAwake;

    [Space(10)] [Header("External Components")] 
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioSource BGM;
    [SerializeField] private Transform CameraPivot;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        onAwake?.Invoke();
    }

    public void SetGameMode(int mode)
    {
        switch (mode)
        {
            case 0:
                gameMode = GameMode.vsAi;
                break;
            case 1:
                gameMode = GameMode.vsPlayer;
                break;
            default:
                Debug.LogError("No GameMode Selected!");
                break;
        }
    }
    
    public void SetPlayerTeam(int mode)
    {
        switch (mode)
        {
            case 0:
                playerTeam = Team.White;
                break;
            case 1:
                playerTeam = Team.Black;
                break;
            case 2:
                int value = Random.Range(1, 101);
                playerTeam = value >= 50 ? Team.White : Team.Black;

                switch (playerTeam)
                {
                    case Team.White:
                        SetCameraFocus(0);
                        break;
                    case Team.Black:
                        SetCameraFocus(1);
                        break;
                    default:
                        Debug.LogWarning("Failed assign camera focus, defaulting to 0");
                        SetCameraFocus(0);
                        break;
                }

                break;
            default:
                Debug.LogError("No Team Selected!");
                break;
        }
    }

    public void SetCameraFocus(int mode)
    {
        switch (mode)
        {
            case 0:
                CameraPivot.eulerAngles = Vector3.zero;
                break;
            case 1:
                CameraPivot.eulerAngles = new Vector3(0, 180, 0);
                break;
            default:
                Debug.LogError("No Camera Mode Selected!");
                break;
        }
    }

    public void ButtonStartGame()
    {
        SFX.volume = 1;
        SFX.PlayOneShot(RepositoryAudioFiles.mouseClick);
    }

    public void PlaySFX(AudioClip clip, float volume = 1)
    {
        SFX.volume = volume;
        SFX.PlayOneShot(clip);
    }

    public void PlaySFXEndGame(bool victory)
    {
        switch (victory)
        {
            case true:
                SFX.clip = RepositoryAudioFiles.win;
                SFX.Play();
                break;
            
            case false:
                SFX.clip = RepositoryAudioFiles.lost;
                SFX.Play();
                break;
        }
    }

    public void StopSFXEndGame()
    {
        SFX.Stop();
    }
}

[Serializable]
public enum GameMode
{
    vsPlayer, vsAi
}
