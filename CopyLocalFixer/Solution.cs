using System.Collections.Generic;

namespace CopyLocalFixer
{
    public class Solution
    {
        public Solution()
        {
            Projects = new List<SolutionProject>();
        }

        public List<SolutionProject> Projects { get; private set; }
    }
}