using System.Text;
using Microsoft.AspNetCore.Mvc;
using VpMeta.Application;
using VpMeta.Models;

namespace VpMeta.Api;

[ApiController]
[Route("/")]
public class ClinicalTrialMetadataController(
    ClinicalTrialMetadataService service,
    IJsonParserWithValidation parser
        ) : Controller
{
    private readonly ClinicalTrialMetadataService service = service;
    private readonly IJsonParserWithValidation parser = parser;

    [HttpPost]
    [Route("upload")]
    public async Task<ActionResult> PostFile(IFormFile file)
    {
        //Accept only files with a .json extension
        if (!file.FileName.EndsWith(".json"))
            return BadRequest("File has not .json extension");
        using var reader = new StreamReader(file.OpenReadStream());
        var str = await reader.ReadToEndAsync();
        return await ProcessStringData(str);
    }
    [HttpPost]
    public async Task<ActionResult> PostSingle()
    {
        var str = await Request.GetRawBodyAsync();
        return await ProcessStringData(str);
    }

    private async Task<ActionResult> ProcessStringData(string str)
    {
        try
        {
            var json = parser.Parse(str);
            await service.Create(json);
            return Created();
        }
        catch (ApiValidationException e)
        {
            return UnprocessableEntity(e.Errors);
        }
        catch (DuplicateKeyException)
        {
            return Conflict("Resource already exists");
        }
        catch
        {
            return BadRequest("Invalid JSON content");
        }
    }

    [HttpGet]
    public async Task<IEnumerable<ClinicalTrialMetadata>> GetAll(
        int? start,
        int? limit,
        ClinicalTrialStatus? status,
        DateOnly? startedBefore,
        DateOnly? startedAfter,
        DateOnly? endedBefore,
        DateOnly? endedAfter
        )
    {
        return await service.Select(new()
        {
            Start = start,
            Limit = limit,
            Status = status,
            StartedBefore = startedBefore,
            StartedAfter = startedAfter,
            EndedBefore = endedBefore,
            EndedAfter = endedAfter
        });
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<ClinicalTrialMetadata>> Get(string id)
    {
        var res = await service.FindById(id);
        if (res == null) return NotFound();
        return res;
    }
}

public static class RequestExtensions
{
    public static async Task<string> GetRawBodyAsync(
     this HttpRequest request,
     Encoding? encoding = null)
    {
        if (!request.Body.CanSeek)
            request.EnableBuffering();

        request.Body.Position = 0;
        var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;
        return body;
    }

}