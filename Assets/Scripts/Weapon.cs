using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations.Rigging;

namespace com.AstralSky.FPS
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        
        #region Variables

        public List<Gun> loadout;
        [HideInInspector] public Gun currentGunData;
        public Transform weaponParent;
        public GameObject bulletHolePrefab;
        public GameObject bloodParticles;
        public GameObject reloadExplosion;
        public float weaponThrowSpeed;
        public LayerMask canBeShot;
        public AudioSource sfx;
        public bool isAiming = false;
        public TwoBoneIKConstraint rightArmLocation;
        public TwoBoneIKConstraint leftArmLocation;


        private float currentCooldown;
        private float fullAutoAccuracyCooldown;
        private float chargingUpCooldown = 2f;
        private int currentIndex;
        private GameObject currentWeapon;
        private bool isReloading;
        

  
        #endregion


        #region MonoBehaviour Callbacks

        void Start()
        {
            if (photonView.IsMine) foreach(Gun g in loadout) g.Initialize();
            Equip(0);
            
            //rightArmLocation = this.gameObject.transform.Find("Design/RightHandTarget");
            //leftArmLocation = this.gameObject.transform.Find("Weapon/LeftHandTarget");
        }

        // Update is called once per frame
        void Update()
        {

            if(Pause.paused && photonView.IsMine) return;

            if(photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            }

            if(photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2)) 
            {
                photonView.RPC("Equip", RpcTarget.All, 1);
            }

            if(currentWeapon != null) 
            {   
                if(photonView.IsMine) 
                {

                    switch(loadout[currentIndex].firingType)
                    {
                        //Semi-Auto
                        case 0:
                            if(Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading)
                            {   
                                //Shoot if ammo is there
                                if(loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            
                            }
                            break;

                        //Full Auto
                        case 1:
                            if(Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading)
                            {      
                                //Shoot if ammo is there
                                if(loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        
                            }
                            else if(!Input.GetMouseButton(0))
                            {
                                
                                //Reset Accuracy
                                fullAutoAccuracyCooldown = 0.25f;

                            }
                            break;

                        //Burst Fire
                        case 2:
                            if(Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading )
                            {   
                                //Shoot if ammo is there
                                if(loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                                
                            
                            } 
                            else if(Input.GetMouseButtonUp(0)  && currentCooldown <= 0 && !isReloading) 
                            {
                                
                                loadout[currentIndex].ResetBurst();
                            }
                            break;

                        //Charge Fire
                        case 3:
                            if(Input.GetMouseButton(0) && !isReloading && currentGunData.GetClip() > 0)
                            {   
                                if(chargingUpCooldown == 2f) {
                                    sfx.Stop();
                                    sfx.clip = currentGunData.chargingSound;
                                    sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
                                    sfx.Play();
                                }

                                if (chargingUpCooldown > 0) chargingUpCooldown -= Time.deltaTime;
                            
                            } 
                            else if(!Input.GetMouseButton(0)  && chargingUpCooldown <= 0 && !isReloading) 
                            {
                                
                                //Shoot if ammo is there
                                if(loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                                chargingUpCooldown = 2f;    

                            }
                            else if(!Input.GetMouseButton(0)  && chargingUpCooldown > 0 && chargingUpCooldown < 2f && !isReloading) 
                            {
                                
                                sfx.Stop();
                                chargingUpCooldown = 2f;    

                            } 
                            else 
                            {
                                chargingUpCooldown = 2f;
                            }
                            break;

                        //Catch All Case   
                        default:
                            break;
                    }

                    //reload
                    if(Input.GetKeyDown(KeyCode.R) && loadout[currentIndex].GetStash() > 0 && loadout[currentIndex].clipsize != loadout[currentIndex].GetClip() && !isReloading) StartCoroutine(Reload(loadout[currentIndex].reloadTime));

                    //cooldown
                    if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
                }
                        
                        
                //Weapon position elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
                //rightArmLocation.data.target.position = currentWeapon.transform.Find("Anchor/Design/RightArm").position;
                //leftArmLocation.data.target.position = currentWeapon.transform.Find("Anchor/Design/LeftArm").position;
                         
            }

        }


        #endregion


        #region Private Methods


        [PunRPC]
        void Equip(int p_ind)
        {
            
            if(loadout.Count - 1 < p_ind) return; 
            
            if(currentWeapon != null)
            {
                if(isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }

            currentIndex = p_ind;


            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

            currentWeapon = t_newWeapon;
            currentGunData = loadout[p_ind];

            if(photonView.IsMine) {
                ChangeLayersRecursively(t_newWeapon, 14);
                playEquip();
            } 
            else ChangeLayersRecursively(t_newWeapon, 0);



            //rightArmLocation.data.target.position = currentWeapon.transform.Find("Anchor/Design/RightArm").position;
            //leftArmLocation.data.target.position = currentWeapon.transform.Find("Anchor/Design/LeftArm").position;
        } 


        [PunRPC]
        void PickupWeapon(string name)
        {
            Gun newWeapon = GunLibrary.FindGun(name);
            newWeapon.Initialize();

            if(loadout.Count >= 2) 
            {
                if(loadout[0].gunName != name && loadout[1].gunName != name) {
                    loadout[currentIndex] = newWeapon;
                    Equip(currentIndex);
                }
            }
            else 
            {
                loadout.Add(newWeapon);
                Equip(loadout.Count - 1);

            }
        }


        private void ChangeLayersRecursively(GameObject p_target, int p_layer)
        {
            p_target.layer = p_layer;
            foreach(Transform a in p_target.transform) ChangeLayersRecursively(a.gameObject, p_layer);
        }


        public bool Aim(bool p_isAiming)
        {
            if(!currentWeapon) return false;
            if(isReloading || !currentGunData.canBeAimed) p_isAiming = false;

            isAiming = p_isAiming;
            
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");
            GameObject t_sight_toggle = currentWeapon.transform.Find("Anchor/Design/Body/Sights").gameObject;


            if(p_isAiming) 
            {
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
                t_sight_toggle.SetActive(true);
            }
            else 
            {
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
                t_sight_toggle.SetActive(false);
            }

            return p_isAiming;
        }



        [PunRPC]
        void Shoot()
        {   
            Transform t_spawn = transform.Find("Camera/Normal Camera");

            //cooldown
            currentCooldown = loadout[currentIndex].firerate;


            //Accuracy
            float t_accuracyImpact = 1f;
            if(currentGunData.firingType == 1)
            {   
                if (fullAutoAccuracyCooldown > 0) fullAutoAccuracyCooldown -= Time.deltaTime;
                t_accuracyImpact = (0.25f - fullAutoAccuracyCooldown) * 4f;
            }
           

            float t_bloomImpact = 0;
            if(isAiming) 
            {
                t_bloomImpact = loadout[currentIndex].adsBloom;
            } 
            else 
            {
                t_bloomImpact = loadout[currentIndex].bloom;
            }



            for (int i = 0; i < Mathf.Max(1, currentGunData.pellets); i++)
            {
                //bloom
                Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-t_bloomImpact * t_accuracyImpact, t_bloomImpact * t_accuracyImpact) * t_spawn.up;
                t_bloom += Random.Range(-t_bloomImpact * t_accuracyImpact, t_bloomImpact * t_accuracyImpact) * t_spawn.right;
                t_bloom -= t_spawn.position;
                t_bloom.Normalize();
            

                //raycast
                RaycastHit t_hit = new RaycastHit();
                if(Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
                {  
                    bool t_playerHit = false;

                    if(photonView.IsMine)
                    {   
                        //shooting other player on network
                        if(t_hit.collider.gameObject.layer == 15)
                        {
                            t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, currentGunData.damage, PhotonNetwork.LocalPlayer.ActorNumber);
                            t_playerHit = true;
                        }
                        else if (t_hit.collider.gameObject.layer == 16)
                        {
                            t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, currentGunData.damage*currentGunData.headShotMultiplier, PhotonNetwork.LocalPlayer.ActorNumber);
                            t_playerHit = true;
                        }


                    }

                    if(t_playerHit)
                    {
                        GameObject t_newHole = Instantiate (bloodParticles, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                        t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                        Destroy(t_newHole, 2f);
                    }
                    else
                    {
                        GameObject t_newHole = Instantiate (bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                        t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                        Destroy(t_newHole, 5f);
                    }
                }
            }


            //sounds
            sfx.Stop();
            sfx.clip = currentGunData.gunshotSound;
            sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
            sfx.Play();
            
            
            //gun muzzle fx
            Transform t_muzzle = currentWeapon.transform.Find("Anchor/Resources/Muzzle");
            GameObject t_muzzleFlash = Instantiate (currentGunData.muzzleFlash, t_muzzle.position, t_muzzle.rotation) as GameObject;
            t_muzzleFlash.transform.parent = t_muzzle;
            t_muzzleFlash.transform.localScale = new Vector3(currentGunData.muzzleFlashScale, currentGunData.muzzleFlashScale, currentGunData.muzzleFlashScale);
            Destroy(t_muzzleFlash, 1f);

            
            //gun recoil fx
            if(isAiming) currentWeapon.transform.Rotate(-loadout[currentIndex].adsRecoil, 0,0);
            else currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0,0);
            
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
            //if(currentGunData.recovery) currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);


        }

        [PunRPC]
        private void TakeDamage(int p_damage, int p_actor)
        {
            GetComponent<Player>().TakeDamage(p_damage, p_actor);
        }

        IEnumerator Reload(float p_wait)
        {
            isReloading = true;
            currentWeapon.SetActive(false);
            
            playReload();

            GameObject t_reloadProp = Instantiate (currentGunData.throwable, weaponParent.position, weaponParent.rotation) as GameObject;
            t_reloadProp.GetComponent<Rigidbody>().AddForce(weaponParent.forward * weaponThrowSpeed, ForceMode.Impulse);

            yield return new WaitForSeconds(p_wait);

            GameObject t_reloadExplosion = Instantiate (reloadExplosion, t_reloadProp.transform.position, t_reloadProp.transform.rotation) as GameObject;
            t_reloadExplosion.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            Destroy(t_reloadExplosion, 2f);
            Destroy(t_reloadProp);

            loadout[currentIndex].Reload();
            currentWeapon.SetActive(true);
            isReloading = false;

            playEquip();
           
            
        }


        private void playReload() {
            sfx.Stop();
            sfx.clip = currentGunData.reloadSound;
            sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
            sfx.Play();
        }


        private void playEquip() {
            sfx.Stop();
            sfx.clip =  currentGunData.equipSound;
            sfx.pitch = 1 -  currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
            sfx.Play();
        }
        #endregion

        
        #region Public Methods

        public void RefreshAmmo(Text p_text) 
        {
            int t_clip = loadout[currentIndex].GetClip();
            int t_stash = loadout[currentIndex].GetStash();

            p_text.text = t_clip.ToString() + " / " + t_stash.ToString();
        }

        #endregion
    
    }
}