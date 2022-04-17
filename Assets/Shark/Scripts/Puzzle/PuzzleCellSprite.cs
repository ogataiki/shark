using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCellSprite : MonoBehaviour
{
  [SerializeField] PuzzleLevelMaster.CellTypeEnum cellType;
  public PuzzleLevelMaster.CellTypeEnum CellType { get { return cellType; } }

  [SerializeField] EventTriggerHandler eventTriggerHandler;

  public UnityEvent<PuzzleCellSprite> onClick;

  private void Awake()
  {
    eventTriggerHandler.onPointerClickEvent.RemoveAllListeners();
    eventTriggerHandler.onPointerClickEvent.AddListener(OnClick);
  }

  public void OnClick()
  {
    onClick?.Invoke(this);
  }
}
