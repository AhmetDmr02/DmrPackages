#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class SceneViewCameraSetter : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SceneViewCameraSetter))]
    public class CameraSetterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Add a button to set the camera location
            if (GUILayout.Button("Set Camera Location"))
            {
                SceneViewCameraSetter cameraSetter = (SceneViewCameraSetter)target;
                SetCameraLocation(cameraSetter);
            }
        }

        private void SetCameraLocation(SceneViewCameraSetter cameraSetter)
        {
            // Get the Scene view camera
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // Set position
                cameraSetter.transform.position = sceneView.camera.transform.position;

                // Set rotation
                cameraSetter.transform.rotation = sceneView.camera.transform.rotation;
            }
            else
            {
                Debug.LogWarning("Scene view camera not found!");
            }
        }
    }
#endif
}
