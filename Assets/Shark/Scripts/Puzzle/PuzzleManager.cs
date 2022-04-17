using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : SingletonMonoBehaviour<PuzzleManager>
{
  [SerializeField] List<PuzzleLevel> puzzleLevelPrefabList;
  [SerializeField] List<PuzzleLevelMaster> puzzleLevelMasterList;


  // UI
  [SerializeField] GameObject gameClearUI;
  [SerializeField] Button nextLevelButton;

  [SerializeField] GameObject gameOverUI;
  [SerializeField] Button retryLevelButton;

  [SerializeField] int levelMax = 1;

  int _currentLevel = 0;
  PuzzleLevel _currentPuzzleLevel = null;

  private void Awake()
  {
    nextLevelButton.onClick.AddListener(OnClickNextLevelButton);
    retryLevelButton.onClick.AddListener(OnClickRetryLevelButton);
  }

  private void Start()
  {
    DeployCurrentLevel();
  }

  public void OnClickNextLevelButton()
  {
    DeployNextLevel();

    GameStart();
  }

  public void OnClickRetryLevelButton()
  {
    ResetCurrentLevel();

    GameStart();
  }

  public void GameStart()
  {
    gameClearUI.SetActive(false);
    gameOverUI.SetActive(false);
  }

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
  public void DeployNextLevel()
  {
    _currentLevel += 1;
    _currentLevel = Math.Min(levelMax, _currentLevel);

    if (_currentPuzzleLevel != null)
    {
      Destroy(_currentPuzzleLevel.gameObject);
      _currentPuzzleLevel = null;
    }
    DeployCurrentLevel();
  }
  public void ResetCurrentLevel()
  {
    if (_currentPuzzleLevel == null) { return; }
    _currentPuzzleLevel.Init(puzzleLevelMasterList[_currentLevel]);
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

    // TODO : 消す演出や消した後に上のやつを下に落とす演出を入れる

    // 消す
    foreach (var c in chainCells)
    {
      c.UpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
    }

    // 盤面整理
    LevelRemap(level);

    var state = GameState(level);
    switch(state)
    {
      case GameStateEnum.GameClear: ToGameClear(); break;
      case GameStateEnum.GameOver: ToGameOver(); break;
    }
  }

  // 消えた後に再配置する
  public void LevelRemap(PuzzleLevel level)
  {
    var cells = level.CellList;

    for (var x = 0; x < level.CellHorizontalCount(); x++)
    {
      var verticalCells = cells.Where(_ => _.IndexX == x).OrderBy(_ => _.IndexY).ToList();
      for (var y = 0; y < level.CellVerticalCount(); y++)
      {
        var cell = verticalCells[y];
        if (cell.CellType != PuzzleLevelMaster.CellTypeEnum.VOID) { continue; }

        // 入れ替え対象を抽出
        for (var replaceY = y + 1; replaceY < verticalCells.Count; replaceY++)
        {
          var replacementCell = verticalCells[replaceY];
          if (replacementCell.CellType != PuzzleLevelMaster.CellTypeEnum.VOID)
          {
            cell.UpdateCellType(replacementCell.CellType);
            replacementCell.UpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
            break;
          }
        }
      }
    }
  }

  public enum GameStateEnum
  {
    Wait = 0, Idle, GameClear, GameOver,
  }
  public GameStateEnum GameState(PuzzleLevel level)
  {
    var cells = level.CellList;
    var voidAll = true;
    foreach(var cell in cells)
    {
      if (cell.CellType == PuzzleLevelMaster.CellTypeEnum.VOID) { continue; }

      voidAll = false;

      // 一つでも連結セルが見つかったらゲーム有効

      var lIndexX = cell.IndexX - 1;
      PuzzleCell lCell = (lIndexX < 0) ? null : cells.First(_ => _.IndexX == lIndexX && _.IndexY == cell.IndexY);
      if (lCell != null && lCell.CellType == cell.CellType) { return GameStateEnum.Idle; }

      var rIndexX = cell.IndexX + 1;
      PuzzleCell rCell = (rIndexX < 0) ? null : cells.First(_ => _.IndexX == rIndexX && _.IndexY == cell.IndexY);
      if (rCell != null && rCell.CellType == cell.CellType) { return GameStateEnum.Idle; }

      var uIndexY = cell.IndexY - 1;
      PuzzleCell uCell = (uIndexY < 0) ? null : cells.First(_ => _.IndexX == cell.IndexX && _.IndexY == uIndexY);
      if (uCell != null && uCell.CellType == cell.CellType) { return GameStateEnum.Idle; }

      var tIndexY = cell.IndexY + 1;
      PuzzleCell tCell = (tIndexY < 0) ? null : cells.First(_ => _.IndexX == cell.IndexX && _.IndexY == tIndexY);
      if (tCell != null && tCell.CellType == cell.CellType) { return GameStateEnum.Idle; }
    }

    // 全てVOIDセルならゲームクリア、連結セルが一つもないならゲームオーバー
    return voidAll ? GameStateEnum.GameClear : GameStateEnum.GameOver;
  }

  public void ToGameClear()
  {
    gameClearUI.SetActive(true);
  }

  public void ToGameOver()
  {
    gameOverUI.SetActive(true);
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
