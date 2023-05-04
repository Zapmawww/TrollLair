using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public GameObject startText;
    public GameObject enterText;
    public GameObject simulateText;

    public GameObject editPannel;
    public GameObject agentholder;
    public GameObject objholder;

    public SelectTile selectComp; 
    public Spawn spa;
    public GameMap gameMap;

    public GameObject grids;
    public enum Stage
    {
        STAGE_START,
        STAGE_GENERATE,
        STAGE_SIMULATE,
    }
    public Stage CurStage;

    void Start()
    {
        CurStage = Stage.STAGE_START;
    }

    void Update()
    {
        switch (CurStage)
        {
            case Stage.STAGE_START:
                if (Input.GetKey(KeyCode.Space))
                {
                    startText.SetActive(false);

                    grids.SetActive(true);
                    enterText.SetActive(true);
                    selectComp.enabled = true;

                    CurStage = Stage.STAGE_GENERATE;
                }
                break;
            case Stage.STAGE_GENERATE:
                if (Input.GetKey(KeyCode.Return))
                {
                    selectComp.enabled = false;
                    editPannel.SetActive(false);
                    enterText.SetActive(false);

                    simulateText.SetActive(true);
                    spa.SpawnAgents();
                    spa.SpawnObjs();
                    gameMap.UnSelectTile();

                    CurStage = Stage.STAGE_SIMULATE;
                }
                break;
            case Stage.STAGE_SIMULATE:
                if (Input.GetKey(KeyCode.Escape))
                {
                    simulateText.SetActive(false);
                    grids.SetActive(false);

                    startText.SetActive(true);
                    gameMap.init();

                    foreach (Transform child in agentholder.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    foreach (Transform child in objholder.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }

                    CurStage = Stage.STAGE_START;
                }
                break;
            default:
                break;
        }
    }
}
