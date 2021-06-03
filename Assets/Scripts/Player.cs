using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;


namespace com.AstralSky.FPS
{
    public class Player : MonoBehaviourPunCallbacks
    {

        #region Variables


        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public float maxHealth;
        public Camera normalCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;
        
        private Transform ui_healthBar;
        private Text ui_ammo;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;

        private float movementCounter;
        private float idleCounter;

        private float baseFOV;
        private float sprintFOVModifier = 1.25f;
        private float currentHealth;
        private Manager manager;
        private Weapon weapon;

        #endregion

        #region MonoBehaviour Callbacks


        void Start()
        {  
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            currentHealth = maxHealth;

            cameraParent.SetActive(photonView.IsMine);
            if(!photonView.IsMine) gameObject.layer = 15;
            
            if(Camera.main) Camera.main.enabled = false;
            
            baseFOV = normalCam.fieldOfView;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;

            //set Healthbar
            if(photonView.IsMine)
            {
                ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefreshHealthBar();
            }
        }


        void Update()
        {
            
            if(!photonView.IsMine) return;


            //Axis
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKey(KeyCode.Space);

            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

            //Jumping
            if(isJumping) 
            {
                rig.AddForce(Vector3.up * jumpForce);
            }


            //Debug Death Reset
            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(500);


            //Head bob
            if (t_hmove == 0 && t_vmove == 0) {
                HeadBob(idleCounter, 0.02f,0.02f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            } else if (!isSprinting) {
                HeadBob(movementCounter, 0.035f,0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            } else {
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

            //States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;


            //Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustSpeed = speed;
            if (isSprinting) t_adjustSpeed *= sprintModifier;

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustSpeed * Time.fixedDeltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            //Field of View
            if (isSprinting) { 
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.fixedDeltaTime * 8f);
            } else {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.fixedDeltaTime * 8f);
            }
        }

        #endregion


        #region Private Methods


        void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            float t_aim_adjust = 1f;
            if(weapon.isAiming) t_aim_adjust = 0.1f;
            targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
        }

        void RefreshHealthBar() {
            float t_health_ratio = (float) currentHealth / (float)maxHealth;
            ui_healthBar.localScale = Vector3.Lerp(ui_healthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
        }

        #endregion

        #region Public Methods


        public void TakeDamage (int p_damage)
        {
            if(photonView.IsMine)
            {
                currentHealth -= p_damage;
                RefreshHealthBar();

                if(currentHealth <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            
        }


        #endregion
    
    }
}
