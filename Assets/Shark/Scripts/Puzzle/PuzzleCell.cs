using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCell : MonoBehaviour
{
  [SerializeField]
  PuzzleLevelMaster.CellTypeEnum cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return cellType; } }

  int _indexX = 0;
  public int IndexX { get { return _indexX; } }
  int _indexY = 0;
  public int IndexY { get { return _indexY; } }

  public void Init(int indexX, int indexY)
  {
    _indexX = indexX;
    _indexY = indexY;
  }
}
