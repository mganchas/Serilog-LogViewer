using System;

namespace LogViewer.ViewModel.Abstractions
{
    public interface IComponent
    {
        void RegisterComponent();
        void RemoveComponent();
        void ClearComponent();
        bool IsValidComponent(in Span<ComponentVM> components);
    }
}