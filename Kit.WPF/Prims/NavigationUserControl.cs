using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Kit.WPF.Prims
{
    public class NavigationUserControl : UserControl, INavigationAware, INotifyPropertyChanged
    {
        public const string ContentRegion = "ContentRegion";
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }
        #region PerfomanceHelpers
        public void Raise<T>(Expression<Func<T>> propertyExpression)
        {
            if (this.PropertyChanged != null)
            {
                MemberExpression body = propertyExpression.Body as MemberExpression;
                if (body == null)
                    throw new ArgumentException("'propertyExpression' should be a member expression");

                ConstantExpression expression = body.Expression as ConstantExpression;
                if (expression == null)
                    throw new ArgumentException("'propertyExpression' body should be a constant expression");

                object target = Expression.Lambda(expression).Compile().DynamicInvoke();

                PropertyChangedEventArgs e = new PropertyChangedEventArgs(body.Member.Name);
                PropertyChanged(target, e);
            }
        }

        public void Raise<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (Expression<Func<T>> propertyExpression in propertyExpressions)
            {
                Raise<T>(propertyExpression);
            }
        }

        #endregion
        #endregion
        private readonly IRegionManager RegionManager;
        private NavigationContext NavigationContext;

        public NavigationUserControl() { }

        protected void Push<T>(Dictionary<string, object> parameters)
        {
            NavigationParameters parames = new NavigationParameters();
            foreach (KeyValuePair<string, object> value in parameters)
            {
                parames.Add(value.Key, value.Value);
            }
            Push<T>(parames);
        }
        protected void Push<T>(NavigationParameters parameters = null)
        {
            string Name = typeof(T).Name;
            this.RegionManager.Regions[ContentRegion].NavigationService.Journal.NavigationTarget.RequestNavigate(Name, parameters);
        }
        protected bool Pop()
        {
            if (this.RegionManager.Regions[ContentRegion].NavigationService.Journal.CanGoBack)
            {
                this.RegionManager.Regions[ContentRegion].NavigationService.Journal.GoBack();
                return true;
            }
            return false;
        }
        protected void PopAll()
        {
            while (this.Pop())
            {
                //;)
            }
        }

        public NavigationUserControl(IRegionManager IRegionManager)
        {
            this.RegionManager = IRegionManager;
        }
        public bool IsNavigationTarget(NavigationContext NavigationContext)
        {
            this.NavigationContext = NavigationContext;
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext NavigationContext)
        {
            OnNavigatedToFirstTime = true;
            this.NavigationContext = NavigationContext;
        }
        private bool OnNavigatedToFirstTime = true;
        public void OnNavigatedTo(NavigationContext NavigationContext)
        {
            if (!OnNavigatedToFirstTime)
            {
                return;
            }
            OnNavigatedToFirstTime = false;
            this.NavigationContext = NavigationContext;
            OnNavigatedTo();
        }
        protected virtual void OnNavigatedTo() { }
        protected NavigationParameters GetParameters()
        {
            return this.NavigationContext.Parameters;
        }
        protected T GetParameter<T>(string Name)
        {
            return (T)this.NavigationContext.Parameters[Name];
        }

    }
}
