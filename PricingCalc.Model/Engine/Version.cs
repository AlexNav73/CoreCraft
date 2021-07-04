using System;

namespace PricingCalc.Model.Engine
{
    public struct Version
    {
        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;

        public Version(int major, int minor, int patch)
        {
            _major = major;
            _minor = minor;
            _patch = patch;
        }

        public static Version FromString(string version)
        {
            var parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);

            return new Version(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]));
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v1._major > v2._major
                || v1._minor > v2._minor
                || v1._patch > v2._patch;
        }

        public static bool operator <(Version v1, Version v2)
        {
            return v1._major < v2._major
                || v1._minor < v2._minor
                || v1._patch < v2._patch;
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return v1 > v2 || v1 == v2;
        }

        public static bool operator <=(Version v1, Version v2)
        {
            return v1 < v2 || v1 == v2;
        }

        public static bool operator ==(Version v1, Version v2)
        {
            return v1._major == v2._major
                && v1._minor == v2._minor
                && v1._patch == v2._patch;
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public override string ToString()
        {
            return $"{_major}.{_minor}.{_patch}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Version version && this == version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_major, _minor, _patch);
        }
    }
}
