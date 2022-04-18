using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleLevelMasterData", menuName = "ScriptableObjects/PuzzleLevelMaster")]
public class PuzzleLevelMaster : ScriptableObject
{
  // パズルレベルの設定

  // 色数 最低2は必要 多いほど難しくなる
  // セルの種類
  [SerializeField]
  public List<CellTypeEnum> CellTypeList = new List<CellTypeEnum>();
  public enum CellTypeEnum
  {
    VOID = 0,
    TEST_PLAIN, TEST_RED, TEST_GREEN, TEST_YELLOW,
  }
  public CellTypeEnum GetRandomCellType()
  {
    return CellTypeList[UnityEngine.Random.Range(0, CellTypeList.Count)];
  }
  public List<CellTypeEnum> CreateRandomCellTypes()
  {
    var totalCellNum = GetCellCountNum();

    // 最初に全色2個を保証する
    var requireCellTypes = new List<CellTypeEnum>();
    foreach(var requireType in CellTypeList)
    {
      requireCellTypes.Add(requireType);
      requireCellTypes.Add(requireType);
    }

    // 難易度を考慮したランダム配置を生成する
    var randomCellNum = totalCellNum - requireCellTypes.Count;
    var ramdomCellTypes = new List<CellTypeEnum>();
    var currentCellType = GetRandomCellType();
    for (var num = randomCellNum; num > 0; num--)
    {
      ramdomCellTypes.Add(currentCellType);
      if (LotCellChange())
      {
        currentCellType = GetRandomCellType();
      }
    }

    // 必須セルをランダムに分配
    foreach(var requireType in requireCellTypes)
    {
      var randomIndex = UnityEngine.Random.Range(0, ramdomCellTypes.Count);
      ramdomCellTypes.Insert(randomIndex, requireType);
    }

    return ramdomCellTypes;
  }

  // 縦横セル数の合計 Levelプレハブを選択するのに使う
  // 横7,縦9の63セルが基本
  // もっと簡単なら少なく、もっと難しいなら多い物を用意する
  [SerializeField]
  public CellCountEnum CellCount;
  public enum CellCountEnum
  {
    PLAIN = 0, EASY63, NORMAL99,
  }
  public int GetCellCountNum()
  {
    switch(CellCount)
    {
      case CellCountEnum.PLAIN: return 7 * 9;
      case CellCountEnum.EASY63: return 7 * 9;
      case CellCountEnum.NORMAL99: return 9 * 11;
      default: return 7 * 9;
    }
  }

  // セル配置時の複雑さ 同じセルがどれだけ並びやすくなるかの値 大きいほど難しい
  // 1000で100%切り替わる(毎セル配置時にセルの抽選を行う)
  // 1で0.1%で切り替わる(ほとんど同じセルを並べ続ける)
  [SerializeField, Range(COMPLEXITY_MIN, COMPLEXITY_MAX)]
  public int Complexity;
  public const int COMPLEXITY_MIN = 1;
  public const int COMPLEXITY_MAX = 1000;
  public bool LotCellChange()
  {
    var seed = UnityEngine.Random.Range(COMPLEXITY_MIN, COMPLEXITY_MAX + 1);
    return (seed <= Complexity);
  }
}
