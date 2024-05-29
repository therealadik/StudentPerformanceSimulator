using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkingSpeed = 7.5f;
    [SerializeField] float runningSpeed = 11.5f;
    [SerializeField] float gravity = 20.0f;
    [SerializeField] float lookSpeed = 2.0f;
    [SerializeField] float lookXLimit = 45.0f;

    [SerializeField] Transform playerCamera;
    [SerializeField] Nofity notifySystem;
    [SerializeField] LiftSystem listSystem;


    [Header("ButtonUI")]
    [SerializeField] GameObject Button;
    [SerializeField] TMP_Text ActionText;

    [Header("Sounds")]
    [SerializeField] AudioSource footstep;

    public bool canMove { get; private set; } = true;
    public bool isControlled { get; private set; } = true;

    public bool isMoving => Math.Abs(moveDirection.x) + Math.Abs(moveDirection.z) > 0.05f;

    private Vector2 directionInput;
    private Vector3 moveDirection = Vector3.zero;

    private float rotationX = 0;
    private bool isRunning = false;

    private Animator animator;
    private CharacterController characterController;
    

    public void SetControll(bool value)
    {
        isControlled = value;

        if (isControlled == false)
        {
            characterController.enabled = false;
            Button.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            characterController.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        SetControll(true);
    }

    void Update()
    {
        if (isControlled)
        {
            Move();
            CameraRotate();
            CheckTriggers();
        }
       
        UpdateAnimator();
        UpdateAudio();
    }

    private void CheckTriggers()
    {
        List<Collider> list = Physics.OverlapSphere(transform.position, 1).ToList();
        Button.SetActive(false);


        foreach (Collider collider in list)
        {
            if (collider.TryGetComponent(out LuftButtons buttonsLift))
            {
                notifySystem.ShowNofity("����� ����� �� ����������, ����� ������� �� ������ ����");
                int numberTo = 0;
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    numberTo = 4;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    numberTo = 1;
                }

                if (numberTo != 0)
                {
                    listSystem.LiftTo(buttonsLift.transform.parent.GetComponent<Lift>(), numberTo);
                }
            }

            if (collider.TryGetComponent(out ProximityButton button))
            {
                ActionText.text = button.actionName;
                Button.SetActive(button.isActive);
            }
        }
    }

    private void Move()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        // Press Left Shift to run
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * directionInput.y : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * directionInput.x : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = movementDirectionY;

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void UpdateAudio()
    {
        return;
        if (isMoving)
        {
            if (footstep.isPlaying == false)
            {
                footstep.Play();
            }
        }
        else
        {
            footstep.Stop();
        }
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isWalk", isMoving);
        animator.SetBool("isRun", isRunning && isMoving);
    }

    private void CameraRotate()
    {
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        directionInput = inputValue.Get<Vector2>();
    }

    private void OnRun(InputValue inputValue)
    {
        isRunning = inputValue.isPressed;
    }

    private void OnInteract(InputValue inputValue)
    {
        if (!isControlled)
            return;

        List<Collider> list = Physics.OverlapSphere(transform.position, 1).ToList();

        foreach (Collider collider in list)
        {
            if (collider.TryGetComponent(out ProximityButton button) && button.isActive)
            {
                button.Interact();
                return;
            }
        }
    }
}