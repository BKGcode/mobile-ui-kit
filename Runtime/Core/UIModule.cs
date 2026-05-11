namespace KitforgeLabs.UIKit.Core
{
    public abstract class UIModule<TData> : UIModuleBase
    {
        public abstract void Bind(TData data);

        protected internal sealed override void BindUntyped(object data) => Bind((TData)data);
    }
}
