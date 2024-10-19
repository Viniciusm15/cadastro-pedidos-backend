namespace Domain.Models.Base
{
    public class ImageBaseModel
    {
        public required string Description { get; set; }
        public required string ImageMimeType { get; set; }
        public required byte[] ImageData { get; set; }
    }
}
