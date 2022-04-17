using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerHandler : MonoBehaviour
{
  public UnityEvent onPointerClickEvent;

  public void OnPointerClick()
  {
    Debug.Log($"[EventTriggerHandler] OnPointerClick!!");
    onPointerClickEvent?.Invoke();
  }
}
