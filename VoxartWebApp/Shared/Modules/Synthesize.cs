namespace VoxartWebApp.Shared.Modules.Synthesize
{
    public enum SynthesizeGender
    {
        Male, Female
    }

    public class ResponseModel
    {
        public required string ResultUri { get; set; }
        public required string AudioUri { get; set; }
    }
}
