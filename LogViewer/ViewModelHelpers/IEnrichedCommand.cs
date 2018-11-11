using System.Windows.Input;

namespace LogViewer.ViewModelHelpers
{
    public interface IEnrichedCommand : ICommand
    {
        void Execute();
    }
}