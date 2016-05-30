//First written by Rick Strahl in https://github.com/RickStrahl/AspNetWebApiArticle/blob/master/AspNetWebApi/Code/WebApi/Binders/SimplePostVariableParameterBinding.cs
//He has an article about this situation:http://weblog.west-wind.com/posts/2012/May/08/Passing-multiple-POST-parameters-to-Web-API-Controller-Methods

//I as "Tahir Cakmak" copied his file and add some features
//application/json support and Nullable Type support.
//http://linkedin.com/in/mtcakmak

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace WebAPIPost.Common
{
    public class ArgumentsParameterBinding : HttpParameterBinding
    {
        private const string MultipleBodyParameters = "MultipleBodyParameters";

        public ArgumentsParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        /// <summary>
        /// Check for simple binding parameters in POST data. Bind POST
        /// data as well as query string data
        /// </summary>
        /// <param name="metadataProvider"></param>
        /// <param name="actionContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
                                                    HttpActionContext actionContext,
                                                    CancellationToken cancellationToken)
        {
            string stringValue = null;

            NameValueCollection col = TryReadBody(actionContext.Request);
            if (col != null)
                stringValue = col[Descriptor.ParameterName];

            // try reading query string if we have no POST/PUT match
            if (stringValue == null)
            {
                var query = actionContext.Request.GetQueryNameValuePairs();
                if (query != null)
                {
                    var matches = query.Where(kv => kv.Key.ToLower() == Descriptor.ParameterName.ToLower());
                    if (matches.Count() > 0)
                        stringValue = matches.First().Value;
                }
            }

            object value = StringToType(stringValue);

            // Set the binding result here
            SetValue(actionContext, value);

            // now, we can return a completed task with no result 
            TaskCompletionSource<AsyncVoid> tcs = new TaskCompletionSource<AsyncVoid>();
            tcs.SetResult(default(AsyncVoid));
            return tcs.Task;
        }


        /// <summary>
        /// Method that implements parameter binding hookup to the global configuration object's
        /// ParameterBindingRules collection delegate.
        /// 
        /// This routine filters based on POST/PUT method status and simple parameter
        /// types.
        /// </summary>
        /// <example>
        /// GlobalConfiguration.Configuration.
        ///       .ParameterBindingRules
        ///       .Insert(0,SimplePostVariableParameterBinding.HookupParameterBinding);
        /// </example>    
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static HttpParameterBinding HookupParameterBinding(HttpParameterDescriptor descriptor)
        {
            var supportedMethods = descriptor.ActionDescriptor.SupportedHttpMethods;

            // Only apply this binder on POST and PUT operations
            if (supportedMethods.Contains(HttpMethod.Post) ||
                supportedMethods.Contains(HttpMethod.Put))
            {
                var parameterType = Nullable.GetUnderlyingType(descriptor.ParameterType) ?? descriptor.ParameterType;
                var supportedTypes = new Type[] { typeof(string),
                                                typeof(int),
                                                typeof(decimal),
                                                typeof(double),
                                                typeof(bool),
                                                typeof(DateTime),
                                                typeof(byte[]),
                                                typeof(Guid)
                                            };

                if (supportedTypes.Any(typ => typ == parameterType))
                    return new ArgumentsParameterBinding(descriptor);
            }

            return null;
        }


        private object StringToType(string stringValue)
        {
            return stringValue == null ? null : TypeDescriptor.GetConverter(Descriptor.ParameterType).ConvertFromString(stringValue);
        }

        /// <summary>
        /// Read and cache the request body
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private NameValueCollection TryReadBody(HttpRequestMessage request)
        {
            object result = null;

            // try to read out of cache first
            if (!request.Properties.TryGetValue(MultipleBodyParameters, out result))
            {
                var contentType = request.Content.Headers.ContentType;

                // only read if there's content and it's form data
                if (contentType.MediaType == "application/x-www-form-urlencoded")
                {
                    // parsing the string like firstname=Hongmei&lastname=ASDASD            
                    result = request.Content.ReadAsFormDataAsync().Result;

                }
                else if (contentType.MediaType == "application/json")
                {
                    result = request.Content.ReadAsStringAsync().Result;

                    var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
                    if (values != null)
                    {
                        result = values.Aggregate(new NameValueCollection(),
                                                             (seed, current) =>
                                                             {
                                                                 seed.Add(current.Key, current.Value == null ? null : current.Value.ToString());
                                                                 return seed;
                                                             });
                    }
                }

                request.Properties.Add(MultipleBodyParameters, result);
            }

            return result as NameValueCollection;
        }

        private struct AsyncVoid
        {
        }
    }
}