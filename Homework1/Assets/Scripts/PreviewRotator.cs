using UnityEngine;

public class PreviewRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 45f * Time.deltaTime);
    }
}