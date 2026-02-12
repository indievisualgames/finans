using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingLight : MonoBehaviour
{
    // Public variables for customization
    public Image lightImage;        // The Image component that represents the light
    public AudioSource audioSource; // The AudioSource to play the blink sound
    public AudioClip blinkSound;    // The sound that will play when the light blinks

    public float blinkDuration = 1f; // Duration of the light blink (in seconds)
    public float blinkFrequency = 2f; // How often the light blinks (in blinks per second)
    public float maxOpacity = 1f;    // Max opacity of the light
    public float minOpacity = 0f;    // Min opacity of the light (fully transparent)

    // Public Color (lightColor will transition to colorB)
    public Color lightColor = Color.red;  // Initial color of the light
    public Color colorB = Color.blue;    // Target color to transition to

    private bool isBlinking = false;

    void Start()
    {
        // Error checking to ensure required components are assigned
        if (lightImage == null)
        {
            Debug.LogError("Light Image is not assigned!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("Audio Source is not assigned!");
            return;
        }

        if (blinkSound == null)
        {
            Debug.LogError("Blink Sound is not assigned!");
            return;
        }

        // Start the blinking process
        StartCoroutine(BlinkLight());
    }

    // Coroutine to handle blinking behavior with color transition
    private IEnumerator BlinkLight()
    {
        while (true)
        {
            // Blink cycle
            isBlinking = true;

            // Start the blink effect
            float elapsedTime = 0f;
            while (elapsedTime < blinkDuration)
            {
                // Calculate opacity based on time elapsed (fade in/fade out effect)
                float opacity = Mathf.Lerp(minOpacity, maxOpacity, Mathf.PingPong(elapsedTime * blinkFrequency, 1f));

                // Smoothly transition the light color from lightColor to colorB
                lightColor = Color.Lerp(lightColor, colorB, Mathf.PingPong(elapsedTime / blinkDuration, 1f));
                
                // Set the color with the calculated opacity
                Color newColor = lightColor;
                newColor.a = opacity; // Set the opacity
                lightImage.color = newColor; // Apply the new color to the image

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Play the blink sound
            audioSource.PlayOneShot(blinkSound);

            // Wait before next blink (to control the frequency)
            yield return new WaitForSeconds(1f / blinkFrequency);
        }
    }

    // You can stop the blinking effect manually if needed
    public void StopBlinking()
    {
        StopCoroutine(BlinkLight());
        lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, 1f); // Reset opacity to max
        isBlinking = false;
    }

    // You can start the blinking effect manually if needed
    public void StartBlinking()
    {
        if (!isBlinking)
        {
            StartCoroutine(BlinkLight());
        }
    }

    // This function allows you to change the light's color dynamically via a color picker in the Unity Inspector
    public void SetLightColor(Color newColor)
    {
        lightColor = newColor;
        lightImage.color = lightColor; // Immediately update the image color
    }
}
