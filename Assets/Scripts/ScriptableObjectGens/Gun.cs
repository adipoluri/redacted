using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.AstralSky.FPS
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
    public class Gun : ScriptableObject 
    {
        public string gunName;
        public float firerate;
        public GameObject prefab;    
    }
}
