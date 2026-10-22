using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PlayerSpawnManager : MonoBehaviour
{
    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. спавн на точке
        if (!string.IsNullOrEmpty(DoorTransition.targetDoorId))
        {
            SpawnPoint[] points = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SpawnPoint point in points)
            {
                if (point.doorId == DoorTransition.targetDoorId)
                {
                    transform.position = point.transform.position;
                    break;
                }
            }

            DoorTransition.targetDoorId = null;
        }

        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
        }
    }
}