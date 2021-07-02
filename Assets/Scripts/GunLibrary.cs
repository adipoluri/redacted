using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.AstralSky.FPS
{
    public class GunLibrary : MonoBehaviour
    {
        public Gun[] allGuns;
        public static Gun[] guns;

        private void Awake() {
            guns = allGuns;
        }

        public static Gun FindGun (string name)
        {
            foreach(Gun a in guns)
            {
                if(a.gunName.Equals(name)) return a;
            }

            return guns[0];
        }    
    }
}