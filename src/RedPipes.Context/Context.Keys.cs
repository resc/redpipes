﻿namespace RedPipes
{
    public static partial class Context
    {
        private sealed class Key
        {
            private readonly string _name;

            public Key(string name)
            {
                _name = name;
            }

            public override string ToString()
            {
                return _name ?? base.ToString();
            }
        }

        /// <summary> returns a new key </summary>
        public static object NewKey()
        {
            return new Key(null);
        }

        /// <summary> returns a new key with the given name ( the name is for debugging purposes only ) </summary>
        public static object NewKey(string name)
        {
            return new Key(name);
        }
    }
}
