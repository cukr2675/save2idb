using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.UI;

namespace Save2IDB.Samples
{
    public class PerformanceTestingCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform _content = null;
        [SerializeField] private Text _headerPrefab = null;
        [SerializeField] private Button _buttonPrefab = null;

        [Space]
        [SerializeField] private InputField _iterationPrefab = null;

        private int Iteration => int.Parse(_iterationPrefab.text);

        private void Awake()
        {
            var tests = new RuntimeTestClass[]
            {
                new StorageTest(),
            };

            foreach (var test in tests)
            {
                {
                    // Header
                    var header = Instantiate(_headerPrefab, _content);
                    header.text = test.GetType().Name;
                }

                foreach (var testItem in test.GetPerformanceTests())
                {
                    // Buttons
                    var button = Instantiate(_buttonPrefab, _content);
                    button.onClick.AddListener(() => testItem.action(Iteration));
                    var buttonText = button.GetComponentInChildren<Text>();
                    buttonText.text = testItem.name;
                }
            }
        }
    }
}
