using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    [SerializeField] private List<Cell> listCell;
    private Cell[,] grid;
    private const int GridSize = 8;

    void Start()
    {
    }

    // Hàm nạp vào list
    public void AddCell(Cell cell)
    {
        if (cell != null)
        {
            listCell.Add(cell);
        }
    }

    // Hàm get được list
    public List<Cell> GetCells()
    {
        return listCell;
    }

    // Hàm sửa
    public void EditCell(int index, Cell newCell)
    {
        if (index >= 0 && index < listCell.Count && newCell != null)
        {
            listCell[index] = newCell;
        }
    }

    // Hàm tìm
    public Cell FindCell(int index)
    {
        if (index >= 0 && index < listCell.Count)
        {
            return listCell[index];
        }

        return null;
    }

    // Hàm xóa by index
    public void RemoveCell(int index)
    {
        if (index >= 0 && index < listCell.Count)
        {
            listCell.RemoveAt(index);
        }
    }

    // Hàm xóa by cell
    public void RemoveCell(Cell cell)
    {
        if (cell != null)
        {
            listCell.Remove(cell);
        }
    }
}
