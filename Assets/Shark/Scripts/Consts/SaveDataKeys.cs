using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataKeys
{
  public const string ScoreDataKeyHeader = "ScoreData";
  public enum ScoreDataKeyTypeEnum
  {
    TopScore = 0,
    Ranking,
  }
  public static string GetScoreDataKey(ScoreDataKeyTypeEnum type)
  {
    return $"{ScoreDataKeyHeader}{type}";
  }
}
