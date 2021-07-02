using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;


namespace com.AstralSky.FPS
{
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Variables

        [Header("Movement Settings")]
        public float speed;
        public float sprintModifier;
        public float crouchModifier;
        public float jumpForce;
        public float crouchAmount;
        
        [Header("Player Health")]
        public float maxHealth;


        [Header("Object Setup")]
        public Camera normalCam;
        public Camera weaponCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;
        public GameObject mesh;
        public GameObject standingCollider;
        public GameObject crouchingCollider;
        public GameObject standingHeadCollider;
        public GameObject crouchingHeadCollider;
        public GameObject deathAnimPrefab;
        public AudioSource ambientAudio;
        public AudioClip respawnSound; 
        [HideInInspector] public ProfileData playerProfile;
        public TextMeshPro playerUsername;
        

        private Transform ui_healthBar;
        private Text ui_ammo;
        private TMP_Text ui_username;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;
        private Vector3 weaponParentCurrentPos;

        private float movementCounter;
        private float idleCounter;

        private float baseFOV;
        private float sprintFOVModifier = 1.25f;
        private Vector3 origin;
        private float currentHealth;
        private Manager manager;
        private Weapon weapon;

        private bool crouched;
        private bool isAiming;
        private float aimAngle;



        #endregion

        #region Photon Callbacks

        public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
        {
            if(p_stream.IsWriting)
            {
                p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
            }
            else
            {
                aimAngle = (int) p_stream.ReceiveNext() / 100f;
            }
        }

        #endregion
    
