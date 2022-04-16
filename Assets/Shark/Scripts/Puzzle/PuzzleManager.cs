using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : SingletonMonoBehaviour<PuzzleManager>
{
  [SerializeField] List<PuzzleLevel> puzzleLevelPrefabList;
  [SerializeField] List<PuzzleCell> puzzleCellPrefabList;
  public List<PuzzleCell> PuzzleCellPrefabList { get { return puzzleCellPrefabList; } }
  [SerializeField] List<PuzzleLevelMaster> puzzleLevelMasterList;

  int _currentLevel = 0;

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

    var ground = Instantiate(groundPrefab, this.transform);
    ground.Init(puzzleLevelMasterList[_currentLevel]);
  }

  private void Start()
  {
    DeployCurrentLevel();
  }


}
