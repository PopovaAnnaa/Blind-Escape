using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private string doorId;

    public static string targetDoorId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetDoorId = doorId;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}