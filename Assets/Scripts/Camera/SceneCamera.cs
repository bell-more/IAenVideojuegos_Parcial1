using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera targetCam = Camera.main;

#if UNITY_EDITOR
        if (UnityEditor.SceneView.lastActiveSceneView != null)
        {
            targetCam = UnityEditor.SceneView.lastActiveSceneView.camera;
        }
#endif

        if (targetCam != null)
        {
            transform.LookAt(transform.position + targetCam.transform.forward);
        }
    }
}
