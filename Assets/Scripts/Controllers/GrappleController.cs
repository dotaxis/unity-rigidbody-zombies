using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GrappleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private LayerMask _isGrappleAble;
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance;
    [SerializeField] private float _grappleDelayTime;
    [SerializeField] private float _overShootY;
    private Vector3 _grapplePoint, _velocityToSet, _initialPoint;
    private bool _isShooting, _isGrappling;
    private float _drag;


    [FormerlySerializedAs("grapplingCd")]
    [Header("Cooldown")]
    [SerializeField] private float _grapplingCd;
    private float _grapplingCdTimer;

    [Header("Input")] [SerializeField]
    private KeyCode grappleKey = KeyCode.Mouse0;


    void Start()
    {
        _lineRenderer.enabled = false;
        _isShooting = false;
        _isGrappling = false;
        _drag = _rigidbody.drag;
    }


    void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();
        
        if (_grapplingCdTimer > 0)
            _grapplingCdTimer -= Time.deltaTime;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isGrappling)
        {
            _isGrappling = false;
            _rigidbody.drag = _drag;
        }
    }


    private void LateUpdate()
    {
        if (_isGrappling)
        {
            _lineRenderer.SetPosition(0, _gunTip.position);
        }
    }


    private void StartGrapple()
    {
        if (_grapplingCdTimer > 0) return;

        _isGrappling = true;
        
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray.origin, ray.direction, out var hit, _maxGrappleDistance, _isGrappleAble)
            &&
            Physics.Raycast(_gunTip.position, hit.point - _gunTip.position, out var hit2, _maxGrappleDistance, _isGrappleAble))
        {
            _grapplePoint = hit2.point;
            _lineRenderer.enabled = true;
            var normalizedNewTransform = (_grapplePoint - transform.position).normalized;
            transform.forward = new Vector3(normalizedNewTransform.x, transform.forward.y, normalizedNewTransform.z);
            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);

        }
        else
        {
            _grapplePoint = ray.origin + ray.direction * _maxGrappleDistance;

            Invoke(nameof(StopGrapple), _grappleDelayTime);
        }
        _lineRenderer.SetPosition(1, _grapplePoint);
    }
    
    private void ExecuteGrapple() {
        _rigidbody.drag = 0;
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeY = _grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeY + _overShootY;

        if (grapplePointRelativeY < 0) highestPointOnArc = _overShootY;
            
        JumpToPosition(_grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple), 1f);
    }

    private void StopGrapple()
    {
        _grapplingCdTimer = _grapplingCd;
        _lineRenderer.enabled = false;
    }

    private void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        _isGrappling = true;
        _velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        

        Invoke(nameof(SetVelocity),0.1f);
    }

    private void SetVelocity()
    {
        _rigidbody.velocity = _velocityToSet;
    }
    
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}