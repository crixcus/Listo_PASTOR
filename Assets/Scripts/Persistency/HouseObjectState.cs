using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class HouseObjectState
{
    public string id;
    public string prefabKey; // optional
    public float[] position;
    public float[] rotation;
    public bool active;
}


[Serializable]
public class SceneSaveData
{
    public List<HouseObjectState> objects = new List<HouseObjectState>();
}