namespace Company.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension(this string fileName)
        {
            if (!fileName.Contains('.'))
                return fileName;

            return fileName.Substring(fileName.LastIndexOf('.') + 1);
        }
    }
}