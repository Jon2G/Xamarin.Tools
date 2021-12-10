using System;
using System.Data;
using System.Threading;


namespace Kit.Razor.Data
{
    public class RequestRange
    {
        public int From { get; private set; }
        public int To { get; private set; }
        public int Step { get; private set; }
        public bool CanContinue { get; private set; }
        
        private CancellationTokenSource CancellationTokenSource { get; set; }

        public RequestRange(int Step)
        {
            this.Step = Step;
            this.From = 0;
            this.To = Step;
            this.CanContinue = true;
            RenewCancellationTokenToken();
        }

        public CancellationTokenSource RenewCancellationTokenToken()
        {
            CancellationTokenSource = new CancellationTokenSource();
            return this.CancellationTokenSource;
        }

        public void NextStep()
        {
            this.From = To;
            this.To += this.Step;
        }

        public void Reset()
        {
            this.CanContinue = true;
            this.From = 0;
            this.To = this.Step;
        }

        public void NoMoreRecords()
        {
            this.CanContinue = false;
        }

        public RequestRange Cancel()
        {
            if (CancellationTokenSource.Token.CanBeCanceled)
            {
                CancellationTokenSource.Cancel(false);
            }
            return this;
        }

        public string GetOffset(IDbConnection con)
        {
            string Offset;
            if (con is IDbConnection)
            {
                Offset = $"OFFSET {From} ROWS FETCH NEXT {Step} ROWS ONLY";
            }
            else
            {
                Offset = $"LIMIT {Step} OFFSET 0";
            }
            return Offset;
        }
    }
}