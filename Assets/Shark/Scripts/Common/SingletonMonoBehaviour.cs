using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T _instance;

  public static T Instance
  {
    get { return _instance ?? (_instance = FindObjectOfType<T>() as T); }
  }

  protected void OnDestroy()
  {
    if (_instance == this)
    {
      _instance = null;
    }
  }
}
