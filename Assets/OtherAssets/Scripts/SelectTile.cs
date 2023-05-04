using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Tilemap))]
public class SelectTile : MonoBehaviour
{
    public TMP_Text text;
    public GameObject panel;
    public GameMap gameMap;

    private Tilemap tilemap;
    private Vector3Int selected;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var select = tilemap.WorldToCell(mouseWorldPos);
            if (selected != select && gameMap.SelectTile(select.x, select.y))
            {
                selected = select;
                panel.SetActive(true);
                UpdateInfo();
            }
            else
            {
                selected = new Vector3Int(0, 0, -1);
                gameMap.UnSelectTile();
                panel.SetActive(false);
            }
        }
        if (Input.GetKey(KeyCode.Space))//regenerate
        {
            panel.SetActive(false);
        }
    }

    private void UpdateInfo()
    {
        text.text = "Tile selected: ("
                   + selected.x + ", " + selected.y
                   + ")\nTerrain = "
                   + gameMap.GetTerrainInfo(selected.x, selected.y)
                   + "\nObject = "
                   + gameMap.GetObjectInfo(selected.x, selected.y)
                   + "\nAgent = "
                   + gameMap.GetAgentInfo(selected.x, selected.y)
                   ;
    }

    private bool Check()
    {
        var terr = gameMap.GetTerrainInfo(selected.x, selected.y);
        if (terr >= CAgen.TerrainType.WALL_SOLID) return false;
        return true;
    }

    public void SwitchToWall()
    {
        gameMap.SetTerrainInfo(selected.x, selected.y, CAgen.TerrainType.WALL_SOLID);
        gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.NULL);
        gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.NULL);
        UpdateInfo();
    }
    public void SwitchToWater()
    {
        gameMap.SetTerrainInfo(selected.x, selected.y, CAgen.TerrainType.WATER);
        UpdateInfo();
    }
    public void SwitchToLand()
    {
        gameMap.SetTerrainInfo(selected.x, selected.y, CAgen.TerrainType.LAND);
        UpdateInfo();
    }
    public void SetGem()
    {
        if (!Check()) return;
        var prev = gameMap.GetObjectInfo(selected.x, selected.y);
        if (prev == CAgen.ObjectType.GEM)
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.NULL);
        else
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.GEM);
        UpdateInfo();
    }
    public void SetTorch()
    {
        if (!Check()) return;
        var prev = gameMap.GetObjectInfo(selected.x, selected.y);
        if (prev == CAgen.ObjectType.TORCH)
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.NULL);
        else
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.TORCH);
        UpdateInfo();
    }
    public void SetChest()
    {
        if (!Check()) return;
        var prev = gameMap.GetObjectInfo(selected.x, selected.y);
        if (prev == CAgen.ObjectType.CHEST)
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.NULL);
        else
            gameMap.SetObjectInfo(selected.x, selected.y, CAgen.ObjectType.CHEST);
        UpdateInfo();
    }
    public void PutTroll()
    {
        if (!Check()) return;
        var prev = gameMap.GetAgentInfo(selected.x, selected.y);
        if (prev == CAgen.AgentType.TROLL)
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.NULL);
        else
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.TROLL);
        UpdateInfo();
    }
    public void PutTrollChief()
    {
        if (!Check()) return;
        var prev = gameMap.GetAgentInfo(selected.x, selected.y);
        if (prev == CAgen.AgentType.TROLL_CHIEF)
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.NULL);
        else
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.TROLL_CHIEF);
        UpdateInfo();
    }
    public void PutThief()
    {
        if (!Check()) return;
        var prev = gameMap.GetAgentInfo(selected.x, selected.y);
        if (prev == CAgen.AgentType.THIEF)
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.NULL);
        else
            gameMap.SetAgentInfo(selected.x, selected.y, CAgen.AgentType.THIEF);
        UpdateInfo();
    }
}
