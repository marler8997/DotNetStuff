using System;

namespace More
{
    public class NpcClientNamespaceCaller : INpcClientCaller
    {
        public readonly NpcClient client;
        public readonly String @namespace;
        private readonly String namespacePrefix;

        public NpcClientNamespaceCaller(NpcClient client, String @namespace)
        {
            this.client = client;
            this.@namespace = @namespace;
            this.namespacePrefix = (@namespace[@namespace.Length - 1] == '.') ? @namespace : @namespace + ".";
        }
        public void UpdateAndVerifyEnumAndObjectTypes()
        {
            client.UpdateAndVerifyEnumAndObjectTypes();
        }
        public void VerifyMethodDefinitions(bool forceMethodUpdateFromServer, SosMethodDefinition[] expectedMethods)
        {
            client.VerifyMethodDefinitions(forceMethodUpdateFromServer, expectedMethods);
        }
        public object Call(string methodName, params object[] parameters)
        {
            return client.Call(namespacePrefix + methodName, parameters);
        }
        public object Call(Type returnType, string methodName, params object[] parameters)
        {
            return client.Call(returnType, namespacePrefix + methodName, parameters);
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
