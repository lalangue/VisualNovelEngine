using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelEngine.Model.Project;

namespace VisualNovelEngine.Model.PathGetter
{
    public static class PathGetter
    {
        public static string GetGroupPath(string groupName)
        {
            return Path.Combine(ProjectData.ProjectPath, groupName);
        }
    }
}
