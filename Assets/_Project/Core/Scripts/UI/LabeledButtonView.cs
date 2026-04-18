using System;
using DG.Tweening;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.UI;


namespace ANut.Core.UI
{
    public class LabeledButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _labelText;

        private readonly Subject<Unit> _onClick = new();

        public Observable<Unit> OnClicked => _onClick;

        private void Awake()
        {
            _button.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
                .Subscribe(_ => _onClick.OnNext(Unit.Default))
                .AddTo(this);
        }

        public void SetLabel(string label) => _labelText.text = label;

        public void SetInteractable(bool value) => _button.interactable = value;

        public void PlayShakeAnimation()
        {
            transform.DOKill();
            transform.DOShakePosition(0.3f, new Vector3(10f, 0f, 0f), 10, 0)
                .SetEase(Ease.OutQuad);
        }

        private void OnDestroy() => _onClick.Dispose();
    }
}