using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

namespace com.AstralSky.FPS
{

    [System.Serializable]
    public class ProfileData
    {
        public ProfileData(string u, int l, int x)
        {
            this.username = u;
            this.level = l;
            this.xp = x;
        }

        public ProfileData()
        {
            this.username = "DEFAULT";
            this.level = 0;
            this.xp = 0;
        }

        public string username;
        public int level;
        public int xp;

        //object[] convertToObjectArr ()
        //{
        //    object[] ret = new object();
        //    return ret;
        //}
    }

    
    public class Launcher : MonoBehaviourPunCallbacks
    {   
        #region Variables

        [Header("GAME VERSION")]
        public string gameVersion;

        public TMP_InputField usernameField;
        public TMP_InputField roomnameField;
        public static ProfileData myProfile = new ProfileData();

        public GameObject tabMain;
        public GameObject tabRooms;
        public GameObject tabCreate;

        public GameObject buttonRoom;

        public Camera mainCam;

        private List<RoomInfo> roomList;
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
            
            myProfile = Data.LoadProfile();
            usernameField.text = myProfile.username;

            Connect();
        }


        public override void OnConnectedToMaster()
        {
            Debug.Log("CONNECTED!");

            PhotonNetwork.JoinLobby();
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
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        
        public void Create()
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 20;    

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", 0);
            options.CustomRoomProperties = properties;

            PhotonNetwork.CreateRoom(roomnameField.text, options);
        }

        public void ChangeMap()
        {

        }
        
        public void TabCloseAll()
        {
            tabMain.SetActive(false);
            tabRooms.SetActive(false);
            tabCreate.SetActive(false);
        }

        public void tabOpenMain()
        {
            TabCloseAll();
            tabMain.SetActive(true);
        }

        public void TabOpenRooms ()
        {
            TabCloseAll();
            tabRooms.SetActive(true);
        }

         public void TabOpenCreate ()
        {
            TabCloseAll();
            tabCreate.SetActive(true);
        }

        private void ClearRoomlist()
        {
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
            foreach (Transform a in content) Destroy(a.gameObject);
        }

        private void VerifyUsername()
        {
            if (string.IsNullOrEmpty(usernameField.text)) 
            {
                myProfile.username = "RandomPlayer_" + Random.Range(100,1000);
            }
            else 
            {
                myProfile.username = usernameField.text;

            }
        }


        public override void OnRoomListUpdate(List<RoomInfo> p_list)
        {
            
            roomList = p_list;
            ClearRoomlist();
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");

            foreach(RoomInfo a in roomList)
            {
                GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

                newRoomButton.transform.Find("Name").GetComponent<TMP_Text>().text = a.Name;
                newRoomButton.transform.Find("Capacity").GetComponent<TMP_Text>().text = a.PlayerCount + "/" + a.MaxPlayers;

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
            }
            base.OnRoomListUpdate(roomList);
        }


        public void JoinRoom (Transform p_button)
        {
            string t_roomName = p_button.transform.Find("Name").GetComponent<TMP_Text>().text;

            VerifyUsername(); 

            PhotonNetwork.JoinRoom(t_roomName);
        }
        public void StartGame()
        {
            VerifyUsername(); 

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Data.SaveProfile(myProfile);
                PhotonNetwork.LoadLevel(1);
            }
        }
    }

    #endregion
}