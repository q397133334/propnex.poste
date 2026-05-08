using Microsoft.Extensions.Logging;
using Propnex.Poster.PropertyGuru.Listing.V2;
using Propnex.Poster.PropertyGuru.Listing.V3;
using Propnex.Poster.PropertyGuru.Mobile;
using Propnex.Poster.PropertyGuru.Mobile.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class Agent : ClientBase
{
    private const string baseUrl = "https://agentnet.propertyguru.com.sg";


    public Token Token { get; set; }

    public ILogger<Agent> Logger { get; set; }
    public Agent() : base(baseUrl)
    {

    }

    public Agent(Token token) : base(baseUrl)
    {
        Token = token;
    }

    public Agent(string proxyIp) : base(baseUrl, proxyIp)
    {

    }

    public Agent(Token token, string proxyIp) : base(baseUrl, proxyIp)
    {
        Token = token;
    }

    CookieCollection cookies;

    //get create listing page , save cookies
    public async Task GetCreateListingAsync()
    {
        var request = GetRequest();
        request.Resource = $"v3/create-listing";
        request.Method = Method.Get;
        //request.AddHeader("x-logger-edited-by", $"{Token.User.AgentId}");
        request.AddHeader(KnownHeaders.Authorization, $"Bearer {Token.accessToken}");
        request.AddHeader("refresh_token", Token.refreshToken);
        request.AddHeader("mobileapp", "true");
        request.AddHeader(KnownHeaders.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Version = new Version(2, 0);

        var response = await ExecuteAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            cookies = response.Cookies;
        }
    }

    public async Task<HttpResult<CreateListingResult>> CreateListingAsync(Propnex.Poster.PropertyGuru.Listing.V3.CreateListingV3 listing)
    {

        if (cookies == null)
        {
            await GetCreateListingAsync();
        }
        var request = GetRequest();
        request.Resource = $"api/agentnet/listings";
        request.Method = Method.Post;
        //request.AddHeader("x-logger-edited-by", $"{Token.User.AgentId}");
        request.AddHeader(KnownHeaders.Authorization, $"Bearer {Token.accessToken}");
        if (request.CookieContainer == null)
        {
            request.CookieContainer = new CookieContainer();
        }
        foreach (var cookie in cookies)
        {
            request.CookieContainer.Add(cookies);
        }
        try
        {
            var stringBody = Newtonsoft.Json.JsonConvert.SerializeObject(listing);
            Log(stringBody, false);
            request.AddStringBody(stringBody, DataFormat.Json);

            var response = await ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateListingResult>(response.Content);
                return new HttpResult<CreateListingResult>
                {
                    HttpStatusCode = response.StatusCode,
                    Data = result
                };
            }
            else
            {
                return new HttpResult<CreateListingResult>
                {
                    Message = response.Content,
                    HttpStatusCode = response.StatusCode,
                    Data = new CreateListingResult { Id = -1 }
                };
            }
        }
        catch (Exception ex)
        {
            Log(ex.ToString(), true);
            return new HttpResult<CreateListingResult>
            {
                Message = ex.Message,
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Data = new CreateListingResult { Id = -1 }
            };

        }
    }
}