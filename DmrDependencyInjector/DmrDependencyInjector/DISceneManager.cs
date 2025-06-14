using UnityEngine;
using UnityEngine.SceneManagement;

namespace DmrDependencyInjector
{
    public static class DISceneManager 
    {
        public static void LoadScene(string sceneName)
        {
            DIInjectorManager.SetSceneChanging();

            SceneManager.LoadScene(sceneName);
        }

        public static void LoadScene(int sceneIndex)
        {
            DIInjectorManager.SetSceneChanging();

            SceneManager.LoadScene(sceneIndex);

        }

        public static AsyncOperation LoadSceneAsync(string sceneName)
        {
            DIInjectorManager.SetSceneChanging();

            return SceneManager.LoadSceneAsync(sceneName);
        }

        public static AsyncOperation LoadSceneAsync(int sceneIndex)
        {
            DIInjectorManager.SetSceneChanging();

            return SceneManager.LoadSceneAsync(sceneIndex);
        }
    }
}
