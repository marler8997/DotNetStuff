using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web;

#if WindowsCE
using EnumReflection = System.MissingInCEEnumReflection;
#else
using EnumReflection = System.Enum;
#endif

namespace More
{
    public interface INpcHtmlGenerator
    {
        void GenerateCss(StringBuilder htmlBuilder);

        String TypeAsHtml(Type type);
        void GenerateHtmlValue(StringBuilder htmlBuilder, Object returnObject);

        void GenerateHtmlHeaders(StringBuilder htmlBuilder, String resourceString);

        void GenerateExceptionHtml(StringBuilder htmlBuilder, Exception e);

        void GenerateMethodsPage(StringBuilder htmlBuilder);
        void GenerateTypesPage(StringBuilder htmlBuilder);
        void GenerateTypePage(StringBuilder htmlBuilder, String type);
        void GenerateCallPage(StringBuilder htmlBuilder, String call);
    }
    public class DefaultNpcHtmlGenerator : INpcHtmlGenerator
    {
        public readonly String htmlPageTitle;
        private readonly NpcExecutor npcExecutor;

        public DefaultNpcHtmlGenerator(String htmlPageTitle, NpcExecutor npcExecutor)
        {
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");

            this.htmlPageTitle = htmlPageTitle;
            this.npcExecutor = npcExecutor;
        }

        public void GenerateCss(StringBuilder htmlBuilder)
        {
            htmlBuilder.Append("*{margin:0;padding:0;}");
            htmlBuilder.Append("a:link{font-weight:bold;text-decoration:none;}a:visited{text-decoration:none;}a:hover{text-decoration:underline;}");
            htmlBuilder.Append("body{font-family:\"Courier New\";background:#eee;text-align:center;}");
            htmlBuilder.Append("#PageDiv{margin:auto;width:900px;text-align:left;position:relative;}");
            htmlBuilder.Append("#Nav{margin-top:20px;}#NavLinkWrapper{height:50px;border-bottom:1px solid #333;}.NavLink{display:inline-block;height:49px;margin:0 5px;padding:0 5px;line-height:50px;border:1px solid #333;border-bottom:none;background:#333;color:#fff;}#CurrentNav{background:#fff;color:#000;height:50px;}");
            htmlBuilder.Append("#ContentDiv{background:#fff;border:1px solid #333;border-top:none;margin-bottom:20px;overflow-x:auto;padding:10px;}");
            htmlBuilder.Append(".executebutton{font-weight:bold;margin:3px;padding:3px;}");
            htmlBuilder.Append(".SectionTitle{display:inline-block;background:#333;color:#fff;font-weight:bold;padding:5px;}");
            htmlBuilder.Append(".methods{padding:10px 0;}");
            htmlBuilder.Append("table{border-collapse:collapse;} /*table.noborder ,tr.noborder ,td.noborder  {border:none;}*/");
            htmlBuilder.Append(".methodtable table{width:100%;} .methodtable td{padding:5px 0;}");
            htmlBuilder.Append(".csobjecttable td{border:1px solid #aaa;padding:3px;}");
            htmlBuilder.Append(".csarraytable table,.csarraytable td{border: 1px solid #000;}.csarraytable td{padding:2px;}.csstring{color:#A31515}.cstype{color:#2b91af;font-weight:bold;}.cskeyword{color:#00f;font-weight:bold;}.bold{font-weight:bold;}");
            htmlBuilder.Append(".formathelp{background-color:#ddd; padding:5px;}.stacktrace{}.methodform{margin:3px;padding:3px;}");
        }

