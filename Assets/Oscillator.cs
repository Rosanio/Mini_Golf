using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector;
    [SerializeField] float period = 2f;

    [Range(0, 1)] float movementFactor;

    private Vector3 startingPosition;

    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        float cycles = Time.time / period;
        const float tau = 2 * Mathf.PI;
        float rawSinWave = Mathf.Sin(cycles * tau);
        movementFactor = rawSinWave / 2 + 0.5f;
        transform.position = startingPosition + (movementVector * movementFactor);
    }
}
