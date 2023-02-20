using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public TestService(Auth auth, Mobile mobile)
        {
            Logger = NullLogger<TestService>.Instance;
            _auth = auth;
            _mobile = mobile;
        }

        public async Task SayHelloAsync()
        {
            // token "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnRJZCI6Ikw3QzlZS1Y5LUVTRjM2MDZRLUdIRjlIMUY1LThMSk1LUk81IiwiY2xpZW50TmFtZSI6Im1vYmlsZS1pb3MiLCJyZWdpb24iOiJzZyIsInNjb3BlIjpbInNpbmdhcG9yZSJdLCJ1c2VybmFtZSI6ImphY2VjaGFuZ3JlYWxlc3RhdGVAZ21haWwuY29tIiwiYWdlbnRJZCI6MTQ4NTc2NjQsInVzZXJJZCI6MTQ4NTc2NjQsImlkIjoiZjljNzlhMGQtNGIyNi00NDc1LWIwZmYtYmVhYmZjMWZjNjFlIiwidW1zdGlkIjoiZDQzYWQwOWUtYWY4Mi00MWZjLWExNjEtNjBlYWRhMzY2MzRmIiwicm9sZXMiOlsiQUdFTlQiLCJVU0VSIl0sImdyYW50VHlwZSI6InBhc3N3b3JkIiwiaWF0IjoxNjc2ODc1MTA5LCJleHAiOjE2NzgxNzExMDksImlzcyI6ImF1dGhvcml6YXRpb24tc2VydmVyIn0.MCbvGN8Wh09x8hW4jYA-q3pbi1vtXOvOntj9Z0UUB3oXQui1JQ8e-7MbdeFQBKoV3SnQ0fom0auSlGBEBXBKb-8OsELBZInBFFsTPmst_fZgm1xcrY8yKuzC8RZO1x7HFitKC8OhZScCNpxHiOtm1ZLzbxTAb465gUKxMQTkoxU6ebMdyfmev6ciZ4q9x_0JtEAs06OaP5MxgnL1VQmpVewxFhXBK9l-eCOtniQvbGPJlwielMfxJOqf_1MezAZuwY0pyFk9I7KgPUNsgeOJ0sU0YZOO4btSsN40PwP2uatliQiQMFKyKjQHj7PPqP-p9P61zPQLPptxnAF7cuU2-hdIQx30-f3hh5H-MbM38Won13jc1LtG2x5-NU6Ny5p4mp1Km-Uo8gKf8AITnxpcRUO-Jablwa8eGo7VmjcoGdZX-ECbV1vXAqo_e9nxYBVmORMmV1uHRWuG8B_QjkV6QL9ARxS_i3F5EdzbNZxlNRhSETIJBJNSH4XadxhLn9nCQvgF3CKxiyitdxY57BFqD--Bb0KcIokTDUtM9DFalccauWpOHF2WJ9RPPJrIM4a3tsOlesqZuGdpGI3FwdWEWqDhA8b3VtJ-4jfcwDJIik0DrN4r0_ylf2locSmZof1sYPxSni0DF_-1NZSP252YiaWpaijar9T37lDWzMUCjCY"

            //var token = await _auth.LoginAsync(new AuthLogin()
            //{
            //    UserName = "jacechangrealestate@gmail.com",
            //    Password = "P@ssword221!"
            //});

            var token = new Token()
            {
                accessToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnRJZCI6Ikw3QzlZS1Y5LUVTRjM2MDZRLUdIRjlIMUY1LThMSk1LUk81IiwiY2xpZW50TmFtZSI6Im1vYmlsZS1pb3MiLCJyZWdpb24iOiJzZyIsInNjb3BlIjpbInNpbmdhcG9yZSJdLCJ1c2VybmFtZSI6ImphY2VjaGFuZ3JlYWxlc3RhdGVAZ21haWwuY29tIiwiYWdlbnRJZCI6MTQ4NTc2NjQsInVzZXJJZCI6MTQ4NTc2NjQsImlkIjoiZjljNzlhMGQtNGIyNi00NDc1LWIwZmYtYmVhYmZjMWZjNjFlIiwidW1zdGlkIjoiZDQzYWQwOWUtYWY4Mi00MWZjLWExNjEtNjBlYWRhMzY2MzRmIiwicm9sZXMiOlsiQUdFTlQiLCJVU0VSIl0sImdyYW50VHlwZSI6InBhc3N3b3JkIiwiaWF0IjoxNjc2ODc1MTA5LCJleHAiOjE2NzgxNzExMDksImlzcyI6ImF1dGhvcml6YXRpb24tc2VydmVyIn0.MCbvGN8Wh09x8hW4jYA-q3pbi1vtXOvOntj9Z0UUB3oXQui1JQ8e-7MbdeFQBKoV3SnQ0fom0auSlGBEBXBKb-8OsELBZInBFFsTPmst_fZgm1xcrY8yKuzC8RZO1x7HFitKC8OhZScCNpxHiOtm1ZLzbxTAb465gUKxMQTkoxU6ebMdyfmev6ciZ4q9x_0JtEAs06OaP5MxgnL1VQmpVewxFhXBK9l-eCOtniQvbGPJlwielMfxJOqf_1MezAZuwY0pyFk9I7KgPUNsgeOJ0sU0YZOO4btSsN40PwP2uatliQiQMFKyKjQHj7PPqP-p9P61zPQLPptxnAF7cuU2-hdIQx30-f3hh5H-MbM38Won13jc1LtG2x5-NU6Ny5p4mp1Km-Uo8gKf8AITnxpcRUO-Jablwa8eGo7VmjcoGdZX-ECbV1vXAqo_e9nxYBVmORMmV1uHRWuG8B_QjkV6QL9ARxS_i3F5EdzbNZxlNRhSETIJBJNSH4XadxhLn9nCQvgF3CKxiyitdxY57BFqD--Bb0KcIokTDUtM9DFalccauWpOHF2WJ9RPPJrIM4a3tsOlesqZuGdpGI3FwdWEWqDhA8b3VtJ-4jfcwDJIik0DrN4r0_ylf2locSmZof1sYPxSni0DF_-1NZSP252YiaWpaijar9T37lDWzMUCjCY"
            };
            _mobile.Token= token;
            var listing = await _mobile.ListingManagementAsync(new QueryListingManagement());
        }
    }
}
