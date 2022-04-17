using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleManager : SingletonMonoBehaviour<PuzzleManager>
{
  [SerializeField] List<PuzzleLevel> puzzleLevelPrefabList;
  [SerializeField] List<PuzzleLevelMaster> puzzleLevelMasterList;

  int _currentLevel = 0;
  PuzzleLevel _currentPuzzleLevel = null;

  // 現在攻略中のレベルを展開
  public void DeployCurrentLevel()
  {
    if (puzzleLevelPrefabList.Count <= _currentLevel)
    {
      Debug.LogError($"レベル[{_currentLevel}]の puzzle ground が登録されていません。");
      return;
    }

    var groundPrefab = puzzleLevelPrefabList[_currentLevel];
    if (groundPrefab == null)
    {
      Debug.LogError($"レベル[{_currentLevel}]の puzzle ground が null です。");
      return;
    }

    _currentPuzzleLevel = Instantiate(groundPrefab, this.transform);
    _currentPuzzleLevel.Init(puzzleLevelMasterList[_currentLevel]);
    _currentPuzzleLevel.onCellClick.RemoveAllListeners();
    _currentPuzzleLevel.onCellClick.AddListener(OnClickCell);
  }

  private void Start()
  {
    DeployCurrentLevel();
  }

  public void OnClickCell(PuzzleLevel level, PuzzleCell cell)
  {
    Debug.Log($"[PuzzleManager] OnClickCell[{level.LevelMasterData.CellCount}, {cell.CellType}]");

    var chainCells = GetChainCells(level, cell);
    if (chainCells.Count < 2)
    {
      // TODO : 消せないよということを表す何らかの表現を入れる
      return;
    }

    // TODO : 消す演出や消した後に上のやつを下に落とす処理を入れる

    // 消す
    foreach(var c in chainCells)
    {
      c.UpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
    }
  }

  //=====================
  // 連結セルの取得
  //=====================
  HashSet<PuzzleCell> _chainCellsCache = new HashSet<PuzzleCell>();

  // ベースのセルを起点に連結範囲を探索して取得
  public HashSet<PuzzleCell> GetChainCells(PuzzleLevel level, PuzzleCell baseCell)
  {
    _chainCellsCache.Clear();
    _chainCellsCache.Add(baseCell);
    GetChainCellCross(level, baseCell);
    return new HashSet<PuzzleCell>(_chainCellsCache);
  }

  public void GetChainCellCross(PuzzleLevel level, PuzzleCell baseCell)
  {
    GetChainCellUp(level, baseCell, (addCell) => { GetChainCellCross(level, addCell); });
    GetChainCellDown(level, baseCell, (addCell) => { GetChainCellCross(level, addCell); });
    GetChainCellRight(level, baseCell, (addCell) => { GetChainCellCross(level, addCell); });
    GetChainCellLeft(level, baseCell, (addCell) => { GetChainCellCross(level, addCell); });
  }

  // 右方向の探索
  public void GetChainCellRight(PuzzleLevel level, PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellHorizontal(level, baseCell, 1, addCallback);
  }
  // 左方向の探索
  public void GetChainCellLeft(PuzzleLevel level, PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellHorizontal(level, baseCell, -1, addCallback);
  }
  // 上方向の探索
  public void GetChainCellUp(PuzzleLevel level, PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellVertical(level, baseCell, 1, addCallback);
  }
  // 下方向の探索
  public void GetChainCellDown(PuzzleLevel level, PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellVertical(level, baseCell, -1, addCallback);
  }

  // 水平方向の探索
  public void GetChainCellHorizontal(PuzzleLevel level, PuzzleCell baseCell, int direction, Action<PuzzleCell> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexX = baseCell.IndexX + direction;
    if (0 <= nextIndexX && nextIndexX < level.CellHorizontalCount())
    {
      var nextCell = level.CellList.FirstOrDefault(_ => _.IndexX == nextIndexX && _.IndexY == baseCell.IndexY);
      if (nextCell.CellType == baseCell.CellType)
      {
        if (_chainCellsCache.Any(_ => _.IndexX == nextCell.IndexX && _.IndexY == nextCell.IndexY)) { return; }
        _chainCellsCache.Add(nextCell);
        addCallback?.Invoke(nextCell);
      }
    }
  }
  // 垂直方向の探索
  public void GetChainCellVertical(PuzzleLevel level, PuzzleCell baseCell, int direction, Action<PuzzleCell> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexY = baseCell.IndexY + direction;
    if (0 <= nextIndexY && nextIndexY < level.CellVerticalCount())
    {
      var nextCell = level.CellList.FirstOrDefault(_ => _.IndexX == baseCell.IndexX && _.IndexY == nextIndexY);
      if (nextCell.CellType == baseCell.CellType)
      {
        if (_chainCellsCache.Any(_ => _.IndexX == nextCell.IndexX && _.IndexY == nextCell.IndexY)) { return; }
        _chainCellsCache.Add(nextCell);
        addCallback?.Invoke(nextCell);
      }
    }
  }

}
