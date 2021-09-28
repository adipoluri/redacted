using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace com.AstralSky.FPS
{
    public class Settings : MonoBehaviour
    {

        public AudioMixer audioMixer_AMB;
        public AudioMixer audioMixer_GAME;

        public List<GameObject> crossHairs;
        public Transform crossHairParent;
        public GameObject player;

        public void SetAmbientVolume (float volume)
        {
            audioMixer_AMB.SetFloat("Ambient Volume", volume);
        }


        public void SetGameVolume (float volume)
        {
            audioMixer_GAME.SetFloat("Game Volume", volume);
        }

        public void setCrossHiar(int p_ind)
        {
            foreach (Transform t in crossHairParent) {
                GameObject.Destroy(t.gameObject);
            }

            GameObject t_ch = Instantiate(crossHairs[p_ind], crossHairParent) as GameObject;
        }

         public void SetSensitivity (float volume)
        {
            player.GetComponent<Look>().updateSens(volume);
            player.GetComponent<Look>().updateSens(volume);
        }




    }
}
