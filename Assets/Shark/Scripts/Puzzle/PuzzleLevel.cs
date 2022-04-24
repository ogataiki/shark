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
  [SerializeField] Transform cellParent;
  [SerializeField] PuzzleCell cellPrefab;
  [SerializeField] float cellOffset;
  [SerializeField] int cellHorizontalCount;
  [SerializeField] int cellVerticalCount;
  [SerializeField] float cellScale = 1f;

  List<PuzzleCell> _cellList = new List<PuzzleCell>();
  public List<PuzzleCell> CellList { get { return _cellList; } }

  PuzzleLevelMaster _levelMasterData;
  public PuzzleLevelMaster LevelMasterData { get { return _levelMasterData; } }

  public UnityEvent<PuzzleLevel, PuzzleCell> onCellClick;

  public int CellHorizontalCount { get { return cellHorizontalCount; } }
  public int CellVerticalCount { get { return cellVerticalCount; } }

  public void Init(PuzzleLevelMaster level)
  {
    _levelMasterData = level;

    DestroyCell();
    DeployCell();
  }

  public void DeployCell()
  {
    var randomCellTypes = _levelMasterData.CreateRandomCellTypes();
    for (var y = 0; y < CellVerticalCount; y++)
    {
      var positionY = (((CellVerticalCount / 2) - CellVerticalCount) + y + (CellVerticalCount % 2)) * cellOffset;
      if ((CellVerticalCount % 2) == 0)
      {
        positionY += (cellOffset * 0.5f);
      }
      for (var x = 0; x < CellHorizontalCount; x++)
      {
        var positionX = (((CellHorizontalCount / 2) - CellHorizontalCount) + x + (CellHorizontalCount % 2)) * cellOffset;
        if ((CellHorizontalCount % 2) == 0)
        {
          positionX += (cellOffset * 0.5f);
        }

        var cellType = randomCellTypes[((1 * y) * CellHorizontalCount) + x];
        var cell = Instantiate(cellPrefab, cellParent);
        cell.gameObject.SetActive(true);
        cell.Init(cellType, x, y, cellScale);
        cell.transform.localPosition = new Vector3(positionX, positionY, 0f);
        cell.onClick.RemoveAllListeners();
        cell.onClick.AddListener(OnClickCell);
        _cellList.Add(cell);
      }
    }
  }

  public void DestroyCell()
  {
    foreach(var cell in _cellList)
    {
      Destroy(cell.gameObject);
    }
    _cellList.Clear();
  }

  public async UniTask<bool> ToVoid(PuzzleCell baseCell)
  {
    var chainCells = GetChainCells(baseCell);
    if (chainCells.Count < 2)
    {
      return false;
    }

    // 消す
    var tasks = new List<UniTask>();
    foreach (var cell in chainCells)
    {
      var task = ToVoidCell(cell);
      tasks.Add(task);
    }
    await UniTask.WhenAll(tasks);

    return true;
  }
  public UniTask ToVoidCell(PuzzleCell cell)
  {
    cell.PreUpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
    cell.FireUpdateCellType();
    return UniTask.WaitUntil(() => cell.IsIdle());
  }

  // 消えた後に再配置する
  public async UniTask LevelRemap()
  {
    // 縦軸を左から順に再配置
    var verticalTasks = new List<UniTask>();
    for (var x = 0; x < CellHorizontalCount; x++)
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
    var updateCells = new List<PuzzleCell>();

    var verticalCells = CellList.Where(_ => _.IndexX == x).OrderBy(_ => _.IndexY).ToList();
    for (var y = 0; y < CellVerticalCount; y++)
    {
      var cell = verticalCells[y];
      if (cell.CellTypeWaitUpdate != PuzzleLevelMaster.CellTypeEnum.VOID) { continue; }

      // 入れ替え対象を抽出
      for (var replaceY = y + 1; replaceY < verticalCells.Count; replaceY++)
      {
        var replacementCell = verticalCells[replaceY];
        if (replacementCell.CellTypeWaitUpdate != PuzzleLevelMaster.CellTypeEnum.VOID)
        {
          cell.PreUpdateCellType(replacementCell.CellTypeWaitUpdate);
          replacementCell.PreUpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
          updateCells.Add(cell);
          updateCells.Add(replacementCell);
          break;
        }
      }
    }

    var tasks = new List<UniTask>();
    foreach (var updateCell in updateCells)
    {
      updateCell.FireUpdateCellType();
      var task = UniTask.WaitUntil(() => updateCell.IsIdle());
      tasks.Add(task);
    }
    return tasks;
  }

  // 全てVOIDの縦軸があれば左に寄せる処理
  List<UniTask> LevelRemapVerticalAllVoid()
  {
    var updateCells = new List<PuzzleCell>();
    var allVoidCount = 0;
    for (var x = 0; x + allVoidCount < CellHorizontalCount;)
    {
      var verticalCells = CellList.Where(_ => _.IndexX == x).OrderBy(_ => _.IndexY).ToList();
      if (verticalCells.Count(_ => _.CellTypeWaitUpdate == PuzzleLevelMaster.CellTypeEnum.VOID) >= CellVerticalCount)
      {
        for (var overwrittenX = x; overwrittenX + 1 < CellHorizontalCount; overwrittenX++)
        {
          var replaceX = overwrittenX + 1;
          var overwrittenVerticalCells = CellList.Where(_ => _.IndexX == overwrittenX).OrderBy(_ => _.IndexY).ToList();
          var replacementVerticalCells = CellList.Where(_ => _.IndexX == replaceX).OrderBy(_ => _.IndexY).ToList();
          for (var y = 0; y < CellVerticalCount; y++)
          {
            var overwrittenCell = overwrittenVerticalCells[y];
            var replacementCell = replacementVerticalCells[y];
            overwrittenCell.PreUpdateCellType(replacementCell.CellTypeWaitUpdate);
            replacementCell.PreUpdateCellType(PuzzleLevelMaster.CellTypeEnum.VOID);
            updateCells.Add(overwrittenCell);
            updateCells.Add(replacementCell);
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
    foreach (var updateCell in updateCells)
    {
      updateCell.FireUpdateCellType();
      var task = UniTask.WaitUntil(() => updateCell.IsIdle());
      tasks.Add(task);
    }
    return tasks;
  }

  //=====================
  // 連結セルの取得
  //=====================
  HashSet<PuzzleCell> _chainCellsCache = new HashSet<PuzzleCell>();

  // ベースのセルを起点に連結範囲を探索して取得
  public HashSet<PuzzleCell> GetChainCells(PuzzleCell baseCell)
  {
    _chainCellsCache.Clear();
    _chainCellsCache.Add(baseCell);
    GetChainCellCross(baseCell);
    return new HashSet<PuzzleCell>(_chainCellsCache);
  }

  public void GetChainCellCross(PuzzleCell baseCell)
  {
    GetChainCellUp(baseCell, (addCell) => { GetChainCellCross(addCell); });
    GetChainCellDown(baseCell, (addCell) => { GetChainCellCross(addCell); });
    GetChainCellRight(baseCell, (addCell) => { GetChainCellCross(addCell); });
    GetChainCellLeft(baseCell, (addCell) => { GetChainCellCross(addCell); });
  }

  // 右方向の探索
  public void GetChainCellRight(PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellHorizontal(baseCell, 1, addCallback);
  }
  // 左方向の探索
  public void GetChainCellLeft(PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellHorizontal(baseCell, -1, addCallback);
  }
  // 上方向の探索
  public void GetChainCellUp(PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellVertical(baseCell, 1, addCallback);
  }
  // 下方向の探索
  public void GetChainCellDown(PuzzleCell baseCell, Action<PuzzleCell> addCallback)
  {
    GetChainCellVertical(baseCell, -1, addCallback);
  }

  // 水平方向の探索
  public void GetChainCellHorizontal(PuzzleCell baseCell, int direction, Action<PuzzleCell> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexX = baseCell.IndexX + direction;
    if (0 <= nextIndexX && nextIndexX < CellHorizontalCount)
    {
      var nextCell = CellList.FirstOrDefault(_ => _.IndexX == nextIndexX && _.IndexY == baseCell.IndexY);
      if (nextCell.CellType == baseCell.CellType)
      {
        if (_chainCellsCache.Any(_ => _.IndexX == nextCell.IndexX && _.IndexY == nextCell.IndexY)) { return; }
        _chainCellsCache.Add(nextCell);
        addCallback?.Invoke(nextCell);
      }
    }
  }
  // 垂直方向の探索
  public void GetChainCellVertical(PuzzleCell baseCell, int direction, Action<PuzzleCell> addCallback)
  {
    if (direction == 0) { return; }
    var nextIndexY = baseCell.IndexY + direction;
    if (0 <= nextIndexY && nextIndexY < CellVerticalCount)
    {
      var nextCell = CellList.FirstOrDefault(_ => _.IndexX == baseCell.IndexX && _.IndexY == nextIndexY);
      if (nextCell.CellType == baseCell.CellType)
      {
        if (_chainCellsCache.Any(_ => _.IndexX == nextCell.IndexX && _.IndexY == nextCell.IndexY)) { return; }
        _chainCellsCache.Add(nextCell);
        addCallback?.Invoke(nextCell);
      }
    }
  }

  public void OnClickCell(PuzzleCell cell)
  {
    onCellClick?.Invoke(this, cell);
  }
}
