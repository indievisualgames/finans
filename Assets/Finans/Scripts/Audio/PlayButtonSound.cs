using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayButtonSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip clickClip;

    [SerializeField, Range(0f, 1f)]
    private float volume = 1f;

    [SerializeField, Range(0.5f, 1.5f)]
    private float pitch = 1f;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (cachedButton != null)
        {
            cachedButton.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (cachedButton != null)
        {
            cachedButton.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(clickClip, volume, pitch);
        }
    }
}


