using Bolt;
using DetentionRoom.Networking.Interfaces;
using DetentionRoom.Scripts;
using DetentionRoom.Scripts.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DetentionRoom.Networking.States.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(BoltEntity))]
    [RequireComponent(typeof(AudioListener))]
    public class Player : EntityEventListener<IPlayer>
    {
        #region Variables

        /// <summary>
        /// Collider of the player in form of a capsule.
        /// </summary>
        private CapsuleCollider _capsuleCollider;

        public GameObject characterModel;
        
        /// <summary>
        /// Component to receive to sound.
        /// </summary>
        private AudioListener _audioListener;

        /// <summary>
        /// The camera component.
        /// </summary>
        public Camera playerCamera;

        /// <summary>
        /// The physic component.
        /// </summary>
        private Rigidbody _rigidBody;

        /// <summary>
        /// The transform component.
        /// </summary>
        private Transform _playerTransform;

        /// <summary>
        /// The origin position of the ground checking ray.
        /// </summary>
        public Transform collisionTransform;

        /// <summary>
        /// Weapon system.
        /// </summary>
        public WeaponSystem weaponSystem;

        /// <summary>
        /// Reference of the HUD.
        /// </summary>
        public HUD hud;

        /// <summary>
        /// Sensitivity of the mouse.
        /// </summary>
        [SerializeField] private float lookSensitivity = 10;

        /// <summary>
        /// Field of view.
        /// </summary>
        [SerializeField] private int viewField = 60;

        /// <summary>
        /// Camera rotation.
        /// </summary>
        [SerializeField] private Vector3 cameraRotation = Vector3.zero;

        /// <summary>
        /// Determines if the player is on the ground.
        /// </summary>
        public bool onGround;

        /// <summary>
        /// Is the player jumping.
        /// </summary>
        private bool _jumping;

        /// <summary>
        /// Jump timer
        /// </summary>
        private float _jumpingTimer;

        /// <summary>
        /// Determines the distance that the player needs to be to trigger the onGround-condition
        /// </summary>
        private readonly float _groundCheck = -.05f;

        /// <summary>
        /// Current Speed of the player/// </summary>
        private float _speed;

        /// <summary>
        /// Acceleration over time.
        /// </summary>
        private const float Acceleration = 10f;

        /// <summary>
        /// Player material for the teacher team.
        /// </summary>
        public Material teacherMaterial;

        /// <summary>
        /// Player material for the student team.
        /// </summary>
        public Material studentMaterial;

        /// <summary>
        /// Render component.
        /// </summary>
        public SkinnedMeshRenderer skinnedMeshRenderer;

        /// <summary>
        /// Transform component of the focused-object.
        /// </summary>
        private Transform _focusedObject;

        /// <summary>
        /// Index of the current carries weapon.
        /// </summary>
        private int _activeWeaponIndex;

        /// <summary>
        /// Back Text component of the player to display the username.
        /// </summary>
        public TextMeshProUGUI backUsernameDisplay;

        /// <summary>
        /// Front Text component of the player to display the username.
        /// </summary>
        public TextMeshProUGUI frontUsernameDisplay;

        /// <summary>
        /// Text-component for the focus object.
        /// </summary>
        public TextMeshProUGUI focusText;

        /// <summary>
        /// Collection of sounds for walking.
        /// </summary>
        public AudioClip[] walkSounds;

        /// <summary>
        /// Audio source component.
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// Definition of the controllable inputs.
        /// </summary>
        public struct Input
        {
            public Vector2 Movement;
            public Vector2 Mouse;
            public bool IsCrouching;
            public bool IsSprinting;
            public float ScrollWheel;
            public bool Interaction;
            public bool Reload;
            public bool Jump;
            public bool LeftMouse;
        }

        /// <summary>
        /// Instance of the controllable inputs.
        /// </summary>
        public Input PlayerInput { get; private set; }

        /// <summary>
        /// Rotation of the y-axis of the player.
        /// </summary>
        private float _playerYRotation;

        /// <summary>
        /// Walk direction of the player.
        /// </summary>
        private Vector3 _direction;

        /// <summary>
        /// Is the player currently reloading.
        /// </summary>
        private bool _isReloading;

        /// <summary>
        /// After reloading how much ammo should the weapon have.
        /// </summary>
        private int _newCurrentAmmoInMagazine;

        /// <summary>
        /// Animation parameter for reloading.
        /// </summary>
        private static readonly int IsReloading = Animator.StringToHash("IsReloading");

        /// <summary>
        /// Animation parameter for punching.
        /// </summary>
        private static readonly int IsPunching = Animator.StringToHash("isPunching");
        private static readonly int InputY = Animator.StringToHash("input_y");
        private static readonly int IsCrouching = Animator.StringToHash("isCrouching");

        public Animator animator;
        
        
        #endregion

        #region Unity Methods

        private void Update()
        {
            if (!entity.HasControl)
            {
                return;
            }

            //Retrieve input.
            PlayerInput = new Input
            {
                Movement = new Vector2(UnityEngine.Input.GetAxis("Horizontal"),
                    UnityEngine.Input.GetAxis("Vertical")),
                Mouse = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y")),
                IsCrouching = UnityEngine.Input.GetKey(KeyCode.LeftControl),
                IsSprinting = UnityEngine.Input.GetKey(KeyCode.LeftShift),
                ScrollWheel = UnityEngine.Input.GetAxis("Mouse ScrollWheel"),
                Interaction = UnityEngine.Input.GetKeyDown(KeyCode.E),
                Reload = UnityEngine.Input.GetKeyDown(KeyCode.R),
                Jump = UnityEngine.Input.GetKeyDown(KeyCode.Space),
                LeftMouse = UnityEngine.Input.GetMouseButtonDown(0)
            };

            if (PlayerInput.Reload)
            {
                Weapon currentWeapon = state.Weapons[state.ActiveWeaponIndex];

                if (currentWeapon.CurrentAmmoInMagazine < currentWeapon.MagazineSize &&
                    currentWeapon.EntireAmmo > currentWeapon.CurrentAmmoInMagazine)
                {
                    switch (state.ActiveWeaponIndex)
                    {
                        case 0:
                            StartCoroutine(weaponSystem.chalkWeaponState.Reload());
                            break;

                        case 1:
                            StartCoroutine(weaponSystem.staplerWeaponState.Reload());
                            break;

                        case 2:
                            StartCoroutine(weaponSystem.slingshotWeaponState.Reload());
                            break;
                    }
                }
            }

            #region Jump

            if (PlayerInput.Jump && onGround)
            {
                _jumping = true;
                _jumpingTimer = 0.2f;
            }

            if (_jumpingTimer > 0)
            {
                _jumpingTimer -= Time.deltaTime;
            }

            if (_jumpingTimer <= 0)
            {
                _jumping = false;
            }

            #endregion

            //Cast a ray
            Ray ray = playerCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 3))
            {
                _focusedObject = hit.transform;

                IInteractable intractable = _focusedObject.GetComponent<IInteractable>();
                IFocusable focusable = _focusedObject.GetComponent<IFocusable>();

                if (intractable != null && PlayerInput.Interaction)
                {
                    intractable.Interact(entity);
                }

                if (focusable != null)
                {
                    focusText.text = focusable.Focus();
                }
            }
            else
            {
                focusText.text = "";
            }

            Ray groundRay = new Ray(collisionTransform.position, new Vector3(0, _groundCheck, 0));
            onGround = Physics.Raycast(groundRay, out _, 0.25f);
        }

        private void FixedUpdate()
        {
            if (!entity.HasControl)
            {
                return;
            }

            if (Mathf.Abs(PlayerInput.Movement.x) > 0 || Mathf.Abs(PlayerInput.Movement.y) > 0)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource.PlayOneShot(walkSounds[Random.Range(0, walkSounds.Length)]);
                }
            }

            _direction = _playerTransform.right * PlayerInput.Movement.x +
                         _playerTransform.forward * PlayerInput.Movement.y;

            _playerYRotation = _playerTransform.localEulerAngles.y + PlayerInput.Mouse.x * lookSensitivity;

            _playerTransform.localEulerAngles = new Vector3(0, _playerYRotation, 0);

            _speed += Acceleration * Time.deltaTime;
            _speed = Mathf.Clamp(_speed, 0f, state.CurrentSpeed);

            Vector3 velocity = _direction.normalized * _speed;
            velocity.y = onGround ? 0 : -9.81f;

            if (_jumping)
            {
                velocity.y = 9.81f;
            }

            _rigidBody.velocity = velocity;

            DoRotation();
        }

        #endregion

        #region Bolt Callbacks

        //Gets called first - Initialization.
        public override void Attached()
        {
            //Fetch all the references that are required.
            GetRequiredReferences();

            //Disable components that affects game-play due to multiple players in scene.
            DisableComponentsFromOtherPlayers();

            //After fetching the references, initialize proper values.
            InitializeComponents();

            //Set the playerTransform to sync position over the network.
            state.SetTransforms(state.Transform, transform);

            //As owner of this entity (server), setup all the states for this entity.
            if (entity.IsOwner)
            {
                InitializeNetworkStateOfPlayer();
            }

            //Add callbacks to trigger methods when a state-value changes.
            AddCallbacks();
        }

        //When a player gains control over this entity.
        public override void ControlGained()
        {
            gameObject.layer = 9;
            characterModel.layer = 9;
            
            //Enable the hud.
            hud.SetPlayer(this);

            //Enable the camera.
            playerCamera.enabled = true;

            //Enable the audio-listener.
            _audioListener.enabled = true;

            //Refresh the health.
            hud.RefreshHealth();

            //Refresh the ammo display.
            hud.RefreshAmmoDisplay();

            //Enable the execution of the update method for the weapons of this player.
            weaponSystem.chalkWeaponState.skipUpdate = false;
            weaponSystem.staplerWeaponState.skipUpdate = false;
            weaponSystem.slingshotWeaponState.skipUpdate = false;
            weaponSystem.spongeWeaponState.skipUpdate = false;
            weaponSystem.handsWeaponState.skipUpdate = false;
        }

        //Gets called every frame.
        public override void SimulateController()
        {
            //Player Command Input
            var playerCommandInput = PlayerCommand.Create();
            playerCommandInput.Position = _rigidBody.position;
            playerCommandInput.RotationY = _playerYRotation;
            playerCommandInput.ActiveWeaponIndex = _activeWeaponIndex;
            playerCommandInput.CurrentSpeed = PlayerInput.IsCrouching ? 4 : 6;
            playerCommandInput.ColliderHeight = PlayerInput.IsCrouching ? 1 : 2;
            playerCommandInput.IsCrouching = PlayerInput.IsCrouching;
            playerCommandInput.MouseLeftInput = _activeWeaponIndex == 4 && PlayerInput.LeftMouse;
            playerCommandInput.IsSprinting = PlayerInput.IsSprinting;
            playerCommandInput.PlayerInput = PlayerInput.Movement;
            entity.QueueInput(playerCommandInput);
        }

        //Execute commands from this controller or/and owner.
        public override void ExecuteCommand(Command command, bool resetState)
        {
            if (!(command is PlayerCommand playerCommand))
            {
                return;
            }
            
            animator.SetFloat(InputY, Mathf.Abs(playerCommand.Input.PlayerInput.x) + Mathf.Abs(playerCommand.Input.PlayerInput.y));
            animator.SetBool(IsCrouching, playerCommand.Input.IsCrouching);
            animator.SetBool(IsPunching, playerCommand.Input.MouseLeftInput);

            if (entity.IsOwner)
            {
                state.ActiveWeaponIndex = playerCommand.Input.ActiveWeaponIndex;
                state.CurrentSpeed = playerCommand.Input.CurrentSpeed;
                state.ColliderHeight = playerCommand.Input.ColliderHeight;
                state.IsCrouching = playerCommand.Input.IsCrouching;
                state.IsSprinting = playerCommand.Input.IsSprinting;

                _playerTransform.position = playerCommand.Input.Position;
            }

            _capsuleCollider.height = state.IsCrouching ? 1 : 2;
            _capsuleCollider.radius = state.IsCrouching ? 0.2f : 0.5f;
            _capsuleCollider.center = state.IsCrouching ? new Vector3(0, -.5f, 0) : Vector3.zero;

            _playerTransform.localEulerAngles = new Vector3(0, playerCommand.Input.RotationY, 0);

            playerCamera.transform.localPosition = state.IsCrouching ? new Vector3(0, -0.1f, 0.75f) : new Vector3(0, .74f, 0.2f);
        }

        #endregion

        #region Custom Methods

        private void WeaponActiveIndexChanged()
        {
            //Disable all weapons.
            foreach (var weapon in weaponSystem.allWeapons)
            {
                weapon.gameObject.SetActive(false);
            }

            //Enable the next one.
            weaponSystem.allWeapons[state.ActiveWeaponIndex].gameObject.SetActive(true);


            //Return if not our player.
            if (!entity.HasControl)
            {
                return;
            }

            //Refresh HUD.
            hud.RefreshWeaponDisplay();
            hud.RefreshAmmoDisplay();
        }

        /// <summary>
        /// Rotate the player.
        /// </summary>
        private void DoRotation()
        {
            cameraRotation = new Vector3(PlayerInput.Mouse.y, 0f, 0f) * lookSensitivity;

            playerCamera.transform.Rotate(-cameraRotation);
            if (playerCamera.transform.eulerAngles.x > viewField &&
                playerCamera.transform.eulerAngles.x < 180)
            {
                playerCamera.transform.rotation = Quaternion.identity;
                playerCamera.transform.Rotate(new Vector3(viewField, transform.rotation.eulerAngles.y, 0), Space.World);
            }
            else if (playerCamera.transform.eulerAngles.x < 360 - viewField &&
                     playerCamera.transform.eulerAngles.x > 180)
            {
                playerCamera.transform.rotation = Quaternion.identity;
                playerCamera.transform.Rotate(new Vector3(-viewField, transform.rotation.eulerAngles.y, 0), Space.World);
            }
        }

        /// <summary>
        /// Gets executed if the player changes team.
        /// </summary>
        private void TeamChanged()
        {
            var materials = new Material[2];
            materials[0] = state.Team == "Teacher" ? teacherMaterial : studentMaterial;
            materials[1] = state.Team == "Teacher" ? teacherMaterial : studentMaterial;
            skinnedMeshRenderer.materials = materials;
        }

        /// <summary>
        /// Gets executed when the username changes.
        /// </summary>
        private void UsernameChanged()
        {
            backUsernameDisplay.text = state.Username;
            frontUsernameDisplay.text = state.Username;
        }

        /// <summary>
        /// Disable components from other players.
        /// </summary>
        private void DisableComponentsFromOtherPlayers()
        {
            //Disable the player-camera.
            playerCamera.enabled = false;

            //Disable the audio-listener.
            _audioListener.enabled = false;

            //Prevent the weapon to execute the update method.
            weaponSystem.chalkWeaponState.skipUpdate = true;
            weaponSystem.staplerWeaponState.skipUpdate = true;
            weaponSystem.slingshotWeaponState.skipUpdate = true;
            weaponSystem.spongeWeaponState.skipUpdate = true;
            weaponSystem.handsWeaponState.skipUpdate = true;
        }

        /// <summary>
        /// Get the required references of the components.
        /// </summary>
        private void GetRequiredReferences()
        {
            _audioSource = GetComponent<AudioSource>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _audioListener = GetComponent<AudioListener>();
            playerCamera = GetComponentInChildren<Camera>();
            _rigidBody = GetComponent<Rigidbody>();
            _playerTransform = transform;
            hud = HUD.GetInstance();

            weaponSystem.chalkWeaponState.weapon = state.Weapons[0];
            weaponSystem.staplerWeaponState.weapon = state.Weapons[1];
            weaponSystem.slingshotWeaponState.weapon = state.Weapons[2];
            weaponSystem.spongeWeaponState.weapon = state.Weapons[3];
            weaponSystem.handsWeaponState.weapon = state.Weapons[4];
        }

        /// <summary>
        /// Initialize the values of the components.
        /// </summary>
        private void InitializeComponents()
        {
            //Set the active weapon index to 0.
            _activeWeaponIndex = 0;

            _rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            playerCamera.enabled = false;
            _audioListener.enabled = false;

            foreach (var go in weaponSystem.allWeapons)
            {
                go.gameObject.SetActive(false);
            }

            weaponSystem.allWeapons[0].gameObject.SetActive(true);
        }

        /// <summary>
        /// Set the values for the player and the weapons.
        /// </summary>
        private void InitializeNetworkStateOfPlayer()
        {
            //Set the start health of the player.
            state.Health = 100;

            //Set the active weapon index to 0.
            state.ActiveWeaponIndex = 0;

            state.Weapons[0].Id = 0;
            state.Weapons[0].Label = "Chalk";
            state.Weapons[0].Damage = 20;
            state.Weapons[0].IsEquipped = true;
            state.Weapons[0].EntireAmmo = 20;
            state.Weapons[0].FireRate = 2;
            state.Weapons[0].MaxCarryAmmo = 30;
            state.Weapons[0].ReloadTime = 2;
            state.Weapons[0].SpreadFactor = 0.02f;
            state.Weapons[0].IsReloading = false;
            state.Weapons[0].CurrentAmmoInMagazine = 12;
            state.Weapons[0].MagazineSize = 12;
            state.Weapons[0].MaxDistance = 0;
            state.Weapons[0].Force = 0;
            state.Weapons[0].Player = entity;
            state.Weapons[0].Radius = 0f;
            state.Weapons[0].IsWet = false;
            state.Weapons[0].DestroyOnPickup = false;


            state.Weapons[1].Id = 1;
            state.Weapons[1].Label = "Stapler";
            state.Weapons[1].Damage = 10;
            state.Weapons[1].IsEquipped = false;
            state.Weapons[1].EntireAmmo = 30;
            state.Weapons[1].FireRate = 10;
            state.Weapons[1].MaxCarryAmmo = 45;
            state.Weapons[1].ReloadTime = 3;
            state.Weapons[1].SpreadFactor = 0.05f;
            state.Weapons[1].IsReloading = false;
            state.Weapons[1].CurrentAmmoInMagazine = 15;
            state.Weapons[1].MagazineSize = 15;
            state.Weapons[1].MaxDistance = 0;
            state.Weapons[1].Force = 0;
            state.Weapons[1].Player = entity;
            state.Weapons[1].Radius = 0f;
            state.Weapons[1].IsWet = false;
            state.Weapons[1].DestroyOnPickup = false;


            state.Weapons[2].Id = 2;
            state.Weapons[2].Label = "Slingshot";
            state.Weapons[2].Damage = 60;
            state.Weapons[2].IsEquipped = false;
            state.Weapons[2].EntireAmmo = 8;
            state.Weapons[2].FireRate = 1;
            state.Weapons[2].MaxCarryAmmo = 20;
            state.Weapons[2].ReloadTime = 3;
            state.Weapons[2].SpreadFactor = 0;
            state.Weapons[2].IsReloading = false;
            state.Weapons[2].CurrentAmmoInMagazine = 1;
            state.Weapons[2].MagazineSize = 1;
            state.Weapons[2].MaxDistance = 0;
            state.Weapons[2].Force = 0;
            state.Weapons[2].Player = entity;
            state.Weapons[2].Radius = 0f;
            state.Weapons[2].IsWet = false;
            state.Weapons[1].DestroyOnPickup = false;


            state.Weapons[3].Id = 3;
            state.Weapons[3].Label = "Sponge";
            state.Weapons[3].Damage = 80;
            state.Weapons[3].IsEquipped = true;
            state.Weapons[3].EntireAmmo = 0;
            state.Weapons[3].FireRate = 1;
            state.Weapons[3].MaxCarryAmmo = 1;
            state.Weapons[3].ReloadTime = 0;
            state.Weapons[3].SpreadFactor = 0;
            state.Weapons[3].IsReloading = false;
            state.Weapons[3].CurrentAmmoInMagazine = 1;
            state.Weapons[3].MagazineSize = 1;
            state.Weapons[3].MaxDistance = 1000;
            state.Weapons[3].Force = 1500;
            state.Weapons[3].Player = entity;
            state.Weapons[3].Radius = 2.5f;
            state.Weapons[3].IsWet = false;
            state.Weapons[3].DestroyOnPickup = true;


            state.Weapons[4].Id = 4;
            state.Weapons[4].Label = "Hands";
            state.Weapons[4].Damage = 25;
            state.Weapons[4].IsEquipped = true;
            state.Weapons[4].EntireAmmo = 0;
            state.Weapons[4].FireRate = 1;
            state.Weapons[4].MaxCarryAmmo = 0;
            state.Weapons[4].ReloadTime = 0;
            state.Weapons[4].SpreadFactor = 0;
            state.Weapons[4].IsReloading = false;
            state.Weapons[4].CurrentAmmoInMagazine = 0;
            state.Weapons[4].MagazineSize = 0;
            state.Weapons[4].MaxDistance = 2;
            state.Weapons[4].Force = 0;
            state.Weapons[4].Player = entity;
            state.Weapons[4].Radius = 0.5f;
            state.Weapons[4].IsWet = false;
            state.Weapons[4].DestroyOnPickup = false;
        }

        /// <summary>
        /// Add callback to certain values and trigger execution of methods.
        /// </summary>
        private void AddCallbacks()
        {
            state.AddCallback("ActiveWeaponIndex", WeaponActiveIndexChanged);
            state.AddCallback("Health", hud.RefreshHealth);
            state.AddCallback("Team", TeamChanged);
            state.AddCallback("Username", UsernameChanged);

            state.AddCallback("Weapons[]", UpdateWeapon);
        }

        /// <summary>
        /// Update a certain state of a weapon when a value changes.
        /// </summary>
        /// <param name="iState"></param>
        /// <param name="path"></param>
        /// <param name="indices"></param>
        private void UpdateWeapon(IState iState, string path, ArrayIndices indices)
        {
            var index = indices[0];
            IPlayer iPlayer = (IPlayer) iState;

            switch (path)
            {
                case "Weapons[].IsReloading":
                    switch (index)
                    {
                        case 0:
                            weaponSystem.chalkWeaponState.animator.SetBool(IsReloading,
                                iPlayer.Weapons[index].IsReloading);
                            break;
                        case 1:
                            weaponSystem.staplerWeaponState.animator.SetBool(IsReloading,
                                iPlayer.Weapons[index].IsReloading);
                            break;
                        case 2:
                            weaponSystem.slingshotWeaponState.animator.SetBool(IsReloading,
                                iPlayer.Weapons[index].IsReloading);
                            break;
                    }

                    break;

                case "Weapons[].CurrentAmmoInMagazine":
                    hud.RefreshAmmoDisplay();
                    break;

                case "Weapons[].EntireAmmo":
                    hud.RefreshAmmoDisplay();
                    break;
            }
        }

        public void ChangeWeaponIndex(int newWeaponIndex)
        {
            _activeWeaponIndex = newWeaponIndex;
            hud.RefreshAmmoDisplay();
            hud.RefreshWeaponDisplay();
        }

        #endregion

        #region Bolt Events

        public override void OnEvent(Respawn respawn)
        {
            EventLog.GetInstance().AddEntryLog
            (
                state.Username + " was killed by " + respawn.KilledByPlayerName + " with " + respawn.KilledByWeaponName
            );

            if (respawn.PlayerToRespawn != entity)
            {
                return;
            }

            if (state.Team == "Student")
            {
                MenuController.PlayerLives--;
            }

            foreach (var spawnPosition in FindObjectsOfType<SpawnPosition>())
            {
                if (spawnPosition.stage.ToString() == "Dunno" && spawnPosition.classType.ToString() == state.Team)
                {
                    transform.position = spawnPosition.transform.position;
                }
            }
        }

        #endregion

        #region Getters

        public Rigidbody GetRigidbody()
        {
            return _rigidBody;
        }

        #endregion

        public void EndGame()
        {
            BoltLauncher.Shutdown();
            SceneManager.LoadScene("Menu");
        }
    }
}