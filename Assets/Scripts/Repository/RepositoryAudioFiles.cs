using UnityEngine;

[CreateAssetMenu(fileName = "Repository Audio Files", menuName = "Repository/Audio Files")]
public class RepositoryAudioFiles : ScriptableObject
{
    public AudioClip mouseHover;
    public AudioClip mouseClick;
    
    public AudioClip uiClick;

    public AudioClip move;
    public AudioClip moveKill;
    public AudioClip castling;
    public AudioClip promotion;

    public AudioClip win;
    public AudioClip lost;
}
