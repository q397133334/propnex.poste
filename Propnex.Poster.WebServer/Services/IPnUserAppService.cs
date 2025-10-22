using Propnex.Poster.Dtos;
using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Propnex.Poster.WebServer.Services
{
    public interface IPnUserAppService : IApplicationService
    {
        public Task<PnUserDto> GetUser(string account);
    }

    public class PnUserAppService : ApplicationService, IPnUserAppService
    {

        public readonly IRepository<PnUser> _repository;

        public PnUserAppService(IRepository<PnUser> repository)
        {
            _repository = repository;
        }

        public async Task<PnUserDto> GetUser(string account)
        {
            var user = (await _repository.GetQueryableAsync()).Where(q => q.Account == account).FirstOrDefault();

            if (user == null)
                return new PnUserDto() { };
            return ObjectMapper.Map<PnUser, PnUserDto>(user);
        }

        public async Task Update(PnUserDto userDto)
        {
            var user = (await _repository.GetQueryableAsync()).Where(q => q.Account == userDto.Account).FirstOrDefault();
            if (user == null)
            {
                user = new PnUser();
                user.Account = userDto.Account;
                user.TokenJson = userDto.TokenJson;
                user.Password = userDto.Password;
                user.PhoneModel = userDto.PhoneModel;
                await _repository.InsertAsync(user);
            }
            else
            {
                user.Account = userDto.Account;
                user.TokenJson = userDto.TokenJson;
                user.Password = userDto.Password;
                user.PhoneModel= userDto.PhoneModel;
                await _repository.UpdateAsync(user);
            }
        }

        public async Task UpateToken(PnUserDto userDto)
        {
            var user = (await _repository.GetQueryableAsync()).Where(q => q.Account == userDto.Account).FirstOrDefault();
            if (user != null)
            {
                user.TokenJson = userDto.TokenJson;
                await _repository.UpdateAsync(user);
            }
        }
    }
}
