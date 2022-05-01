using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HamsterController : MonoBehaviour
{
    private Vector2 _moveInput;
    private Vector3 _movementInput;
    private Rigidbody _rb;
    private bool _ballMode;

    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _gravity;
    [SerializeField] private float _rollSpeed;
    [SerializeField] private float _rollAcceleration;
    [SerializeField] private float _torqueMultiplier;
    [SerializeField] private GameObject _ball;
    public int pid;
    private Transform camTrans;

    [SerializeField] private MeshRenderer hamMeshRenderer;
    private void Awake()
    {
        camTrans = Camera.main.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveInput = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        _movementInput = moveInput;
    }

    public void OnMovement(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        
    }

    private void FixedUpdate()
    {
        if (_ballMode)
        {
            BallTorque();
            transform.position = _ball.transform.position;
            // _ball.transform.GetComponent<BallController>().OnMove(_moveInput);
        }
        else
        {
            Vector3 up = Vector3.up;
            Vector3 right = Camera.main.transform.right;
            Vector3 forward = Vector3.Cross(right, up);
            Vector3 moveInput = forward * _moveInput.y + right * _moveInput.x;
            Vector3 targetAcceleration = moveInput * _speed;
            Vector3 currentAcceleration = _rb.velocity;
            Vector3 finalAcceleration = (targetAcceleration - currentAcceleration) * _acceleration;
            finalAcceleration.y = -_gravity;
            _rb.AddForce(finalAcceleration);
        }
    }

    public void BallTorque()
    {
        Vector3 InputVec = _moveInput.x * camTrans.right + _moveInput.y * camTrans.forward;
        
        Rigidbody rb = _ball.GetComponent<Rigidbody>();
        Vector3 targetAcceleration = InputVec.normalized * _rollSpeed;
        Vector3 currentAcceleration = rb.velocity;
        Vector3 finalAcceleration = (targetAcceleration - currentAcceleration) * _rollAcceleration;
        Vector3 Torque = Vector3.Cross(Vector3.up, finalAcceleration);
        rb.AddTorque(Torque * _torqueMultiplier);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HamsterBall"))
        {
            _ball = collision.gameObject;
            OnMount();
        }
    }

    public void OnMount()
    {
        Debug.Log($"{transform.name} is riding {_ball.name}");
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), _ball.GetComponent<Collider>(), true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        _ballMode = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        // hide Ham mesh
        // GetComponent<PlayerModelManager>().HideObject(true, Vector3.zero);
    }

    public void OnDissMount()
    {
        hamMeshRenderer.enabled = true;
        _ballMode = false;
        Destroy(_ball);
    }
}