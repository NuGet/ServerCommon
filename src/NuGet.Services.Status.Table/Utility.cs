namespace NuGet.Services.Status.Table
{
    internal static class Utility
    {
        public static string ToRowKeySafeComponentPath(string componentPath)
        {
            return componentPath.Replace(Constants.ComponentPathDivider, '_');
        }
    }
}
