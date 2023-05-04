using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Cellular Automata Map Generator for Cave
/// Some code are modified from http://www.roguebasin.com/
/// Or stackoverflow
/// </summary>
[RequireComponent(typeof(GameMap))]
public class CAgen : MonoBehaviour
{
    [Header("Terrain generation")]
    public int basicTerrainSteps = 15;
    public float wallFac = 0.48f;
    public float waterFac = 0.103f;
    public int nearbyFactor = 4;

    [Header("Cave number control")]
    public int largeCaveLimitFactor = 2500; // disjoint caves if possible
    public int smallCaveLimitFactor = 800;  // not too much

    [Header("Object and agent control")]
    public int chestCaveNumFactor = 1300;   // affect chest count
    public int trollCampNumFactor = 1300;   // inv troll density
    public int trollCampSizeFactor = 5;     // troll camp size
    public int chiefNumFactor = 1;          // chiefs in a camp
    public int trollNumFactor = 2;          // trolls in a camp

    [Header("Debug Settings")]
    public bool debug = false;
    public int seed = 0; // for debug
    public StageController stc;

    #region private_defs
    private bool failed = true;
    private GameMap gameMap;
    public enum TerrainType
    {
        WATER = -1,
        LAND = 0,
        WALL_SOLID = 1,
        WALL_TOP,
        WALL_DOWN,
        WALL_LEFT,
        WALL_RIGHT,
        WALL_CORNER_LT,
        WALL_CORNER_RT,
        WALL_CORNER_LD,
        WALL_CORNER_RD,
        WALL_INV_CORNER_LT,
        WALL_INV_CORNER_RT,
        WALL_INV_CORNER_LD,
        WALL_INV_CORNER_RD,
        WALL_SINGLE,
    }
    public enum ObjectType
    {
        NULL = 0,
        GEM,
        TORCH,
        CHEST,
    }
    public enum AgentType
    {
        NULL = 0,
        TROLL,
        TROLL_CHIEF,
        THIEF,
    }
    #endregion



    private void Start()
    {
        gameMap = GetComponent<GameMap>();
        if (debug)
            Random.InitState(seed);
        else
            Random.InitState((int)System.DateTime.Now.ToBinary());
    }
    private void Update()
    {
        if (stc.CurStage == StageController.Stage.STAGE_GENERATE
            && Input.GetKeyDown(KeyCode.Space))
        {
            failed = true;
            while (failed)
                Generate();
            gameMap.ShowMap();
        }
    }

