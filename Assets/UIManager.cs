using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class UIManager : GenericSingleton<UIManager>
{
    [SerializeField] private List<GameObject> pageList;

    [SerializeField] private Transform canvas;

    private Transform popupParent;

    [SerializeField] private GameObject popupParentPrefab;

    public Color darkGrayTextColor;

    public Color perfectScoreColor;

    public Color normalScoreColor;

    public Color badScoreColor;
    
    public GameObject currentPage;

    private void Start()
    {
        PageChange(0);
    }

    public void PageChange(int index)
    {
        foreach (Transform child in canvas)
        {
            Destroy(child.gameObject);
        }

        GameObject page = Instantiate(pageList[index], canvas);

        currentPage = page;
        
        GameObject popupTransformObj = Instantiate(popupParentPrefab, canvas);

        popupTransformObj.transform.SetSiblingIndex(canvas.childCount - 1);

        popupParent = popupTransformObj.transform;
    }
}
