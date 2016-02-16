namespace Nancy
{
    using Extensions;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using Nancy.Extensions;

    /// <summary>
    /// Serializes/Deserializes objects for sessions
    /// </summary>
    public class DefaultObjectSerializer : IObjectSerializer
    {
        private readonly IAssemblyCatalog assemblyCatalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultObjectSerializer"/> class.
        /// </summary>
        /// <param name="assemblyCatalog"></param>
        public DefaultObjectSerializer(IAssemblyCatalog assemblyCatalog)
        {
            this.assemblyCatalog = assemblyCatalog;
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="sourceObject">Source object</param>
        /// <returns>Serialised object string</returns>
        public string Serialize(object sourceObject)
        {
            if (sourceObject == null)
            {
                return string.Empty;
            }

            dynamic serializedObject = null;

            //In the case of adding values to cookies it is a string else an object for sessions
            if (sourceObject is string)
            {
                serializedObject = sourceObject;
            }
            else
            {
                var sourceType = this.assemblyCatalog.GetAssemblies().Select(assembly => assembly.GetType(sourceObject.GetType().FullName)).FirstOrDefault(type => type != null);
                if (sourceType == null)
                {
                    throw new SerializationException("Unable to find type " + sourceObject.GetType() + " in its assembly to serialize");
                }

                var assemblyQualifiedName = sourceType.GetTypeInfo().AssemblyQualifiedName;
                if (!string.IsNullOrWhiteSpace(assemblyQualifiedName))
                {
                    serializedObject = sourceObject.ToDynamic();
                    serializedObject.TypeObject = assemblyQualifiedName;
                }
            }
            var json = SimpleJson.SerializeObject(serializedObject);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Deserialize an object string
        /// </summary>
        /// <param name="sourceString">Source object string</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(string sourceString)
        {
            if (string.IsNullOrEmpty(sourceString))
            {
                return null;
            }

            try
            {
                var inputBytes = Convert.FromBase64String(sourceString);
                var json = Encoding.UTF8.GetString(inputBytes);
                //When deserializing objects it will have a TypeObject property
                if (json.Contains("TypeObject"))
                {
                    dynamic serializedObject = SimpleJson.DeserializeObject(json);
                    var actual = SimpleJson.DeserializeObject(json, Type.GetType(serializedObject.TypeObject));
                    return actual;
                }
                return SimpleJson.DeserializeObject(json);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (SerializationException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}