using UnityEngine;
using UnityEngine.XR;

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

    void Start()
    {
        prefabs = Resources.LoadAll<GameObject>("PREFABS");
        UnityEngine.Debug.Log("Loaded " + prefabs.Length + " prefabs");
    }

    void Update()
    {
        if (prefabs == null || prefabs.Length == 0) return;
    
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // --- Trigger → Toggle Menu ---
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);
        if (triggerPressed && !triggerWasPressed)
        {
            ToggleMenu();
        }
        triggerWasPressed = triggerPressed;

        if (menuOpen)
        {
            // --- Thumbstick Left/Right → Cycle Prefabs ---
            rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick);

            bool thumbstickLeft = thumbstick.x < -0.5f;
            bool thumbstickRight = thumbstick.x > 0.5f;

            if (thumbstickLeft && !thumbstickWasLeft)
            {
                GoLeft();
            }
            thumbstickWasLeft = thumbstickLeft;

            if (thumbstickRight && !thumbstickWasRight)
            {
                GoRight();
            }
            thumbstickWasRight = thumbstickRight;

            // --- Grip → Confirm Spawn ---
            rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed);
            if (gripPressed && !gripWasPressed)
            {
                ConfirmSpawn();
            }
            gripWasPressed = gripPressed;
        }
    }

    void ToggleMenu()
    {
        menuOpen = !menuOpen;

        if (menuOpen)
            ShowPreview();
        else
            DestroyPreview();
    }

    void ShowPreview()
    {
        DestroyPreview();

        Vector3 previewPosition = transform.position + transform.forward * 1.5f + Vector3.up * 1f;
        previewObject = Instantiate(prefabs[currentIndex], previewPosition, Quaternion.identity);

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = previewObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        if (previewObject.GetComponent<PreviewRotator>() == null)
            previewObject.AddComponent<PreviewRotator>();

        UnityEngine.Debug.Log("Previewing: " + prefabs[currentIndex].name);
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

        PreviewRotator rotator = previewObject.GetComponent<PreviewRotator>();
        if (rotator != null)
            Destroy(rotator);

        if (previewObject.GetComponent<Collider>() == null)
            previewObject.AddComponent<BoxCollider>();

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = previewObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;

        if (previewObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = previewObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.throwOnDetach = true;
        }

        UnityEngine.Debug.Log("Spawned: " + previewObject.name);

        previewObject = null;
        menuOpen = false;
    }
}