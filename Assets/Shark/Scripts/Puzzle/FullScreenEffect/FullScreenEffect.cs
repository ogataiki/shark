using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class FullScreenEffect : MonoBehaviour
{
  [SerializeField] OpenChestAnimation openChest = default;

  public async UniTask PlayOpenChest(bool hit, Action openCallback)
  {
    var anim = Instantiate(openChest, this.transform);
    anim.openEvent.AddListener(() => { openCallback?.Invoke(); });
    anim.destroyEvent.AddListener(() => { DestroyOpenChestAnimation(anim); });
    anim.gameObject.SetActive(true);
    anim.PlayOpenChest(hit);
    await UniTask.WaitUntil(() => anim.Finished);
  }

  public void DestroyOpenChestAnimation(OpenChestAnimation anim)
  {
    Destroy(anim.gameObject);
  }
}