        #region MonoBehaviour Callbacks

    
        void Start()
        {  
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            currentHealth = maxHealth;

            if(!photonView.IsMine) 
            {
                gameObject.layer = 15;
                standingCollider.gameObject.layer = 15; 
                crouchingCollider.gameObject.layer = 15;
                standingHeadCollider.gameObject.layer = 16;
                crouchingHeadCollider.gameObject.layer = 16;
                //ChangeLayerRecursively(mesh.transform, 15);
            }

            cameraParent.SetActive(photonView.IsMine);
            if(Camera.main) Camera.main.enabled = false;
            
            baseFOV = normalCam.fieldOfView;
            origin = normalCam.transform.localPosition;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentCurrentPos = weaponParentOrigin;
            

            //Respawn Sound
            ambientAudio.Stop();
            ambientAudio.clip =  respawnSound;
            ambientAudio.Play();

            //set Healthbar
            if(photonView.IsMine)
            {
                ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                ui_username = GameObject.Find("HUD/Username/name").GetComponent<TMP_Text>();
                
                RefreshHealthBar();
                ui_username.text = Launcher.myProfile.username;

                photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfile.username, Launcher.myProfile.level, Launcher.myProfile.xp);
            }
        }


        private void ChangeLayerRecursively(Transform p_trans, int p_layer)
        {
            p_trans.gameObject.layer = p_layer;
            foreach(Transform t in p_trans) ChangeLayerRecursively(t, p_layer);
        }

        void Update()
        {
            
            if(!photonView.IsMine) 
            {
                RefreshMultiplayerState();
                return;
            } 

            //Axis
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool crouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool pause = Input.GetKeyDown(KeyCode.Escape);
            
            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
            bool isCrouching = crouch && !isSprinting && !isJumping && isGrounded;

            //Pause
            if(pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }

            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                crouch = false;
                pause = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
                isCrouching = false;
            }


            //Crouching
            if(isCrouching)
            {
                photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
            }


            //Debug Death Reset
            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(500, -1);


            
            //Head bob
            if(!isGrounded) 
            {
                HeadBob(idleCounter, 0.02f,0.02f);
                idleCounter += 0;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (t_hmove == 0 && t_vmove == 0) 
            {
                HeadBob(idleCounter, 0.02f,0.02f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            } 
            else if (!isSprinting && !crouched) 
            {
                HeadBob(movementCounter, 0.035f,0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            } 
            else if(crouched)
            {
                HeadBob(movementCounter, 0.02f,0.02f);
                movementCounter += Time.deltaTime * 1.75f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else 
            {
                HeadBob(movementCounter, 0.15f,0.075f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f); 
            }


            //UI Refresh
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);
        }


        void FixedUpdate()
        {

            if(!photonView.IsMine) return;


            //Axis
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKey(KeyCode.Space);
            bool aim = Input.GetMouseButton(1);

            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;
            isAiming = aim && !isSprinting;
            
            //paused
            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                isGrounded = false;
                isJumping = false;
                isSprinting = false;
                isAiming = false;
            }

            //Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustSpeed = speed;
            if (isSprinting) 
            {
                if(crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                t_adjustSpeed *= sprintModifier;
            } 
            else if (crouched)
            {
                t_adjustSpeed *= crouchModifier;
            }

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustSpeed * Time.fixedDeltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            //Jumping
            if(isJumping) 
            {
                if(crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                Vector3 t_velCurrent = rig.velocity;
                rig.velocity = new Vector3(t_velCurrent.x, jumpForce, t_velCurrent.z);
                //rig.AddForce(Vector3.up * jumpForce);
            }



            //Aiming
            isAiming = weapon.Aim(isAiming);


            //Camera Stuff
            if (isSprinting) 
            { 
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
            } 
            else if(isAiming) 
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * weapon.currentGunData.mainFOV, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV * weapon.currentGunData.weaponFOV, Time.deltaTime * 8f);
            } 
            else 
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
            }


            if (crouched) 
            { 
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition,origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                weaponCam.transform.localPosition = Vector3.Lerp(weaponCam.transform.localPosition,origin + Vector3.down * crouchAmount, Time.deltaTime * 32f);
            } 
            else 
            {
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin, Time.deltaTime * 6f);
                weaponCam.transform.localPosition = Vector3.Lerp(weaponCam.transform.localPosition,origin, Time.deltaTime * 6f);
            }
        }

        #endregion


        #region Private Methods
        

        void RefreshMultiplayerState ()
        {
            float cacheEuly = weaponParent.localEulerAngles.y;

            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cacheEuly;
            
            weaponParent.localEulerAngles = finalRotation;
        }


        void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            float t_aim_adjust = 1f;
            if(isAiming) t_aim_adjust = 0.1f;
            targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
        }


        void RefreshHealthBar() {
            float t_health_ratio = (float) currentHealth / (float)maxHealth;
            ui_healthBar.localScale = Vector3.Lerp(ui_healthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }


        [PunRPC]
        void SetCrouch(bool p_state)
        {
            if (crouched == p_state) return;

            crouched = p_state;

            if(crouched)
            {
                standingCollider.SetActive(false);
                crouchingCollider.SetActive(true);
                weaponParentCurrentPos += Vector3.down * crouchAmount;
            }
            else
            {
                standingCollider.SetActive(true);
                crouchingCollider.SetActive(false);
                weaponParentCurrentPos -= Vector3.down * crouchAmount;
            }
        }


        #endregion

        #region Public Methods


        public void TakeDamage (int p_damage, int p_actor)
        {
            if(photonView.IsMine)
            {
                currentHealth -= p_damage;
                RefreshHealthBar();

                if(currentHealth <= 0)
                {
                    photonView.RPC("Die", RpcTarget.All);
                    manager.Spawn();
                    manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

                    if(p_actor >= 0) manager.ChangeStat_S(p_actor, 0, 1);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            
        }


        [PunRPC]
        void Die()
        {
            GameObject t_deathAnim = Instantiate (deathAnimPrefab, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
            Destroy(t_deathAnim, 7f);
        }

        

        [PunRPC]
        private void SyncProfile(string p_username, int p_level, int p_xp)
        {
            playerProfile = new ProfileData(p_username, p_level, p_xp);
            playerUsername.text = p_username;
        }

        #endregion


    }
}

