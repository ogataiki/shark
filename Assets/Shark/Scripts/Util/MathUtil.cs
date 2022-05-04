using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MathUtil
{
  public static IEnumerable<T> LotRandomBox<T>(IEnumerable<T> list, int count)
  {
    var resultList = new List<T>();

    var tempList = new List<T>(list);
    while (tempList.Count() > 0 && resultList.Count < count)
    {
      int index = UnityEngine.Random.Range(0, tempList.Count);

      T value = tempList[index];
      resultList.Add(value);
      tempList.RemoveAt(index);
    }
    return resultList;
  }
}
