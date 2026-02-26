using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject respawnText;

    void Awake()
    {
        instance = this;
    }

    public void ShowRespawnText()
    {
        respawnText.SetActive(true);
    }

    public void HideRespawnText()
    {
        respawnText.SetActive(false);
    }
}