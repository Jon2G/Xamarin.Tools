using Kit.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Kit.WPF.Controls
{
    public abstract class ISearchTextBoxProvider : ModelBase
    {
        protected ObservableCollection<object> _Suggestions;
        public ObservableCollection<object> Suggestions
        {
            get => _Suggestions;
        }
        private bool _IsOpen;
        public bool IsOpen
        {
            get => _IsOpen; set
            {
                _IsOpen = value;
                Raise(() => IsOpen);
            }
        }

        private object _Lock = new object();
        public ISearchTextBoxProvider()
        {
            this._Suggestions = new ObservableCollection<object>();
            // Collection Sync should be enabled from the UI thread. Rest of the collection access can be done on any thread
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            { BindingOperations.EnableCollectionSynchronization(this._Suggestions, _Lock); }));

        }
        public abstract string GetId(object SelectedItem);
        protected abstract void GetSuggestions(string filter);
        private bool IsLoading;
        public void LoadSuggestions(string filter)
        {

            Thread th = new Thread(() =>
            {
                try
                {
                    if (IsLoading)
                    {
                        return;
                    }
                    IsLoading = true;
                    GetSuggestions(filter);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Al cargar las sugerencias");
                }
                finally
                {
                    IsLoading = false;
                    OnPropertyChanged(nameof(Suggestions));
                }
            });
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();


        }

        public abstract string Query
        {
            get;
        }
    }
}
