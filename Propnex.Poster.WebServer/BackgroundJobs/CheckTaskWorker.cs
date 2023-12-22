using Blazorise;
using MimeKit;
using Propnex.Poster.WebServer.Entities;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace Propnex.Poster.WebServer.BackgroundJobs
{
    public class CheckTaskWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private static DateTime SendTime = DateTime.Now;

        public CheckTaskWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 1000 * 60; //* 60;//1 hour
        }

        [Volo.Abp.Uow.UnitOfWork(false)]
        protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var repository = workerContext.ServiceProvider.GetRequiredService<IRepository<PnTask>>();
            var mm = workerContext.ServiceProvider.GetService<IStringEncryptionService>();
            var a= mm.Encrypt("c85898ae765fcdb9fc3c1a347d2b6e66-2ae2c6f3-aa147286");
            var waitCount = (await repository.GetQueryableAsync()).Where(q => q.Status == Share.TaskStatus.Wait).Count();
            if (waitCount > 100 && (SendTime < DateTime.Now.AddDays(-1)))
            {
                SendTime = DateTime.Now; using (var clinet = new MailKit.Net.Smtp.SmtpClient())
                {
                    clinet.Connect("smtp.mailgun.org", 587, false);
                    //clinet.AuthenticationMechanisms.Remove("XOAUTH2");
                    clinet.Authenticate("postmaster@mg1.propnex.net", "c85898ae765fcdb9fc3c1a347d2b6e66-2ae2c6f3-aa147286");

                    var message = new MimeKit.MimeMessage();
                    message.From.Add(new MailboxAddress("postmaster", "postmaster@mg1.propnex.net"));
                    message.To.Add(new MailboxAddress("523291540@qq.com", "523291540@qq.com"));
                    message.To.Add(new MailboxAddress("ymail2000@gmail.com", "ymail2000@gmail.com"));
                    message.To.Add(new MailboxAddress("michael.koh@propnex.com", "michael.koh@propnex.com"));
                    message.To.Add(new MailboxAddress("lindachan@propnex.com", "lindachan@propnex.com"));
                    message.Subject = string.Format($"HIGH ALERT! Poster Detected. Please check", "523291540@qq.com,ymail2000@gmail.com,michael.koh@propnex.com,lindachan@propnex.com");

                    message.Body = new TextPart("plain")
                    {
                        Text = $"Guru poster has over {waitCount} tasks waiting to be executed"
                };

                    clinet.Send(message);
                    clinet.Disconnect(true);
                }
            }
        }
    }
}
