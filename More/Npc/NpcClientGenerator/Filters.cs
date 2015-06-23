using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace More
{
    public interface IStringFilter
    {
        Boolean Include(String str);
    }
    public class IncludeOnlyFilter
    {
        readonly HashSet<String> includeSet;
        public IncludeOnlyFilter(IEnumerable<String> includeList)
        {
            includeSet = new HashSet<String>(includeList);
        }
        public IncludeOnlyFilter(IEnumerator<String> includes)
        {
            includeSet = new HashSet<String>();
            while (includes.MoveNext())
            {
                includeSet.Add(includes.Current);
            }
        }
        public Boolean Include(String str)
        {
            return includeSet.Contains(str);
        }
    }
    public class IncludeAllExceptFilter
    {
        readonly HashSet<String> excludeSet;
        public IncludeAllExceptFilter(IEnumerable<String> excludeList)
        {
            excludeSet = new HashSet<String>(excludeList);
        }
        public IncludeAllExceptFilter(IEnumerator<String> excludes)
        {
            excludeSet = new HashSet<String>();
            while (excludes.MoveNext())
            {
                excludeSet.Add(excludes.Current);
            }
        }
        public Boolean Include(String str)
        {
            return !excludeSet.Contains(str);
        }
    }
    public delegate Boolean Match(String str);
    /*
    public class Filter
    {
        public static Match Create(String matchFilter)
        {
            Int32 starIndex = matchFilter.IndexOf('*');
            if (starIndex < 0) return new VerbatimMatcher(matchFilter).Match;

            Regex regex = new Regex(matchFilter, RegexOptions.Compiled);
            return new RegexM




        }
    }
    public class VerbatimMatcher
    {
        public readonly String matchString;
        public VerbatimMatcher(String matchString)
        {
            this.matchString = matchString;
        }
        public Boolean Match(String str)
        {
            return this.matchString.Equals(str);
        }
    }
    */
    public class RegexMatcher
    {
        public readonly Regex regex;
        public RegexMatcher(Regex regex)
        {
            this.regex = regex;
        }
        public Boolean Match(String str)
        {
            return regex.IsMatch(str);
        }
    }


    /*
    public class TypeFilter
    {
        const String IncludeOnly = "IncludeOnly";
        const String IncludeAllExcept = "IncludeAllExcept";

        public static Include CreateTypeNameFilter(String[] config)
        {
            if (config == null || config.Length <= 1) throw new FormatException(
                "The 'TypeNameFilter' configuration must have at least 2 arguments");

            String filterTypeString = config[0];
            if (filterTypeString.Equals(IncludeOnly, StringComparison.CurrentCultureIgnoreCase))
            {
                return new IncludeOnlyFilter(config.GetArrayEnumerator(1)).Include;
            }
            if (filterTypeString.Equals(IncludeAllExcept, StringComparison.CurrentCultureIgnoreCase))
            {
                return new IncludeAllExceptFilter(config.GetArrayEnumerator(1)).Include;
            }
            throw new FormatException(String.Format(
                "Expected first argument of 'TypeNameFilter' option to be '{0}' of '{1}' but was '{2}'",
                IncludeOnly, IncludeAllExcept, filterTypeString));

        }

        readonly Include typeNameIncludeFilter;
        readonly Include typeNamespaceIncludeFilter;

        public TypeFilter(Include typeNameIncludeFilter, Include typeNamespaceIncludeFilter)
        {
            this.typeNameIncludeFilter = typeNameIncludeFilter;
            this.typeNamespaceIncludeFilter = typeNamespaceIncludeFilter;
        }
        public Boolean IncludeType(String fullTypeName, String typeName)
        {
            if (typeNameIncludeFilter != null)
            {
                if (!typeNameIncludeFilter(typeName)) return false;
            }
            if (typeNamespaceIncludeFilter != null)
            {
                if (!typeNamespaceIncludeFilter(fullTypeName)) return false;
            }
            return true;
        }
    }
    */

}
