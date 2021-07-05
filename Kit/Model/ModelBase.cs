using Kit.Sql.Attributes;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Kit.Model
{
    public abstract class ModelBase : INotifyPropertyChanged, IDisposable
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        //[Obsolete("Use Raise para mejor rendimiento evitando la reflección")]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }

        #endregion INotifyPropertyChanged

        #region PerfomanceHelpers

        protected async void Raise<T>(Expression<Func<T>> propertyExpression)
        {
            await Task.Yield();
            if (this.PropertyChanged != null)
            {
                MemberExpression body = propertyExpression.Body as MemberExpression;
                if (body == null)
                    throw new ArgumentException("'propertyExpression' should be a member expression");

                //ConstantExpression expression = body.Expression as ConstantExpression;
                //if (expression == null)
                //{
                //    throw new ArgumentException("'propertyExpression' body should be a constant expression");
                //}

                object target = Expression.Lambda(body.Expression).Compile().DynamicInvoke();

                PropertyChangedEventArgs e = new PropertyChangedEventArgs(body.Member.Name);
                try
                {
                    PropertyChanged(target, e);
                    OnPropertyRaised(target, e.PropertyName);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "On RAISE");
                }
            }
        }

        protected void Raise<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (Expression<Func<T>> propertyExpression in propertyExpressions)
            {
                Raise<T>(propertyExpression);
            }
        }

        protected virtual void OnPropertyRaised(object target, string PropertyName)
        {
        }

        #endregion PerfomanceHelpers

        protected async Task RunTask(Task task,
            bool handleException = true,
            CancellationTokenSource token = default(CancellationTokenSource), [CallerMemberName] string caller = "")
        {
            if (token != null && token.IsCancellationRequested)
                return;

            Exception exception = null;

            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {
                Log.Logger.Information($"{caller} Task Cancelled");
            }
            catch (AggregateException e)
            {
                var ex = e.InnerException;
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
                Log.Logger.Error($"{caller}  Error", exception);
            }
            finally
            {
                if (exception is not null && !handleException)
                {
                    throw exception;
                }
            }
        }

        protected void RunTask(Action task,
    bool handleException = true,
    CancellationTokenSource token = default(CancellationTokenSource), [CallerMemberName] string caller = "")
        {
            if (token != null && token.IsCancellationRequested)
                return;

            Exception exception = null;

            try
            {
                task.Invoke();
            }
            catch (TaskCanceledException)
            {
                Log.Logger.Information($"{caller} Task Cancelled");
            }
            catch (AggregateException e)
            {
                var ex = e.InnerException;
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
                Log.Logger.Error($"{caller}  Error", exception);
            }
            finally
            {
                if (!handleException)
                {
                    throw exception;
                }
            }
        }

        public virtual void Dispose()
        {
        }
    }
}