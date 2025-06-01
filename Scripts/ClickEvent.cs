using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEvent : MonoBehaviour
{
    public void onClickAct(BaseEventData data) {
        var eventData = (PointerEventData)data;
        Debug.Log(eventData);
    }
}
