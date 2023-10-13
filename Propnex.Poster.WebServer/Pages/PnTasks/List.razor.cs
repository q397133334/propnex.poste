using Blazorise;
using Propnex.Poster.Dtos;
using Propnex.Poster.WebServer.Services;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;

namespace Propnex.Poster.WebServer.Pages.PnTasks
{
    public partial class List
    {

        protected PageToolbar Toolbar { get; } = new();

        protected Modal CreatePropertyTaskModal;

        protected CreatePropertyTaskDto CreatePropertyTaskDto;

        public List()
        {
            CreatePropertyTaskDto = new CreatePropertyTaskDto();
        }


        private async Task RetryTask(PnTaskDto pnTask)
        {
            await AppService.PnTaskRetry(new Guid(), pnTask.Id, "system action");
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
