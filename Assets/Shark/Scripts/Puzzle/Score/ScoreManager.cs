using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreManager
{
  private static ScoreManager _instance;

  private ScoreManager()
  {
  }

  public static ScoreManager Instance
  {
    get
    {
      if (_instance == null) _instance = new ScoreManager();
      return _instance;
    }
  }

  public void Init()
  {
    LoadScoreData();
    ClearCurrentScoreCache();
  }

  public void LoadScoreData()
  {
    LoadTopScore();
    LoadScoreRanking();
  }

  // =========
  // 最高スコア
  // =========
  ScoreData _topScore;
  public ScoreData TopScore
  {
    get
    {
      if (_topScore == null) { _topScore = new ScoreData(); }
      return _topScore;
    }
  }
  public void SetTopScore(ScoreData scoreData)
  {
    _topScore = scoreData;
  }
  public void SaveTopScore()
  {
    ScoreData.Save(_topScore, SaveDataKeys.ScoreDataKeyTypeEnum.TopScore);
  }
  public void LoadTopScore()
  {
    _topScore = ScoreData.Load(SaveDataKeys.ScoreDataKeyTypeEnum.TopScore);
  }

  // ================
  // ランキング
  // ================
  int _scoreRankingMax = 10;
  List<ScoreData> _scoreRanking = new List<ScoreData>();
  public List<ScoreData> ScoreRanking
  {
    get
    {
      return _scoreRanking;
    }
  }
  public void AddScoreRanking(ScoreData scoreData)
  {
    _scoreRanking.Add(scoreData);
    _scoreRanking = _scoreRanking.OrderByDescending(_ => _.GetScore()).ToList();
    if (_scoreRanking.Count > _scoreRankingMax)
    {
      _scoreRanking.RemoveAt(_scoreRanking.Count - 1);
    }
  }
  public void SaveScoreRanking()
  {
    for(var i = 0; i < ScoreRanking.Count; i++)
    {
      var ranking = ScoreRanking[i];
      ScoreData.Save(ranking, SaveDataKeys.ScoreDataKeyTypeEnum.Ranking, i.ToString());
    }
  }
  public void LoadScoreRanking()
  {
    var load = true;
    var i = 0;
    do
    {
      var ranking = ScoreData.Load(SaveDataKeys.ScoreDataKeyTypeEnum.Ranking, i.ToString());
      if (ranking == null)
      {
        load = false;
        break;
      }
      ScoreRanking.Add(ranking);
      i += 1;
    } while (load);
    _scoreRanking = _scoreRanking.OrderByDescending(_ => _.GetScore()).ToList();
  }

  // ===================
  // プレイ中ゲームのスコア
  // ===================
  ScoreData _currentScoreCache;
  public ScoreData CurrentScoreCache
  {
    get
    {
      if (_currentScoreCache == null) { _currentScoreCache = new ScoreData(); }
      return _currentScoreCache;
    }
  }
  public void ClearCurrentScoreCache()
  {
    _currentScoreCache = null;
  }
}
