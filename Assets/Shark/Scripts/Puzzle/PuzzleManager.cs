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

  int _currentLevel = 1;
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

    foreach(var master in puzzleLevelMasterList)
    {
      master.ClearCache();
    }
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

#if DEBUG
    DebugGameStart();
#endif
  }

  // 現在攻略中のレベルを展開
  public void DeployCurrentLevel()
  {
    if (_currentPuzzleLevel != null)
    {
      Destroy(_currentPuzzleLevel.gameObject);
      _currentPuzzleLevel = null;
    }

    if (puzzleLevelMasterList.Count <= _currentLevel)
    {
      Debug.LogError($"レベル[{_currentLevel}]の puzzle level master が登録されていません。");
      return;
    }

    var master = puzzleLevelMasterList[_currentLevel];
    master.ClearCache();

    if (puzzleLevelPrefabList.Count <= (int)master.GetCellCount())
    {
      Debug.LogError($"マスタのCellCount[{master.GetCellCount()}]の puzzle ground が登録されていません。");
      return;
    }

    var groundPrefab = puzzleLevelPrefabList[(int)master.GetCellCount()];
    if (groundPrefab == null)
    {
      Debug.LogError($"マスタのCellCount[{master.GetCellCount()}]の puzzle ground が null です。");
      return;
    }

    currentLevelText.text = $"レベル {_currentLevel}";

    _currentPuzzleLevel = Instantiate(groundPrefab, this.transform);
    _currentPuzzleLevel.Init(puzzleLevelMasterList[_currentLevel]);
    _currentPuzzleLevel.onClickSlot.RemoveAllListeners();
    _currentPuzzleLevel.onClickSlot.AddListener(OnClickSlot);
  }
  public void DeployNextLevel()
  {
    _currentLevel += 1;
    _currentLevel = Math.Min(puzzleLevelMasterList.Count-1, _currentLevel);

    DeployCurrentLevel();
  }
  public void ResetCurrentLevel()
  {
    DeployCurrentLevel();
  }

  public void OnClickSlot(PuzzleLevel level, PuzzleSlot slot)
  {
    Debug.Log($"[PuzzleManager] OnClickSlot[{level.LevelMasterData.GetCellCount()}, {slot.CellType}]");

    if (_state != StateEnum.Idle) { return; }
    if (slot.CellType == PuzzleLevelMaster.CellTypeEnum.VOID) { return; }

    ChangeState(StateEnum.PlayAnimation);

    var task = PlayCell(level, slot);
    task.ContinueWith(() => {
      ChangeState(StateEnum.Idle);
    });
  }

  public async UniTask PlayCell(PuzzleLevel level, PuzzleSlot slot)
  {
    var toVoidSuccess = await level.ToVoid(slot);
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
    var slots = level.SlotList;
    var voidAll = true;
    foreach(var slot in slots)
    {
      if (slot.CellType == PuzzleLevelMaster.CellTypeEnum.VOID) { continue; }

      voidAll = false;

      // 一つでも連結セルが見つかったらゲーム有効

      var lIndexX = slot.IndexX - 1;
      PuzzleSlot lSlot = (lIndexX < 0) ? null : slots.First(_ => _.IndexX == lIndexX && _.IndexY == slot.IndexY);
      if (lSlot != null && lSlot.CellType == slot.CellType) { return GameStateEnum.Idle; }

      var rIndexX = slot.IndexX + 1;
      PuzzleSlot rSlot = (rIndexX >= level.SlotHorizontalCount) ? null : slots.First(_ => _.IndexX == rIndexX && _.IndexY == slot.IndexY);
      if (rSlot != null && rSlot.CellType == slot.CellType) { return GameStateEnum.Idle; }

      var uIndexY = slot.IndexY - 1;
      PuzzleSlot uSlot = (uIndexY < 0) ? null : slots.First(_ => _.IndexX == slot.IndexX && _.IndexY == uIndexY);
      if (uSlot != null && uSlot.CellType == slot.CellType) { return GameStateEnum.Idle; }

      var tIndexY = slot.IndexY + 1;
      PuzzleSlot tSlot = (tIndexY >= level.SlotVerticalCount) ? null : slots.First(_ => _.IndexX == slot.IndexX && _.IndexY == tIndexY);
      if (tSlot != null && tSlot.CellType == slot.CellType) { return GameStateEnum.Idle; }
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


#if DEBUG

  // Debug UI
  [SerializeField] GameObject debugUI;
  [SerializeField] Button debugLevelReloadButton;
  [SerializeField] Button debugClearButton;
  [SerializeField] Button debugLevelResetButton;
  [SerializeField] Button debugLevelZeroButton;

  public void DebugGameStart()
  {
    debugUI.SetActive(true);

    debugClearButton.onClick.RemoveAllListeners();
    debugClearButton.onClick.AddListener(DeployNextLevel);
    debugLevelReloadButton.onClick.RemoveAllListeners();
    debugLevelReloadButton.onClick.AddListener(DeployCurrentLevel);
    debugLevelResetButton.onClick.RemoveAllListeners();
    debugLevelResetButton.onClick.AddListener(() => {
      _currentLevel = 1;
      DeployCurrentLevel();
    });
    debugLevelZeroButton.onClick.RemoveAllListeners();
    debugLevelZeroButton.onClick.AddListener(() => {
      _currentLevel = 0;
      DeployCurrentLevel();
    });
  }

#endif


}
