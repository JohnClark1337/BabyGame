using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Ensures the object has a Rigidbody2D
public class GyroRigidbodyController2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool gyroEnabled;
    private Gyroscope gyro;

    [Tooltip("Controls the sensitivity of the gyroscope rotation.")]
    [Range(0, 500)]
    public float sensitivity = 300f;

    void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject.
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Check if the device has a gyroscope.
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyroEnabled = true;
            UnityEngine.Debug.Log("Gyroscope enabled for 2D Rigidbody control!");
        }
        else
        {
            gyroEnabled = false;
            UnityEngine.Debug.LogWarning("Gyroscope not supported on this device.");
        }
    }

    void FixedUpdate()
    {
        if (gyroEnabled)
        {
            // For 2D, we typically care about the Z-axis rotation (rolling the device).
            // We use the negative value because the gyro's axis may be inverted from
            // what feels intuitive for screen rotation.
            float rotationRate = -gyro.rotationRateUnbiased.z;

            // Set the angular velocity of the Rigidbody2D.
            // This is a single float value representing degrees per second.
            rb.angularVelocity = rotationRate * sensitivity;
        }
    }
}