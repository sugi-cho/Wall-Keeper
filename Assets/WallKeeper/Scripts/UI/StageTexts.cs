using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class StageTexts : RendererBehaviour
{

    public GameObject[] stageTitleModels;
    public GameObject startTextModel;
    public GameObject finishTextModel;

    GameObject current;

    public void ShowTitle(int i)
    {
        if (current != null)
            Hide();
        current = stageTitleModels[i];
        current.SetActive(true);
    }

    public void ShowStart()
    {
        if (current != null)
            Hide();
        current = startTextModel;
        current.SetActive(true);
    }

    public void ShowFinish()
    {
        if (current != null)
            Hide();
        current = finishTextModel;
        current.SetActive(true);
    }

    public void Hide()
    {
        if (current != null)
            current.SetActive(false);
        current = null;
    }

    private void Start()
    {
        foreach (var r in renderers)
            r.SetColor("_Color", ProjectionController.MyColor);
        foreach (var go in stageTitleModels)
            go.SetActive(false);
        startTextModel.SetActive(false);
        finishTextModel.SetActive(false);
    }
}
