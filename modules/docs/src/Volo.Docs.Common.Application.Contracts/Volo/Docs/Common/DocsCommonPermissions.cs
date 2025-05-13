using Volo.Abp.Reflection;

namespace Volo.Docs.Common
{
    public class DocsCommonPermissions
    {
        public const string GroupName = "Docs.Common";

        public static class Projects
        {
            public const string PdfGeneration = GroupName + ".PdfGeneration";
        }

        public static string[] GetAll()
        {
            return ReflectionHelper.GetPublicConstantsRecursively(typeof(DocsCommonPermissions));
        }
    }
}
