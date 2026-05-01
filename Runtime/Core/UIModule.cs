namespace KitforgeLabs.MobileUIKit.Core
{
    public abstract class UIModule<TData> : UIModuleBase
    {
        public abstract void Bind(TData data);

        internal sealed override void BindUntyped(object data) => Bind((TData)data);
    }
}
