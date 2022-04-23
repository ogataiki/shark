using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : SingletonMonoBehaviour<PuzzleManager>
{
  [SerializeField] List<PuzzleLevel> puzzleLevelPrefabList;
  [SerializeField] List<PuzzleLevelMaster> puzzleLevelMasterList;


  // UI
  [SerializeField] GameObject gameUI;
  [SerializeField] TextMeshProUGUI currentLevelText;
  [SerializeField] TextMeshProUGUI playCountText;

  [SerializeField] GameObject gameClearUI;
  [SerializeField] Button nextLevelButton;

  [SerializeField] GameObject gameOverUI;
  [SerializeField] Button retryLevelButton;

  int _currentLevel = 0;
  int _playCount = 0;
  PuzzleLevel _currentPuzzleLevel = null;

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

  private void Awake()
  {
    nextLevelButton.onClick.AddListener(OnClickNextLevelButton);
    retryLevelButton.onClick.AddListener(OnClickRetryLevelButton);
  }

  private void Start()
  {
    DeployCurrentLevel();

    GameStart();
    ResetPlayCount();
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
    gameUI.SetActive(true);
    gameClearUI.SetActive(false);
    gameOverUI.SetActive(false);
  }

  // 現在攻略中のレベルを展開
  public void DeployCurrentLevel()
  {
    if (puzzleLevelMasterList.Count <= _currentLevel)
    {
      Debug.LogError($"レベル[{_currentLevel}]の puzzle level master が登録されていません。");
      return;
    }

    var master = puzzleLevelMasterList[_currentLevel];

    if (puzzleLevelPrefabList.Count <= (int)master.CellCount)
    {
      Debug.LogError($"マスタのCellCount[{master.CellCount}]の puzzle ground が登録されていません。");
      return;
    }

    var groundPrefab = puzzleLevelPrefabList[(int)master.CellCount];
    if (groundPrefab == null)
    {
      Debug.LogError($"マスタのCellCount[{master.CellCount}]の puzzle ground が null です。");
      return;
    }

    currentLevelText.text = $"レベル {_currentLevel}";

    _currentPuzzleLevel = Instantiate(groundPrefab, this.transform);
    _currentPuzzleLevel.Init(puzzleLevelMasterList[_currentLevel]);
    _currentPuzzleLevel.onCellClick.RemoveAllListeners();
    _currentPuzzleLevel.onCellClick.AddListener(OnClickCell);
  }
  public void DeployNextLevel()
  {
    _currentLevel += 1;
    _currentLevel = Math.Min(puzzleLevelMasterList.Count, _currentLevel);

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

    if (_state != StateEnum.Idle) { return; }
    if (cell.CellType == PuzzleLevelMaster.CellTypeEnum.VOID) { return; }

    ChangeState(StateEnum.PlayAnimation);

    var task = PlayCell(level, cell);
    task.ContinueWith(() => {
      ChangeState(StateEnum.Idle);
    });
  }

  public async UniTask PlayCell(PuzzleLevel level, PuzzleCell cell)
  {
    var toVoidSuccess = await level.ToVoid(cell);
    if (toVoidSuccess == false)
    {
      // TODO : 消せないよということを表す何らかの表現を入れる
      return;
    }

    AddPlayCount();

    // TODO : 消す演出や消した後に上のやつを下に落とす演出を入れる

    // 盤面整理
    await level.LevelRemap();

    var state = GameState(level);
    switch (state)
    {
      case GameStateEnum.GameClear: ToGameClear(); break;
      case GameStateEnum.GameOver: ToGameOver(); break;
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
      PuzzleCell rCell = (rIndexX >= level.CellHorizontalCount) ? null : cells.First(_ => _.IndexX == rIndexX && _.IndexY == cell.IndexY);
      if (rCell != null && rCell.CellType == cell.CellType) { return GameStateEnum.Idle; }

      var uIndexY = cell.IndexY - 1;
      PuzzleCell uCell = (uIndexY < 0) ? null : cells.First(_ => _.IndexX == cell.IndexX && _.IndexY == uIndexY);
      if (uCell != null && uCell.CellType == cell.CellType) { return GameStateEnum.Idle; }

      var tIndexY = cell.IndexY + 1;
      PuzzleCell tCell = (tIndexY >= level.CellVerticalCount) ? null : cells.First(_ => _.IndexX == cell.IndexX && _.IndexY == tIndexY);
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

  public void AddPlayCount()
  {
    _playCount += 1;
    playCountText.text = $"操作 {_playCount} 回";
  }
  public void ResetPlayCount()
  {
    _playCount = 0;
    playCountText.text = $"操作 {_playCount} 回";
  }
}
