using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
