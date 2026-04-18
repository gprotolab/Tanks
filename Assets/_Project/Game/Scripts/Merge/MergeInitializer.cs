using System;
using VContainer.Unity;

namespace Game.Merge
{
    public sealed class MergeInitializer : IInitializable, IDisposable
    {
        private readonly MergeSessionController _session;
        private readonly MergePurchaseController _purchase;
        private readonly MergeExpansionController _expansion;
        private readonly MergeDragController _drag;

        public MergeInitializer(
            MergeSessionController session,
            MergePurchaseController purchase,
            MergeExpansionController expansion,
            MergeDragController drag)
        {
            _session = session;
            _purchase = purchase;
            _expansion = expansion;
            _drag = drag;
        }

        public void Initialize()
        {
            _session.Initialize();
            _purchase.Initialize();
            _expansion.Initialize();
            _drag.Initialize();
        }

        public void Dispose()
        {
            _drag.Dispose();
            _expansion.Dispose();
            _purchase.Dispose();
            _session.Dispose();
        }
    }
}