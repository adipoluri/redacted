using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.AstralSky.FPS
{
    public class HealthPickup : MonoBehaviourPunCallbacks
    {
        public float healthLevel;
        public float cooldown;
        public GameObject gunDisplay;
        public GameObject milk;
        public GameObject HealEffect;
        public AudioClip healSound;
        public AudioSource sfx;
        public List<GameObject> targets;
        private bool isDisabled;
        private float wait;


        private void Start() {
            foreach(Transform t in gunDisplay.transform) Destroy(t.gameObject);
            GameObject newDisplay = Instantiate(milk, gunDisplay.transform.position, gunDisplay.transform.rotation) as GameObject;   
            newDisplay.transform.SetParent(gunDisplay.transform);
        }


        private void Update() {
            if(isDisabled) 
            {   
                if(wait > 0) 
                {
                    wait -= Time.deltaTime;
                }
                else 
                {
                    Enable();
                }
            }
            
        }


        private void OnTriggerEnter(Collider other) {
            if(other.attachedRigidbody == null) return;

            if(other.attachedRigidbody.gameObject.tag.Equals("Player"))
            {
                GameObject t_effects = Instantiate (HealEffect, gunDisplay.transform.position, gunDisplay.transform.rotation) as GameObject;
                t_effects.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                Destroy(t_effects, 2f);
                
                other.attachedRigidbody.gameObject.GetComponent<Player>().Heal();
                photonView.RPC("DisableHealth", RpcTarget.All);
            }
        }

        [PunRPC]
        public void DisableHealth()
        {
            isDisabled = true;
            wait = cooldown;
            sfx.Stop();
            sfx.Play();
            foreach (GameObject a in targets) a.SetActive(false);
        }

        private void Enable()
        {
            isDisabled = false;
            wait = 0;

            foreach (GameObject a in targets) a.SetActive(true);
        }
    }
}