using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANut.Core.Ads
{
    [RequireComponent(typeof(Canvas))]
    public class MockAdOverlay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _adTypeLabel;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMP_Text _buttonLabel;

        private UniTaskCompletionSource<bool> _tcs;
        private bool _rewarded;

        private void Awake()
        {
            _actionButton.onClick.AddListener(OnActionClicked);
        }

        public async UniTask<bool> ShowAsync(string adTypeText, bool rewarded, CancellationToken ct)
        {
            _adTypeLabel.text = adTypeText;
            _buttonLabel.text = rewarded ? "Get Reward" : "Close";
            _rewarded = rewarded;
            _tcs = new UniTaskCompletionSource<bool>();

            bool result = false;
            try
            {
                result = await _tcs.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                // Propagate cancellation — caller handles it
                throw;
            }
            finally
            {
                Destroy(gameObject);
            }

            return result;
        }

        private void OnActionClicked()
        {
            _tcs.TrySetResult(_rewarded);
        }
    }
}