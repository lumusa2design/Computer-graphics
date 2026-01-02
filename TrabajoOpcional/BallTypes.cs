using System;
using UnityEngine;

public enum BallType { Poke, Ultra, Safari, Master }

[Serializable]
public class BallEntry
{
    public BallType type;
    public GameObject prefab;
}
