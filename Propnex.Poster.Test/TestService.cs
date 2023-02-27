using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Propnex.Poster.PropertyGuru.Listing;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using Propnex.Poster.PropertyGuru.Mobile.Model;
using Propnex.Poster.PropertyGuru.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Propnex.Poster.Test
{
    public class TestService : ITransientDependency
    {
        public ILogger<TestService> Logger { get; set; }

        public Auth _auth;
        public Mobile _mobile;
        public Api _api;
        public ProjectsApi _projectsApi;
        public TestService(Auth auth, Mobile mobile, Api api, ProjectsApi projectsApi)
        {
            Logger = NullLogger<TestService>.Instance;
            _auth = auth;
            _mobile = mobile;
            _api = api;
            _projectsApi = projectsApi;
        }

        public async Task SayHelloAsync()
        {
            // await Auth();

            //await ApiTest();

            await ProjectApiTest();
        }

        public async Task ProjectApiTest()
        {
            _projectsApi.Token = new PropertyGuru.Mobile.Dto.Token()
            {
                accessToken= "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnRJZCI6Ikw3QzlZS1Y5LUVTRjM2MDZRLUdIRjlIMUY1LThMSk1LUk81IiwiY2xpZW50TmFtZSI6Im1vYmlsZS1pb3MiLCJyZWdpb24iOiJzZyIsInNjb3BlIjpbInNpbmdhcG9yZSJdLCJ1c2VybmFtZSI6ImRhdmlkcHJvcGVydHlAcHJvcG5leC5jb20iLCJhZ2VudElkIjo4NTUwMCwidXNlcklkIjo4NTUwMCwiaWQiOiJkZmIxMWQ5Mi04NjhiLTRiZjMtYjg5My1jZTQ0YzQ5ZTkyNjMiLCJ1bXN0aWQiOiJhYjE5NDgwMC1kNzIzLTQxZDQtYmNkYy0zYWI1YTRhMzVlMGMiLCJyb2xlcyI6WyJBR0VOVCIsIlVTRVIiXSwiZ3JhbnRUeXBlIjoicGFzc3dvcmQiLCJpYXQiOjE2Nzc0NjEyMTIsImV4cCI6MTY3ODc1NzIxMiwiaXNzIjoiYXV0aG9yaXphdGlvbi1zZXJ2ZXIifQ.p_4c6ul7_iyckA_juGXO0EEZknSTTU57Wlq8THUXWUSXm6ke9m7DELuGIJj14gE-zPQa0FQ2yWTCzzNCf-VOeWZi8wZuq1KDMURE2kgaRPDn_fhu_Rt1L77S07GdOY0QELDCGplQzZ5CKSLE0iFbTmDjB5xCIJy0wQUjFfA0s0MAW3Ih9enM4HR153ubbPSUzJWFBiv2L1Lxh00G1cTFPFUUSNKIOJWtBJ_GRgRGyboto7nFVUU3_3jy0Tx4UmoVCprCtPqtLVwe3Nt4_0xtsD0ir2SFi-KHLvY2dN3dLkF_VdEe5P1eXzIEjAtTjUT31FpAK-Ms7aUTjn3SFC4oB-HWJCpO2BKm0SfM6M0ZWt1__cuJMQCquQoqfN5ACl71dAwW-2AQ4GT4NKeYi1RCg1uyTu06fCXTlegXrNzwefybvyEFco16LiF-sXuhJlc7c6jfMkD8B6OQ5IKNtS6XMrEfMre7ptS89rFhCfb1yKCRtt1YkpRAZS75phzfDCOxpUPowmbdPT6HB3zGT4aOxFaonwG92v3L9Ba9YfziQ1NgQPwKf5uoFAfRV7wFmCzOKEV2bAIgqC5EOLgXQGzyS53HVYbtc2DCCU8ADS2BEHHucutqjA-c51XznTwUscUJOFh4TpUPrO7rZUzaqqO0_INXm0URHvov0Sk9KAIKSiE"
            };
            _api.Token = _projectsApi.Token;
            var listing = await _api.ListingsAsync(24233221, new QueryListing() { status_code= "DRAFT" });
            await _api.CreateAsync(listing.Data);
            //var context = await File.ReadAllTextAsync("E:\\885002.guru.tsk");
            //var lenght = context.IndexOf("Xpressor-Listing-File===");
            //var taskContext = context.Substring(0, lenght == -1 ? context.Length : lenght);
            //var guruTasks = new GuruTasks(context, taskContext);
            //foreach (var task in guruTasks.Tasks)
            //{
            //    var token = await _auth.LoginAsync(new AuthLogin()
            //    {
            //        UserName = task.Account,
            //        Password = task.Password
            //    });
            //    _api.Token = token;
            //    _mobile.Token = token;
            //    _projectsApi.Token = token;
            //    if (task.TaskType.ToLower() == "post only")
            //    {
            //        foreach (var listing in task.Listings.Listings)
            //        {
            //            //var listings = _mobile.ListingManagementAsync(new QueryListingManagement(token.User.AgentId.ToString()));
            //            //1. 获取邮政编号
            //            var locales = await _api.AutocompleteAsync(new QueryAutocomplete(listing.Listing.Location.postalCode));
            //            var locale = locales.Data.FirstOrDefault();
            //            //2. 获取loca 信息
            //            var project = (await _projectsApi.GetProjectAsync(int.Parse(locale.ObjectId))).Data;
            //            //3. 组织 createlisting
            //            };
            //            //4. 发布
            //        }
            //    }
            //}
        }

        public async Task ApiTest()
        {
            _api.Token = new PropertyGuru.Mobile.Dto.Token()
            {
                accessToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnRJZCI6Ikw3QzlZS1Y5LUVTRjM2MDZRLUdIRjlIMUY1LThMSk1LUk81IiwiY2xpZW50TmFtZSI6Im1vYmlsZS1pb3MiLCJyZWdpb24iOiJzZyIsInNjb3BlIjpbInNpbmdhcG9yZSJdLCJ1c2VybmFtZSI6InN1bml0YUBzZ2x1eHVyeWhvbWVzLmNvbS5zZyIsImFnZW50SWQiOjE1MzI4MiwidXNlcklkIjoxNTMyODIsImlkIjoiYjkxNGJkMWEtZmFkMS00Y2M4LTg5OTUtYWJkODVkNWJkOGVlIiwidW1zdGlkIjoiZDhkM2EyNDctYjEwZC00MWMwLWE1ZDgtMTE3YzIzMjU3MmFjIiwicm9sZXMiOlsiQUdFTlQiLCJVU0VSIl0sImdyYW50VHlwZSI6InBhc3N3b3JkIiwiaWF0IjoxNjc1ODQyNDIxLCJleHAiOjE2NzcxMzg0MjEsImlzcyI6ImF1dGhvcml6YXRpb24tc2VydmVyIn0.lLuo3a0oETdVaQRbtdwbkZZZhD4Tqkhm5-_GLTeSG427DBs0OygblPkg3ZWZk1YVysZHVG3Q5s45EtD-nzqkftSrY2sglAKtWWESXM_PNBypY-yg6pOWJJhWxznrNpVVJSris57CXMHRcQFPOFP2VjEyoYvhw11W3iz5Z-qGYsRzSrAZL0kCcihPu3gQ5W5mOCqcIaNOwCokIcH3cq1ScrjYX35EVKWRzy2i9p8Hi0HVT9CgHj9jY1seAUfi9A3aXBlmTXBvVVg2fYSh_ZhXXwg-SfGEvkFne_3oSTRl1SIzLVl35_iYSzvUkUdh7deteQLZgLYda3Ye67fiAHwbkrUxHLQEVVEAi_z1_ppvJfH6tOUL4ndu_CbzkeUP_4RMEAoI8QiFr1WCMX79Px3kRC6fNpPCS8ZPKDYGKENz5g2Xb_WeLVbJSD-RH5e3tjfk5ddFSSeTDlITJdtXRgKREq2KcHPZ72BQ0gzPAYWwbMHj1uKyxs0ppEMDDB9uAb4HkSWWsQQ1g9hupUWZVEIL2ZNQM44P4W6fFtsH2w6U3Sz--0UthHIzX-tWw0BUXYU3WFerYJKhoph44UjcKgGBR-FIaqGmJm6zRRSX2aAO_DjewNX1rIUPQrb9Vbtrex-SvkidU1TA7VLJ-m13neEjPIEJw07rt3gM0SrJwXQmBxo"
            };
            //await _api.ListingsAsync(24189499, new QueryListing());
            var result = await _api.AutocompleteAsync(new QueryAutocomplete("111") { });
        }

        public async Task Auth()
        {
            //token "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnRJZCI6Ikw3QzlZS1Y5LUVTRjM2MDZRLUdIRjlIMUY1LThMSk1LUk81IiwiY2xpZW50TmFtZSI6Im1vYmlsZS1pb3MiLCJyZWdpb24iOiJzZyIsInNjb3BlIjpbInNpbmdhcG9yZSJdLCJ1c2VybmFtZSI6ImphY2VjaGFuZ3JlYWxlc3RhdGVAZ21haWwuY29tIiwiYWdlbnRJZCI6MTQ4NTc2NjQsInVzZXJJZCI6MTQ4NTc2NjQsImlkIjoiZjljNzlhMGQtNGIyNi00NDc1LWIwZmYtYmVhYmZjMWZjNjFlIiwidW1zdGlkIjoiZDQzYWQwOWUtYWY4Mi00MWZjLWExNjEtNjBlYWRhMzY2MzRmIiwicm9sZXMiOlsiQUdFTlQiLCJVU0VSIl0sImdyYW50VHlwZSI6InBhc3N3b3JkIiwiaWF0IjoxNjc2ODc1MTA5LCJleHAiOjE2NzgxNzExMDksImlzcyI6ImF1dGhvcml6YXRpb24tc2VydmVyIn0.MCbvGN8Wh09x8hW4jYA-q3pbi1vtXOvOntj9Z0UUB3oXQui1JQ8e-7MbdeFQBKoV3SnQ0fom0auSlGBEBXBKb-8OsELBZInBFFsTPmst_fZgm1xcrY8yKuzC8RZO1x7HFitKC8OhZScCNpxHiOtm1ZLzbxTAb465gUKxMQTkoxU6ebMdyfmev6ciZ4q9x_0JtEAs06OaP5MxgnL1VQmpVewxFhXBK9l-eCOtniQvbGPJlwielMfxJOqf_1MezAZuwY0pyFk9I7KgPUNsgeOJ0sU0YZOO4btSsN40PwP2uatliQiQMFKyKjQHj7PPqP-p9P61zPQLPptxnAF7cuU2-hdIQx30-f3hh5H-MbM38Won13jc1LtG2x5-NU6Ny5p4mp1Km-Uo8gKf8AITnxpcRUO-Jablwa8eGo7VmjcoGdZX-ECbV1vXAqo_e9nxYBVmORMmV1uHRWuG8B_QjkV6QL9ARxS_i3F5EdzbNZxlNRhSETIJBJNSH4XadxhLn9nCQvgF3CKxiyitdxY57BFqD--Bb0KcIokTDUtM9DFalccauWpOHF2WJ9RPPJrIM4a3tsOlesqZuGdpGI3FwdWEWqDhA8b3VtJ-4jfcwDJIik0DrN4r0_ylf2locSmZof1sYPxSni0DF_-1NZSP252YiaWpaijar9T37lDWzMUCjCY"

            var token = await _auth.LoginAsync(new AuthLogin()
            {
                UserName = "jacechangrealestate@gmail.com",
                Password = "P@ssword221!"
            });
        }
    }
}
