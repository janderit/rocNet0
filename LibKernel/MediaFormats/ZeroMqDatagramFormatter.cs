using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LibKernel.MediaFormats
{
    public class ZeroMqDatagramFormatter
    {
        public Request DeserializeRequest(IEnumerable<string> datagram)
        {
            var headers = new List<HeaderLine>();
            var request = "";

            request = datagram.Last();
            foreach (var headerline in datagram.Take(datagram.Count() - 1))
            {
                if (headerline.Trim() != "")
                {
                    var header = headerline.Split(new char[] {':'}, 2);
                    headers.Add(new HeaderLine {Header = header[0].Trim(), Value = header[1].Trim()});
                }
            }

            return new Request {Timestamp = DateTime.Parse(headers.Where(_=>_.Header==ResourceRequestHeaders.Timestamp).Select(_=>_.Value).Single()), AcceptableMediaTypes = headers.Where(_=>_.Header==ResourceRequestHeaders.AcceptMediaType).Select(_=>_.Value).ToList(), NetResourceLocator=request};
        }

        public IEnumerable<string> Serialize(Request request)
        {
            foreach (var acceptableMediaType in request.AcceptableMediaTypes??new List<string>())
            {
                yield return Header(ResourceRequestHeaders.AcceptMediaType, acceptableMediaType);
            }
            yield return Header(ResourceRequestHeaders.Timestamp, request.Timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            /*if ((request.AcceptableMediaTypes??new List<string>()).Count() > 0) */yield return "";
            yield return request.NetResourceLocator;
        }

        public IEnumerable<string> Serialize(Response response)
        {
            yield return ((int)response.Status)+" rocNet 1.0";
            if (response.Status == ResponseCode.Ok)
            {
                foreach (var h in response.Via) yield return Header(ResponseHeaders.Via, h);
                yield return Header(ResponseHeaders.XCache,  ((int)response.XCache).ToString());

                yield return Header(ResourceHeaders.NetResourceIdentifier, response.Resource.NetResourceIdentifier);
                yield return Header(ResourceHeaders.MediaType, response.Resource.MediaType);
                yield return Header(ResourceHeaders.Modified, response.Resource.Modified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                yield return Header(ResourceHeaders.Cacheable, response.Resource.Cacheable ? "YES" : "NO");
                if (response.Resource.Cacheable)
                {
                    if (response.Resource.Expires == DateTime.MaxValue) yield return Header(ResourceHeaders.Expires, "NEVER");
                    else yield return Header(ResourceHeaders.Expires, response.Resource.Expires.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                }
                yield return Header(ResourceHeaders.Energy, response.Resource.Energy.ToString());

                foreach (var h in response.Resource.Correlations ?? new List<Guid>()) yield return Header(ResourceHeaders.Correlation, h.ToString());
                foreach (var h in response.Resource.RevokationTokens ?? new List<Guid>()) yield return Header(ResourceHeaders.Revokation, h.ToString());
                foreach (var h in response.Resource.Relations ?? new List<string>()) yield return Header(ResourceHeaders.Relation, h);

                yield return "";
                yield return response.Resource.Body;
            }
            else
            {
                yield return response.Information;
            }

        }
        public Response DeserializeResponse(IEnumerable<string> datagram)
        {
            if (datagram == null)
            {
                return new Response {Status = ResponseCode.CommunicationsError, Information = "No data received"};
            }

            var ident = datagram.First().Split(new char[] {' '}, 3);

            if (ident[1]!="rocNet") throw new InvalidDataException("Unknown datagram format: "+ident[1]);
            if (ident[2]!="1.0") throw new InvalidDataException("Unsupported datagram version: rocNet "+ident[2]);
            var code = Int32.Parse(ident[0]);
            var status = (ResponseCode) code;

            if (status!=ResponseCode.Ok)
            {
                var info = datagram.Last();
                return new Response {Status = status, Information = info};
            }

            var headers = new List<HeaderLine>();
            var body = "";
            var inbody = false;
            var resource = new ResourceRepresentation();

            foreach (var line in datagram.Skip(1))
            {

                if (inbody)
                {
                    body += line + Environment.NewLine;
                }
                else
                {
                    if (line.Trim() == "") inbody = true;
                    else
                    {
                        var header = line.Split(new char[] {':'}, 2);
                        headers.Add(new HeaderLine {Header = header[0].Trim(), Value = header[1].Trim()});
                    }
                }

            }

            if (!headers.Any(_ => _.Header == ResourceHeaders.NetResourceIdentifier)) return Fail("Missing resource identifier in response");
            if (!headers.Any(_ => _.Header == ResourceHeaders.Modified)) return Fail("Missing mtime in response");
            if (!headers.Any(_ => _.Header == ResourceHeaders.MediaType)) return Fail("Missing media type in response");
            if (!headers.Any(_ => _.Header == ResourceHeaders.Cacheable)) return Fail("Missing cacheable in response");
            if (!headers.Any(_ => _.Header == ResourceHeaders.Energy)) return Fail("Missing energy in response");
            
            resource.Cacheable = headers.Single(_ => _.Header == ResourceHeaders.Cacheable).Value.ToLower().Trim() == "yes";
            resource.Energy = Int32.Parse(headers.Single(_ => _.Header == ResourceHeaders.Energy).Value);
            resource.MediaType = headers.Single(_ => _.Header == ResourceHeaders.MediaType).Value;
            resource.Modified = DateTime.Parse(headers.Single(_ => _.Header == ResourceHeaders.Modified).Value);
            if (headers.Any(_ => _.Header == ResourceHeaders.Expires))
            {
                var value = headers.Single(_ => _.Header == ResourceHeaders.Expires).Value;
                if (value=="NEVER") resource.Expires = DateTime.MaxValue; 
                else resource.Expires = DateTime.Parse(value);
            }
            resource.NetResourceIdentifier= headers.Single(_ => _.Header == ResourceHeaders.NetResourceIdentifier).Value;
            resource.Correlations = headers.Where(_=>_.Header==ResourceHeaders.Correlation).Select(_=>_.Value).Select(_=>new Guid(_)).ToList();
            resource.Relations = headers.Where(_=>_.Header==ResourceHeaders.Relation).Select(_=>_.Value).ToList();
            resource.RevokationTokens = headers.Where(_=>_.Header==ResourceHeaders.Revokation).Select(_=>_.Value).Select(_=>new Guid(_)).ToList();
            var vias = headers.Where(_ => _.Header == ResponseHeaders.Via).Select(_ => _.Value).ToList();
            var xcache = headers.Where(_ => _.Header == ResponseHeaders.XCache).Select(_ => _.Value).SingleOrDefault()
                         ?? ((int)XCache.None).ToString();


            resource.Body = body.Trim();

            return new Response
                       {
                           Status = status,
                           Information = "Ok",
                           Resource = resource,
                           Via = vias,
                           XCache = (XCache) Int32.Parse(xcache)
                       };
        }

        private Response Fail(string info)
        {
            return new Response {Status=ResponseCode.InternalError, Information=info};
        }

        private static string Header(string header, string value)
        {
            return header.Trim() + ": " + value.Trim();
        }
    }
}
