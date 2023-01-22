using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanoidLandInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool MoveIsPressed;
    public Vector2 LookInput { get; private set; }
    public bool InvertMouseY { get; private set; } = true;
    public float SwitchWeaponUpDown { get; private set; }
    public bool ChangeCameraWasPressedThisFrame { get; private set; }
    public bool ZoomCameraIsPressed { get; private set; }
    public bool SprintIsPressed { get; private set; }
    public bool JumpIsPressed { get; private set; }
    public bool CrouchIsPressed { get; private set; }
    public bool ShootIsPressed { get; private set; }

    private InputActions _input;

    private void OnEnable()
    {
        _input = new InputActions();
        _input.HumanoidLand.Enable();

        _input.HumanoidLand.Move.performed += SetMove;
        _input.HumanoidLand.Move.canceled += SetMove;

        _input.HumanoidLand.Look.performed += SetLook;
        _input.HumanoidLand.Look.canceled += SetLook;
        
        _input.HumanoidLand.SwitchWeapon.performed += SetSwitch;
        _input.HumanoidLand.SwitchWeapon.canceled += SetSwitch;
        
        _input.HumanoidLand.Shoot.started += SetShoot;
        _input.HumanoidLand.Shoot.canceled += SetShoot;

        _input.HumanoidLand.ZoomCamera.started += SetZoom;
        _input.HumanoidLand.ZoomCamera.canceled += SetZoom;
        
        _input.HumanoidLand.Sprint.started += SetSprint;
        _input.HumanoidLand.Sprint.canceled += SetSprint;
        
        _input.HumanoidLand.Jump.started += SetJump;
        _input.HumanoidLand.Jump.canceled += SetJump;

        _input.HumanoidLand.Crouch.started += SetCrouch;
        _input.HumanoidLand.Crouch.canceled += SetCrouch;

    }

    private void OnDisable()
    {
        _input.HumanoidLand.Move.performed -= SetMove;
        _input.HumanoidLand.Move.canceled -= SetMove;

        _input.HumanoidLand.Look.performed -= SetLook;
        _input.HumanoidLand.Look.canceled -= SetLook;
        
        _input.HumanoidLand.SwitchWeapon.performed -= SetSwitch;
        _input.HumanoidLand.SwitchWeapon.canceled -= SetSwitch;
        
        _input.HumanoidLand.Shoot.started -= SetShoot;
        _input.HumanoidLand.Shoot.canceled -= SetShoot;

        _input.HumanoidLand.ZoomCamera.started -= SetZoom;
        _input.HumanoidLand.ZoomCamera.canceled -= SetZoom;
        
        _input.HumanoidLand.Sprint.started -= SetSprint;
        _input.HumanoidLand.Sprint.canceled -= SetSprint;
        
        _input.HumanoidLand.Jump.started -= SetJump;
        _input.HumanoidLand.Jump.canceled -= SetJump;

        _input.HumanoidLand.Crouch.started -= SetCrouch;
        _input.HumanoidLand.Crouch.canceled -= SetCrouch;
        
        _input.HumanoidLand.Disable();
    }

    private void Update()
    {
        ChangeCameraWasPressedThisFrame = _input.HumanoidLand.ChangeCamera.WasPressedThisFrame();
    }

    private void SetMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
        MoveIsPressed = !(MoveInput == Vector2.zero);
    }
    
    private void SetLook(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();
    }
    
    private void SetSwitch(InputAction.CallbackContext ctx)
    {
        SwitchWeaponUpDown = ctx.ReadValue<float>();
    }
    
    private void SetShoot(InputAction.CallbackContext ctx)
    {
        ShootIsPressed = ctx.started;
    }
    private void SetZoom(InputAction.CallbackContext ctx)
    {
        ZoomCameraIsPressed = ctx.started;
    }
    
    private void SetSprint(InputAction.CallbackContext ctx)
    {
        SprintIsPressed = ctx.started;
    }
    
    private void SetJump(InputAction.CallbackContext ctx)
    {
        JumpIsPressed = ctx.started;
    }
    
    private void SetCrouch(InputAction.CallbackContext ctx)
    {
        SprintIsPressed = ctx.started;
    }

}
