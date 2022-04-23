using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCellSprite : MonoBehaviour
{
  [SerializeField] PuzzleLevelMaster.CellTypeEnum cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return cellType; } }

  [SerializeField] SpriteRenderer spriteRenderer;
  [SerializeField] BoxCollider2D boxCollider;
  [SerializeField] EventTriggerHandler eventTriggerHandler;
  [SerializeField] Animator animator;

  public UnityEvent<PuzzleCellSprite> onClick;
  public UnityEvent<PuzzleCellSprite> onFinishOnActive;
  public UnityEvent<PuzzleCellSprite> onFinishToVoid;

  public enum StateEnum
  {
    Idle = 0,
    PlayAnimation,
  }
  StateEnum _state = StateEnum.Idle;
  void ChangeState(StateEnum state)
  {
    _state = state;
  }

  private void Awake()
  {
    eventTriggerHandler.onPointerClickEvent.RemoveAllListeners();
    eventTriggerHandler.onPointerClickEvent.AddListener(OnClick);
  }

  public void Init(bool active)
  {
    boxCollider.enabled = false;
    if (active)
    {
      PlayOnActive();
    }
  }

  public void OnClick()
  {
    onClick?.Invoke(this);
  }

  public void PlayToVoid()
  {
    ChangeState(StateEnum.PlayAnimation);
    animator.Play("ToVoid");
    Debug.Log($"[PuzzleCellSprite] PlayAnimation ToVoid");
  }
  public void PlayOnActive()
  {
    ChangeState(StateEnum.PlayAnimation);
    animator.Play("OnActive");
    Debug.Log($"[PuzzleCellSprite] PlayAnimation OnActive");
  }

  // アニメーションイベント
  public void OnFinishOnActinve()
  {
    ChangeState(StateEnum.Idle);
    boxCollider.enabled = true;
    onFinishOnActive?.Invoke(this);
  }
  public void OnFinishToVoid()
  {
    ChangeState(StateEnum.Idle);
    boxCollider.enabled = false;
    onFinishToVoid?.Invoke(this);
  }
}
