using System;
using System.Collections;
using System.Collections.Generic;
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
    TEST_PLAIN, TEST_RED,
  }
  public CellTypeEnum GetRandomCellType()
  {
    return CellTypeList[UnityEngine.Random.Range(0, CellTypeList.Count)];
  }

  // 縦横セル数の合計 Levelプレハブを選択するのに使う
  // 横7,縦9の63セルが基本
  // もっと簡単なら少なく、もっと難しいなら多い物を用意する
  [SerializeField]
  public CellCountEnum CellCount;
  public enum CellCountEnum
  {
    PLAIN = 0, EASY63,
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
    var seed = UnityEngine.Random.Range(1, Complexity + 1);
    return (seed <= Complexity);
  }
}
