using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleLevel : MonoBehaviour
{
  [SerializeField] Transform slotParent;
  [SerializeField] PuzzleSlot slotPrefab;
  [SerializeField] PuzzleCell cellPrefab;
  [SerializeField] float slotOffset;
  [SerializeField] int slotHorizontalCount;
  [SerializeField] int slotVerticalCount;
  [SerializeField] Transform cellParent;
  [SerializeField] float cellScale = 1f;

  List<PuzzleSlot> _slotList = new List<PuzzleSlot>();
  public List<PuzzleSlot> SlotList { get { return _slotList; } }

  List<PuzzleCell> _cellList = new List<PuzzleCell>();
  public List<PuzzleCell> CellList { get { return _cellList; } }

  PuzzleLevelMaster _levelMasterData;
  public PuzzleLevelMaster LevelMasterData { get { return _levelMasterData; } }

  public UnityEvent<PuzzleLevel, PuzzleSlot> onClickSlot;

  public int SlotHorizontalCount { get { return slotHorizontalCount; } }
  public int SlotVerticalCount { get { return slotVerticalCount; } }

  public void Init(PuzzleLevelMaster level)
  {
    _levelMasterData = level;

    DestroyLevel();
    DeployLevel();
  }

  public void DeployLevel()
  {
    var randomCellTypes = _levelMasterData.CreateRandomCellTypes();
    for (var y = 0; y < SlotVerticalCount; y++)
    {
      var positionY = (((SlotVerticalCount / 2) - SlotVerticalCount) + y + (SlotVerticalCount % 2)) * slotOffset;
      if ((SlotVerticalCount % 2) == 0)
      {
        positionY += (slotOffset * 0.5f);
      }
      for (var x = 0; x < SlotHorizontalCount; x++)
      {
        var positionX = (((SlotHorizontalCount / 2) - SlotHorizontalCount) + x + (SlotHorizontalCount % 2)) * slotOffset;
        if ((SlotHorizontalCount % 2) == 0)
        {
          positionX += (slotOffset * 0.5f);
        }

        var cellType = randomCellTypes[((1 * y) * SlotHorizontalCount) + x];

        var slot = Instantiate(slotPrefab, slotParent);
        slot.Init(x, y);
        slot.transform.localPosition = new Vector3(positionX, positionY, 0f);
        slot.onClick.AddListener(OnClickSlot);
        _slotList.Add(slot);

        var cell = Instantiate(cellPrefab, cellParent);
        cell.gameObject.SetActive(true);
        cell.Init(cellType, cellScale, slot);
        cell.onClick.RemoveAllListeners();
        _cellList.Add(cell);

        slot.PutOnCell(cell);
      }
    }
  }

  public void DestroyLevel()
  {
    foreach(var cell in _cellList)
    {
      Destroy(cell.gameObject);
    }
    _cellList.Clear();
    foreach (var slot in _slotList)
    {
      Destroy(slot.gameObject);
    }
    _slotList.Clear();
  }

  public async UniTask<bool> ToVoid(PuzzleSlot baseSlot)
  {
    var chainSlots = GetChainSlots(baseSlot);
    if (chainSlots.Count < 2)
    {
      return false;
    }

    // 消す
    var tasks = new List<UniTask>();
    foreach (var slot in chainSlots)
    {
      var task = slot.ToVoid();
      tasks.Add(task);
    }
    await UniTask.WhenAll(tasks);

    return true;
  }

  // 消えた後に再配置する
  public async UniTask LevelRemap()
  {
    // 縦軸を左から順に再配置
    var verticalTasks = new List<UniTask>();
    for (var x = 0; x < SlotHorizontalCount; x++)
    {
      var verticalTask = LevelRemapVertical(x);
      verticalTasks.AddRange(verticalTask);
    }
    await UniTask.WhenAll(verticalTasks);

    // 全てVOIDの縦軸があれば左に寄せる処理
    var verticalAllVoidTasks = LevelRemapVerticalAllVoid();
    await UniTask.WhenAll(verticalAllVoidTasks);
  }

  // 縦一列の再配置
  List<UniTask> LevelRemapVertical(int x)
  {
    var updateSlots = new List<PuzzleSlot>();

    var verticalSlots = SlotList.Where(_ => _.IndexX == x).OrderBy(_ => _.IndexY).ToList();
    for (var y = 0; y < SlotVerticalCount; y++)
    {
      var slot = verticalSlots[y];
      if (slot.CellTypeWaitUpdate != PuzzleLevelMaster.CellTypeEnum.VOID) { continue; }

      // 入れ替え対象を抽出
      for (var replaceY = y + 1; replaceY < verticalSlots.Count; replaceY++)
      {
        var replacementSlot = verticalSlots[replaceY];
        if (replacementSlot.CellTypeWaitUpdate != PuzzleLevelMaster.CellTypeEnum.VOID)
        {
          var slotBeforCell = slot.CellWaitUpdate;
          slot.PreMoveCell(replacementSlot.CellWaitUpdate);
          replacementSlot.PreMoveCell(slotBeforCell);
          updateSlots.Add(slot);
          updateSlots.Add(replacementSlot);
          break;
        }
      }
    }

    var tasks = new List<UniTask>();
    foreach (var updateSlot in updateSlots)
    {
      updateSlot.FireMoveCell();
      var task = UniTask.WaitUntil(() => updateSlot.IsIdle());
      tasks.Add(task);
    }
    return tasks;
  }

  // 全てVOIDの縦軸があれば左に寄せる処理
  List<UniTask> LevelRemapVerticalAllVoid()
  {
    var updateSlots = new List<PuzzleSlot>();
    var allVoidCount = 0;
    for (var x = 0; x + allVoidCount < SlotHorizontalCount;)
    {
      var verticalSlots = SlotList.Where(_ => _.IndexX == x).OrderBy(_ => _.IndexY).ToList();
      if (verticalSlots.Count(_ => _.CellTypeWaitUpdate == PuzzleLevelMaster.CellTypeEnum.VOID) >= SlotVerticalCount)
      {
        for (var overwrittenX = x; overwrittenX + 1 < SlotHorizontalCount; overwrittenX++)
        {
          var replaceX = overwrittenX + 1;
          var overwrittenVerticalSlots = SlotList.Where(_ => _.IndexX == overwrittenX).OrderBy(_ => _.IndexY).ToList();
          var replacementVerticalSlots = SlotList.Where(_ => _.IndexX == replaceX).OrderBy(_ => _.IndexY).ToList();
          for (var y = 0; y < SlotVerticalCount; y++)
          {
            var overwrittenSlot = overwrittenVerticalSlots[y];
            var replacementSlot = replacementVerticalSlots[y];
            var overwrittenSlotBeforCell = overwrittenSlot.CellWaitUpdate;
            overwrittenSlot.PreMoveCell(replacementSlot.CellWaitUpdate);
            replacementSlot.PreMoveCell(overwrittenSlotBeforCell);
            updateSlots.Add(overwrittenSlot);
            updateSlots.Add(replacementSlot);
          }
        }
        allVoidCount += 1;
      }
      else
      {
        x += 1;
      }
    }

    var tasks = new List<UniTask>();
    foreach (var updateSlot in updateSlots)
    {
      updateSlot.FireMoveCell();
      var task = UniTask.WaitUntil(() => updateSlot.IsIdle());
      tasks.Add(task);
    }
    return tasks;
  }

  //=====================
  // 連結セルの取得
  //=====================
  HashSet<PuzzleSlot> _chainSlotsCache = new HashSet<PuzzleSlot>();

  // ベースのセルを起点に連結範囲を探索して取得
  public HashSet<PuzzleSlot> GetChainSlots(PuzzleSlot baseSlot)
  {
    _chainSlotsCache.Clear();
    _chainSlotsCache.Add(baseSlot);
    GetChainSlotCross(baseSlot);
    return new HashSet<PuzzleSlot>(_chainSlotsCache);
  }

  public void GetChainSlotCross(PuzzleSlot baseSlot)
  {
    GetChainSlotUp(baseSlot, (addSlot) => { GetChainSlotCross(addSlot); });
    GetChainSlotDown(baseSlot, (addSlot) => { GetChainSlotCross(addSlot); });
    GetChainSlotRight(baseSlot, (addSlot) => { GetChainSlotCross(addSlot); });
    GetChainSlotLeft(baseSlot, (addSlot) => { GetChainSlotCross(addSlot); });
  }

  // 右方向の探索
  public void GetChainSlotRight(PuzzleSlot baseSlot, Action<PuzzleSlot> addSlotback)
  {
    GetChainSlotHorizontal(baseSlot, 1, addSlotback);
  }
  // 左方向の探索
  public void GetChainSlotLeft(PuzzleSlot baseSlot, Action<PuzzleSlot> addSlotback)
  {
    GetChainSlotHorizontal(baseSlot, -1, addSlotback);
  }
  // 上方向の探索
  public void GetChainSlotUp(PuzzleSlot baseSlot, Action<PuzzleSlot> addSlotback)
  {
    GetChainSlotVertical(baseSlot, 1, addSlotback);
  }
  // 下方向の探索
  public void GetChainSlotDown(PuzzleSlot baseSlot, Action<PuzzleSlot> addSlotback)
  {
    GetChainSlotVertical(baseSlot, -1, addSlotback);
  }

  // 水平方向の探索
  public void GetChainSlotHorizontal(PuzzleSlot baseSlot, int direction, Action<PuzzleSlot> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexX = baseSlot.IndexX + direction;
    if (0 <= nextIndexX && nextIndexX < SlotHorizontalCount)
    {
      var nextSlot = SlotList.FirstOrDefault(_ => _.IndexX == nextIndexX && _.IndexY == baseSlot.IndexY);
      if (nextSlot.CellType == baseSlot.CellType)
      {
        if (_chainSlotsCache.Any(_ => _.IndexX == nextSlot.IndexX && _.IndexY == nextSlot.IndexY)) { return; }
        _chainSlotsCache.Add(nextSlot);
        addCallback?.Invoke(nextSlot);
      }
    }
  }
  // 垂直方向の探索
  public void GetChainSlotVertical(PuzzleSlot baseSlot, int direction, Action<PuzzleSlot> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexY = baseSlot.IndexY + direction;
    if (0 <= nextIndexY && nextIndexY < SlotVerticalCount)
    {
      var nextSlot = SlotList.FirstOrDefault(_ => _.IndexX == baseSlot.IndexX && _.IndexY == nextIndexY);
      if (nextSlot.CellType == baseSlot.CellType)
      {
        if (_chainSlotsCache.Any(_ => _.IndexX == nextSlot.IndexX && _.IndexY == nextSlot.IndexY)) { return; }
        _chainSlotsCache.Add(nextSlot);
        addCallback?.Invoke(nextSlot);
      }
    }
  }

  public void OnClickSlot(PuzzleSlot slot)
  {
    onClickSlot?.Invoke(this, slot);
  }
}
