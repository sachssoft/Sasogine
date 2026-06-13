using System;

namespace Sachssoft.Sasogine.Common.Models
{
    public static class IdentifierFactory
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string CreatePretty()
        {
            return Guid.NewGuid().ToString("D");
        }

        public static bool IsValid(string? id)
        {
            return Guid.TryParse(id, out _);
        }

        public static string EnsureOrCreate(string? id)
        {
            return string.IsNullOrWhiteSpace(id)
                ? Create()
                : id;
        }
    }
}