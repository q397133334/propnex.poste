using SlackBotMessages;
using SlackBotMessages.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Propnex
{
    public static class SlackBotMessage
    {
        static SbmClient clinet = new SbmClient("https://hooks.slack.com/services/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");

        public static Task<string> SendAsync(string text)
        {
            var message = new Message()
            {
                Text = text
            };
            return clinet.SendAsync(message);
        }
    }
}
