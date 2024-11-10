using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Save2IDB.Samples
{
    [RequireComponent(typeof(Button))]
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private string _sceneName = null;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() => SceneManager.LoadScene(_sceneName));
        }
    }
}
