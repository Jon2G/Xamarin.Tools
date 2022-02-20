namespace Kit.WPF.Reportes
{
    public class PrintOptions
    {
        public readonly string Impresora;
        public readonly short VecesTicket;
        public readonly short FromPage;
        public readonly short ToPage;

        public PrintOptions(string Impresora, short VecesTicket = 1, short FromPage = 0, short ToPage = 1)
        {
            this.Impresora = Impresora;
            this.VecesTicket = VecesTicket;
            this.FromPage = FromPage;
            this.ToPage = ToPage;
        }
    }
}