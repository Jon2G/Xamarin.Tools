using AsyncAwaitBestPractices;
using Kit.Razor.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Kit.Razor.Components
{
    public partial class DropZone
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        private Callbacker Callbacker;
        public string FileData64 { get; set; }
        public DropZone()
        {

        }
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
               JSRuntime.InvokeVoidAsync("InitDropZone").SafeFireAndForget();
            }
        }
        public void Upload()
        {

        }
    }
}