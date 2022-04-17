using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCell : MonoBehaviour
{
  [SerializeField] PuzzleCellSprite spriteVoid;
  [SerializeField] PuzzleCellSprite spriteTestPlain;
  [SerializeField] PuzzleCellSprite spriteTestRed;

  PuzzleLevelMaster.CellTypeEnum _cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return _cellType; } }
  int _indexX = 0;
  public int IndexX { get { return _indexX; } }
  int _indexY = 0;
  public int IndexY { get { return _indexY; } }

  public UnityEvent<PuzzleCell> onClick;

  private void Awake()
  {
    spriteVoid.onClick.RemoveAllListeners();
    spriteVoid.onClick.AddListener(OnClick);
    spriteTestPlain.onClick.RemoveAllListeners();
    spriteTestPlain.onClick.AddListener(OnClick);
    spriteTestRed.onClick.RemoveAllListeners();
    spriteTestRed.onClick.AddListener(OnClick);
  }

  public void Init(PuzzleLevelMaster.CellTypeEnum cellType, int indexX, int indexY)
  {
    _indexX = indexX;
    _indexY = indexY;

    UpdateCellType(cellType);
  }
  public void UpdateCellType(PuzzleLevelMaster.CellTypeEnum cellType)
  {
    _cellType = cellType;
    CellSetting();
  }
  public void CellSetting()
  {
    spriteVoid.gameObject.SetActive(false);
    spriteTestPlain.gameObject.SetActive(false);
    spriteTestRed.gameObject.SetActive(false);
    var currentCellSprite = CurrentCellSprite();
    currentCellSprite.gameObject.SetActive(true);
  }
  public PuzzleCellSprite CurrentCellSprite()
  {
    switch (_cellType)
    {
      case PuzzleLevelMaster.CellTypeEnum.VOID: return spriteVoid;
      case PuzzleLevelMaster.CellTypeEnum.TEST_PLAIN: return spriteTestPlain;
      case PuzzleLevelMaster.CellTypeEnum.TEST_RED: return spriteTestRed;
      default: return null;
    }
  }

  public void OnClick(PuzzleCellSprite cellSprite)
  {
    Debug.Log($"[PuzzleCell] OnClick[{cellSprite.CellType}]");
    onClick?.Invoke(this);
  }
}
