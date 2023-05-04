using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CAgen;

public class GameMap : MonoBehaviour
{
    [Header("Size")]
    public int width = 64;
    public int height = 64;

    [Header("Tilemaps")]
    public Tilemap wallLayer;
    public Tilemap landLayer;
    public Tilemap waterLayer;
    public Tilemap stoneLayer;
    public Tilemap objectLayer;
    public Tilemap agentLayer;
    public Tilemap selectLayer;

    [Header("Tile Sprites")]
    public TileBase land;
    public TileBase wall_solid;
    public TileBase wall_single;
    public TileBase wall_top;
    public TileBase wall_down;
    public TileBase wall_left;
    public TileBase wall_right;
    public TileBase wall_corner_lt;
    public TileBase wall_corner_rt;
    public TileBase wall_corner_ld;
    public TileBase wall_corner_rd;
    public TileBase wall_inv_corner_lt;
    public TileBase wall_inv_corner_rt;
    public TileBase wall_inv_corner_ld;
    public TileBase wall_inv_corner_rd;

    [Header("Object Sprites")]
    public TileBase gem;
    public TileBase torch;
    public TileBase chest;
    public TileBase water;

    [Header("Agent Sprites")]
    public TileBase troll;
    public TileBase trollChief;
    public TileBase thief;

    public TileBase highlight;

    public int[,] terrain = null;
    public int[,] objects = null;
    public int[,] agents = null;

    bool selected;
    Vector2Int selectedTile;

