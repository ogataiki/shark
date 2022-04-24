using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
      PlayOnActive().Forget();
    }
  }

  public void OnClick()
  {
    if (_state != StateEnum.Idle) { return; }
    onClick?.Invoke(this);
  }

  public async UniTask PlayToVoid()
  {
    ChangeState(StateEnum.PlayAnimation);
    animator.Play("ToVoid");
    Debug.Log($"[PuzzleCellSprite] PlayAnimation ToVoid");
    await UniTask.DelayFrame(1); // ステートの反映に1フレームいるかも？
    var nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    await UniTask.WaitWhile(() => {
      var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
      return currentAnimatorState.fullPathHash == nameHash && (currentAnimatorState.normalizedTime < 1);
    });
    OnFinishToVoid();
  }
  public async UniTask PlayOnActive()
  {
    ChangeState(StateEnum.PlayAnimation);
    animator.Play("OnActive");
    Debug.Log($"[PuzzleCellSprite] PlayAnimation OnActive");
    await UniTask.DelayFrame(1); // ステートの反映に1フレームいるかも？
    var nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    await UniTask.WaitWhile(() => {
      var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
      return currentAnimatorState.fullPathHash == nameHash && (currentAnimatorState.normalizedTime < 1);
    });
    OnFinishOnActinve();
  }

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
