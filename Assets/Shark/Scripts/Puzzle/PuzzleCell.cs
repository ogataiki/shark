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

  private void Awake()
  {
    foreach(var s in sprites)
    {
      s.onClick.RemoveAllListeners();
      s.onClick.AddListener(OnClick);
    }
  }

  public void Init(PuzzleLevelMaster.CellTypeEnum cellType, int indexX, int indexY, float scale)
  {
    _indexX = indexX;
    _indexY = indexY;
    _spriteScale = scale;
    UpdateCellType(cellType);
  }
  public void UpdateCellType(PuzzleLevelMaster.CellTypeEnum cellType)
  {
    _cellType = cellType;
    CellSetting();
  }
  public void CellSetting()
  {
    foreach (var s in sprites)
    {
      s.transform.localScale = new Vector3(_spriteScale, _spriteScale, _spriteScale);
      s.gameObject.SetActive(false);
    }

    var currentCellSprite = CurrentCellSprite();
    currentCellSprite.gameObject.SetActive(true);
  }
  public PuzzleCellSprite CurrentCellSprite()
  {
    if ((int)_cellType >= sprites.Count) { return null; }
    return sprites[(int)_cellType];
  }

  public void OnClick(PuzzleCellSprite cellSprite)
  {
    Debug.Log($"[PuzzleCell] OnClick[{cellSprite.CellType}]");
    onClick?.Invoke(this);
  }
}
