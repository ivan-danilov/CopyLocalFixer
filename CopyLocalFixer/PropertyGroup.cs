using System;
using System.Collections.Generic;

namespace CopyLocalFixer
{
    public class PropertyGroup
    {
        /// <summary>
        /// Makes PropertyGroup without conditions
        /// </summary>
        public PropertyGroup()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Makes standard VS-styled property group with condition on Configuration and Platform variables
        /// </summary>
        public PropertyGroup(string configuration, string platform)
            : this(String.Format(" '$(Configuration)|$(Platform)' == '{0}|{1}' ", configuration, platform))
        {
        }

        /// <summary>
        /// Makes custom-conditioned property group
        /// </summary>
        /// <param name="condition"></param>
        public PropertyGroup(string condition)
        {
            Condition = condition;
            Properties = new Dictionary<string, string>();
        }

        public string Condition { get; set; }

        public Dictionary<string, string> Properties { get; private set; }
    }
}