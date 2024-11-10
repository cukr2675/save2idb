using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace Save2IDB.Samples
{
    public class DialogCanvas : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private Text _text = null;
        [SerializeField] private Button _yesButton = null;
        [SerializeField] private Button _noButton = null;
        [SerializeField] private Button _cancelButton = null;

        private Result result;
        private bool isClosed;

        private void Awake()
        {
            _yesButton.onClick.AddListener(() => OnClick(Result.Yes));
            _noButton.onClick.AddListener(() => OnClick(Result.No));
            _cancelButton.onClick.AddListener(() => OnClick(Result.Cancel));

            void OnClick(Result result)
            {
                this.result = result;
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                isClosed = true;
            };
        }

        public async UniTask<Result> ShowAsync(string text)
        {
            _text.text = text;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            isClosed = false;

            // •Â‚¶‚ç‚ê‚é‚Ü‚Å‘Ò‹@
            await UniTask.WaitUntil(() => isClosed);

            return result;
        }

        public enum Result
        {
            Yes,
            No,
            Cancel
        }
    }
}
