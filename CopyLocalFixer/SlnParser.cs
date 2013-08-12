using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace CopyLocalFixer
{
    public class SlnParser
    {
        public Solution Parse(TextReader sr)
        {
            var sln = new Solution();

            string content = sr.ReadToEnd();

            const string pattern = @"\nProject\(""\{[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-Z0-9]{12}\}""\)\s*=\s*""(?<projName>[^""]+)"", ""(?<projPath>[^""]+)"",\s*""\{(?<projGuid>[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-Z0-9]{12})\}""";
            var regexObj = new Regex(pattern, RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            Match match = regexObj.Match(content);
            while (match.Success)
            {
                string projName = match.Groups["projName"].Value;
                string projPath = match.Groups["projPath"].Value;
                string projGuid = match.Groups["projGuid"].Value.ToUpperInvariant();

                if (String.Compare(Path.GetExtension(projPath), ".csproj", true, CultureInfo.InvariantCulture) == 0)
                {
                    sln.Projects.Add(new SolutionProject
                                         {
                                             Name = projName,
                                             Path = projPath,
                                             Guid = projGuid
                                         });
                }

                match = match.NextMatch();
            }

            return sln;
        }
    }
}