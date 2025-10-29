namespace LiquidCode.Api.Media.Requests;

public class UploadMediaRequest
{
    public required IFormFile File { get; set; }
}