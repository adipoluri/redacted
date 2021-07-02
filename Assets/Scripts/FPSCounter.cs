using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace com.AstralSky.FPS
{
    public class FPSCounter : MonoBehaviour
    {
    
        public float timer, refresh, avgFramerate;
        string display = "{0} FPS";
        private TMP_Text m_Text;
    
        private void Start()
        {
            m_Text = GetComponent<TMP_Text>();
        }
    
    
        private void Update()
        {
            //Change smoothDeltaTime to deltaTime or fixedDeltaTime to see the difference
            float timelapse = Time.smoothDeltaTime;
            timer = timer <= 0 ? refresh : timer -= timelapse;
    
            if(timer <= 0) avgFramerate = (int) (1f / timelapse);
            m_Text.text = string.Format(display,avgFramerate.ToString());
        }
    }
}