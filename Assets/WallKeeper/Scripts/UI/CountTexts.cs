using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class CountTexts : RendererBehaviour {

    public Color color;
    public bool isCounting;
    public GameObject[] numberModels;
    GameObject current;

    //count down 5-4-3-2-1-
    public void CountDouwn()
    {
        isCounting = true;
         StartCoroutine(CountDownRoutine());
    }
    IEnumerator CountDownRoutine()
    {
        for (var i = 0; i < 5; i++)
        {
            Show(i);
            yield return new WaitForSeconds(1f);
        }
        Hide();
        isCounting = false;
    }

    void Hide()
    {
        if (current != null)
            current.SetActive(false);
        current = null;
    }

    void Show(int i)
    {
        if (current != null)
            current.SetActive(false);
        current = numberModels[i];
        current.SetActive(true);
    }

    private void Start()
    {
        foreach (var r in renderers)
            r.SetColor("_Color", color);
        foreach (var n in numberModels)
            n.SetActive(false);
    }
}
