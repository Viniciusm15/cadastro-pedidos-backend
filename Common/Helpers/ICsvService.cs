namespace Common.Helpers
{
    public interface ICsvService
    {
        byte[] WriteCsvToByteArray<T>(IEnumerable<T> records);
    }
}
