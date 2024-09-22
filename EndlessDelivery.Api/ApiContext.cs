﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Api.Requests;

namespace EndlessDelivery.Api;

public class ApiContext
{
    private const string ProdUrl = "https://delivery.wafflethings.dev/api/";
    public readonly Uri BaseUri;
    public readonly HttpClient Client;
    public string? Token;
    private readonly Func<string> _getTicket;

    public ApiContext(HttpClient client, Func<string> getTicket, Uri? baseUri = null)
    {
        Client = client;
        _getTicket = getTicket;
        BaseUri = baseUri == null ? new Uri(ProdUrl) : baseUri;
    }

    public async Task EnsureAuth()
    {
        try
        {
            Token = await this.GetToken(_getTicket());
        }
        catch (InternalServerException ex)
        {
            await Task.FromException(ex);
        }
    }
}
