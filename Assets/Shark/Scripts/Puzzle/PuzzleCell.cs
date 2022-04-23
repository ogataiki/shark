using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCell : MonoBehaviour
{
  [Header("PuzzleLevelMaster.CellTypeEnum の値をindexとする")]
  [SerializeField] List<PuzzleCellSprite> sprites;

  PuzzleLevelMaster.CellTypeEnum _cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return _cellType; } }
  int _indexX = 0;
  public int IndexX { get { return _indexX; } }
  int _indexY = 0;
  public int IndexY { get { return _indexY; } }
  float _spriteScale = 1f;

  public UnityEvent<PuzzleCell> onClick;
  public UnityEvent<PuzzleCell> onFinishOnActive;
  public UnityEvent<PuzzleCell> onFinishToVoid;

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
      s.onFinishOnActive.RemoveAllListeners();
      s.onFinishOnActive.AddListener(OnFinishOnActinve);
      s.onFinishToVoid.RemoveAllListeners();
      s.onFinishToVoid.AddListener(OnFinishToVoid);
    }
  }

  public void Init(PuzzleLevelMaster.CellTypeEnum cellType, int indexX, int indexY, float scale)
  {
    _indexX = indexX;
    _indexY = indexY;
    _cellType = cellType;
    _spriteScale = scale;
    foreach (var s in sprites)
    {
      s.gameObject.SetActive(true);
      s.Init(s.CellType == _cellType);
      s.transform.localScale = new Vector3(_spriteScale, _spriteScale, _spriteScale);
    }
  }
  public void UpdateCellType(PuzzleLevelMaster.CellTypeEnum cellType)
  {
    var beforCellType = _cellType;
    _cellType = cellType;

    if (_cellType == PuzzleLevelMaster.CellTypeEnum.VOID)
    {
      if (beforCellType != _cellType)
      {
        PlayCellAnimToVoid(beforCellType);
      }
    }
    else
    {
      PlayCellAnimOnActive(beforCellType);
    }
  }
  public PuzzleCellSprite CurrentCellSprite()
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

  public void PlayCellAnimToVoid(PuzzleLevelMaster.CellTypeEnum beforType)
  {
    ChangeState(StateEnum.PlayAnimation);
    var beforSprite = sprites[(int)beforType];
    beforSprite.PlayToVoid();
  }
  public void PlayCellAnimOnActive(PuzzleLevelMaster.CellTypeEnum beforType)
  {
    ChangeState(StateEnum.PlayAnimation);
    var beforSprite = sprites[(int)beforType];
    beforSprite.PlayToVoid();
    var currentSprite = CurrentCellSprite();
    currentSprite.PlayOnActive();
  }

  // spriteのアニメーション終了でこちらのアニメーションも終了扱い
  public void OnFinishOnActinve(PuzzleCellSprite cell)
  {
    ChangeState(StateEnum.Idle);
    onFinishOnActive?.Invoke(this);
  }
  public void OnFinishToVoid(PuzzleCellSprite cell)
  {
    ChangeState(StateEnum.Idle);
    onFinishToVoid?.Invoke(this);
  }

}
