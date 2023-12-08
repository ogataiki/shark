using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OpenChestAnimation : MonoBehaviour
{
  [SerializeField] Animator animator = default;
  public UnityEvent openEvent;
  public UnityEvent finishEvent;
  public UnityEvent destroyEvent;
  bool _finished = false;
  public bool Finished { get { return _finished; } }

  public void PlayOpenChest(bool hit)
  {
    _finished = false;
    animator.Play(hit ? "OpenChest" : "OpenChest");
  }
  public void OnOpenChestOpenEvent()
  {
    openEvent.Invoke();
  }
  public void OnOpenChestFinishEvent()
  {
    finishEvent.Invoke();
    _finished = true;
  }
  public void OnOpenChestDestroyEvent()
  {
    destroyEvent.Invoke();
  }
}
