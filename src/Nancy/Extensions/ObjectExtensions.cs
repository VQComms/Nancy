namespace Nancy.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;

    /// <summary>
    /// Contains extensions to <see cref="object"/> class.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert an object to a dynamic type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic ToDynamic<T>(this T value)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var property in typeof(T).GetProperties())
            {
                expando.Add(property.Name, property.GetValue(value));
            }

            return (ExpandoObject)expando;
        }
    }
}