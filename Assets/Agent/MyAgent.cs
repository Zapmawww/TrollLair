using NPBehave;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityMovementAI;

[RequireComponent(typeof(WallAvoidance))]
[RequireComponent(typeof(SteeringBasics))]
[RequireComponent(typeof(MovementAIRigidbody))]
[RequireComponent(typeof(FollowPath))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Wander2))]
[RequireComponent(typeof(Evade))]
[RequireComponent(typeof(Cohesion))]
[RequireComponent(typeof(Separation))]
[RequireComponent(typeof(VelocityMatch))]
public abstract class MyAgent : MonoBehaviour
{
    protected Blackboard ownBlackboard;
    protected Blackboard globalBB;
    protected Root behaviorTree;

    protected SteeringBasics steeringBasics;
    private WallAvoidance wallAvoidance;
    private FollowPath followPath;
    private Evade evade;
    protected Wander2 wander;
    protected Cohesion cohesion;
    protected Separation separation;
    protected VelocityMatch velocityMatch;

    protected LinePath path;
    protected MovementAIRigidbody evadingTarget;
    protected Transform seekingTarget;

    protected GameMap gameMap;
    protected Tilemap tileMap;

    public float cohesionWeight = 1f;
    public float separationWeight = 1f;
    public float velocityMatchWeight = 1f;

    protected System.Action action = null;

    protected int[,] knownMap;

    void Start()
    {
        steeringBasics = GetComponent<SteeringBasics>();
        wallAvoidance = GetComponent<WallAvoidance>();
        followPath = GetComponent<FollowPath>();
        evade = GetComponent<Evade>();
        wander = GetComponent<Wander2>();
        cohesion = GetComponent<Cohesion>();
        separation = GetComponent<Separation>();
        velocityMatch = GetComponent<VelocityMatch>();

        path = new LinePath(new Vector3[1]);

        gameMap = GameObject.Find("Generator").GetComponent<GameMap>();
        tileMap = gameMap.wallLayer;
        knownMap = new int[gameMap.width, gameMap.height];


        globalBB = UnityContext.GetSharedBlackboard("global-shared");
        Init();
        behaviorTree = CreateBehaviourTree();
        behaviorTree.Start();
    }
    public void OnDestroy()
    {
        StopBehaviorTree();
    }

    public void StopBehaviorTree()
    {
        if (behaviorTree != null && behaviorTree.CurrentState == Node.State.ACTIVE)
        {
            behaviorTree.Stop();
        }
    }

    protected abstract Root CreateBehaviourTree();
    protected abstract void Init();

    protected void MoveTo(Vector3 targetPosition)
    {
        Vector3 accel = wallAvoidance.GetSteering();

        if (accel.magnitude < 0.005f)
        {
            accel = steeringBasics.Arrive(targetPosition);
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    protected void Evade()
    {
        if (evadingTarget == null) return;
        Vector3 accel = wallAvoidance.GetSteering();

        if (accel.magnitude < 0.005f)
        {
            accel = evade.GetSteering(evadingTarget);
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }

    protected void Seek()
    {
        if (seekingTarget == null) return;
        Vector3 accel = wallAvoidance.GetSteering();

        if (accel.magnitude < 0.005f)
        {
            accel = steeringBasics.Seek(seekingTarget.position);
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }
    protected void Wonder()
    {
        Vector3 accel = wallAvoidance.GetSteering();

        if (accel.magnitude < 0.005f)
        {
            accel = wander.GetSteering();
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }
    protected void FollowPath()
    {
        ownBlackboard["Arrived"] = false;
        if (followPath.IsAtEndOfPath(path))
        {
            ownBlackboard["Arrived"] = true;
            return;
        }

        Vector3 accel = wallAvoidance.GetSteering();

        if (accel.magnitude < 0.005f)
        {
            accel = followPath.GetSteering(path);
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }
    protected void SetPath(Vector2Int t)
    {
        path.Draw();
        var res = TryAStar(t, out bool d);
        var vec3Res = new List<Vector3>();
        foreach (var item in res)
        {
            var pos = tileMap.CellToWorld(new Vector3Int(item.x - gameMap.width / 2, item.y - gameMap.height / 2, 0));
            vec3Res.Add(new Vector3(pos.x, pos.y, 0));
        }
         if (d)
        {
            path.nodes = vec3Res.ToArray();
        }
        else
        {
            var vec3ResD = new List<Vector3>();
            for (int i = 0; i < res.Count; i++)
            {
                if (knownMap[res[i].x, res[i].y] == 0) break;
                vec3ResD.Add(vec3Res[i]);
            }
            path.nodes = vec3ResD.ToArray();
        }
        path.CalcDistances();
    }
    protected List<Vector2Int> TryAStar(Vector2Int Target, out bool direct)
    {
        //firstly, try known area
        int[,] toAstar = new int[gameMap.width, gameMap.height];
        for (int x = 0; x < gameMap.width; x++)
        {
            for (int y = 0; y < gameMap.height; y++)
            {
                if (knownMap[x, y] == 1 || x == 0 || y == 0 || x == gameMap.width - 1 || y == gameMap.height - 1)
                {//seen or edge, place wall
                    toAstar[x, y] = gameMap.terrain[x, y] >= 1 ? 1 : 0;
                }
                else toAstar[x, y] = 1; // block all
            }
        }
        var cellPos = tileMap.WorldToCell(transform.position);
        cellPos.x += gameMap.width / 2;
        cellPos.y += gameMap.height / 2;

        var result = AStar.FindPath(toAstar, (Vector2Int)cellPos, Target);
        if (result != null)
        {
            direct = true;
            return result;
        }
        direct = false;

        //then head to the unknown
        toAstar = new int[gameMap.width, gameMap.height];
        for (int x = 0; x < gameMap.width; x++)
        {
            for (int y = 0; y < gameMap.height; y++)
            {
                if (knownMap[x, y] == 1 || x == 0 || y == 0 || x == gameMap.width - 1 || y == gameMap.height - 1)
                {//seen or edge, place wall
                    toAstar[x, y] = gameMap.terrain[x, y] >= 1 ? 1 : 0;
                }
                else toAstar[x, y] = 0; // try unknown
            }
        }

        result = AStar.FindPath(toAstar, (Vector2Int)cellPos, Target);

        return result;
    }
}
