using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupNotification : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private TextMeshProUGUI _tmpNotification;
    [SerializeField] private CanvasGroupController _canvasGroupController;
    
    [Space(10)] [Header("External Components")]
    [ShowOnly] public List<string> popUpMessage;

    public bool ShowingPopup { get; set; }

    private void OnEnable()
    {
        ShowingPopup = false;
        _canvasGroupController.ResetState();
    }

    public void ShowNotification()
    {
        if (!ShowingPopup)
        {
            if (popUpMessage.Count > 0)
            {
                _tmpNotification.text = popUpMessage[0];
                
                _canvasGroupController.FadeIn();
            
                popUpMessage.RemoveAt(0);
                
                ShowingPopup = true;
            }
        }
    }
}
