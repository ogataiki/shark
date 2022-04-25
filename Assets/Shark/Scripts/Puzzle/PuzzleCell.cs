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

  PuzzleLevelMaster.CellTypeEnum _cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return _cellType; } }

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
  }

  public PuzzleCellSprite GetCellSprite()
  {
    if ((int)_cellType >= sprites.Count) { return null; }
    return sprites[(int)_cellType];
  }

  public void OnClick(PuzzleCellSprite cellSprite)
  {
    if (cellSprite.CellType != _cellType) { return; }
    if (!IsIdle()) { return; }

    Debug.Log($"[PuzzleCell] OnClick[{cellSprite.CellType}]");
    onClick?.Invoke(this);
  }

  public async UniTask ToVoid()
  {
    var currentSprite = GetCellSprite();
    if (currentSprite == null) { return; }
    ChangeState(StateEnum.PlayAnimation);
    _cellType = PuzzleLevelMaster.CellTypeEnum.VOID;
    await currentSprite.PlayToVoid();
    await UniTask.WaitUntil(() => currentSprite.IsIdle());
    ChangeState(StateEnum.Idle);
  }
}
