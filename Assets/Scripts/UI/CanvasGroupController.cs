using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CanvasGroupController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [Space(10)] [Header("Settings")]
    [SerializeField] private float fadeInHoldOutTime = 0f;
    [SerializeField] private float fadeInTime = 0.5f;
    
    [SerializeField] private float fadeOutHoldOutTime = 0f;
    [SerializeField] private float fadeOutTime = 0.5f;
    
    [Space(10)] [Header("Unity Events")]
    public UnityEvent startFadeIn;
    public UnityEvent finishedFadeIn;
    public UnityEvent startFadeOut;
    public UnityEvent finishedFadeOut;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    #endif

    private void Awake()
    {
        if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        startFadeIn?.Invoke();
        
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        StartCoroutine(StartFadeIn());
    }
    
    public void FadeOut()
    {
        startFadeOut?.Invoke();
        
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        StartCoroutine(StartFadeOut());
    }

    IEnumerator StartFadeIn()
    {
        yield return new WaitForSeconds(fadeInHoldOutTime);
        
        float t = 0f;

        while (true)
        {
            while (t < fadeInTime)
            {
                t += Time.deltaTime;

                float value = t / fadeInTime;
                _canvasGroup.alpha = value;
                yield return null;
            }

            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            finishedFadeIn?.Invoke();
            yield break;
        }
    }
    
    IEnumerator StartFadeOut()
    {
        yield return new WaitForSeconds(fadeOutHoldOutTime);
        
        float t = fadeOutTime;

        while (true)
        {
            while (t > 0f)
            {
                t -= Time.deltaTime;
                
                float value = t / fadeOutTime;
                _canvasGroup.alpha = value;
                yield return null;
            }

            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            finishedFadeOut?.Invoke();
            yield break;
        }
    }

    public void ResetState()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
