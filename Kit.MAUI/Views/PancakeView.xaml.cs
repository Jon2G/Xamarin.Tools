using AsyncAwaitBestPractices;
using System.Windows.Input;

namespace Kit.MAUI.Views;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PancakeView
{
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        propertyName: nameof(Command),
        returnType: typeof(ICommand),
        declaringType: typeof(PancakeView),
        defaultValue: null,
        propertyChanged: OnCommandChanged);


    private static void OnCommandChanged(BindableObject e, object old_value, object new_value)
    {
        if (e is PancakeView view && new_value != old_value)
        {
            view.Command = new_value as ICommand;
        }
    }
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private bool IsLocked = false;
    const uint AnimationLenght = 250U;
    public PancakeView()
    {
        InitializeComponent();
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        if (!IsLocked && this.Command is not null && this.Command.CanExecute(null))
        {
            IsLocked = true;
            this.ScaleTo(1.2, AnimationLenght)
                .ContinueWith(t =>
                {
                    try
                    {

                        HapticFeedback.Perform();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                })
                .ContinueWith(t => this.ScaleTo(1, AnimationLenght)
                )
                .ContinueWith(t => this.Command.Execute(null)
                ).ContinueWith(t =>
                {
                    IsLocked = false;
                }).SafeFireAndForget();
        }
    }
}