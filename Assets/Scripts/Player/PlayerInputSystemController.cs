using System;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystemController : MonoBehaviour
{
    public PlayerInputs _playerInputs;

    RPGCharacterController _rpgCharacterController;

    //Inputs
    bool _inputJump;
    bool _inputAttackL;
    bool _inputAttackR;
    bool _inputRoll;
    bool _inputAim;
    Vector2 _inputMovement;
    bool _inputFace;
    Vector2 _inputFacing;
    bool _inputSwitchUp;
    
    //Variables
    Vector3 _moveInput;
    Vector3 _currentAim;
    float _inputPauseTimeout = 0;
    bool _inputPaused = false;

    void Awake()
    {
        _rpgCharacterController = GetComponent<RPGCharacterController>();
        _playerInputs = new PlayerInputs();
        _currentAim = Vector3.zero;
    }

    void OnEnable()
    {
        _playerInputs.Enable();
    }

    void OnDisable()
    {
        _playerInputs.Disable();
    }

    public bool HasMoveInput() => _moveInput.magnitude > 0.1f;

    public bool HasFacingInput() => _inputFacing != Vector2.zero || _inputFace;

    void Update()
    {
        if (_inputPaused)
        {
            if (Time.time > _inputPauseTimeout) { _inputPaused = false; } 
            else { return; }
        }

        if (!_inputPaused) { Inputs(); }

        Moving();
        Jumping();
        SwitchWeapons();
        Strafing();
        Facing();
        Aiming();
        Rolling();
        Attacking();
    }

    public void PauseInput(float timeout)
    {
        _inputPaused = true;
        _inputPauseTimeout = Time.time + timeout;
    }

    void Inputs()
    {
        try
        {
            _inputAttackL = _playerInputs.Player.AttackL.WasPressedThisFrame();
            _inputAttackR = _playerInputs.Player.AttackR.WasPressedThisFrame();
            _inputFace = _playerInputs.Player.Face.IsPressed();
            _inputFacing = _playerInputs.Player.Facing.ReadValue<Vector2>();
            _inputJump = _playerInputs.Player.Jump.IsPressed();
            _inputMovement = _playerInputs.Player.Move.ReadValue<Vector2>();
            _inputRoll = _playerInputs.Player.Roll.WasPressedThisFrame();
            _inputSwitchUp = _playerInputs.Player.WeaponUp.WasPressedThisFrame();
            
            // Slow time toggle.
            if (Keyboard.current.tKey.wasPressedThisFrame) {
                if (_rpgCharacterController.CanStartAction("SlowTime"))
                { _rpgCharacterController.StartAction("SlowTime", 0.125f); }
                else if (_rpgCharacterController.CanEndAction("SlowTime"))
                { _rpgCharacterController.EndAction("SlowTime"); }
            }
            // Pause toggle.
            if (Keyboard.current.pKey.wasPressedThisFrame) {
                if (_rpgCharacterController.CanStartAction("SlowTime"))
                { _rpgCharacterController.StartAction("SlowTime", 0f); }
                else if (_rpgCharacterController.CanEndAction("SlowTime"))
                { _rpgCharacterController.EndAction("SlowTime"); }
            }
        }
        catch (System.Exception) { Debug.LogError("Inputs not found! " + 
                                                  "Make sure your project is using the new InputSystem: Edit>Project Settings>Player>Active Input Handling  - change to 'Input System Package (New)'.");}
    }

    void Moving()
    {
        _moveInput = new Vector3(_inputMovement.x,_inputMovement.y, 0f);

        // Filter the 0.1 threshold of HasMoveInput.
        if (HasMoveInput()) { _rpgCharacterController.SetMoveInput(_moveInput); }
        else { _rpgCharacterController.SetMoveInput(Vector3.zero); }
    }
    
    void Jumping()
    {
        // Set the input on the jump axis every frame.
        Vector3 jumpInput = _inputJump ? Vector3.up : Vector3.zero;
        _rpgCharacterController.SetJumpInput(jumpInput);

        // If we pressed jump button this frame, jump.
        if (_inputJump && _rpgCharacterController.CanStartAction("Jump")) { _rpgCharacterController.StartAction("Jump"); }
        else if (_inputJump && _rpgCharacterController.CanStartAction("DoubleJump")) { _rpgCharacterController.StartAction("DoubleJump"); }
    }
    
    void Rolling()
    {
        if (!_inputRoll) { return; }
        if (!_rpgCharacterController.CanStartAction("DiveRoll")) { return; }

        _rpgCharacterController.StartAction("DiveRoll", 1);
    }
    
    void Aiming()
    { Strafing(); }
    
    void Strafing()
    {
        if (_rpgCharacterController.canStrafe) {
            if (_inputAim) {
                if (_rpgCharacterController.CanStartAction("Strafe")) { _rpgCharacterController.StartAction("Strafe"); }
            }
            else {
                if (_rpgCharacterController.CanEndAction("Strafe")) { _rpgCharacterController.EndAction("Strafe"); }
            }
        }
    }
    
    void Facing()
    {
        if (_rpgCharacterController.canFace) {
            if (HasFacingInput()) {
                if (_inputFace) {

                    // Get world position from mouse position on screen and convert to direction from character.
                    Plane playerPlane = new Plane(Vector3.up, transform.position);
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    float hitdist = 0.0f;
                    if (playerPlane.Raycast(ray, out hitdist)) {
                        Vector3 targetPoint = ray.GetPoint(hitdist);
                        Vector3 lookTarget = new Vector3(targetPoint.x - transform.position.x, transform.position.z - targetPoint.z, 0);
                        _rpgCharacterController.SetFaceInput(lookTarget);
                    }
                }
                else { _rpgCharacterController.SetFaceInput(new Vector3(_inputFacing.x, _inputFacing.y, 0)); }

                if (_rpgCharacterController.CanStartAction("Face")) { _rpgCharacterController.StartAction("Face"); }
            }
            else {
                if (_rpgCharacterController.CanEndAction("Face")) { _rpgCharacterController.EndAction("Face"); }
            }
        }
    }
    
    void Attacking()
    {
        // Check to make sure Attack Action exists.
        if (!_rpgCharacterController.HandlerExists(HandlerTypes.Attack)) { return; }

        // Check to make character can Attack.
        if (!_rpgCharacterController.CanStartAction(HandlerTypes.Attack)) { return; }

        if (_inputAttackL)
        { _rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Left)); }
        else if (_inputAttackR)
        { _rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Right)); }
    }
    
    void SwitchWeapons() 
    {
        // Check if SwitchWeapon Action exists.
        if (!_rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }
        
        // Bail out if we can't switch weapons.
        if (!_rpgCharacterController.CanStartAction(HandlerTypes.SwitchWeapon)) { return; }

        var doSwitch = false;
        var switchWeaponContext = new SwitchWeaponContext();

        if (_inputSwitchUp)
        {
            // Unarmed.
            if (_rpgCharacterController.rightWeapon != Weapon.Unarmed
                || _rpgCharacterController.leftWeapon != Weapon.Unarmed)
            {
                doSwitch = true;
                switchWeaponContext.type = "Switch";
                switchWeaponContext.side = "Both";
                switchWeaponContext.leftWeapon = Weapon.Unarmed;
                switchWeaponContext.rightWeapon = Weapon.Unarmed;

            }

            // TwoHanded weapon.
            if (_rpgCharacterController.rightWeapon != Weapon.TwoHandSword)
            {
                doSwitch = true;
                switchWeaponContext.type = "Switch";
                switchWeaponContext.side = "None";
                switchWeaponContext.leftWeapon = Weapon.Unarmed;
                switchWeaponContext.rightWeapon = Weapon.TwoHandSword;
            }
        }

        // Perform the weapon switch using the SwitchWeaponContext created earlier.
        if (doSwitch) { _rpgCharacterController.TryStartAction(HandlerTypes.SwitchWeapon, switchWeaponContext); }
    }
}

/// <summary>
/// Extension Method to allow checking InputSystem without Action Callbacks.
/// </summary>
public static class InputActionExtensions
{
    public static bool IsPressed(this InputAction inputAction) => inputAction.ReadValue<float>() > 0f;

    public static bool WasPressedThisFrame(this InputAction inputAction) => inputAction.triggered && inputAction.ReadValue<float>() > 0f;

    public static bool WasReleasedThisFrame(this InputAction inputAction) => inputAction.triggered && inputAction.ReadValue<float>() == 0f;
}
