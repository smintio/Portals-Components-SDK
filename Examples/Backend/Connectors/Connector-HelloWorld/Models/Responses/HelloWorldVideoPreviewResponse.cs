namespace SmintIo.Portals.Connector.HelloWorld.Models.Responses
{
    public class HelloWorldVideoPreviewResponse : HelloWorldImagePreviewResponse
    {
        public decimal? FrameRate { get; set; }

        public decimal? DurationInSeconds { get; set; }
    }
}
