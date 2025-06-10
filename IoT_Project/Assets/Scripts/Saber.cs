using UnityEngine;

public class Saber : MonoBehaviour
{
    [SerializeField] private Vector3 AccelerationInput;
    [SerializeField] private Vector3 AngularVelocityInput;
    [SerializeField] private float YRotationSensitivity = 1.0f;
    [SerializeField] private float ZRotationSensitivity = 1.0f;
    [SerializeField] private float HorizontalMovementSensitivity = 0.05f;
    [SerializeField] private float VerticalMovementSensitivity = 0.05f;
    [SerializeField] private float ReturnToOriginSpeed = 0.1f;
    [SerializeField] private float MaxHorizontalOffset = 0.5f;
    [SerializeField] private float MaxVerticalOffset = 0.5f;
    [SerializeField] private KeyCode CalibrateKey = KeyCode.C;
    [SerializeField] private Axis MpuYRotationAxis = Axis.MPU_Z;
    [SerializeField] private Axis MpuZRotationAxis = Axis.MPU_X;
    [SerializeField] private Axis MpuHorizontalAxis = Axis.MPU_X;
    [SerializeField] private Axis MpuVerticalAxis = Axis.MPU_Y;

    public enum Axis { MPU_X, MPU_Y, MPU_Z, NEG_MPU_X, NEG_MPU_Y, NEG_MPU_Z }

    private Quaternion _initialSwordRotation;
    private Vector3 _initialLocalPosition;
    private float _currentYAngle = 0f;
    private float _currentZAngle = 0f;
    private Vector3 _currentPositionalOffset = Vector3.zero;

    private void Start()
    {
        _initialLocalPosition = transform.localPosition;
        _initialSwordRotation = transform.localRotation;
        Calibrate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(CalibrateKey))
        {
            Calibrate();
        }

        ApplySlicingRotation();
        Apply2DMovement();

        transform.localRotation = Quaternion.Euler(0, _currentYAngle, _currentZAngle) * _initialSwordRotation;
        transform.localPosition = Vector3.Lerp(transform.localPosition, _initialLocalPosition + _currentPositionalOffset, ReturnToOriginSpeed);
    }

    private void ApplySlicingRotation()
    {
        var rawYRotationInput = GetMappedValue(AngularVelocityInput, MpuYRotationAxis);
        var rawZRotationInput = GetMappedValue(AngularVelocityInput, MpuZRotationAxis);

        _currentYAngle += rawYRotationInput * Time.deltaTime * YRotationSensitivity;
        _currentZAngle += rawZRotationInput * Time.deltaTime * ZRotationSensitivity;
    }

    private void Apply2DMovement()
    {
        var rawHorizontalInput = GetMappedValue(AccelerationInput, MpuHorizontalAxis);
        var rawVerticalInput = GetMappedValue(AccelerationInput, MpuVerticalAxis);

        var xImpulse = rawHorizontalInput * Time.deltaTime * HorizontalMovementSensitivity;
        var yImpulse = rawVerticalInput * Time.deltaTime * VerticalMovementSensitivity;

        _currentPositionalOffset.x += xImpulse;
        _currentPositionalOffset.y += yImpulse;
        _currentPositionalOffset.z = 0;

        _currentPositionalOffset.x = Mathf.Clamp(_currentPositionalOffset.x, -MaxHorizontalOffset, MaxHorizontalOffset);
        _currentPositionalOffset.y = Mathf.Clamp(_currentPositionalOffset.y, -MaxVerticalOffset, MaxVerticalOffset);
    }

    private float GetMappedValue(Vector3 inputVector, Axis axis)
    {
        switch (axis)
        {
            case Axis.MPU_X: return inputVector.x;
            case Axis.MPU_Y: return inputVector.y;
            case Axis.MPU_Z: return inputVector.z;
            case Axis.NEG_MPU_X: return -inputVector.x;
            case Axis.NEG_MPU_Y: return -inputVector.y;
            case Axis.NEG_MPU_Z: return -inputVector.z;
            default: return 0f;
        }
    }

    [ContextMenu("Calibrate Sword")]
    public void Calibrate()
    {
        _currentYAngle = 0f;
        _currentZAngle = 0f;
        _currentPositionalOffset = Vector3.zero;

        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _initialSwordRotation;

        Debug.Log("Saber calibrated. Rotation and 2D Position Reset.");
    }

    public void SetSensorData(Vector3 accel, Vector3 gyro)
    {
        AccelerationInput = accel;
        AngularVelocityInput = gyro;
    }
}