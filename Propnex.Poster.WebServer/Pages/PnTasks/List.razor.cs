using Blazorise;
using Microsoft.AspNetCore.Components;
using Propnex.Poster.Dtos;
using Propnex.Poster.WebServer.Services;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;

namespace Propnex.Poster.WebServer.Pages.PnTasks
{
    public partial class List
    {

        protected PageToolbar Toolbar { get; } = new();

        protected Modal CreatePropertyTaskModal;
        protected Modal LogModal;

        protected CreatePropertyTaskDto CreatePropertyTaskDto;

        protected List<PnTaskLogDto> TaskLogs;
        protected string LogsTaskNumber;
        protected bool LogsLoading;

        protected string NumberFilter { get; set; }

        public List()
        {
            CreatePropertyTaskDto = new CreatePropertyTaskDto();
        }

        protected override async Task GetEntitiesAsync()
        {
            GetListInput.NumberFilter = NumberFilter;
            await base.GetEntitiesAsync();
        }

        private async Task OnNumberFilterChanged(ChangeEventArgs e)
        {
            NumberFilter = e.Value?.ToString();
            await GetEntitiesAsync();
        }


        private async Task RetryTask(PnTaskDto pnTask)
        {
            await AppService.PnTaskRetry(new Guid(), pnTask.Id, "system action");
        }

        private async Task ShowLogsAsync(PnTaskDto pnTask)
        {
            LogsTaskNumber = pnTask.Number;
            TaskLogs = null;
            LogsLoading = true;
            await LogModal.Show();
            TaskLogs = await AppService.GetLogsAsync(pnTask.Id);
            LogsLoading = false;
        }

        private async Task CreatePropertyTaskAsync()
        {
            await AppService.CreatePropertyTasks(CreatePropertyTaskDto);
            await GetEntitiesAsync();
            await Notify.Success("Create success");
            await CreatePropertyTaskModal.Close(closeReason: CloseReason.None);
        }

        private string GetConfirmationMessage(string message)
        {
            return message;
        }

        private async Task OpenCreatePropertyTaskModalAsync()
        {
            await CreatePropertyTaskModal?.Show();
        }


        protected override ValueTask SetToolbarItemsAsync()
        {
            Toolbar.AddButton("Create Property Tasks", OpenCreatePropertyTaskModalAsync, IconName.Add);
            return base.SetToolbarItemsAsync();
        }
    }
}
