#pragma warning disable OPENAI001

using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using System.Text.Json;

namespace iTender.Compliance.Infrastructure.Services;

public class TenderDiscoveryAgent : ITenderDiscoveryAgent
{
    private readonly OpenAIOptions _options;
    private readonly ILogger<TenderDiscoveryAgent> _logger;

    public TenderDiscoveryAgent(
        IOptions<OpenAIOptions> options,
        ILogger<TenderDiscoveryAgent> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DiscoveredTenderDto>> FindTendersAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key has not been configured.");
        }

        var client = new ResponsesClient(_options.Model,_options.ApiKey);

        var prompt = $"""
You are the CRCIP Tender Discovery Agent.

Your job is to identify South African construction tender advertisements.

Search for tenders published between:

From: {fromDate:yyyy-MM-dd}
To: {toDate:yyyy-MM-dd}

Prioritise the following sources:

1. National Treasury eTender Portal
2. iTender
3. Official client/entity websites

Only identify actual tender advertisements.

Do not include:

- News articles
- General procurement announcements
- Construction news
- Duplicate tenders
- Tenders outside the requested date range
- General procurement documents that are not tender advertisements

For every tender found, extract:

- Tender number
- Tender title
- Tender description
- Employer/client
- Contact person
- Contact email
- Contact telephone
- Advertised date
- Closing date
- Source
- Tender URL
- Whether the tender relates to construction
- CIDB class of works, if explicitly stated

Do not guess missing information.

If a value cannot be established from the source,
return null for that field.

Only return tenders where sufficient evidence exists
to establish that the tender advertisement is real.

Return ONLY a valid JSON array.

Each object must contain:

tenderNumber
title
description
employerName
contactName
contactEmail
contactNumber
advertisedDate
closingDate
source
tenderUrl
isConstruction
classOfWorks
confidence

The confidence value must be between 0 and 1.

Do not return markdown.
Do not return ```json.
Do not provide explanations.
Return only the JSON array.
""";

        try
        {
            var options = new CreateResponseOptions
            {
                Model = _options.Model
            };

            options.Tools.Add(
                ResponseTool.CreateWebSearchTool());

            options.InputItems.Add(
                ResponseItem.CreateUserMessageItem(prompt));

            var result =
                await client.CreateResponseAsync(
                    options,
                    cancellationToken);

            var response = result.Value;

            var output = response.GetOutputText();

            if (string.IsNullOrWhiteSpace(output))
            {
                _logger.LogWarning(
                    "OpenAI returned an empty tender discovery response.");

                return [];
            }

            _logger.LogInformation(
                "OpenAI tender discovery response received.");

            _logger.LogDebug(
                "OpenAI response: {Response}",
                output);

            var json = CleanJsonResponse(output);

            try
            {
                var tenders =
                    JsonSerializer.Deserialize<
                        List<DiscoveredTenderDto>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return tenders ?? [];
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "OpenAI returned invalid JSON. Response was: {Response}",
                    output);

                return [];
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Tender discovery operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "AI tender discovery failed.");

            throw;
        }
    }

    private static string CleanJsonResponse(string response)
    {
        response = response.Trim();

        if (response.StartsWith("```"))
        {
            var firstNewLine =
                response.IndexOf('\n');

            if (firstNewLine >= 0)
            {
                response =
                    response[(firstNewLine + 1)..];
            }

            var closingFence =
                response.LastIndexOf("```");

            if (closingFence >= 0)
            {
                response =
                    response[..closingFence];
            }
        }

        return response.Trim();
    }
}