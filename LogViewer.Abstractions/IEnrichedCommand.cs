using System.Windows.Input;

namespace LogViewer.Abstractions
{
    public interface IEnrichedCommand : ICommand
    {
        void Execute();
    }
}