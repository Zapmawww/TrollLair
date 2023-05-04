using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityMovementAI;

public class Spawn : MonoBehaviour
{
    public GameObject thief;
    public GameObject troll;
    public GameObject trollChief;

    public GameObject gem;
    public GameObject torch;
    public GameObject chest;

    public GameObject parentAgent;
    public GameObject parentObj;

    public GameMap gameMap;

    public void SpawnObjs()
    {
        for (int x = 0; x < gameMap.width; x++)
        {
            for (int y = 0; y < gameMap.height; y++)
            {
                GameObject spawned;
                var mapX = x - gameMap.width / 2;
                var mapY = y - gameMap.height / 2;
                var cellPos = gameMap.wallLayer.CellToWorld(new Vector3Int(mapX, mapY, 0));
                switch (gameMap.GetObjectInfo(mapX, mapY))
                {
                    case CAgen.ObjectType.GEM:
                        spawned = Instantiate(gem, cellPos, Quaternion.identity);
                        break;
                    case CAgen.ObjectType.TORCH:
                        spawned = Instantiate(torch, cellPos, Quaternion.identity);
                        break;
                    case CAgen.ObjectType.CHEST:
                        spawned = Instantiate(chest, cellPos, Quaternion.identity);
                        break;
                    default:
                        continue;
                }
                spawned.transform.SetParent(parentObj.transform);
            }
        }
        gameMap.RemoveObjs();
    }
    public void SpawnAgents()
    {
        for (int x = 0; x < gameMap.width; x++)
        {
            for (int y = 0; y < gameMap.height; y++)
            {
                GameObject spawned;
                var mapX = x - gameMap.width / 2;
                var mapY = y - gameMap.height / 2;
                var cellPos = gameMap.wallLayer.CellToWorld(new Vector3Int(mapX, mapY, 0));
                switch (gameMap.GetAgentInfo(mapX, mapY))
                {
                    case CAgen.AgentType.TROLL:
                        spawned = Instantiate(troll, cellPos, Quaternion.identity);
                        break;
                    case CAgen.AgentType.TROLL_CHIEF:
                        spawned = Instantiate(trollChief, cellPos, Quaternion.identity);
                        break;
                    case CAgen.AgentType.THIEF:
                        spawned = Instantiate(thief, cellPos, Quaternion.identity);
                        break;
                    default:
                        continue;
                }
                spawned.transform.SetParent(parentAgent.transform);
            }
        }
        gameMap.RemoveAgents();
    }
}
