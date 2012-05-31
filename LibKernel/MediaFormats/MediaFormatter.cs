using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LibKernel.Exceptions;
using Newtonsoft.Json;

namespace LibKernel.MediaFormats
{
    public static class MediaFormatter<T> where T:class,new()
    {

        public static T Parse(ResourceRepresentation resource, string tMediaType)
        {
            if (!resource.MediaType.EndsWith(tMediaType)) throw MediaTypeException.Create(tMediaType, resource.MediaType, resource.NetResourceIdentifier);

            if (resource.MediaType.StartsWith("json/"))
            {
                return JsonConvert.DeserializeObject<T>(resource.Body);
            }
            if (resource.MediaType.StartsWith("xml/"))
            {
                var stream = new StringReader(resource.Body);
                return new XmlSerializer(typeof(T)).Deserialize(stream) as T;
            }
            return null;
        }

        public static ResourceRepresentation Pack(string nri, T that, string mediaformat, string tMediatype)
        {
            var body = "";

            if (mediaformat == "xml")
            {
                var sw = new StringWriter();
                new XmlSerializer(typeof(T)).Serialize(sw, that);
                body = sw.ToString();
                sw.Dispose();
            }
            else

                if (mediaformat == "json")
                {
                    body = JsonConvert.SerializeObject(that, Formatting.Indented);
                }

                else throw MediaFormatNotSupportedException.Create(mediaformat, tMediatype);

            return new ResourceRepresentation
            {
                NetResourceIdentifier = nri,
                Body = body,
                MediaType = mediaformat+"/"+tMediatype
            };
        }

    }
}
