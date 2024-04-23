using System;
using UnityEngine;

namespace Agava.Wink
{
    /// <summary>
    ///     Load target scene after auth.
    /// </summary>
    internal class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string _startSceneName;

        private void Start()
        {
            if (string.IsNullOrEmpty(_startSceneName))
                throw new NullReferenceException("Start Name Scene is Empty on Boot!");
        }

        /// <summary>
        ///     Load game scene.
        /// </summary>
        internal void LoadGameScene() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_startSceneName);
        internal void LoadScene(string sceneName) => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        internal void LoadScene(int sceneBuildIndex) => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex);
    }
}