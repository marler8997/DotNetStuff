using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.NetworkTools
{

    public class ExtensionFilteredResourceHandler : IResourceHandler
    {
        private readonly IResourceHandler defaultHandler;
        private readonly Dictionary<String,IResourceHandler> handlerDictionary;

        public ExtensionFilteredResourceHandler(IResourceHandler defaultHandler)
        {
            this.defaultHandler = defaultHandler;
            this.handlerDictionary = new Dictionary<String, IResourceHandler>();
        }

        public void AddExtensionHandler(String extension, IResourceHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            handlerDictionary.Add(extension, handler);
        }

        public void HandleResource(HttpRequest request, HttpResponse response)
        {
            Int32 dotIndex = request.url.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex + 1 < request.url.Length)
            {
                String extension = request.url.Substring(dotIndex + 1);

                IResourceHandler handler;
                if (handlerDictionary.TryGetValue(extension, out handler))
                {
                    handler.HandleResource(request, response);
                    return;
                }
            }

            defaultHandler.HandleResource(request, response);
        }
    }
}
