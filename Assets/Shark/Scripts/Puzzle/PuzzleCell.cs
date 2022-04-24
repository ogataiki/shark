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
  PuzzleLevelMaster.CellTypeEnum _nextCellType = PuzzleLevelMaster.CellTypeEnum.VOID;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return _cellType; } }
  public PuzzleLevelMaster.CellTypeEnum CellTypeWaitUpdate
  {
    get
    {
      if (_state == StateEnum.WaitUpdateCellType)
      {
        return _nextCellType;
      }
      return _cellType;
    }
  }
  int _indexX = 0;
  public int IndexX { get { return _indexX; } }
  int _indexY = 0;
  public int IndexY { get { return _indexY; } }
  float _spriteScale = 1f;

  public UnityEvent<PuzzleCell> onClick;

  public enum StateEnum
  {
    Idle = 0,
    WaitUpdateCellType,
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

  private void Update()
  {
    if (_state != StateEnum.Idle)
    {
      //Debug.Log($"[PuzzleCell {IndexX},{IndexY}] state[{_state}] cellType[{_cellType}] nextCellType[{_nextCellType}]");
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

  public void PreUpdateCellType(PuzzleLevelMaster.CellTypeEnum nextCellType)
  {
    _nextCellType = nextCellType;
    ChangeState(StateEnum.WaitUpdateCellType);
  }
  public void FireUpdateCellType()
  {
    if (_state != StateEnum.WaitUpdateCellType) { return; }
    UpdateCellType(_nextCellType);
  }

  void UpdateCellType(PuzzleLevelMaster.CellTypeEnum cellType)
  {
    var beforCellType = _cellType;
    _cellType = cellType;

    if (beforCellType == _cellType)
    {
      ChangeState(StateEnum.Idle);
      return;
    }

    ChangeState(StateEnum.PlayAnimation);

    var beforSprite = GetCellSprite(beforCellType);
    beforSprite.PlayToVoid().Forget();

    var currentSprite = GetCellSprite(_cellType);
    if (_cellType == PuzzleLevelMaster.CellTypeEnum.VOID)
    {
      currentSprite.onFinishToVoid.RemoveAllListeners();
      currentSprite.onFinishToVoid.AddListener(UpdateCellTypeFinish);
      currentSprite.PlayToVoid().Forget();
    }
    else
    {
      currentSprite.onFinishOnActive.RemoveAllListeners();
      currentSprite.onFinishOnActive.AddListener(UpdateCellTypeFinish);
      currentSprite.PlayOnActive().Forget();
    }
  }
  public void UpdateCellTypeFinish(PuzzleCellSprite sprite)
  {
    sprite.onFinishToVoid.RemoveAllListeners();
    sprite.onFinishOnActive.RemoveAllListeners();

    // spriteのアニメーション終了でこちらのアニメーションも終了扱い
    ChangeState(StateEnum.Idle);
  }

  public PuzzleCellSprite GetCellSprite(PuzzleLevelMaster.CellTypeEnum cellType)
  {
    if ((int)cellType >= sprites.Count) { return null; }
    return sprites[(int)cellType];
  }

  public void OnClick(PuzzleCellSprite cellSprite)
  {
    if (cellSprite.CellType != _cellType) { return; }
    if (!IsIdle()) { return; }

    Debug.Log($"[PuzzleCell] OnClick[{cellSprite.CellType}]");
    onClick?.Invoke(this);
  }
}
