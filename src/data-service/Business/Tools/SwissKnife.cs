namespace DataService.Business.Tools
{
    public static class SwissKnife
    {
        public static Guid GenerateGuid() =>  Guid.CreateVersion7(DateTimeOffset.UtcNow);
        
    }
}
