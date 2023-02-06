using System.Windows;
using System.Windows.Controls;

namespace Kit.WPF.Pages
{
    /// <summary>
    /// Interaction logic for Promt.xaml
    /// </summary>
    public partial class Promt : Window
    {
        public string Respuesta { get; private set; }
        public Promt(string pregunta, string textoInicial = "")
        {
            InitializeComponent();
            TxtPregunta.Text = pregunta;
            TxtRespuesta.Text = textoInicial;
            //Validando campos 
            //TxtRespuesta.TextChanged += Validaciones.TextBox_ValidarTextoRegEx;

        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Respuesta = TxtRespuesta.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
