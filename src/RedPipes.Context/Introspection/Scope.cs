using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace RedPipes.Introspection
{
    class Scope : SortedDictionary<string, object>, IScope
    {
        private const string ValidNamePattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        private static readonly Regex _validNamePattern = new Regex(ValidNamePattern, RegexOptions.Compiled);
        
        IScope IScope.Scope(string name)
        {
            EnsureNameValid(name);
            //  name =  name;
            if (TryGetValue(name, out var scope) && scope is Scope s)
                return s;

            s = new Scope();
            this[name] = s;
            return s;
        }

        IScope IScope.Attr(string name, [NotNull] object value)
        {
            EnsureNameValid(name);
            EnsureNotNull(value);

            if (!IsValidType(value))
            {
                value = ConvertToString(value);
            }

            if (TryGetValue(name, out var v))
            {
                if (!(v is ArrayList list))
                {
                    list = new ArrayList { v };
                    v = list;
                    this[name] = v;

                }

                list.Add(value);
            }
            else
            {
                this[name] = value;
            }
            return this;
        }

        private static void EnsureNotNull(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }

        private string ConvertToString(object value)
        {
            var c = TypeDescriptor.GetConverter(value);
            return c.ConvertToInvariantString(c);
        }

        private static bool IsValidName(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            return _validNamePattern.IsMatch(s);
        }

        private static bool IsValidType(object value)
        {
            if (value is string || value.GetType().IsPrimitive)
                return true;

            return false;
        }

        private static void EnsureNameValid(string name)
        {
            if (!IsValidName(name))
                throw new ArgumentException("Name must match regular expression " + ValidNamePattern);
        }
    }
}
