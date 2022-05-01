using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;

[Serializable]
public class ScoreData
{
  [Serializable]
  public class ToVoid
  {
    public PuzzleLevelMaster.CellTypeEnum type;
    public int count;
  }

  [Serializable]
  public class PlayData
  {
    public int level;
    public int count;
    public List<ToVoid> toVoidList;
    public int score;

    public void AddToVoid(PuzzleLevelMaster.CellTypeEnum type, int voidCount)
    {
      count += 1;
      var toVoid = GetToVoid(type);
      toVoid.count += voidCount;
      score += (int)Math.Pow(voidCount, 1.2) * level;
    }

    public ToVoid GetToVoid(PuzzleLevelMaster.CellTypeEnum type)
    {
      var toVoid = toVoidList.FirstOrDefault(_ => _.type == type);
      if (toVoid == null)
      {
        toVoid = new ToVoid
        {
          type = type,
          count = 0,
        };
        toVoidList.Add(toVoid);
      }
      return toVoid;
    }

    public void MergeToVoid(ToVoid toVoid)
    {
      var targetToVoid = GetToVoid(toVoid.type);
      targetToVoid.count += toVoid.count;
    }

    public int SumToVoidCount()
    {
      return toVoidList.Sum(_ => _.count);
    }
  }

  [SerializeField]
  List<PlayData> _playDataList = new List<PlayData>();
  public List<PlayData> PlayDataList { get { return _playDataList; } }
  public PlayData CreatePlayData(int level)
  {
    var playData = new PlayData
    {
      level = level,
      count = 0,
      toVoidList = new List<ToVoid>(),
    };
    _playDataList.Add(playData);
    return playData;
  }
  public PlayData GetPlayData(int level)
  {
    var playData = _playDataList.FirstOrDefault(_ => _.level == level);
    if (playData == null)
    {
      playData = CreatePlayData(level);
    }
    return playData;
  }
  public void EntryPlayData(int level, PuzzleLevelMaster.CellTypeEnum type, int voidCount)
  {
    var playData = GetPlayData(level);
    playData.AddToVoid(type, voidCount);
  }
  public void MergePlayData(PlayData data)
  {
    var targetData = GetPlayData(data.level);
    targetData.count += data.count;
    foreach(var toVoid in data.toVoidList)
    {
      targetData.MergeToVoid(toVoid);      
    }
  }


  public void MergeScoreData(ScoreData scoreData)
  {
    foreach(var playData in scoreData.PlayDataList)
    {
      MergePlayData(playData);
    }
  }

  public int SumToVoidCount()
  {
    return PlayDataList.Sum(_ => _.SumToVoidCount());
  }

  public int GetScore()
  {
    return PlayDataList.Sum(_ => _.score);
  }

  // ===================
  // 永続化
  // ===================
  const string jsonSaveFileNameHeader = "4g30qn9gjiehn78";
  public static void Save(ScoreData scoreData, SaveDataKeys.ScoreDataKeyTypeEnum type, string addKey = "")
  {
    var key = SaveDataKeys.GetScoreDataKey(type);
    key += addKey;

    var json = JsonUtility.ToJson(scoreData);
    var jsonHash = StringUtil.GetHashMD5(json);

    var jsonSavePath = $"{Application.persistentDataPath}/{jsonSaveFileNameHeader}_{key}";
    StreamWriter streamWriter = new StreamWriter(jsonSavePath);
    streamWriter.Write(json);
    streamWriter.Flush();
    streamWriter.Close();

    var jsonHashSavePath = $"{Application.persistentDataPath}/{jsonSaveFileNameHeader}_{key}H";
    streamWriter = new StreamWriter(jsonHashSavePath);
    streamWriter.Write(jsonHash);
    streamWriter.Flush();
    streamWriter.Close();

    Debug.Log($"{key} のデータを保存しました。");
  }

  public static ScoreData Load(SaveDataKeys.ScoreDataKeyTypeEnum type, string addKey = "")
  {
    var key = SaveDataKeys.GetScoreDataKey(type);
    key += addKey;

    var saveHash = "";
    var jsonHashSavePath = $"{Application.persistentDataPath}/{jsonSaveFileNameHeader}_{key}H";
    if (File.Exists(jsonHashSavePath))
    {
      StreamReader streamReader;
      streamReader = new StreamReader(jsonHashSavePath);
      saveHash = streamReader.ReadToEnd();
      streamReader.Close();
    }

    var json = "";
    var jsonSavePath = $"{Application.persistentDataPath}/{jsonSaveFileNameHeader}_{key}";
    if (File.Exists(jsonSavePath))
    {
      StreamReader streamReader;
      streamReader = new StreamReader(jsonSavePath);
      json = streamReader.ReadToEnd();
      streamReader.Close();
    }

    if (string.IsNullOrEmpty(json)) { return null; }
    if (string.IsNullOrEmpty(saveHash))
    {
      Debug.LogError($"{key} の保存Hashが見つかりません。");
      return null;
    }

    var jsonHash = StringUtil.GetHashMD5(json);
    if (saveHash != jsonHash)
    {
      Debug.LogError($"{key} の保存データが破損しています。");
      return null;
    }

    var scoreData = JsonUtility.FromJson<ScoreData>(json);
    Debug.Log($"{key} の保存データをロードしました。");
    return scoreData;
  }
}
