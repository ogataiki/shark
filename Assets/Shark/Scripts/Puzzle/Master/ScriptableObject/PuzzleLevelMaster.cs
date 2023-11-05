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
  [Header("セル種類 STONEは指定しなくても必ず含みます")]
  [SerializeField]
  List<CellTypeEnum> CellTypeList = new List<CellTypeEnum>();
  public enum CellTypeEnum
  {
    RANDOM = -1,
    VOID = 0,
    STONE = 1,
    TEST_PLAIN, TEST_RED, TEST_GREEN, TEST_YELLOW,
    MACARON, CHERRY, CAKE,
  }
  List<CellTypeEnum> CellTypeListCache = new List<CellTypeEnum>();
  public List<CellTypeEnum> GetCellTypeList()
  {
    if (CellTypeList.Count <= 0)
    {
      Debug.LogError($"セル種別の設定が一つもありません");
      return CellTypeListCache;
    }
    if (CellTypeListCache.Count <= 0)
    {
      CellTypeListCache = new List<CellTypeEnum>();

      // ランダム指定の場合に使えるセル種類の取得
      var randomCells = new List<CellTypeEnum>();
      foreach (CellTypeEnum Value in Enum.GetValues(typeof(CellTypeEnum)))
      {
        if (Value == CellTypeEnum.RANDOM) { continue; }
        if (Value == CellTypeEnum.VOID) { continue; }
        if (Value == CellTypeEnum.STONE) { continue; }
        if (Value == CellTypeEnum.TEST_PLAIN) { continue; }
        if (Value == CellTypeEnum.TEST_RED) { continue; }
        if (Value == CellTypeEnum.TEST_YELLOW) { continue; }
        if (Value == CellTypeEnum.TEST_GREEN) { continue; }
        randomCells.Add(Value);
      }

      var randomPool = new List<CellTypeEnum>(randomCells);
      foreach (var type in CellTypeList)
      {
        if (type == CellTypeEnum.RANDOM)
        {
          if (randomPool.Count > 0)
          {
            var randomCell = randomPool[UnityEngine.Random.Range(0, randomPool.Count)];
            randomPool.Remove(randomCell);
            CellTypeListCache.Add(randomCell);
          }
          else
          {
            var randomCell = randomCells[UnityEngine.Random.Range(0, randomCells.Count)];
            CellTypeListCache.Add(randomCell);
          }
        }
        else
        {
          CellTypeListCache.Add(type);
        }
      }
    }
    return CellTypeListCache;
  }
  public CellTypeEnum GetRandomCellType()
  {
    var cellTypeList = GetCellTypeList();
    if (cellTypeList.Count <= 0)
    {
      Debug.LogError($"セル種別の設定が一つもありません");
      return CellTypeEnum.VOID;
    }
    var list = cellTypeList
      .Where(_ => _ != CellTypeEnum.STONE)
      .Where(_ => _ != CellTypeEnum.VOID)
      .Where(_ => _ != CellTypeEnum.RANDOM).ToList();
    return list[UnityEngine.Random.Range(0, list.Count)];
  }
  public List<CellTypeEnum> CreateRandomCellTypes()
  {
    var cellTypeList = GetCellTypeList();
    var totalCellNum = GetCellCountNum();
    var totalStoneCellNum = UnityEngine.Random.Range(StoneCountMin, StoneCountMax);

    // 最初に全色2個を保証する
    var requireCellTypes = new List<CellTypeEnum>();
    foreach(var requireType in cellTypeList)
    {
      requireCellTypes.Add(requireType);
      requireCellTypes.Add(requireType);

      // 原石セルは必須数を指定されているので調整
      if (requireType == CellTypeEnum.STONE)
      {
        for (int i = 2; i < totalStoneCellNum; i++)
        {
          requireCellTypes.Add(requireType);
        }
      }
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
  CellCountEnum CellCount;
  public enum CellCountEnum
  {
    RANDOM = 0, EASY63, LIGHT80, NORMAL99, HARD120,
  }
  CellCountEnum CellCountCache = CellCountEnum.RANDOM;
  public CellCountEnum GetCellCount()
  {
    if (CellCount != CellCountEnum.RANDOM)
    {
      CellCountCache = CellCount;
      return CellCount;
    }

    if (CellCountCache == CellCountEnum.RANDOM)
    {
      var randomList = new List<CellCountEnum>();
      foreach (CellCountEnum Value in Enum.GetValues(typeof(CellCountEnum)))
      {
        if (Value == CellCountEnum.RANDOM) { continue; }
        randomList.Add(Value);
      }
      CellCountCache = randomList[UnityEngine.Random.Range(0, randomList.Count)];
    }
    return CellCountCache;
  }
  public int GetCellCountNum()
  {
    var cellCount = GetCellCount();
    switch (cellCount)
    {
      case CellCountEnum.EASY63: return 7 * 9;
      case CellCountEnum.LIGHT80: return 8 * 10;
      case CellCountEnum.NORMAL99: return 9 * 11;
      case CellCountEnum.HARD120: return 10 * 12;
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

  // ?セルの数
  [SerializeField]
  public int QCount = 0;

  // STONEセルの数
  [SerializeField]
  public int StoneCountMin = 2;
  public int StoneCountMax = 10;

  public void ClearCache()
  {
    CellTypeListCache.Clear();
    CellCountCache = CellCountEnum.RANDOM;
  }
}
