using TMPro;
using UnityEngine;

public class WinningPanel : MonoBehaviour
{
    [Header("Scripts")] 
    [SerializeField] private Manager _manager;
    
    [Space(10)][Header("External Components")] 
    [SerializeField] private Animation[] _whiteWinners;
    [SerializeField] private Animation[] _blackWinners;

    [SerializeField] private Transform[] _playerLostWhite;
    [SerializeField] private Transform[] _playerLostBlack;

    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private TextMeshProUGUI _tmpResult;

    public void Initialize(Team winner)
    {
        if (_manager.gameMode == GameMode.vsAi)
        {
            switch (winner == _manager.playerTeam)
            {
                case true:
                    _tmpResult.text = "You Win";
                    _particleSystem.Play();
                    
                    for (int i = 0; i < _playerLostWhite.Length; i++)
                    {
                        if (i==0)
                            _playerLostWhite[i].GetComponent<Animation>().Stop();
                        _playerLostWhite[i].gameObject.SetActive(false);
                    }
                    for (int i = 0; i < _playerLostBlack.Length; i++)
                    {
                        if (i==0)
                            _playerLostBlack[i].GetComponent<Animation>().Stop();
                        _playerLostBlack[i].gameObject.SetActive(false);
                    }
                    
                    foreach (Animation animation in _whiteWinners)
                    {
                        animation.Stop();
                        animation.transform.localScale = Vector3.zero;

                        animation.gameObject.SetActive(winner == Team.White);
                        if (winner == Team.White)
                            animation.Play();
                    }
                    foreach (Animation animation in _blackWinners)
                    {
                        animation.Stop();
                        animation.transform.localScale = Vector3.zero;

                        animation.gameObject.SetActive(winner == Team.Black);
                        if (winner == Team.Black)
                            animation.Play();
                    }
                    
                    _manager.PlaySFXEndGame(true);
                    
                    break;
                
                case false:
                    _tmpResult.text = "You Lose";
                    _particleSystem.Stop();

                    switch (_manager.playerTeam)
                    {
                        case Team.White:
                            for (int i = 0; i < _playerLostWhite.Length; i++)
                            {
                                Transform transform = _playerLostWhite[i];
                        
                                if (i==0)
                                    transform.GetComponent<Animation>().Stop();
                        
                                transform.localScale = Vector3.zero;
                                transform.gameObject.SetActive(true);
                        
                                if (i==0 && winner == Team.Black)
                                    transform.GetComponent<Animation>().Play();
                                else
                                    transform.localScale = Vector3.one;
                            }
                            for (int i = 0; i < _playerLostBlack.Length; i++)
                            {
                                if (i==0)
                                    _playerLostBlack[i].GetComponent<Animation>().Stop();
                                _playerLostBlack[i].gameObject.SetActive(false);
                            }
                            break;
                        
                        case Team.Black:
                            for (int i = 0; i < _playerLostBlack.Length; i++)
                            {
                                Transform transform = _playerLostBlack[i];
                        
                                if (i==0)
                                    transform.GetComponent<Animation>().Stop();
                        
                                transform.localScale = Vector3.zero;
                                transform.gameObject.SetActive(true);
                        
                                if (i==0 && winner == Team.White)
                                    transform.GetComponent<Animation>().Play();
                                else
                                    transform.localScale = Vector3.one;
                            }
                            for (int i = 0; i < _playerLostWhite.Length; i++)
                            {
                                if (i==0)
                                    _playerLostWhite[i].GetComponent<Animation>().Stop();
                                _playerLostWhite[i].gameObject.SetActive(false);
                            }
                            break;
                    }
                    
                    foreach (Animation animation in _whiteWinners)
                    {
                        animation.Stop();
                        animation.gameObject.SetActive(false);
                    }
                    foreach (Animation animation in _blackWinners)
                    {
                        animation.Stop();
                        animation.gameObject.SetActive(false);
                    }
                    
                    
                    
                    
                    _manager.PlaySFXEndGame(false);
                    
                    break;
            }

            _manager.PlaySFXEndGame(_manager.playerTeam == winner);
        }
        else
        {
            _tmpResult.text = "Winner";
            _particleSystem.Play();
            
            for (int i = 0; i < _playerLostWhite.Length; i++)
            {
                if (i==0)
                    _playerLostWhite[i].GetComponent<Animation>().Stop();
                _playerLostWhite[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < _playerLostBlack.Length; i++)
            {
                if (i==0)
                    _playerLostBlack[i].GetComponent<Animation>().Stop();
                _playerLostBlack[i].gameObject.SetActive(false);
            }
            
            foreach (Animation animation in _whiteWinners)
            {
                animation.Stop();
                animation.transform.localScale = Vector3.zero;

                animation.gameObject.SetActive(winner == Team.White);
                if (winner == Team.White)
                    animation.Play();
            }
            foreach (Animation animation in _blackWinners)
            {
                animation.Stop();
                animation.transform.localScale = Vector3.zero;

                animation.gameObject.SetActive(winner == Team.Black);
                if (winner == Team.Black)
                    animation.Play();
            }
            
            _manager.PlaySFXEndGame(true);
        }
    }

    public void Reset()
    {
        
    }
}
