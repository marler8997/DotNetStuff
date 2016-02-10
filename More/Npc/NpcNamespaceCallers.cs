using System;
using System.Collections.Generic;

namespace More
{
    public class NpcClientNamespaceCaller : INpcClientCaller
    {
        public readonly INpcClientCaller client;
        public readonly String @namespace;
        private readonly String namespacePrefix;

        public NpcClientNamespaceCaller(INpcClientCaller client, String @namespace)
        {
            this.client = client;
            this.@namespace = @namespace;
            this.namespacePrefix = (@namespace[@namespace.Length - 1] == '.') ? @namespace : @namespace + ".";
        }
        public void UpdateAndVerifyEnumAndObjectTypes()
        {
            client.UpdateAndVerifyEnumAndObjectTypes();
        }
        public void VerifyObject(Boolean forceInterfaceUpdateFromServer, RemoteNpcObject expectedObject)
        {
            client.VerifyObject(forceInterfaceUpdateFromServer, expectedObject);
        }
        public Object Call(String methodName, params Object[] parameters)
        {
            return client.Call(namespacePrefix + methodName, parameters);
        }
        public Object CallOnObject(String objectName, String methodName, params Object[] parameters)
        {
            return client.Call(namespacePrefix + objectName, methodName, parameters);
        }
        public Object Call(Type expectedReturnType, String methodName, params Object[] parameters)
        {
            return client.Call(expectedReturnType, namespacePrefix + methodName, parameters);
        }
        public Object CallOnObject(Type expectedReturnType, String objectName, String methodName, params Object[] parameters)
        {
            return client.Call(expectedReturnType, namespacePrefix + objectName, methodName, parameters);
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
