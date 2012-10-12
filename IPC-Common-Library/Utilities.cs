/* Copyright (c) 2004-2012 IP Commerce, INC. - All Rights Reserved.
 *
 * This software and documentation is subject to and made
 * available only pursuant to the terms of an executed license
 * agreement, and may be used only in accordance with the terms
 * of said agreement. This software may not, in whole or in part,
 * be copied, photocopied, reproduced, translated, or reduced to
 * any electronic medium or machine-readable form without
 * prior consent, in writing, from IP Commerce, INC.
 *
 * Use, duplication or disclosure by the U.S. Government is subject
 * to restrictions set forth in an executed license agreement
 * and in subparagraph (c)(1) of the Commercial Computer
 * Software-Restricted Rights Clause at FAR 52.227-19; subparagraph
 * (c)(1)(ii) of the Rights in Technical Data and Computer Software
 * clause at DFARS 252.227-7013, subparagraph (d) of the Commercial
 * Computer Software--Licensing clause at NASA FAR supplement
 * 16-52.227-86; or their equivalent.
 *
 * Information in this software is subject to change without notice
 * and does not represent a commitment on the part of IP Commerce.
 * 
 * Sample Code is for reference Only and is intended to be used for educational purposes. It's the responsibility of 
 * the software company to properly integrate into thier solution code that best meets thier production needs. 
*/

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
