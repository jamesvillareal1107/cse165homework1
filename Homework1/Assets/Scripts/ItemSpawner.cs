using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ItemSpawner : MonoBehaviour
{
    private GameObject[] prefabs;

    private int currentIndex = 0;
    private GameObject previewObject;
    private bool menuOpen = false;

    private bool triggerWasPressed = false;
    private bool gripWasPressed = false;
    private bool thumbstickWasLeft = false;
    private bool thumbstickWasRight = false;

    // Placement and orientation
    private Vector3 previewOffset;
    private float previewRotationX = 0f;
    private float previewRotationY = 0f;
    private float previewRotationZ = 0f;
    private float moveSpeed = 1.5f;
    private float rotateSpeed = 90f;

    void Start()
    {
        prefabs = Resources.LoadAll<GameObject>("PREFABS");
        UnityEngine.Debug.Log("Loaded " + prefabs.Length + " prefabs");
    }

    void Update()
    {
        if (prefabs == null || prefabs.Length == 0) return;

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // --- Right Trigger → Toggle Menu ---
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTrigger);
        if (rightTrigger && !triggerWasPressed)
        {
            ToggleMenu();
        }
        triggerWasPressed = rightTrigger;

        if (menuOpen)
        {
            // Get all inputs
            rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightThumbstick);
            leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftThumbstick);
            leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTrigger);
            leftHand.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGrip);
            rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGrip);
            leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool xButton);   // X
            leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yButton); // Y

            // --- Right Thumbstick Left/Right → Cycle Prefabs ---
            bool thumbstickLeft = rightThumbstick.x < -0.5f;
            bool thumbstickRight = rightThumbstick.x > 0.5f;

            if (thumbstickLeft && !thumbstickWasLeft)
                GoLeft();
            thumbstickWasLeft = thumbstickLeft;

            if (thumbstickRight && !thumbstickWasRight)
                GoRight();
            thumbstickWasRight = thumbstickRight;

            // --- Right Thumbstick Up/Down → Move Preview Up/Down ---
            if (rightThumbstick.y > 0.5f)
            {
                previewOffset += Vector3.up * moveSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }
            else if (rightThumbstick.y < -0.5f)
            {
                previewOffset -= Vector3.up * moveSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }

            // --- Left Thumbstick → Move Preview Forward/Back/Left/Right ---
            if (leftThumbstick.magnitude > 0.1f)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;
                camForward.y = 0;
                camRight.y = 0;

                previewOffset += (camForward * leftThumbstick.y + camRight * leftThumbstick.x)
                                 * moveSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }

            // --- Rotate Z ---
            // Left Grip alone → Rotate Z positive
            if (leftGrip && !xButton && !yButton)
            {
                previewRotationZ += rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }
            // Left Trigger alone → Rotate Z negative
            if (leftTrigger && !xButton && !yButton)
            {
                previewRotationZ -= rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }

            // --- Rotate X ---
            // Left Grip + X → Rotate X positive
            if (leftGrip && xButton && !yButton)
            {
                previewRotationX += rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }
            // Left Trigger + X → Rotate X negative
            if (leftTrigger && xButton && !yButton)
            {
                previewRotationX -= rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }

            // --- Rotate Y ---
            // Left Grip + Y → Rotate Y positive
            if (leftGrip && yButton && !xButton)
            {
                previewRotationY += rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }
            // Left Trigger + Y → Rotate Y negative
            if (leftTrigger && yButton && !xButton)
            {
                previewRotationY -= rotateSpeed * Time.deltaTime;
                UpdatePreviewTransform();
            }

            // --- Right Grip → Confirm Spawn ---
            if (rightGrip && !gripWasPressed)
            {
                ConfirmSpawn();
            }
            gripWasPressed = rightGrip;
        }
    }

    void ToggleMenu()
    {
        menuOpen = !menuOpen;

        if (menuOpen)
        {
            previewOffset = transform.forward * 1.5f + Vector3.up * 1f;
            previewRotationX = 0f;
            previewRotationY = 0f;
            previewRotationZ = 0f;
            ShowPreview();
        }
        else
        {
            DestroyPreview();
        }
    }

    void ShowPreview()
    {
        DestroyPreview();

        Vector3 previewPosition = transform.position + previewOffset;
        previewObject = Instantiate(prefabs[currentIndex], previewPosition, Quaternion.Euler(previewRotationX, previewRotationY, previewRotationZ));

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = previewObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        UnityEngine.Debug.Log("Previewing: " + prefabs[currentIndex].name);
    }

    void UpdatePreviewTransform()
    {
        if (previewObject == null) return;

        previewObject.transform.position = transform.position + previewOffset;
        previewObject.transform.rotation = Quaternion.Euler(previewRotationX, previewRotationY, previewRotationZ);
    }

    void DestroyPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    void GoLeft()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = prefabs.Length - 1;
        ShowPreview();
        UnityEngine.Debug.Log("Switched to: " + prefabs[currentIndex].name);
    }

    void GoRight()
    {
        currentIndex++;
        if (currentIndex >= prefabs.Length)
            currentIndex = 0;
        ShowPreview();
        UnityEngine.Debug.Log("Switched to: " + prefabs[currentIndex].name);
    }

    void ConfirmSpawn()
    {
        if (previewObject == null) return;

        if (previewObject.GetComponent<Collider>() == null)
            previewObject.AddComponent<BoxCollider>();

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = previewObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;

        if (previewObject.GetComponent<XRGrabInteractable>() == null)
        {
            XRGrabInteractable grab = previewObject.AddComponent<XRGrabInteractable>();
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.throwOnDetach = true;
        }

        UnityEngine.Debug.Log("Spawned: " + previewObject.name);

        previewObject = null;
        menuOpen = false;
    }
}