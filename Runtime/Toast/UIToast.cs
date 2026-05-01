namespace KitforgeLabs.MobileUIKit.Toast
{
    public abstract class UIToast<TData> : UIToastBase
    {
        public abstract void Bind(TData data);

        protected internal sealed override void BindUntyped(object data) => Bind((TData)data);
    }
}