    public void init()
    {
        terrain = new int[width, height];
        objects = new int[width, height];
        agents = new int[width, height];
        ShowMap();
    }
    public void ShowMap()
    {
        wallLayer.ClearAllTiles();
        landLayer.ClearAllTiles();
        waterLayer.ClearAllTiles();
        stoneLayer.ClearAllTiles();
        objectLayer.ClearAllTiles();
        agentLayer.ClearAllTiles();
        selectLayer.ClearAllTiles();
        if (selected) selectLayer.SetTile(
                new Vector3Int(
                    selectedTile.x - width / 2,
                    selectedTile.y - height / 2,
                    0),
                highlight
            );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector3Int(x - width / 2, y - height / 2, 0);
                TerrainType tt = (TerrainType)terrain[x, y];
                switch (tt)
                {
                    case TerrainType.WALL_SOLID:
                        wallLayer.SetTile(pos, wall_solid);
                        break;
                    case TerrainType.WALL_SINGLE:
                        wallLayer.SetTile(pos, land);
                        stoneLayer.SetTile(pos, wall_single);
                        break;
                    case TerrainType.WALL_TOP:
                        wallLayer.SetTile(pos, wall_top);
                        break;
                    case TerrainType.WALL_DOWN:
                        wallLayer.SetTile(pos, wall_down);
                        break;
                    case TerrainType.WALL_LEFT:
                        wallLayer.SetTile(pos, wall_left);
                        break;
                    case TerrainType.WALL_RIGHT:
                        wallLayer.SetTile(pos, wall_right);
                        break;
                    case TerrainType.WALL_CORNER_LT:
                        wallLayer.SetTile(pos, wall_corner_lt);
                        break;
                    case TerrainType.WALL_CORNER_RT:
                        wallLayer.SetTile(pos, wall_corner_rt);
                        break;
                    case TerrainType.WALL_CORNER_LD:
                        wallLayer.SetTile(pos, wall_corner_ld);
                        break;
                    case TerrainType.WALL_CORNER_RD:
                        wallLayer.SetTile(pos, wall_corner_rd);
                        break;
                    case TerrainType.WALL_INV_CORNER_LT:
                        wallLayer.SetTile(pos, wall_inv_corner_lt);
                        break;
                    case TerrainType.WALL_INV_CORNER_RT:
                        wallLayer.SetTile(pos, wall_inv_corner_rt);
                        break;
                    case TerrainType.WALL_INV_CORNER_LD:
                        wallLayer.SetTile(pos, wall_inv_corner_ld);
                        break;
                    case TerrainType.WALL_INV_CORNER_RD:
                        wallLayer.SetTile(pos, wall_inv_corner_rd);
                        break;
                    case TerrainType.LAND:
                        landLayer.SetTile(pos, land);
                        break;
                    case TerrainType.WATER:
                        landLayer.SetTile(pos, land);
                        waterLayer.SetTile(pos, water);
                        break;
                    default:
                        break;
                }
                ObjectType ot = (ObjectType)objects[x, y];
                pos = new Vector3Int(x - width / 2, y - height / 2, 0);
                switch (ot)
                {
                    case ObjectType.GEM:
                        objectLayer.SetTile(pos, gem);
                        break;
                    case ObjectType.TORCH:
                        objectLayer.SetTile(pos, torch);
                        break;
                    case ObjectType.CHEST:
                        objectLayer.SetTile(pos, chest);
                        break;

                    default:
                        break;
                }
                AgentType at = (AgentType)agents[x, y];
                pos = new Vector3Int(x - width / 2, y - height / 2, 0);
                switch (at)
                {
                    case AgentType.TROLL:
                        agentLayer.SetTile(pos, troll);
                        break;
                    case AgentType.TROLL_CHIEF:
                        agentLayer.SetTile(pos, trollChief);
                        break;
                    case AgentType.THIEF:
                        agentLayer.SetTile(pos, thief);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public bool SelectTile(int x, int y)
    {
        var testX = x + width / 2;
        var testY = y + height / 2;
        if (testX < 0 || testX > width || testY < 0 || testY > height)
        {
            return false;
        }
        selected = true;
        selectedTile.x = testX;
        selectedTile.y = testY;
        ShowMap();
        return true;
    }
    public void UnSelectTile()
    {
        selected = false;
        ShowMap();
    }
    public void RemoveAgents()
    {
        agents = new int[width, height];
        ShowMap();
    }
    public void RemoveObjs()
    {
        objects = new int[width, height];
        ShowMap();
    }
    public TerrainType GetTerrainInfo(int x, int y)
    {
        return (TerrainType)terrain[x + width / 2, y + height / 2];
    }
    public ObjectType GetObjectInfo(int x, int y)
    {
        return (ObjectType)objects[x + width / 2, y + height / 2];
    }
    public AgentType GetAgentInfo(int x, int y)
    {
        return (AgentType)agents[x + width / 2, y + height / 2];
    }
    public void SetTerrainInfo(int x, int y, TerrainType info)
    {
        terrain[x + width / 2, y + height / 2] = (int)info;
        SettleWalls();
        ShowMap();
    }
    public void SetObjectInfo(int x, int y, ObjectType info)
    {
        objects[x + width / 2, y + height / 2] = (int)info;
        SettleWalls();
        ShowMap();
    }
    public void SetAgentInfo(int x, int y, AgentType info)
    {
        agents[x + width / 2, y + height / 2] = (int)info;
        SettleWalls();
        ShowMap();
    }

    public void SettleWalls()
    {
        //wall orintation
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrain[x, y] >= 1)
                    terrain[x, y] = (int)ChoseWall(x, y);
            }
        }
    }

    public TerrainType ChoseWall(int x, int y)
    {
        var walls4_1 = 0;
        var walls4_2 = 0;
        bool wallsD = true; bool wallsT = true;
        bool wallsR = true; bool wallsL = true;
        bool wallsLT = true; bool wallsRT = true;
        bool wallsLD = true; bool wallsRD = true;
        if (!(y - 1 < 0))
        {
            wallsD = terrain[x, y - 1] >= 1;
        }
        if (!(y + 1 >= height))
        {
            wallsT = terrain[x, y + 1] >= 1;
        }
        if (!(x + 1 >= width))
        {
            wallsR = terrain[x + 1, y] >= 1;
        }
        if (!(x - 1 < 0))
        {
            wallsL = terrain[x - 1, y] >= 1;
        }
        if (wallsD) walls4_1++; if (wallsT) walls4_1++;
        if (wallsR) walls4_1++; if (wallsL) walls4_1++;

        if (!(x - 1 < 0 || y + 1 >= height))
        {
            wallsLT = terrain[x - 1, y + 1] >= 1;
        }
        if (!(x + 1 >= width || y + 1 >= height))
        {
            wallsRT = terrain[x + 1, y + 1] >= 1;
        }
        if (!(x - 1 < 0 || y - 1 < 0))
        {
            wallsLD = terrain[x - 1, y - 1] >= 1;
        }
        if (!(x + 1 >= width || y - 1 < 0))
        {
            wallsRD = terrain[x + 1, y - 1] >= 1;
        }
        if (wallsLT) walls4_2++; if (wallsRT) walls4_2++;
        if (wallsLD) walls4_2++; if (wallsRD) walls4_2++;

        if (walls4_1 == 4)
        {
            if (walls4_2 == 2 || walls4_2 == 4) return TerrainType.WALL_SOLID;
            if (walls4_2 == 3)
            {
                if (!wallsLT) return TerrainType.WALL_INV_CORNER_LT;
                if (!wallsRT) return TerrainType.WALL_INV_CORNER_RT;
                if (!wallsLD) return TerrainType.WALL_INV_CORNER_LD;
                if (!wallsRD) return TerrainType.WALL_INV_CORNER_RD;
            }
        }
        else if (walls4_1 == 3)
        {
            if (!wallsT) return TerrainType.WALL_TOP;
            if (!wallsD) return TerrainType.WALL_DOWN;
            if (!wallsL) return TerrainType.WALL_LEFT;
            if (!wallsR) return TerrainType.WALL_RIGHT;
        }
        else if (walls4_1 == 2)
        {
            if (wallsL && wallsT) return TerrainType.WALL_CORNER_RD;
            if (wallsR && wallsT) return TerrainType.WALL_CORNER_LD;
            if (wallsL && wallsD) return TerrainType.WALL_CORNER_RT;
            if (wallsR && wallsD) return TerrainType.WALL_CORNER_LT;
        }
        return TerrainType.WALL_SINGLE;
    }
}
