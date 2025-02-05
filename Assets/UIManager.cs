using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : GenericSingleton<UIManager>
{
    [SerializeField] private List<GameObject> pageList;

    [SerializeField] private Transform canvas;
}
