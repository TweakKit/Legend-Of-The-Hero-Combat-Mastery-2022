using Cysharp.Threading.Tasks;

namespace Runtime.UI
{
    public interface IAwaitableModal
    {
        #region Interface Methods

        UniTask WaitAsync();

        #endregion Interface Methods
    }

    public abstract class AwaitableModal<T> : Modal<T>, IAwaitableModal where T : ModalData
    {
        #region Members

        protected AsyncReactiveProperty<int> terminateWaitProcessReactiveProperty;

        #endregion Members

        #region Class Methods

        public override async UniTask Initialize(T modalData)
        {
            await base.Initialize(modalData);
            terminateWaitProcessReactiveProperty = new(0);
        }

        public async override UniTask Cleanup()
        {
            await base.Cleanup();
            terminateWaitProcessReactiveProperty.Value = 1;
            terminateWaitProcessReactiveProperty.Dispose();
        }

        public virtual async UniTask WaitAsync()
            => await terminateWaitProcessReactiveProperty.WaitAsync();

        #endregion Class Methods
    }
}