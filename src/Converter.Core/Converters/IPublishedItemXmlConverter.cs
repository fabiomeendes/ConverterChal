using Converter.Core.Contracts;

namespace Converter.Core.Converters;

public interface IPublishedItemXmlConverter
{
    Task<string> ConvertAsync(InputDto input);
}
