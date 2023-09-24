using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private float runSpeed;

    [SerializeField]
    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private float crounchSpeed;

    [SerializeField]
    private float cameraSensitivity;

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0;

    [SerializeField]
    private Camera mainCamera;
    private Rigidbody myRigid;
    private CapsuleCollider capsuleCollider;

    private bool isRun = false;
    private bool isCrouch= false;
    private bool isGround = true;

    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;


    private int jumpCnt = 0;

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;

        originPosY = mainCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
    }

    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }
    IEnumerator CrouchCoroutine()
    {
        float _posY = mainCamera.transform.localPosition.y;
        int cnt = 0;

        while (_posY != applyCrouchPosY)
        {
            cnt++;
            _posY = Mathf.Lerp(_posY,applyCrouchPosY,0.3f);
            mainCamera.transform.localPosition = new Vector3(0, _posY, 0);

            if (cnt > 15)
                break;

            yield return null;
        }
        mainCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }
    private void Crouch()
    {
        isCrouch = !isCrouch;

        if (isCrouch)
        {
            applySpeed = crounchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        //mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, applyCrouchPosY, mainCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        if (isGround)
            jumpCnt = 0;
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCnt < 1)
        //if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            if (isCrouch)
                Crouch();

            myRigid.velocity = transform.up * jumpForce;
            jumpCnt++;
        }
    }

    private void TryRun()
    {
        if (!isCrouch && isGround)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRun = true;
                applySpeed = runSpeed;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isRun = false;
                applySpeed = walkSpeed;
            }
        }
    }


    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;   //normalized(삼각함수에 대해 공부)

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * cameraSensitivity;

        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _cameraRotationY = new Vector3(0f, _yRotation, 0f) * cameraSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_cameraRotationY));

        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        mainCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}
