using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleLevel : MonoBehaviour
{
  [SerializeField] Transform cellParent;
  [SerializeField] PuzzleCell cellPrefab;
  [SerializeField] List<float> cellPositionXList; // 左から右
  [SerializeField] List<float> cellPositionYList; // 下から上

  List<PuzzleCell> _cellList = new List<PuzzleCell>();
  public List<PuzzleCell> CellList { get { return _cellList; } }

  PuzzleLevelMaster _level;
  public PuzzleLevelMaster LevelMasterData { get { return _level; } }

  public UnityEvent<PuzzleLevel, PuzzleCell> onCellClick;

  public int CellHorizontalCount()
  {
    return cellPositionXList.Count;
  }

  public int CellVerticalCount()
  {
    return cellPositionYList.Count;
  }

  public void Init(PuzzleLevelMaster level)
  {
    _level = level;

    DestroyCell();
    DeployCell();
  }

  public void DeployCell()
  {
    var horizontalCount = CellHorizontalCount();
    var verticalCount = CellVerticalCount();

    var currentCellType = _level.GetRandomCellType();
    for (var y = 0; y < verticalCount; y++)
    {
      for (var x = 0; x < horizontalCount; x++)
      {
        if (_level.LotCellChange())
        {
          currentCellType = _level.GetRandomCellType();
        }
        var cell = Instantiate(cellPrefab, cellParent);
        cell.Init(currentCellType, x, y);
        cell.transform.localPosition = new Vector3(cellPositionXList[x], cellPositionYList[y], 0f);
        cell.onClick.RemoveAllListeners();
        cell.onClick.AddListener(OnClickCell);
        cell.gameObject.SetActive(true);
        _cellList.Add(cell);
      }
    }
  }

  public void DestroyCell()
  {
    foreach(var cell in _cellList)
    {
      Destroy(cell.gameObject);
    }
    _cellList.Clear();
  }

  public void OnClickCell(PuzzleCell cell)
  {
    onCellClick?.Invoke(this, cell);
  }

  void Start()
  {

  }

  void Update()
  {

  }
}
