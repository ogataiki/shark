using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PuzzleSlot : MonoBehaviour
{
  public PuzzleLevelMaster.CellTypeEnum CellType
  {
    get
    {
      if (_cell == null) { return PuzzleLevelMaster.CellTypeEnum.VOID; }
      return _cell.CellType;
    }
  }
  public PuzzleLevelMaster.CellTypeEnum CellTypeWaitUpdate
  {
    get
    {
      if (_state == StateEnum.WaitMoveCell)
      {
        if (_nextCell == null) { return PuzzleLevelMaster.CellTypeEnum.VOID; }
        return _nextCell.CellType;
      }
      if (_cell == null) { return PuzzleLevelMaster.CellTypeEnum.VOID; }
      return _cell.CellType;
    }
  }

  int _indexX = 0;
  public int IndexX { get { return _indexX; } }
  int _indexY = 0;
  public int IndexY { get { return _indexY; } }

  PuzzleCell _cell = null;
  public PuzzleCell Cell { get { return _cell; } }

  PuzzleCell _nextCell = null;
  public PuzzleCell NextCell { get { return _nextCell; } }

  public PuzzleCell CellWaitUpdate
  {
    get
    {
      if (_state == StateEnum.WaitMoveCell) { return NextCell; }
      return Cell;
    }
  }

  public UnityEvent<PuzzleSlot> onClick;

  public enum StateEnum
  {
    Idle = 0,
    WaitMoveCell,
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
  }

  private void Update()
  {
    //if (_state != StateEnum.Idle)
    //{
    //  Debug.Log($"[PuzzleCell {IndexX},{IndexY}] state[{_state}] cellType[{_cellType}] nextCellType[{_nextCellType}]");
    //}
  }

  public void Init(int indexX, int indexY)
  {
    _indexX = indexX;
    _indexY = indexY;
  }

  public void PutOnCell(PuzzleCell cell)
  {
    _cell = cell;
    _cell.transform.position = this.transform.position;
    _cell.onClick.RemoveAllListeners();
    _cell.onClick.AddListener(OnClick);
  }

  public async UniTask ToVoid()
  {
    if (_cell == null) { return; }
    ChangeState(StateEnum.PlayAnimation);
    await _cell.ToVoid();
    await UniTask.WaitUntil(() => _cell.IsIdle());
    ChangeState(StateEnum.Idle);
  }

  public void PreMoveCell(PuzzleCell nextCell)
  {
    _nextCell = nextCell;
    ChangeState(StateEnum.WaitMoveCell);
  }
  public void FireMoveCell()
  {
    if (_state != StateEnum.WaitMoveCell) { return; }
    MoveCell(_nextCell);
  }

  void MoveCell(PuzzleCell nextCell)
  {
    var beforCell = _cell;
    _cell = nextCell;

    if (beforCell == _cell)
    {
      ChangeState(StateEnum.Idle);
      return;
    }

    beforCell.onClick.RemoveAllListeners();

    ChangeState(StateEnum.PlayAnimation);

    nextCell.transform.DOMove(this.transform.position, 0.1f)
      .OnComplete(() => {
        ChangeState(StateEnum.Idle);
        _cell.onClick.RemoveAllListeners();
        _cell.onClick.AddListener(OnClick);
      });
  }

  public PuzzleCellSprite GetCellSprite()
  {
    if (_cell == null) { return null; }
    return _cell.GetCellSprite();
  }

  public void OnClick(PuzzleCell cell)
  {
    if (!IsIdle()) { return; }

    Debug.Log($"[PuzzleCell] OnClick[{cell.CellType}]");
    onClick?.Invoke(this);
  }
}
