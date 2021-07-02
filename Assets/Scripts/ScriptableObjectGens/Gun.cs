using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.AstralSky.FPS
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
    public class Gun : ScriptableObject 
    {
        
        [Header("Weapon Type")]
        
        public string gunName;
        public int firingType; //0 = semi, 1 = auto, 2 burst fire, 3 explosive
        public int burstAmount;
        public int pellets;
        public bool recovery;
        public bool canBeAimed;
        
        [Space(10)]
        [Header("Weapon Stats")]
        public int damage;
        public int headShotMultiplier;
        public float firerate;
        public int ammo;
        public int clipsize;
        public float reloadTime;
         

        [Space(10)]
        [Header("Player Effects")]
        public float aimSpeed;
        public float bloom;
        public float adsBloom;
        public float recoil;
        public float adsRecoil;
        public float kickback;
        public GameObject muzzleFlash;
        public float muzzleFlashScale;
        public AudioClip gunshotSound;
        public AudioClip equipSound;
        public AudioClip reloadSound;
        public AudioClip chargingSound;
        public float pitchRandomization;
        [Range(0, 1)] public float mainFOV;
        [Range(0,1)] public float weaponFOV;

        [Space(10)]
        [Header("Weapon Objects")]
        public GameObject prefab;
        public GameObject display;
        

        private int stash; //Current Ammo
        private int clip;  //Current Clip

        private bool canShootBurst;
        private int burstCount;

        public void Initialize()
        {
            stash = ammo;
            clip = clipsize;
            ResetBurst();
        }


        public bool FireBullet()
        {   
            if(firingType == 2) 
            {   
                if(!canShootBurst) return false;
                
                
                if(burstCount <= 1)
                {
                    canShootBurst = false;
                }
                else 
                {
                    burstCount -= 1;
                }
                
            }
            

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



        public void ResetBurst()
        {
            canShootBurst = true;
            burstCount = burstAmount;
        }


        public int GetStash() {return stash;}

        public int GetClip() {return clip;}
    }
}