        public String TypeAsHtml(Type type)
        {
            if (type == typeof(void))
                return String.Format("<span class=\"cstype\">Void</span>");

            if (type.IsArray)
                return String.Format("<span class=\"cstype\">{0}[]</span>", TypeAsHtml(type.GetElementType()));

            return String.Format("<a href=\"/type/{0}\" class=\"cstype\">{1}</a>", type.SosTypeName(), type.Name);
        }
        public void GenerateHtmlValue(StringBuilder htmlBuilder, Object returnObject)
        {
            if(returnObject == null)
            {
                htmlBuilder.Append("<font class=\"cskeyword\">null</font>");
                return;
            }

            Type type = returnObject.GetType();
            if (type == typeof(void)) return;

            if (type == typeof(Boolean))
            {
                htmlBuilder.Append(String.Format("<span class=\"cskeyword\">{0}</span>",
                    ((Boolean)returnObject) ? "true" : "false"));
                return;
            }

            if (type == typeof(String))
            {
                htmlBuilder.Append(String.Format("<span class=\"csstring\">\"{0}\"</span>",
                    (String)returnObject));
                return;
            }

            if (type.IsSosPrimitive() || type.IsEnum)
            {
                htmlBuilder.Append(returnObject.SerializeObject());
                return;
            }

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Array array = (Array)returnObject;

                if (array.Length <= 0)
                {
                    htmlBuilder.Append("<font class=\"cskeyword\">[]</font>");
                }
                else
                {
                    htmlBuilder.Append("<table class=\"csarraytable\">");

                    for (int i = 0; i < array.Length; i++)
                    {
                        htmlBuilder.Append("<tr><td><span class=\"cskeyword\">");
                        htmlBuilder.Append(i.ToString());
                        htmlBuilder.Append("</span></td><td>");
                        GenerateHtmlValue(htmlBuilder, array.GetValue(i));
                        htmlBuilder.Append("</td></tr>");
                    }

                    htmlBuilder.Append("</table>");
                }
                return;
            }

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                htmlBuilder.Append("<span class=\"cskeyword\">{}</span>");
            }
            else
            {
                htmlBuilder.Append("<table class=\"csobjecttable\">");
                for(int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    Object fieldValue = fieldInfo.GetValue(returnObject);
                    htmlBuilder.Append(String.Format("<tr><td>{0}</td><td>{1}</td><td>",
                        TypeAsHtml(fieldInfo.FieldType), fieldInfo.Name));
                    GenerateHtmlValue(htmlBuilder, fieldValue);
                    htmlBuilder.Append("</td></tr>");
                }
                htmlBuilder.Append("</table>");
            }
        }

        public void GenerateExceptionHtml(StringBuilder htmlBuilder, Exception e)
        {
            htmlBuilder.Append("<div style=\"text-align:center;\"><div style=\"padding:10px;text-align:left\"><h3>" + e.Message + "</h3></div>");
            htmlBuilder.Append("<div style=\"border:1px solid #aaa;text-align:left;padding:10px;width:850px;margin:0 auto 10px auto;overflow-x:scroll;\"><pre >");
            htmlBuilder.Append(e.ToString());
            htmlBuilder.Append("</pre></div></div>");
        }
        public void GenerateHtmlHeaders(StringBuilder htmlBuilder, String resourceString)
        {
            htmlBuilder.Append("<title>");
            htmlBuilder.Append(htmlPageTitle);
            htmlBuilder.Append(" - ");
            htmlBuilder.Append(resourceString);
            htmlBuilder.Append("</title>");
        }
        public void GenerateMethodsPage(StringBuilder htmlBuilder)
        {
            htmlBuilder.Append("<div class=\"methodgroups\">");

            Int32 tabIndex = 1;

            foreach(NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
            {
                htmlBuilder.Append("<div class=\"methods\"><hr/><h2>");
                htmlBuilder.Append(executionObject.objectName);
                htmlBuilder.Append("</h2><hr/>");

                for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
                {
                    NpcInterfaceInfo interfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                    for(int methodIndex = 0; methodIndex < interfaceInfo.npcMethods.Length; methodIndex++)
                    {
                        NpcMethodInfo npcMethodInfo = interfaceInfo.npcMethods[methodIndex];

                        ParameterInfo[] parameters = npcMethodInfo.parameters;
                        Int32 parameterCount = (parameters == null) ? 0 : parameters.Length;

                        htmlBuilder.Append(String.Format("<form class=\"methodform\" action=\"call/{0}.{1}\" method=\"get\">", executionObject.objectName, npcMethodInfo.methodName));
                        htmlBuilder.Append(String.Format("<input class=\"executebutton\" type=\"submit\" value=\"Execute\" tabindex=\"{0}\"/>", tabIndex + parameterCount));
#if WindowsCE
                    htmlBuilder.Append(TypeAsHtml(npcMethodInfo.methodInfo.ReturnType));
#else
                        htmlBuilder.Append(TypeAsHtml(npcMethodInfo.methodInfo.ReturnParameter.ParameterType));
#endif

                        htmlBuilder.Append(String.Format("&nbsp;<font class=\"bold\">{0}</font>(", npcMethodInfo.methodInfo.Name));
                        if (parameterCount > 0)
                        {
                            htmlBuilder.Append("<div style=\"padding-left:50px;\"><table class=\"methodtable\">");
                            for (UInt16 j = 0; j < parameterCount; j++)
                            {
                                ParameterInfo parameterInfo = parameters[j];
                                htmlBuilder.Append(String.Format("<tr><td>{0}</td><td>&nbsp;{1}</td><td>&nbsp;=&nbsp;</td><td width=\"100%\"><input style=\"width:100%;\" tabindex=\"{3}\" name=\"{2}\"/></td></tr>",
                                    TypeAsHtml(parameterInfo.ParameterType), parameterInfo.Name, j, tabIndex++));
                            }
                            htmlBuilder.Append("</table></div>");
                        }
                        htmlBuilder.Append(")</form>");
                    }
                }
                tabIndex++;
                htmlBuilder.Append("</div>");
            }

            htmlBuilder.Append("</div>");

            htmlBuilder.Append("<div id=\"executeframe\"></div>");
        }
        public void GenerateTypesPage(StringBuilder htmlBuilder)
        {
            Int32 enumTypeCount = 0, objectTypeCount = 0;

            foreach(KeyValuePair<String,Type> pair in npcExecutor.EnumAndObjectTypes)
            {
                if (pair.Value.IsEnum)
                {
                    enumTypeCount++;
                }
                else
                {
                    objectTypeCount++;
                }
            }

            htmlBuilder.Append("<br/><br/><hr/>");
            if (enumTypeCount <= 0)
            {
                htmlBuilder.Append("<h2>There are no enum types</h2><hr/>");
            }
            else
            {
                htmlBuilder.Append(String.Format("<h2>{0} enum types</h2><hr/>", enumTypeCount));
                foreach (KeyValuePair<String, Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    if (pair.Value.IsEnum)
                    {
                        htmlBuilder.Append(TypeAsHtml(pair.Value));
                        htmlBuilder.Append("<br/>");
                    }
                }
            }

            htmlBuilder.Append("<br/><br/><hr/>");
            if (objectTypeCount <= 0)
            {
                htmlBuilder.Append("<h2>There are no object types</h2><hr/>");
            }
            else
            {
                htmlBuilder.Append(String.Format("<h2>{0} object types</h2><hr/>", objectTypeCount));
                foreach (KeyValuePair<String, Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    if (!pair.Value.IsEnum)
                    {
                        htmlBuilder.Append(TypeAsHtml(pair.Value));
                        htmlBuilder.Append("<br/>");
                    }
                }
            }
        }
        public void GenerateTypePage(StringBuilder htmlBuilder, String type)
        {
            Type enumOrObjectType;
            if(npcExecutor.EnumAndObjectTypes.TryGetValue(type, out enumOrObjectType))
            {
                htmlBuilder.Append(TypeAsHtml(enumOrObjectType) + "<br/>");
                if (enumOrObjectType.IsEnum)
                {
                    htmlBuilder.Append("<span class=\"cskeyword\">enum</span> {<table class=\"enumtable\">");
                    Array enumValues = EnumReflection.GetValues(enumOrObjectType);
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        Enum enumValue = (Enum)enumValues.GetValue(i);
                        htmlBuilder.Append("<tr><td>&nbsp;" + enumValue.ToString() + "</td><td>&nbsp;= " + enumValue.ToString("D") + ",</tr>");
                    }
                    htmlBuilder.Append("</table>}");
                }
                else
                {
                    htmlBuilder.Append("<span class=\"cskeyword\">object</span> {<table class=\"objecttable\">");
                    FieldInfo[] fieldInfos = enumOrObjectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (fieldInfos != null && fieldInfos.Length > 0)
                    {
                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            FieldInfo fieldInfo = fieldInfos[i];
                            htmlBuilder.Append("<tr><td>&nbsp;" + TypeAsHtml(fieldInfo.FieldType) + "</td><td>&nbsp;" + fieldInfo.Name + ";</tr>");
                        }
                    }
                    htmlBuilder.Append("</table>}");
                }
                return;
            }

            Type sosPrimitiveType = type.TryGetSosPrimitive();
            if (sosPrimitiveType != null)
            {
                htmlBuilder.Append(TypeAsHtml(sosPrimitiveType) + " is a primitive type");
                return;
            }

            htmlBuilder.Append(String.Format("<a href=\"#\" class=\"cstype\">{0}</a>", type) + " is an unknown type");
        }
        public void GenerateCallPage(StringBuilder htmlBuilder, string call)
        {
            Int32 questionMarkIndex = call.IndexOf('?');

            String methodName;
            String[] parameters = null;
            if (questionMarkIndex < 0)
            {
                methodName = call;
            }
            else
            {
                methodName = call.Remove(questionMarkIndex);
                String parametersQueryString = (questionMarkIndex >= call.Length - 1) ? null :
                    call.Substring(questionMarkIndex + 1);

                if (String.IsNullOrEmpty(methodName))
                {
                    throw new FormatException(String.Format("Call '{0}' is missing method name", call));
                }
                else if (!String.IsNullOrEmpty(parametersQueryString))
                {
                    parameters = parametersQueryString.Split('&');
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        Int32 equalIndex = parameters[i].IndexOf('=');
                        if (equalIndex > 0)
                        {
                            parameters[i] = (equalIndex >= parameters[i].Length - 1) ? "" :
                                parameters[i].Substring(equalIndex + 1);
                        }
                    }
                }
            }
            
            UInt16 parameterCount = (parameters == null) ? (UInt16)0 : (UInt16)parameters.Length;

            NpcExecutionObject executionObject;
            NpcMethodInfo npcMethodInfo = npcExecutor.GetNpcMethodInfo(methodName, parameterCount, out executionObject);

            NpcReturnObjectOrException returnObject = npcExecutor.ExecuteWithStrings(executionObject, npcMethodInfo, parameters);

            if (returnObject.exception == null)
            {
                htmlBuilder.Append(String.Format("<div style=\"background:#333;color:#0f5;padding:5px;\"><h1>Success</h1></div>"));
            }
            else
            {
                htmlBuilder.Append(String.Format("<div style=\"background:#333;color:#f00;padding:5px;\"><h1>Exception</h1></div>"));
            }

            htmlBuilder.Append("<br/>");

            htmlBuilder.Append(String.Format("<div><div><span class=\"SectionTitle\">Function Called</span><hr/></div> {0}&nbsp;<span class=\"bold\">{1}</span>(", TypeAsHtml(npcMethodInfo.methodInfo.ReturnType), methodName));
            if (parameterCount > 0)
            {
                htmlBuilder.Append("<table>");
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameterInfo = npcMethodInfo.parameters[i];
                    String parameterString = parameters[i];
                    htmlBuilder.Append(String.Format("<tr><td>&nbsp;{0}</td><td>&nbsp;{1}</td><td>&nbsp;=&nbsp;</td><td>{2}</td></tr>",
                        TypeAsHtml(parameterInfo.ParameterType), parameterInfo.Name, parameterString));
                }
                htmlBuilder.Append("</table>");
            }
            htmlBuilder.Append(")</div>");

            htmlBuilder.Append("<br/>");

            if (returnObject.exception == null)
            {
                if (returnObject.type != typeof(void))
                {
                    htmlBuilder.Append(String.Format("<div><span class=\"SectionTitle\">Return Value</span>&nbsp;{0}<hr/></div>", TypeAsHtml(returnObject.type)));
                    GenerateHtmlValue(htmlBuilder, returnObject.value);
                }
            }
            else
            {
                htmlBuilder.Append(String.Format("<div><span class=\"SectionTitle\">Exception</span>&nbsp;<span class=\"cstype\">{0}</span><hr/></div>", returnObject.type.FullName));
                GenerateExceptionHtml(htmlBuilder, returnObject.exception);
            }
        }
    }
}
