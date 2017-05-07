using System.ComponentModel;
using System.Runtime.CompilerServices;
using Multithreading.Annotations;

namespace Multithreading.ViewModels
{
    public abstract class ViewModelBase<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected ViewModelBase(T model)
        {
            Model = model;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public T Model { get; }
    }
}
