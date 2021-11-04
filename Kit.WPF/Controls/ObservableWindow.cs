using Kit.Services.Interfaces;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace Kit.WPF.Controls
{
    public class ObservableWindow : Window, INotifyPropertyChanged, ICrossWindow
    {
        #region ICrossWindow

        Task ICrossWindow.Close() => Task.Run(Close);

        Task ICrossWindow.Show() => Task.Run(Show);

        Task ICrossWindow.ShowDialog() => Task.Run(ShowDialog);

        #endregion ICrossWindow

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

        #endregion INotifyPropertyChanged

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

        #endregion PerfomanceHelpers
    }
}