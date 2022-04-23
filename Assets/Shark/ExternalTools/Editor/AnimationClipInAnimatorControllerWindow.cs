using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor.Animations;
using AnimController = UnityEditor.Animations.AnimatorController;

/**
 * AnimatorController内部にAnimationClipを内蔵するための拡張
 * ここにあるやつほぼそのままだが、overrideControllerにも対応できるようにしている
 * http://tsubakit1.hateblo.jp/entry/2015/02/03/232316
 */
public class AnimationClipInAnimatorControllerWindow : EditorWindow
{
  private RuntimeAnimatorController _controller;
  private string _clipName;

  [MenuItem("Assets/CombineAnimationclip")]
  static void Create()
  {
    var window = EditorWindow.GetWindow<AnimationClipInAnimatorControllerWindow>();

    if (Selection.activeObject is AnimController)
    {
      window._controller = Selection.activeObject as RuntimeAnimatorController;
    }
    else if (Selection.activeObject is AnimatorOverrideController)
    {
      window._controller = Selection.activeObject as RuntimeAnimatorController;
    }
  }

  void OnGUI()
  {
    EditorGUILayout.LabelField("target clip");
    _controller = EditorGUILayout.ObjectField(_controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

    if (_controller == null)
    {
      return;
    }

    if (!(_controller is AnimController) && !(_controller is AnimatorOverrideController))
    {
      _controller = null;
      return;
    }

    List<AnimationClip> clipList = new List<AnimationClip>();
    var allAsset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_controller));

    foreach (var asset in allAsset)
    {
      if (asset is AnimationClip)
      {
        var removeClip = asset as AnimationClip;

        if (!clipList.Contains(removeClip))
        {
          clipList.Add(removeClip);
        }
      }
    }

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Add new clip");
    EditorGUILayout.BeginVertical("box");
    _clipName = EditorGUILayout.TextField(_clipName);

    if (clipList.Exists(item => item.name == _clipName) || string.IsNullOrEmpty(_clipName))
    {
      EditorGUILayout.LabelField("can't create duplicate names or empty");
    }
    else
    {
      if (GUILayout.Button("create"))
      {
        AnimationClip animationClip = AnimController.AllocateAnimatorClip(_clipName);
        AssetDatabase.AddObjectToAsset(animationClip, _controller);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_controller));
        AssetDatabase.Refresh();
      }
    }

    EditorGUILayout.EndVertical();

    if (clipList.Count == 0)
    {
      return;
    }

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("remove clip");
    EditorGUILayout.BeginVertical("box");

    foreach (var removeClip in clipList)
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(removeClip.name);

      if (GUILayout.Button("remove", GUILayout.Width(100)))
      {
        Object.DestroyImmediate(removeClip, true);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_controller));
      }

      EditorGUILayout.EndHorizontal();
    }

    EditorGUILayout.EndVertical();
  }
}