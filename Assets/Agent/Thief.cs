using NPBehave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityMovementAI;

public class Thief : MyAgent
{
    private TrollSensor trollSensor;
    private ObjectSensor objectSensor;
    void Update()
    {
        //Update Known Map
        var cellPos = tileMap.WorldToCell(transform.position);
        cellPos.x += gameMap.width / 2;
        cellPos.y += gameMap.height / 2;
        for (int x = cellPos.x - 10; x < cellPos.x + 11; x++)
        {
            for (int y = cellPos.y - 10; y < cellPos.y + 11; y++)
            {
                if (x >= 0 && x < gameMap.width && y >= 0 && y < gameMap.height)
                {
                    knownMap[x, y] = 1;
                }
            }
        }

        //Update Detected Objects
        HashSet<Vector2Int> gemset = new HashSet<Vector2Int>();
        HashSet<Vector2Int> torset = new HashSet<Vector2Int>();
        foreach (var item in objectSensor.targets)
        {
            var objPos = tileMap.WorldToCell(item.transform.position);
            objPos.x += gameMap.width / 2;
            objPos.y += gameMap.height / 2;

            if (item.gameObject.CompareTag("Gem") || item.gameObject.CompareTag("Chest"))
                gemset.Add((Vector2Int)objPos);
            else if (item.gameObject.CompareTag("Torch"))
                torset.Add((Vector2Int)objPos);

        }

        ownBlackboard["GemFound"] = gemset;
        ownBlackboard["TorchFound"] = torset;

    }
    void FixedUpdate()
    {
        action();
    }

    protected override Root CreateBehaviourTree()
    {
        return new Root(ownBlackboard,
            new Service(0.1f, UpdateBlackboards,
                new Selector(
                    new BlackboardCondition("Engaged", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                        OutOfCombat()
                    ),
                    new BlackboardCondition("Engaged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                        EngageEnemy()
                    )
                )
            )
        );
    }

    private Node OutOfCombat()
    {
        return new Selector(
            new BlackboardCondition("NeedExplore", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new NPBehave.Action(() => action = Wonder),
                    new NPBehave.Action(() => ownBlackboard["Arrived"] = false),
                    new NPBehave.Action(() => Debug.Log("Wonder")),
                    new WaitUntilStopped()
                )
            ),
            new BlackboardCondition("Arrived", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new NPBehave.Action(() => action = FollowPath),
                    new NPBehave.Action(() => Debug.Log("FollowPath")),
                    new WaitUntilStopped()
                )
            )
        );
    }
    private Node EngageEnemy()
    {
        return new Selector(
            new BlackboardCondition("NearEnemy", Operator.IS_GREATER_OR_EQUAL, 3, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new NPBehave.Action(() => action = Evade),
                    new NPBehave.Action(() => Debug.Log("Evade")),
                    new WaitUntilStopped()
                )
            ),
            new Sequence(
                    new NPBehave.Action(() => action = Seek),
                new NPBehave.Action(() => Debug.Log("Seek")),
                    new WaitUntilStopped()
            )
        );
    }

    private void UpdateBlackboards()
    {
        // Engaging enemy or not
        ownBlackboard["NearEnemy"] = trollSensor.targets.Count;
        foreach (var item in trollSensor.targets)
        {
            if (item.gameObject.CompareTag("TrollChief"))
                ownBlackboard["NearEnemy"] = 1000;
        }
        if (trollSensor.targets.Count == 0)
            ownBlackboard["Engaged"] = false;
        else
            ownBlackboard["Engaged"] = true;

        //Test Explore
        var gemF = ownBlackboard.Get<HashSet<Vector2Int>>("GemFound");
        var torF = ownBlackboard.Get<HashSet<Vector2Int>>("TorchFound");

        if (gemF.Count == 0)
            ownBlackboard["NeedExplore"] = true;
        else
        {
            ownBlackboard["NeedExplore"] = false;
            int lowestDist = int.MaxValue;
            foreach (var item in gemF)
            {
                var res = TryAStar(item, out _);
                if (res == null) continue;
                else
                {
                    if (res.Count < lowestDist)
                    {
                        ownBlackboard["GemTarget"] = item;
                        lowestDist = res.Count;
                    }
                }
            }

            var gemT = ownBlackboard.Get<Vector2Int>("GemTarget");

            if (gemT.x == -1)//no possible gem to reach
            {
                ownBlackboard["NeedExplore"] = true;
            }
        }

        // Explore: test torch
        if (ownBlackboard.Get<bool>("NeedExplore"))
        {
            if (torF.Count == 0)
            {
                ownBlackboard["TorchTarget"] = new Vector2Int(-1, -1);
                //need torch if nearby
                foreach (var item in torF)
                {
                    var res = TryAStar(item, out _);
                    if (res == null) continue;
                    else if (res.Count >= 10) continue;
                    else
                    {
                        ownBlackboard["TorchTarget"] = item;
                        ownBlackboard["NeedExplore"] = false;
                        break;
                    }
                }
            }
        }
        //update enemyTarget
        if (trollSensor.targets.Count > 0)
        {
            float lowestDist = float.MaxValue;
            foreach (var item in trollSensor.targets)
            {
                var newDist = (item.transform.position - transform.position).magnitude;
                if (newDist < lowestDist)
                {
                    evadingTarget = item;
                    seekingTarget = item.transform;
                    lowestDist = newDist;
                }
            }
            //reset
            path.nodes = new Vector3[1];
            path.nodes[0] = transform.position;
            path.CalcDistances();
        }
        else
        {
            evadingTarget = null;
            seekingTarget = null;

            if (!ownBlackboard.Get<bool>("NeedExplore"))
            {
                var gt = ownBlackboard.Get<Vector2Int>("GemTarget");
                var tt = ownBlackboard.Get<Vector2Int>("TorchTarget");
                if (gt.x != -1)
                {
                    SetPath(gt);
                }
                else if (tt.x != -1)
                {
                    SetPath(tt);
                }
                else
                {
                    //reset
                    path.nodes = new Vector3[1];
                    path.nodes[0] = transform.position;
                    path.CalcDistances();
                }
            }


        }
    }


    protected override void Init()
    {
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;

        trollSensor = transform.Find("TrollSensor").GetComponent<TrollSensor>();
        objectSensor = transform.Find("ObjectSensor").GetComponent<ObjectSensor>();

        ownBlackboard = new Blackboard(globalBB, UnityContext.GetClock());

        ownBlackboard["NeedExplore"] = false;
        ownBlackboard["Engaged"] = false;
        ownBlackboard["NearEnemy"] = 0;

        ownBlackboard["GemFound"] = new HashSet<Vector2Int>();
        ownBlackboard["TorchFound"] = new HashSet<Vector2Int>();

        ownBlackboard["GemTarget"] = new Vector2Int(-1, -1);
        ownBlackboard["TorchTarget"] = new Vector2Int(-1, -1);

        ownBlackboard["Arrived"] = false;
    }
}
