using System.Diagnostics;
using UnityEngine;


public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectPrefab;

    private bool triggerWasPressed = false;

    void Update()
    {
        // Check for trigger button press on right controller
        UnityEngine.XR.InputDevice rightHand =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);

        rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed);

        // Only spawn once per press, not every frame
        if (triggerPressed && !triggerWasPressed)
        {
            SpawnObject();
        }

        triggerWasPressed = triggerPressed;
    }

    void SpawnObject()
    {
        if (objectPrefab == null)
        {
            UnityEngine.Debug.LogError("No prefab assigned!");
            return;
        }

        // Spawn 1.5 units in front of wherever this script's object is
        Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
        GameObject spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

        // Add Collider if missing
        if (spawnedObject.GetComponent<Collider>() == null)
        {
            spawnedObject.AddComponent<BoxCollider>();
        }

        // Add Rigidbody if missing
        if (spawnedObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = spawnedObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Add XR Grab Interactable if missing
        if (spawnedObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = spawnedObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.throwOnDetach = true;
        }

        UnityEngine.Debug.Log("Object spawned: " + spawnedObject.name);
    }
}