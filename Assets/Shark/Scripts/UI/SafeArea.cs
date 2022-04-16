
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// セーフエリアを考慮してCanvas直下のRectTransformのサイズをセーフエリア外の分だけ小さくする
/// </summary>
[DefaultExecutionOrder(-5), RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{

#if UNITY_EDITOR
  private bool _isApply = false;
#endif


  public void ApplySafeArea(int screenWidth, int screenHeight, Rect safeArea)
  {
    var anchorMin = safeArea.min;
    var anchorMax = safeArea.max;

    var invW = 1.0f / screenWidth;
    var invH = 1.0f / screenHeight;

    anchorMin.x *= invW;
    anchorMin.y *= invH;
    anchorMax.x *= invW;
    anchorMax.y *= invH;

    var rect = this.GetComponent<RectTransform>();
    rect.anchorMin = anchorMin;
    rect.anchorMax = anchorMax;

#if UNITY_EDITOR
    _isApply = true;
#endif
  }

  private void ApplySafeAreaIfNeeded()
  {
    Rect safeArea = Screen.safeArea;
    // Note. Xperia 10 Plus で Awake 時に (NaN, NaN, NaN, NaN) が返ってきたため、NaN チェック
    if (!IsSafeAreaValid(safeArea)) { return; }

    if ((Screen.width != (int)safeArea.width) || (Screen.height != (int)safeArea.height))
    {
      this.ApplySafeArea(Screen.width, Screen.height, safeArea);
    }
#if UNITY_EDITOR
    else if (_isApply)
    {
      var rect = this.GetComponent<RectTransform>();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      _isApply = false;
    }
#endif
  }

  private bool IsSafeAreaValid(Rect safeArea)
  {
    if (float.IsNaN(safeArea.xMin)) { return false; }
    if (float.IsNaN(safeArea.yMin)) { return false; }
    if (float.IsNaN(safeArea.xMax)) { return false; }
    if (float.IsNaN(safeArea.yMax)) { return false; }
    return true;
  }

  private void Awake()
  {
    ApplySafeAreaIfNeeded();
  }

  private void Start()
  {
    ApplySafeAreaIfNeeded();
  }

#if UNITY_EDITOR
  private void Update()
  {
    ApplySafeAreaIfNeeded();
  }
#endif
}