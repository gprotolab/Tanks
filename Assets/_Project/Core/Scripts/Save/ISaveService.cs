using Cysharp.Threading.Tasks;
using System.Threading;


namespace ANut.Core.Save
{
    public interface ISaveService
    {
        void Load();

        UniTask LoadAsync(CancellationToken ct);

        void Save();

        UniTask SaveAsync(CancellationToken ct);
    }
}