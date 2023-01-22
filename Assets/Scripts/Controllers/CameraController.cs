using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    public bool UsingOrbitalCamera { get; private set; }
    [SerializeField] private HumanoidLandInput _input;
    private CinemachineVirtualCamera _activeCamera;
    private int _activeCameraPriorityModifier = 65535;

    public Camera MainCamera;
    public CinemachineVirtualCamera cinemachine1stPerson;
    public CinemachineVirtualCamera cinemachine3rdPerson;
    private CinemachineFramingTransposer _framingTransposer3rd;

    private bool _zoomed = false;

    [SerializeField] private int zoomDistance = 2;
    [SerializeField] private int defaultDistance = 5;
    [SerializeField] private float zoomX = 0.1f;
    [SerializeField] private float defaultX = 0.3f;
    //public CinemachineVirtualCamera cinemachineOrbit;

    private void Awake()
    {
        _framingTransposer3rd = cinemachine3rdPerson.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Start()
    {
        ChangeCamera();
    }

    private void Update()
    {
        if (_input.ChangeCameraWasPressedThisFrame)
            ChangeCamera();
        
        ZoomCamera();
    }

    private void ZoomCamera()
    {
        if (_activeCamera != cinemachine3rdPerson) return;
        _framingTransposer3rd.m_CameraDistance = _input.ZoomCameraIsPressed ? zoomDistance : defaultDistance;
        _framingTransposer3rd.m_ScreenX = _input.ZoomCameraIsPressed ? zoomX : defaultX;

    }

    private void ChangeCamera()
    {
        if (cinemachine3rdPerson == _activeCamera)
        {
            SetCameraPriorities(cinemachine3rdPerson, cinemachine1stPerson);
            //UsingOrbitalCamera = false;
        }
        else if (cinemachine1stPerson == _activeCamera)
        {
            SetCameraPriorities(cinemachine1stPerson, cinemachine3rdPerson);
            //UsingOrbitalCamera = true;
        }/*
        else if (cinemachineOrbit == _activeCamera)
        {
            SetCameraPriorities(cinemachineOrbit, cinemachine3rdPerson);
            _activeCamera = cinemachine3rdPerson;
            UsingOrbitalCamera = false;
        }*/
        else
        {
            cinemachine3rdPerson.Priority += _activeCameraPriorityModifier;
            _activeCamera = cinemachine3rdPerson;
        }
    }

    private void SetCameraPriorities(CinemachineVirtualCamera CurrentCameraMode, CinemachineVirtualCamera NewCameraMode)
    {
        CurrentCameraMode.Priority -= _activeCameraPriorityModifier;
        NewCameraMode.Priority += _activeCameraPriorityModifier;
        _activeCamera = NewCameraMode;
    }
}