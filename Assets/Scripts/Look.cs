using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace com.AstralSky.FPS
{
    public class Look : MonoBehaviourPunCallbacks
    {

        #region Variables

        public static bool cursorLock = true;
        
        public Transform player;
        public Transform normalCam;
        public Transform weaponCam;

        public Transform weapon;

        public float xSensitivity;
        public float ySensitivity;
        public float maxAngle;

        private Quaternion camCenter;

        #endregion
        
        #region Monobehaviour Callbacks


        void Start()
        {
            camCenter = normalCam.localRotation;
        }

        // Update is called once per frame
        void Update()
        {
            if(!photonView.IsMine) return;
            if(Pause.paused) return;

            SetY();
            SetX();

            UpdateCursorLock();

            weaponCam.rotation = normalCam.rotation;
        }


        #endregion

        #region Private Methods


        void SetY()
        {
            float t_input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_input, -Vector3.right);
            Quaternion t_delta = normalCam.localRotation * t_adj;

            if(Quaternion.Angle(camCenter, t_delta) < maxAngle) 
            {
                normalCam.localRotation = t_delta;
            }
            
            weapon.rotation = normalCam.rotation;
        }

        void SetX()
        {
            float t_input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_input, Vector3.up);
            Quaternion t_delta = player.localRotation * t_adj;
            player.localRotation = t_delta;
        }

        void UpdateCursorLock()
        {
            if(cursorLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLock = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if(Input.GetKeyDown(KeyCode.Escape) || !Pause.paused)
                {
                    cursorLock = true;
                }
            }
            
        }

        #endregion
    
    }

}