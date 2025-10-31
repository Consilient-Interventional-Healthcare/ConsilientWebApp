namespace Consilient.Api.Client.Models
{
    public class File(byte[] content, string contentType, string fileName)
    {

        public byte[] Content { get;  } = content;
        public string ContentType { get; } = contentType;
        public string FileName { get; } = fileName;
    }
}
