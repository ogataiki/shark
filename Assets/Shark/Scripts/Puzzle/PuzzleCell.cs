using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCell : MonoBehaviour
{
  [Header("PuzzleLevelMaster.CellTypeEnum の値をindexとする")]
  [SerializeField] List<PuzzleCellSprite> sprites;
  [SerializeField] PuzzleCellSprite spriteQ;
  [SerializeField] Animator animator;

  PuzzleLevelMaster.CellTypeEnum _cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return _cellType; } }

  bool _QActive = false;
  public bool QActive { get { return _QActive; } }

  float _spriteScale = 1f;

  PuzzleSlot _slot;
  public PuzzleSlot Slot { get { return _slot; } }

  string _cellHash;

  public UnityEvent<PuzzleCell> onClick;

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
  public bool IsIdle()
  {
    return _state == StateEnum.Idle;
  }

  private void Awake()
  {
    foreach(var s in sprites)
    {
      s.onClick.RemoveAllListeners();
      s.onClick.AddListener(OnClick);
    }

    spriteQ.onClick.RemoveAllListeners();
    spriteQ.onClick.AddListener(OnClick);
  }

  public void Init(PuzzleLevelMaster.CellTypeEnum cellType, float scale, PuzzleSlot slot)
  {
    _slot = slot;
    _cellType = cellType;
    _spriteScale = scale;

    foreach (var s in sprites)
    {
      s.gameObject.SetActive(true);
      s.Init(s.CellType == _cellType);
      s.transform.localScale = new Vector3(_spriteScale, _spriteScale, _spriteScale);
    }
    spriteQ.ClickEnable(false);
  }

  public PuzzleCellSprite GetCellSprite()
  {
    if ((int)_cellType >= sprites.Count) { return null; }
    return sprites[(int)_cellType];
  }

  public void OnClick(PuzzleCellSprite cellSprite)
  {
    if (_cellType == PuzzleLevelMaster.CellTypeEnum.VOID) { return; }
    if (!IsIdle()) { return; }

    Debug.Log($"[PuzzleCell] OnClick[{cellSprite.CellType}]");
    onClick?.Invoke(this);
  }

  public async UniTask ToVoid()
  {
    var currentSprite = GetCellSprite();
    if (currentSprite == null) { return; }
    ChangeState(StateEnum.PlayAnimation);
    spriteQ.gameObject.SetActive(false);
    _cellType = PuzzleLevelMaster.CellTypeEnum.VOID;
    await currentSprite.PlayToVoid();
    await UniTask.WaitUntil(() => currentSprite.IsIdle());
    ChangeState(StateEnum.Idle);
  }

  public async UniTask QUpdate(bool active, bool fast = false)
  {
    if (spriteQ == null) { return; }
    _QActive = active;
    spriteQ.ClickEnable(active);

    var animationName1 = active ? "Show" : "Hide";
    var animationName2 = fast ? "Fast" : "";
    animator.Play($"{animationName1}Q{animationName2}");
    await UniTask.DelayFrame(1); // ステートの反映に1フレームいるかも？
    var nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    await UniTask.WaitWhile(() => {
      var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
      return currentAnimatorState.fullPathHash == nameHash && (currentAnimatorState.normalizedTime < 1);
    });
    var currentSprite = GetCellSprite();
    if (currentSprite == null) { return; }
    if (active)
    {
      await currentSprite.PlayToVoid();
    }
    else
    {
      if (_cellType != PuzzleLevelMaster.CellTypeEnum.VOID)
      {
        await currentSprite.PlayOnActive();
      }
    }
  }
}
