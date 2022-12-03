using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class MouseOverUI
{
    private static int _uiLayer = -1;
    
    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        if (_uiLayer == -1) _uiLayer = LayerMask.NameToLayer("UI");
        
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    
    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        for (int index = 0; index < eventSystemRaycastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaycastResults[index];
            if (curRaysastResult.gameObject.layer == _uiLayer)
                return true;
        }
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
