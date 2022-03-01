using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Expression = System.Linq.Expressions.Expression;

namespace Kit.WPF.Controls
{
    public class CardView : ContentControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        ///[Obsolete("Use Raise para mejor rendimiento evitando la reflección")]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }
        #endregion
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

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                name: nameof(Header), propertyType: typeof(bool),
                ownerType: typeof(UIElement),
                new PropertyMetadata(true, (e, o) =>
                {
                    if (e is CardView card) card.Header = o.NewValue as UIElement;
                }));

        public UIElement Header
        {
            get => (Control)GetValue(HeaderProperty);
            set
            {
                SetValue(HeaderProperty, value);
                OnPropertyChanged();
            }
        }
    }
}
