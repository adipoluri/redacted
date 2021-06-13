using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.AstralSky.FPS
{
    public class Launcher : MonoBehaviourPunCallbacks
    {   
        #region Variables
        public Camera mainCam;

        private bool zoomingIn = false;
        private Quaternion camCenter;
        private Transform cams;

        #endregion


        #region Private Methods
        private void Start() {
            mainCam.fieldOfView =  179f;   
            cams = mainCam.transform;
            camCenter = cams.localRotation;
 
        }

        private void Update() {
            //Zoom
            if(zoomingIn) {
                mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, 50f, Time.deltaTime * 1f);

                if(mainCam.fieldOfView == 50f)
                {
                    zoomingIn = false;
                }
            }

            CamEffects();
        }

        private void CamEffects()
        {
            float t_y = Input.GetAxisRaw("Mouse Y") * 100 * Time.deltaTime;
            float t_x = Input.GetAxisRaw("Mouse X") * 100 * Time.deltaTime;
         

            Quaternion t_y_adj = Quaternion.AngleAxis(t_y, -Vector3.right);
            Quaternion t_y_delta = cams.localRotation * t_y_adj;

            if(Quaternion.Angle(camCenter, t_y_delta) < 7) 
            {
                cams.localRotation = Quaternion.Lerp(cams.localRotation, t_y_delta, Time.deltaTime * 6f);
            }

            
            Quaternion t_x_adj = Quaternion.AngleAxis(t_x, Vector3.up);
            Quaternion t_x_delta = cams.localRotation * t_x_adj;

            if(Quaternion.Angle(camCenter, t_x_delta) < 15) 
            {
                cams.localRotation = Quaternion.Lerp(cams.localRotation, t_x_delta, Time.deltaTime * 6f);
            }
        }
        #endregion



        #region Public Methods

        public void ZoomIn() {
            zoomingIn = true;
        }

        public void Awake() 
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }


        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
        }


        public override void OnJoinedRoom()
        {
            StartGame();

            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();

            base.OnJoinRandomFailed(returnCode, message);
        }

        

        public void Connect()
        {
            PhotonNetwork.GameVersion = "0.0.2";
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        
        public void Create()
        {
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame()
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel(1);
            }
        }
    }

    #endregion
}