    //gen_ruleset
    private void Step_Stable()
    {
        var width = gameMap.width;
        var height = gameMap.height;
        int[,] newterr = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    newterr[x, y] = (int)TerrainType.WALL_SOLID;
                else
                    newterr[x, y] = CountAdjacent9Walls(x, y) >= 5
                        || CountNearbyWalls(x, y) <= 2
                        ? (int)TerrainType.WALL_SOLID : (int)TerrainType.LAND;
            }
        }
        gameMap.terrain = newterr;
    }

    //overlap_water_ruleset
    private void Step_Water()
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        int[,] newterr = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (terrain[x, y] == (int)TerrainType.WATER)
                {
                    newterr[x, y] = CountAdjacent9Waters(x, y) < 2
                    || CountNearbyWaters(x, y) < nearbyFactor * nearbyFactor / 5
                    ? (int)TerrainType.LAND : (int)TerrainType.WATER;
                }
                else if (terrain[x, y] == (int)TerrainType.LAND)
                {
                    newterr[x, y] = CountAdjacent9Waters(x, y) >= 5
                    || CountNearbyWaters(x, y) > nearbyFactor * nearbyFactor * 2 / 3
                    ? (int)TerrainType.WATER : (int)TerrainType.LAND;
                }
                else newterr[x, y] = terrain[x, y];
            }
        }
        terrain = newterr;
    }

    public void Generate()
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        ref var objects = ref gameMap.objects;
        ref var agents = ref gameMap.agents;
        gameMap.init();
        //noise
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrain[x, y] = Random.Range(0f, 1f) < wallFac
                    ? (int)TerrainType.WALL_SOLID : (int)TerrainType.LAND;
            }
        }

        //Terrain
        for (int i = 0; i < basicTerrainSteps; i++)
        {
            Step_Stable();
        }

        //smooth
        bool changeFlag = true;
        while (changeFlag)
        {
            changeFlag = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!(x == 0 || y == 0 || x == width - 1 || y == height - 1))
                        if (terrain[x, y] >= 1
                            && (CountAdjacent9Walls(x, y) < 3 || JudgeSingle(x, y)))
                        {
                            terrain[x, y] = (int)TerrainType.LAND;
                            changeFlag = true;
                        }
                }
            }
        }

        //verify regions
        var topNCamp = height * width / chestCaveNumFactor;
        var topNChest = height * width / chestCaveNumFactor;
        var nCaveMax = height * width / smallCaveLimitFactor;
        var nCaveMin = height * width / largeCaveLimitFactor;
        var regions = GetRegions();
        if (regions == null
            || regions.Count < nCaveMin
            || regions.Count > nCaveMax
            || regions[Mathf.Max(Mathf.Min(regions.Count - 1, topNCamp - 1), 0)].Count
                < chiefNumFactor + trollNumFactor
            || regions[Mathf.Max(Mathf.Min(regions.Count - 1, topNChest - 1), 0)].Count < 2
            )
        {
            failed = true;
            return;
        }

        gameMap.SettleWalls();

        //overlap_Terrain (water)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrain[x, y] == (int)TerrainType.LAND)
                    terrain[x, y] = Random.Range(0f, 1f) < waterFac
                        ? (int)TerrainType.WATER : (int)TerrainType.LAND;
            }
        }
        for (int i = 0; i < basicTerrainSteps; i++)
        {
            Step_Water();
        }

        //overlap_objects
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrain[x, y] == (int)TerrainType.LAND)
                {
                    var i = Random.Range(0f, 1f);
                    if (i < .02f)
                        objects[x, y] = (int)ObjectType.TORCH;
                    else if (i < .027f)
                        objects[x, y] = (int)ObjectType.GEM;
                }
            }
        }
        for (int i = 0; i < Mathf.Min(topNChest, regions.Count); i++)
        {
            // chest in top n regs
            var choice = regions[i][Random.Range(0, regions[i].Count - 1)];
            objects[choice.x, choice.y] = (int)ObjectType.CHEST;
        }

        //agents
        var landRegions = GetRegions(false);
        for (int i = 0; i < 3; i++)
        { // 3 in max
            var choice = landRegions[0][Random.Range(0, landRegions[0].Count - 1)];
            agents[choice.x, choice.y] = (int)AgentType.TROLL;
        }
        for (int i = 1; i < Mathf.Min(topNCamp, landRegions.Count); i++)
        {
            // camp flag in top n regs
            var choice = landRegions[i][Random.Range(0, landRegions[i].Count - 1)];
            agents[choice.x, choice.y] = (int)AgentType.TROLL;
        }
        //place trolls in camp
        int[,] placeTroll = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (agents[x, y] == (int)AgentType.TROLL)
                {
                    var nearReachableList =
                        FindReachablePoints(new Vector2Int(x, y), trollCampSizeFactor, false);
                    //shuffle
                    nearReachableList = nearReachableList.OrderBy(x => Random.value).ToList();
                    var selectedPos = nearReachableList.Take(chiefNumFactor + trollNumFactor).ToList();

                    for (int i = 0; i < selectedPos.Count; i++)
                    {
                        if (i < chiefNumFactor)
                            placeTroll[selectedPos[i].x, selectedPos[i].y] = (int)AgentType.TROLL_CHIEF;
                        else
                            placeTroll[selectedPos[i].x, selectedPos[i].y] = (int)AgentType.TROLL;
                    }
                }
            }
        }
        agents = placeTroll;
        //place thief in the largest
        while (true)
        {
            var choiceThief = regions[0][Random.Range(0, regions[0].Count - 1)];
            if (agents[choiceThief.x, choiceThief.y] != 0) continue;
            agents[choiceThief.x, choiceThief.y] = (int)AgentType.THIEF;
            break;
        }

        failed = false;
    }

    private int CountAdjacent9Walls(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var walls = 0;

        for (var mapX = x - 1; mapX <= x + 1; mapX++)
        {
            for (var mapY = y - 1; mapY <= y + 1; mapY++)
            {
                if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                    continue;
                if (terrain[mapX, mapY] >= 1)
                    walls++;
            }
        }

        return walls;
    }
    private int CountAdjacent9Waters(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var waters = 0;

        for (var mapX = x - 1; mapX <= x + 1; mapX++)
        {
            for (var mapY = y - 1; mapY <= y + 1; mapY++)
            {
                if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                    continue;
                if (terrain[mapX, mapY] == (int)TerrainType.WATER)
                    waters++;
            }
        }

        return waters;
    }
    private int CountAdjacent4Walls(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var walls = 0;

        var positions = new int[4, 2]
        {
            {0,1 },
            {0,-1 },
            {1,0 },
            {-1,0 },
        };
        for (var iter = 0; iter < 4; iter++)
        {
            var mapX = x + positions[iter, 0];
            var mapY = y + positions[iter, 1];
            if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                continue;
            if (terrain[mapX, mapY] >= 1)
            {
                walls++;
            }
        }

        return walls;
    }

    private bool JudgeSingle(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var wallsX = 0;
        var wallsY = 0;
        var walls = 0;

        var positions = new int[4, 2]
        {
            {0,1 },
            {0,-1 },
            {1,0 },
            {-1,0 },
        };
        for (var iter = 0; iter < 4; iter++)
        {
            var mapX = x + positions[iter, 0];
            var mapY = y + positions[iter, 1];
            if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                continue;
            if (terrain[mapX, mapY] >= 1)
            {
                walls++;
                if (mapX == x) wallsY++;
                else wallsX++;
            }
        }

        return walls == wallsX || walls == wallsY;
    }

    private int CountNearbyWalls(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var walls = 0;

        for (var mapX = x - nearbyFactor; mapX <= x + nearbyFactor; mapX++)
        {
            for (var mapY = y - nearbyFactor; mapY <= y + nearbyFactor; mapY++)
            {
                if (System.Math.Abs(mapX - x) * System.Math.Abs(mapX - x)
                    + System.Math.Abs(mapY - y) * System.Math.Abs(mapY - y)
                    > nearbyFactor * nearbyFactor)
                    continue;

                if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                    continue;

                if (terrain[mapX, mapY] >= 1)
                    walls++;
            }
        }

        return walls;
    }
    private int CountNearbyWaters(int x, int y)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        var waters = 0;

        for (var mapX = x - nearbyFactor; mapX <= x + nearbyFactor; mapX++)
        {
            for (var mapY = y - nearbyFactor; mapY <= y + nearbyFactor; mapY++)
            {
                if (System.Math.Abs(mapX - x) * System.Math.Abs(mapX - x)
                    + System.Math.Abs(mapY - y) * System.Math.Abs(mapY - y)
                    > nearbyFactor * nearbyFactor)
                    continue;

                if (mapX < 0 || mapY < 0 || mapX >= width || mapY >= height)
                    continue;

                if (terrain[mapX, mapY] == (int)TerrainType.WATER)
                    waters++;
            }
        }

        return waters;
    }

    private List<List<Vector2Int>> GetRegions(bool containWater = true)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        int[,] visited = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y] == 0 && terrain[x, y] == (int)TerrainType.LAND)
                {
                    List<Vector2Int> region = GetRegionTiles(x, y, ref visited, containWater);
                    regions.Add(region);
                }
            }
        }
        regions.Sort((lhs, rhs) =>
            {
                if (lhs.Count < rhs.Count)
                    return 1;
                else
                    return -1;
            });
        return regions;
    }

    private List<Vector2Int> GetRegionTiles(int startX, int startY, ref int[,] visited, bool containWater)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        List<Vector2Int> tiles = new List<Vector2Int>();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.x - 1; x <= tile.x + 1; x++)
                for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    if (x >= 0 && x < width && y >= 0 && y < height
                        && (x == tile.x || y == tile.y))// only go 4 directions
                    {
                        if (visited[x, y] == 0
                            && terrain[x, y] <= (int)TerrainType.LAND)
                        {
                            visited[x, y] = 1;
                            queue.Enqueue(new Vector2Int(x, y));
                        }
                    }
        }
        List<Vector2Int> ret;

        if (!containWater)
        {
            ret = new List<Vector2Int>();
            foreach (var tile in tiles)
            {
                if (terrain[tile.x, tile.y] == (int)TerrainType.LAND) ret.Add(tile);
            }
        }
        else ret = tiles;

        return ret;
    }


    private struct PointWithDistance
    {
        public Vector2Int point;
        public int distance;
    }

    private List<Vector2Int> FindReachablePoints(Vector2Int start, int steps, bool onWater)
    {
        var width = gameMap.width;
        var height = gameMap.height;
        ref var terrain = ref gameMap.terrain;
        List<Vector2Int> reachablePoints = new List<Vector2Int>();
        Queue<PointWithDistance> pointsToVisit = new Queue<PointWithDistance>();
        HashSet<Vector2Int> visitedPoints = new HashSet<Vector2Int>();

        pointsToVisit.Enqueue(new PointWithDistance { point = start, distance = 0 });

        //BFS
        while (pointsToVisit.Count > 0)
        {
            PointWithDistance currentPoint = pointsToVisit.Dequeue();
            if (visitedPoints.Contains(currentPoint.point))
                continue;
            visitedPoints.Add(currentPoint.point);

            //within given steps
            if (currentPoint.distance <= steps)
                reachablePoints.Add(currentPoint.point);

            // impossible
            if (currentPoint.distance >= steps + 2)
                continue;

            //the adjacent points
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    Vector2Int adjacentPoint = currentPoint.point + new Vector2Int(x, y);

                    if (adjacentPoint.x < 0 || adjacentPoint.x >= width
                        || adjacentPoint.y < 0 || adjacentPoint.y >= height)
                        continue;

                    //wall/water
                    if (terrain[adjacentPoint.x, adjacentPoint.y] >= 1
                        || (!onWater && terrain[adjacentPoint.x, adjacentPoint.y] == -1))
                        continue;

                    //add the adjacent point to the queue
                    pointsToVisit.Enqueue(new PointWithDistance
                    {
                        point = adjacentPoint,
                        distance = currentPoint.distance + 1
                    });
                }
            }
        }
        return reachablePoints;
    }
}

