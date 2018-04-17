using System.Collections;
using UnityEngine;

public class CalculateHeatMap : MonoBehaviour {
    int gridSizeX, gridSizeY;
    Node[,] grid;
    public Grid mGrid;
    public TBSUnit[] enemies;

    // Use this for initialization
    void Start()
    {
        if(mGrid == null)
            mGrid = gameObject.GetComponent<Grid>();
        grid = mGrid.getNodeArray();
        Vector2 gridSize = mGrid.getGridSize();
        gridSizeX = Mathf.RoundToInt(gridSize.x);
        gridSizeY = Mathf.RoundToInt(gridSize.y);
        setupHeatMap();
    }

    public void setupHeatMap ()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if(grid[x, y].walkable)
                    StartCoroutine(calculateStaticThreatLvl(grid[x, y]));
            }
        }
    }

    public IEnumerator calculateStaticThreatLvl(Node node)
    {
        int totalValue = distToUnwalkable(node);  //TODO convert to new vec3 instead of vec.for, optimization
        node.setStaticThreatLvl(totalValue);
        yield return null;
    }

    int distToUnwalkable(Node startPos)
    {
        int u = 0;
        int d = 0;
        int l = 0;
        int r = 0;
        int srchTo = 3;
        Vector3 dir = Vector3.forward;
        Vector3 strtPos = startPos.worldPosition;
        //search till our movement range is exceded or we find an edge
        for (int i = 1; i < srchTo; i++)
        {
            if (!mGrid.NodeFromWorldPoint(strtPos + (dir * (i))).walkable)
                u += i;
            break;
        }
        dir = Vector3.back;
        for (int i = 1; i < srchTo; i++)
        {
            if (!mGrid.NodeFromWorldPoint(strtPos + (dir * (i))).walkable)
                d += i;
            break;
        }
        dir = Vector3.left;
        for (int i = 1; i < srchTo; i++)
        {
            if (!mGrid.NodeFromWorldPoint(strtPos + (dir * (i))).walkable)
                l += i;
            break;
        }
        dir = Vector3.right;
        for (int i = 1; i < srchTo; i++)
        {
            if (!mGrid.NodeFromWorldPoint(strtPos + (dir * (i))).walkable)
                r += i;
            break;
        }
        if (u == d && u == 1)
        {
            u = 0;
            d = 0;
        }
        if (l == r && r == 1)
        {
            l = 0;
            r = 0;
        }
        return u + d + l + r;
    }

    public void recalAddThreatLvl()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (grid[x, y].walkable)
                    grid[x, y].setAddThreatLvl(0);
            }
        }
        StartCoroutine(recalAddThreatLvlCoRutine());
    }
    public IEnumerator recalAddThreatLvlCoRutine()
    {
        foreach (TBSUnit u in enemies) {
            if(u != null)
            u.influenceHeatMap();
        }
        yield return null;
    }

}
