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
        public float aimSpeed;
        public float bloom;
        public float recoil;
        public float kickback;
        public int damage;
        public int ammo;
        public int clipsize;
        public float reloadTime;
        public int burst; //0 = semi, 1 = auto, 2+ burst fire
        public GameObject prefab;

        private int stash; //Current Ammo
        private int clip;  //Current Clip


        public void Initialize()
        {
            stash = ammo;
            clip = clipsize;
        }
        public bool FireBullet()
        {
            if(clip > 0)
            {
                clip -= 1;
                return true;
            }
            else return false;
        }

        public void Reload()
        {
            stash += clip;
            clip = Mathf.Min(clipsize, stash);
            stash -= clip;

        }

        public int GetStash() {return stash;}

        public int GetClip() {return clip;}
    }
}
