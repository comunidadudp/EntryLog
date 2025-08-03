namespace EntryLog.Business.ImageBB
{
    internal record ImageBBOptions
    {
        public string ApiUrl { get; set; }
        public string ApiToken { get; set; }
        public int ExpirationSeconds { get; set; }
    }
}
