using Converter.Api.Services;
using Converter.Core.Contracts;
using Converter.Core.Converters;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Converter.Api.Controllers;

[ApiController]
[Route("convert")]
public class ConvertController : ControllerBase
{
    private readonly IPublishedItemXmlConverter _converter;

    public ConvertController(IPublishedItemXmlConverter converter) => _converter = converter;

    /// <summary>
    /// Receives input JSON and returns a downloadable XML file.
    /// Fails if: Status != 3, PublishDate < 2024-08-24, or TestRun != true.
    /// </summary>
    [Produces("application/xml")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("json-to-xml")]
    public async Task<IActionResult> JsonToXml([FromBody] InputDto body)
    {
        var xml = await _converter.ConvertAsync(body);

        var fileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xml";

        return File(Encoding.UTF8.GetBytes(xml), "application/xml", fileName);
    }
}
