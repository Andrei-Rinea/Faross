namespace Faross.Util
{
    public static class HashCodeUtil
    {
        /// <summary>
        /// Hashes several objects into a combined hash
        /// </summary>
        /// <param name="components">the components to form the hash</param>
        /// <returns>An <see cref="int"/> as a combined hash</returns>
        public static int GetCombinedHash(params object[] components)
        {
            if (components == null || components.Length == 0) return 0;
            var result = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var component in components)
                result = result * 23 + (component?.GetHashCode() ?? 0);
            return result;
        }
    }
}