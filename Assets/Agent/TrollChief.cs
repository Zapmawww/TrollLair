using NPBehave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollChief : MyAgent
{
    private ThiefSensor thiefSensor;
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
        return new Sequence(
                    new NPBehave.Action(() => action = Wonder),
                    //new NPBehave.Action(() => Debug.Log("Chief Wonder")),
                    new WaitUntilStopped()
                );
    }
    private Node EngageEnemy()
    {
        return new Sequence(
            new NPBehave.Action(() => action = Seek),
            //new NPBehave.Action(() => Debug.Log("Chief Seek")),
            new WaitUntilStopped()
        );
    }

    protected override void Init()
    {
        thiefSensor = transform.Find("ThiefSensor").GetComponent<ThiefSensor>();
        ownBlackboard = new Blackboard(UnityContext.GetClock());
    }
    private void UpdateBlackboards()
    {
        if (thiefSensor.targets.Count == 0)
            ownBlackboard["Engaged"] = false;
        else
            ownBlackboard["Engaged"] = true;

        //update enemyTarget
        if (thiefSensor.targets.Count > 0)
        {
            foreach (var item in thiefSensor.targets)
            {
                seekingTarget = item.transform;
            }
        }
    }
}
