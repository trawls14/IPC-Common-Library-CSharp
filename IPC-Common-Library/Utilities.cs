using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IPC.CommonLibrary
{
    public static class Utilities
    {
        /// <summary>
        /// While dealing with RESTful objects and services references. The object currently being worked with
        /// may need to be converted from a shorter service reference object to the fully qualified namespace
        /// of the object, or the other direction.  
        /// </summary>
        /// <typeparam name="T">The desired object type to convert the object to.</typeparam>
        /// <param name="o">The input object.</param>
        /// <returns>A new object of type T.</returns>
        public static T SwapObjectsNamespace<T>(object o)
        {
            Type objectType = o.GetType();
            
            // Serialize the service reference object into a memory stream
            var ms = new MemoryStream();
            DataContractSerializer ser = new DataContractSerializer(objectType);
            ser.WriteObject(ms, o);

            // Deserialize the memory stream into a data contract object 
            ms.Position = 0;
            ser = new DataContractSerializer(typeof(T));
            return (T) ser.ReadObject(ms);
        }
    }
}
