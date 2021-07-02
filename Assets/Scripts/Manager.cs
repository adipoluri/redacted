using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;


namespace com.AstralSky.FPS
{

    public class PlayerInfo
    {
        public ProfileData profile;
        public int actor;
        public short kills;
        public short deaths;

        public PlayerInfo(ProfileData p, int a, short k, short d)
        {
            this.profile = p;
            this.actor = a;
            this.kills = k;
            this.deaths = d;
        }
    }
    public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
       
        #region Fields

        public string playerPrefab;
        public Transform[] spawnPoints;

        public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        public int myind;

        private TMP_Text ui_mykills;
        private TMP_Text ui_mydeaths;
        private Transform ui_leaderboard;

        #endregion

        #region Codes

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayer,
            ChangeStat
        }

        #endregion

        public void Start()
        {
            ValidateConnection();
            InitializeUI();
            NewPlayer_S(Launcher.myProfile);
            Spawn();
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                if(ui_leaderboard.gameObject.activeSelf) ui_leaderboard.gameObject.SetActive(false);
                else Leaderboard(ui_leaderboard);
            }
        }


        public void Spawn()
        {
            Transform t_spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            PhotonNetwork.Instantiate(playerPrefab, t_spawn.position, t_spawn.rotation);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            EventCodes e = (EventCodes) photonEvent.Code;
            object[] o = (object[]) photonEvent.CustomData;

            switch(e) 
            {
                case EventCodes.NewPlayer:
                    NewPlayer_R(o);
                    break;
                case EventCodes.UpdatePlayer:
                    UpdatePlayers_R(o);
                    break;
                case EventCodes.ChangeStat:
                    ChangeStat_R(o);
                    break; 
            }
        }


        private void InitializeUI()
        {
            ui_mykills = GameObject.Find("HUD/Stats/Kills/Kills").GetComponent<TMP_Text>();
            ui_mydeaths = GameObject.Find("HUD/Stats/Deaths/Deaths").GetComponent<TMP_Text>();
            ui_leaderboard = GameObject.Find("HUD").transform.Find("Leaderboard").transform;

            RefreshMyStats();
        }


        private void RefreshMyStats()
        {
            if(playerInfo.Count > myind)
            {
                ui_mykills.text = $"{playerInfo[myind].kills} kills";
                ui_mydeaths.text = $"{playerInfo[myind].deaths} deaths";
            }
            else
            {
                ui_mykills.text = "0 kills";
                ui_mydeaths.text = "0 deaths";
            }
        }


        private void ValidateConnection()
        {
            if(PhotonNetwork.IsConnected) return;
            SceneManager.LoadScene(0);
        }


        private void Leaderboard (Transform p_lb)
        {
            for (int i = 2; i < p_lb.childCount; i++)
            {
                Destroy(p_lb.GetChild(i).gameObject);
            }

            //set Details
            p_lb.Find("Header/Title").GetComponent<TMP_Text>().text = "Deathmatch";

            //cache prefab
            GameObject playercard = p_lb.GetChild(1).gameObject;
            playercard.SetActive(false);

            //sort
            List<PlayerInfo> sorted = SortPlayers(playerInfo);

            //display
            bool t_alternateColors = false;
            foreach (PlayerInfo a in sorted)
            {
                GameObject newCard = Instantiate(playercard, p_lb) as GameObject;

                //if(t_alternateColors) newCard.GetComponent<Image>().color = new Color32(0,0,0,0);
                //t_alternateColors = !t_alternateColors;

                newCard.transform.Find("Name").GetComponent<TMP_Text>().text = a.profile.username.ToString();
                newCard.transform.Find("Kills").GetComponent<TMP_Text>().text = a.kills.ToString();
                newCard.transform.Find("Deaths").GetComponent<TMP_Text>().text = a.deaths.ToString();
                newCard.transform.Find("Score").GetComponent<TMP_Text>().text = (a.kills*100 - a.deaths*50).ToString();

                newCard.SetActive(true);
            }

            //actiuvate
            p_lb.gameObject.SetActive(true);
        }


        private List<PlayerInfo> SortPlayers (List<PlayerInfo> p_info)
        {
            List<PlayerInfo> sorted = new List<PlayerInfo>();

            while(sorted.Count < p_info.Count)
            {

                // set defaults
                short highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if(sorted.Contains(a)) continue;
                    if(a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                //add Player
                sorted.Add(selection);
            }

            return sorted;
        }



        #region Events

        public void NewPlayer_S(ProfileData p)
        {
            object[] package = new object[6];

            package[0] = p.username;
            package[1] = p.level;
            package[2] = p.xp;
            package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
            package[4] = (short) 0;
            package[5] = (short) 0;

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewPlayer,
                package,
                new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient},
                new SendOptions {Reliability = true}
            );
        }

        public void NewPlayer_R(object[] Data)
        {
            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string) Data[0],
                    (int) Data[1],
                    (int) Data[2]
                    ),
                (int) Data[3],
                (short) Data[4],
                (short) Data[5]
            );

            playerInfo.Add(p);

            UpdatePlayer_S(playerInfo);
        }


        public void UpdatePlayer_S(List<PlayerInfo> info)
        {
            object[] package = new object[info.Count];

            for(int i = 0; i < info.Count; i++)
            {
                object[] piece = new object[6];

                piece[0] = info[i].profile.username;
                piece[1] = info[i].profile.level;
                piece[2] = info[i].profile.xp;
                piece[3] = info[i].actor;
                piece[4] = info[i].kills;
                piece[5] = info[i].deaths;

                package[i] = piece;

            }

            PhotonNetwork.RaiseEvent (
                (byte) EventCodes.UpdatePlayer,
                package,
                new RaiseEventOptions{Receivers = ReceiverGroup.All},
                new SendOptions { Reliability = true}
            );
        }

        public void UpdatePlayers_R(object[] data)
        {
            playerInfo = new List<PlayerInfo>();

            for(int i = 0; i < data.Length; i++)
            {
                object[] extract = (object[]) data[i];

                PlayerInfo p = new PlayerInfo (
                    new ProfileData (
                        (string) extract[0],
                        (int) extract[1],
                        (int) extract[2]
                    ),
                    (int) extract[3],
                    (short) extract[4],
                    (short) extract[5]
                );

                playerInfo.Add(p);
                
                if(PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myind = i;
            }
    
        }

        public void ChangeStat_S(int actor, byte stat,byte amt)
        {
            object[] package = new object[] {actor,stat,amt};

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.ChangeStat,
                package,
                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true}
            );
        }

        public void ChangeStat_R(object[] data)
        {
            int actor = (int) data[0];
            byte stat = (byte) data[1];
            byte amt = (byte) data[2];

            for(int i = 0; i < playerInfo.Count; i++)
            {
                if(playerInfo[i].actor == actor)
                {
                    switch(stat)
                    {
                        case 0: //kills
                        playerInfo[i].kills += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : kills = {playerInfo[i].kills}");
                        break;

                        case 1: //deaths
                        playerInfo[i].deaths += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : deaths = {playerInfo[i].deaths}");
                        break;

                    }

                    if(i == myind) RefreshMyStats();
                    if(ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);

                    return;
                }
            }
        }

        #endregion

    }
    
}