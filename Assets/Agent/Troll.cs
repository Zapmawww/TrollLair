using NPBehave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class Troll : MyAgent
{
    private ThiefSensor thiefSensor;
    private TrollSensor trollSensor;

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
                    EngageEnemy()
                )
            )
        );
    }

    private Node OutOfCombat()
    {
        return new Sequence(
                    new NPBehave.Action(() => action = Flocking),
                    new WaitUntilStopped()
                );
    }
    private Node EngageEnemy()
    {
        return new Selector(
            new BlackboardCondition("NearChief", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new NPBehave.Action(() => action = Evade),
                    new WaitUntilStopped()
                )
            ),
            new Sequence(
                new NPBehave.Action(() => action = Seek),
                new WaitUntilStopped()
            )
        );
    }

    protected override void Init()
    {
        thiefSensor = transform.Find("ThiefSensor").GetComponent<ThiefSensor>();
        trollSensor = transform.Find("AllySensor").GetComponent<TrollSensor>();
        ownBlackboard = new Blackboard(UnityContext.GetClock());

        ownBlackboard["Engaged"] = false;
        ownBlackboard["NearChief"] = false;
    }
    private void UpdateBlackboards()
    {
        if (thiefSensor.targets.Count == 0)
            ownBlackboard["Engaged"] = false;
        else
            ownBlackboard["Engaged"] = true;

        foreach (var item in trollSensor.targets)
        {
            if (item.gameObject.CompareTag("TrollChief"))
                ownBlackboard["NearChief"] = true;
        }
        if (thiefSensor.targets.Count > 0)
        {
            foreach (var item in thiefSensor.targets)
            {
                seekingTarget = item.transform;
                evadingTarget = item;
            }
        }
    }


    protected void Flocking()
    {
        Vector3 accel = Vector3.zero;

        accel += cohesion.GetSteering(trollSensor.targets) * cohesionWeight;
        accel += separation.GetSteering(trollSensor.targets) * separationWeight;
        accel += velocityMatch.GetSteering(trollSensor.targets) * velocityMatchWeight;

        if (accel.magnitude < 0.005f)
        {
            accel = wander.GetSteering();
        }

        steeringBasics.Steer(accel);
        steeringBasics.LookWhereYoureGoing();
    }
}
