using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.App;
using ANut.Core.Currency;
using ANut.Core.Save;
using Game.Equipment;
using Game.Merge;
using R3;
using VContainer.Unity;
using ANut.Core;

namespace Game.Tutorial
{
    public class HomeTutorialController : IInitializable, IDisposable
    {
        private readonly TutorialDataService _tutorialData;
        private readonly MergeDataService _mergeData;
        private readonly EquipmentDataService _equipment;
        private readonly IMergeEconomyService _economy;
        private readonly MergeDragController _dragController;
        private readonly MergeModel _mergeModel;
        private readonly HomeTutorialView _homeTutorialView;
        private readonly ISaveService _saveService;
        private readonly IAppStateMachine _appState;
        private readonly ICurrencyService _currencyService;

        private CancellationTokenSource _cts;

        public HomeTutorialController(
            TutorialDataService tutorialData,
            MergeDataService mergeData,
            EquipmentDataService equipment,
            IMergeEconomyService economy,
            MergeDragController dragController,
            MergeModel mergeModel,
            HomeTutorialView homeTurorialView,
            ISaveService saveService,
            ICurrencyService currencyService,
            IAppStateMachine appState)
        {
            _tutorialData = tutorialData;
            _mergeData = mergeData;
            _equipment = equipment;
            _economy = economy;
            _dragController = dragController;
            _mergeModel = mergeModel;
            _homeTutorialView = homeTurorialView;
            _saveService = saveService;
            _appState = appState;
            _currencyService = currencyService;
        }

        public void Initialize()
        {
            if (_tutorialData.IsHomeDone)
            {
                Log.Info("[HomeTutorial] Already completed — hiding overlay.");
                _homeTutorialView.Hide();
                return;
            }

            Log.Info("[HomeTutorial] Starting home tutorial.");
            _cts = new CancellationTokenSource();
            RunAsync(_cts.Token).Forget();
        }

        private async UniTask RunAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] RunAsync — init overlay.");
            _homeTutorialView.Show();
            _homeTutorialView.ApplyStepInit();

            await RunStepAsync(TutorialStepKeys.HomeTapTank, StepTapTankAsync, ct);
            await RunStepAsync(TutorialStepKeys.HomeBuyFirstPart, StepBuyFirstPartAsync, ct);
            await RunStepAsync(TutorialStepKeys.HomeTapTankAgain, StepTapTankAgainAsync, ct);
            await RunStepAsync(TutorialStepKeys.HomeBuySecondPart, StepBuySecondPartAsync, ct);
            await RunStepAsync(TutorialStepKeys.HomeMergeParts, StepMergePartsAsync, ct);
            await RunStepAsync(TutorialStepKeys.HomeEquipPart, StepEquipPartAsync, ct);
            await StepShowBattleButtonAsync(ct);

            Log.Info("[HomeTutorial] RunAsync — all steps complete.");
        }

        private async UniTask RunStepAsync(
            string stepKey,
            Func<CancellationToken, UniTask> stepAction,
            CancellationToken ct)
        {
            if (_tutorialData.IsStepDone(stepKey))
            {
                Log.Info("[HomeTutorial] Step already completed: {0}", stepKey);
                return;
            }

            await stepAction(ct);
            _tutorialData.CompleteStep(stepKey);
            _saveService.Save();
        }

        private async UniTask StepTapTankAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: TapTank — waiting for enough coins.");
            _homeTutorialView.ApplyStepTapTank();
            await WaitUntilCanAffordPurchaseAsync(ct);
            Log.Info("[HomeTutorial] Step: TapTank — done.");
        }

        private async UniTask StepBuyFirstPartAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: BuyFirstPart — waiting for purchase #1.");
            _homeTutorialView.ApplyStepBuyFirstPart();
            await WaitUntilPurchaseCountAsync(1, ct);
            Log.Info("[HomeTutorial] Step: BuyFirstPart — done.");
        }

        private async UniTask StepTapTankAgainAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: TapTankAgain — waiting for enough coins.");
            _homeTutorialView.ApplyStepTapTankAgain();
            await WaitUntilCanAffordPurchaseAsync(ct);
            Log.Info("[HomeTutorial] Step: TapTankAgain — done.");
        }

        private async UniTask StepBuySecondPartAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: BuySecondPart — waiting for purchase #2.");
            _homeTutorialView.ApplyStepBuySecondPart();
            await WaitUntilPurchaseCountAsync(2, ct);
            Log.Info("[HomeTutorial] Step: BuySecondPart — done.");
        }

        private async UniTask StepMergePartsAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: MergeParts — waiting for merge (level >= 2).");
            _homeTutorialView.ApplyStepMergeParts();
            await WaitUntilMergedAsync(ct);
            Log.Info("[HomeTutorial] Step: MergeParts — done.");
        }

        private async UniTask StepEquipPartAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: EquipPart — waiting for turret equip.");
            _homeTutorialView.ApplyStepEquipPart();
            await WaitUntilEquippedAsync(ct);
            Log.Info("[HomeTutorial] Step: EquipPart — done.");
        }

        private async UniTask StepShowBattleButtonAsync(CancellationToken ct)
        {
            Log.Info("[HomeTutorial] Step: ShowBattleButton — waiting for battle tap.");
            _homeTutorialView.ApplyStepShowBattleButton();

            var battleTappedTcs = new UniTaskCompletionSource();

            void OnBattleClicked()
            {
                battleTappedTcs.TrySetResult();
            }

            _homeTutorialView.BattleClicked += OnBattleClicked;

            try
            {
                using var reg = ct.Register(() => battleTappedTcs.TrySetCanceled());
                await battleTappedTcs.Task;
            }
            finally
            {
                _homeTutorialView.BattleClicked -= OnBattleClicked;
            }

            Log.Info("[HomeTutorial] Step: ShowBattleButton — battle tapped, completing tutorial.");
            _tutorialData.CompleteHome();
            _saveService.Save();

            _appState.EnterTutorialBattle();
        }

        private async UniTask WaitUntilCanAffordPurchaseAsync(CancellationToken ct)
        {
            await _currencyService
                .GetBalance(CurrencyType.Coins)
                .Where(_ => _economy.CanBuyPart())
                .FirstAsync(cancellationToken: ct);
        }

        private async UniTask WaitUntilPurchaseCountAsync(int count, CancellationToken ct)
        {
            await _mergeData.TotalPurchasesProperty
                .Where(total => total >= count)
                .FirstAsync(cancellationToken: ct);
        }

        private async UniTask WaitUntilMergedAsync(CancellationToken ct)
        {
            await _dragController.DropCompleted
                .Where(_ => _mergeModel.GetMaxPartLevel() >= 1)
                .FirstAsync(cancellationToken: ct);
        }

        private UniTask WaitUntilEquippedAsync(CancellationToken ct)
        {
            if (_equipment.GetLevel(TankPartType.Turret) > 0)
                return UniTask.CompletedTask;

            return UniTask.WaitUntil(
                () => _equipment.GetLevel(TankPartType.Turret) > 0,
                cancellationToken: ct);
        }

        public void Dispose()
        {
            if (_cts == null)
                return;

            Log.Info("[HomeTutorial] Dispose — cancelling tutorial flow.");
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}