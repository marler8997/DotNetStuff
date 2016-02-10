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
        void GenerateCss(ITextBuilder htmlBuilder);

        String TypeAsHtml(Type type);
        void GenerateHtmlValue(ITextBuilder htmlBuilder, Object returnObject);

        void GenerateHtmlHeaders(ITextBuilder htmlBuilder, String resourceString);

        void GenerateExceptionHtml(ITextBuilder htmlBuilder, Exception e);

        void GenerateMethodsPage(ITextBuilder htmlBuilder);
        void GenerateTypesPage(ITextBuilder htmlBuilder);
        void GenerateTypePage(ITextBuilder htmlBuilder, String type);
        void GenerateCallPage(ITextBuilder htmlBuilder, String call);
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

        public void GenerateCss(ITextBuilder htmlBuilder)
        {
            htmlBuilder.AppendAscii("*{margin:0;padding:0;}");
            htmlBuilder.AppendAscii("a:link{font-weight:bold;text-decoration:none;}a:visited{text-decoration:none;}a:hover{text-decoration:underline;}");
            htmlBuilder.AppendAscii("body{font-family:\"Courier New\";background:#eee;text-align:center;}");
            htmlBuilder.AppendAscii("#PageDiv{margin:auto;width:900px;text-align:left;position:relative;}");
            htmlBuilder.AppendAscii("#Nav{margin-top:20px;}#NavLinkWrapper{height:50px;border-bottom:1px solid #333;}.NavLink{display:inline-block;height:49px;margin:0 5px;padding:0 5px;line-height:50px;border:1px solid #333;border-bottom:none;background:#333;color:#fff;}#CurrentNav{background:#fff;color:#000;height:50px;}");
            htmlBuilder.AppendAscii("#ContentDiv{background:#fff;border:1px solid #333;border-top:none;margin-bottom:20px;overflow-x:auto;padding:10px;}");
            htmlBuilder.AppendAscii(".executebutton{font-weight:bold;margin:3px;padding:3px;}");
            htmlBuilder.AppendAscii(".SectionTitle{display:inline-block;background:#333;color:#fff;font-weight:bold;padding:5px;}");
            htmlBuilder.AppendAscii(".methods{padding:10px 0;}");
            htmlBuilder.AppendAscii("table{border-collapse:collapse;} /*table.noborder ,tr.noborder ,td.noborder  {border:none;}*/");
            htmlBuilder.AppendAscii(".methodtable table{width:100%;} .methodtable td{padding:5px 0;}");
            htmlBuilder.AppendAscii(".csobjecttable td{border:1px solid #aaa;padding:3px;}");
            htmlBuilder.AppendAscii(".csarraytable table,.csarraytable td{border: 1px solid #000;}.csarraytable td{padding:2px;}.csstring{color:#A31515}.cstype{color:#2b91af;font-weight:bold;}.cskeyword{color:#00f;font-weight:bold;}.bold{font-weight:bold;}");
            htmlBuilder.AppendAscii(".formathelp{background-color:#ddd; padding:5px;}.stacktrace{}.methodform{margin:3px;padding:3px;}");
        }

        public String TypeAsHtml(Type type)
        {
            if (type == typeof(void))
                return String.Format("<span class=\"cstype\">Void</span>");

            if (type.IsArray)
                return String.Format("<span class=\"cstype\">{0}[]</span>", TypeAsHtml(type.GetElementType()));

            return String.Format("<a href=\"/type/{0}\" class=\"cstype\">{1}</a>", type.SosTypeName(), type.Name);
        }
        public void GenerateHtmlValue(ITextBuilder htmlBuilder, Object returnObject)
        {
            if(returnObject == null)
            {
                htmlBuilder.AppendAscii("<font class=\"cskeyword\">null</font>");
                return;
            }

            Type type = returnObject.GetType();
            if (type == typeof(void)) return;

            if (type == typeof(Boolean))
            {
                htmlBuilder.AppendAscii("<span class=\"cskeyword\">");
                htmlBuilder.AppendAscii(((Boolean)returnObject) ? "true" : "false");
                htmlBuilder.AppendAscii("<span>");
                return;
            }

            if (type == typeof(String))
            {
                //htmlBuilder.AppendFormat("<span class=\"csstring\">\"{0}\"</span>", (String)returnObject);
                htmlBuilder.AppendAscii("<span class=\"csstring\">\"");
                htmlBuilder.AppendAscii((String)returnObject);
                htmlBuilder.AppendAscii("\"</span>");
                return;
            }

            if (type.IsSosPrimitive() || type.IsEnum)
            {
                htmlBuilder.AppendAscii(returnObject.SerializeObject());
                return;
            }

            if (type.IsArray)
            {
                Array array = (Array)returnObject;

                if (array.Length <= 0)
                {
                    htmlBuilder.AppendAscii("<font class=\"cskeyword\">[]</font>");
                }
                else
                {
                    htmlBuilder.AppendAscii("<table class=\"csarraytable\">");

                    for (int i = 0; i < array.Length; i++)
                    {
                        htmlBuilder.AppendAscii("<tr><td><span class=\"cskeyword\">");
                        htmlBuilder.AppendAscii(i.ToString());
                        htmlBuilder.AppendAscii("</span></td><td>");
                        GenerateHtmlValue(htmlBuilder, array.GetValue(i));
                        htmlBuilder.AppendAscii("</td></tr>");
                    }

                    htmlBuilder.AppendAscii("</table>");
                }
                return;
            }

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfos == null || fieldInfos.Length <= 0)
            {
                htmlBuilder.AppendAscii("<span class=\"cskeyword\">{}</span>");
            }
            else
            {
                htmlBuilder.AppendAscii("<table class=\"csobjecttable\">");
                for(int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];
                    Object fieldValue = fieldInfo.GetValue(returnObject);
                    //htmlBuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>",
                    //    TypeAsHtml(fieldInfo.FieldType), fieldInfo.Name);
                    htmlBuilder.AppendAscii("<tr><td>");
                    htmlBuilder.AppendAscii(TypeAsHtml(fieldInfo.FieldType));
                    htmlBuilder.AppendAscii("</td><td>");
                    htmlBuilder.AppendAscii(fieldInfo.Name);
                    htmlBuilder.AppendAscii("</td><td>");
                    GenerateHtmlValue(htmlBuilder, fieldValue);
                    htmlBuilder.AppendAscii("</td></tr>");
                }
                htmlBuilder.AppendAscii("</table>");
            }
        }

        public void GenerateExceptionHtml(ITextBuilder htmlBuilder, Exception e)
        {
            htmlBuilder.AppendAscii("<div style=\"text-align:center;\"><div style=\"padding:10px;text-align:left\"><h3>" + e.Message + "</h3></div>");
            htmlBuilder.AppendAscii("<div style=\"border:1px solid #aaa;text-align:left;padding:10px;width:850px;margin:0 auto 10px auto;overflow-x:scroll;\"><pre >");
            htmlBuilder.AppendAscii(e.ToString());
            htmlBuilder.AppendAscii("</pre></div></div>");
        }
        public void GenerateHtmlHeaders(ITextBuilder htmlBuilder, String resourceString)
        {
            htmlBuilder.AppendAscii("<title>");
            htmlBuilder.AppendAscii(htmlPageTitle);
            htmlBuilder.AppendAscii(" - ");
            htmlBuilder.AppendAscii(resourceString);
            htmlBuilder.AppendAscii("</title>");
        }
        public void GenerateMethodsPage(ITextBuilder htmlBuilder)
        {
            htmlBuilder.AppendAscii("<div class=\"methodgroups\">");

            Int32 tabIndex = 1;

            foreach(NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
            {
                htmlBuilder.AppendAscii("<div class=\"methods\"><hr/><h2>");
                htmlBuilder.AppendAscii(executionObject.objectName);
                htmlBuilder.AppendAscii("</h2><hr/>");

                for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
                {
                    NpcInterfaceInfo interfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                    for(int methodIndex = 0; methodIndex < interfaceInfo.npcMethods.Length; methodIndex++)
                    {
                        NpcMethodInfo npcMethodInfo = interfaceInfo.npcMethods[methodIndex];

                        ParameterInfo[] parameters = npcMethodInfo.parameters;
                        Int32 parameterCount = (parameters == null) ? 0 : parameters.Length;

                        //htmlBuilder.AppendFormat("<form class=\"methodform\" action=\"call/{0}.{1}\" method=\"get\">", executionObject.objectName, npcMethodInfo.methodName);
                        htmlBuilder.AppendAscii("<form class=\"methodform\" action=\"call/");
                        htmlBuilder.AppendAscii(executionObject.objectName);
                        htmlBuilder.AppendAscii(".");
                        htmlBuilder.AppendAscii(npcMethodInfo.methodName);
                        htmlBuilder.AppendAscii("\" method=\"get\">");

                        //htmlBuilder.AppendFormat("<input class=\"executebutton\" type=\"submit\" value=\"Execute\" tabindex=\"{0}\"/>", tabIndex + parameterCount);
                        htmlBuilder.AppendAscii("<input class=\"executebutton\" type=\"submit\" value=\"Execute\" tabindex=\"");
                        htmlBuilder.AppendNumber(tabIndex + parameterCount);
                        htmlBuilder.AppendAscii("\"/>");
#if WindowsCE
                        htmlBuilder.Append(TypeAsHtml(npcMethodInfo.methodInfo.ReturnType));
#else
                        htmlBuilder.AppendAscii(TypeAsHtml(npcMethodInfo.methodInfo.ReturnParameter.ParameterType));
#endif

                        //htmlBuilder.AppendFormat("&nbsp;<font class=\"bold\">{0}</font>(", npcMethodInfo.methodInfo.Name);
                        htmlBuilder.AppendAscii("&nbsp;<font class=\"bold\">");
                        htmlBuilder.AppendAscii(npcMethodInfo.methodInfo.Name);
                        htmlBuilder.AppendAscii("</font>(");
                        if (parameterCount > 0)
                        {
                            htmlBuilder.AppendAscii("<div style=\"padding-left:50px;\"><table class=\"methodtable\">");
                            for (UInt16 j = 0; j < parameterCount; j++)
                            {
                                ParameterInfo parameterInfo = parameters[j];
                                //htmlBuilder.AppendFormat("<tr><td>{0}</td><td>&nbsp;{1}</td><td>&nbsp;=&nbsp;</td><td width=\"100%\"><input style=\"width:100%;\" tabindex=\"{3}\" name=\"{2}\"/></td></tr>",
                                //    TypeAsHtml(parameterInfo.ParameterType), parameterInfo.Name, j, tabIndex++);
                                htmlBuilder.AppendAscii("<tr><td>");
                                htmlBuilder.AppendAscii(TypeAsHtml(parameterInfo.ParameterType));
                                htmlBuilder.AppendAscii("</td><td>&nbsp;");
                                htmlBuilder.AppendAscii(parameterInfo.Name);
                                htmlBuilder.AppendAscii("</td><td>&nbsp;=&nbsp;</td><td width=\"100%\"><input style=\"width:100%;\" tabindex=\"");
                                htmlBuilder.AppendNumber(tabIndex++);
                                htmlBuilder.AppendAscii("\" name=\"");
                                htmlBuilder.AppendNumber(j);
                                htmlBuilder.AppendAscii("\"/></td></tr>");
                            }
                            htmlBuilder.AppendAscii("</table></div>");
                        }
                        htmlBuilder.AppendAscii(")</form>");
                    }
                }
                tabIndex++;
                htmlBuilder.AppendAscii("</div>");
            }

            htmlBuilder.AppendAscii("</div>");

            htmlBuilder.AppendAscii("<div id=\"executeframe\"></div>");
        }
        public void GenerateTypesPage(ITextBuilder htmlBuilder)
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

            htmlBuilder.AppendAscii("<br/><br/><hr/>");
            if (enumTypeCount <= 0)
            {
                htmlBuilder.AppendAscii("<h2>There are no enum types</h2><hr/>");
            }
            else
            {
                //htmlBuilder.AppendFormat("<h2>{0} enum types</h2><hr/>", enumTypeCount);
                htmlBuilder.AppendAscii("<h2>");
                htmlBuilder.AppendNumber(enumTypeCount);
                htmlBuilder.AppendAscii(" enum types</h2><hr/>");
                foreach (KeyValuePair<String, Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    if (pair.Value.IsEnum)
                    {
                        htmlBuilder.AppendAscii(TypeAsHtml(pair.Value));
                        htmlBuilder.AppendAscii("<br/>");
                    }
                }
            }

            htmlBuilder.AppendAscii("<br/><br/><hr/>");
            if (objectTypeCount <= 0)
            {
                htmlBuilder.AppendAscii("<h2>There are no object types</h2><hr/>");
            }
            else
            {
                //htmlBuilder.AppendFormat("<h2>{0} object types</h2><hr/>", objectTypeCount);
                htmlBuilder.AppendAscii("<h2>");
                htmlBuilder.AppendNumber(objectTypeCount);
                htmlBuilder.AppendAscii(" object types</h2><hr/>");
                foreach (KeyValuePair<String, Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    if (!pair.Value.IsEnum)
                    {
                        htmlBuilder.AppendAscii(TypeAsHtml(pair.Value));
                        htmlBuilder.AppendAscii("<br/>");
                    }
                }
            }
        }
        public void GenerateTypePage(ITextBuilder htmlBuilder, String type)
        {
            Type enumOrObjectType;
            if(npcExecutor.EnumAndObjectTypes.TryGetValue(type, out enumOrObjectType))
            {
                htmlBuilder.AppendAscii(TypeAsHtml(enumOrObjectType) + "<br/>");
                if (enumOrObjectType.IsEnum)
                {
                    htmlBuilder.AppendAscii("<span class=\"cskeyword\">enum</span> {<table class=\"enumtable\">");
                    Array enumValues = EnumReflection.GetValues(enumOrObjectType);
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        Enum enumValue = (Enum)enumValues.GetValue(i);
                        htmlBuilder.AppendAscii("<tr><td>&nbsp;" + enumValue.ToString() + "</td><td>&nbsp;= " + enumValue.ToString("D") + ",</tr>");
                    }
                    htmlBuilder.AppendAscii("</table>}");
                }
                else
                {
                    htmlBuilder.AppendAscii("<span class=\"cskeyword\">object</span> {<table class=\"objecttable\">");
                    FieldInfo[] fieldInfos = enumOrObjectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (fieldInfos != null && fieldInfos.Length > 0)
                    {
                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            FieldInfo fieldInfo = fieldInfos[i];
                            htmlBuilder.AppendAscii("<tr><td>&nbsp;");
                            htmlBuilder.AppendAscii(TypeAsHtml(fieldInfo.FieldType));
                            htmlBuilder.AppendAscii("</td><td>&nbsp;");
                            htmlBuilder.AppendAscii(fieldInfo.Name);
                            htmlBuilder.AppendAscii(";</tr>");
                        }
                    }
                    htmlBuilder.AppendAscii("</table>}");
                }
                return;
            }

            Type sosPrimitiveType = type.TryGetSosPrimitive();
            if (sosPrimitiveType != null)
            {
                htmlBuilder.AppendAscii(TypeAsHtml(sosPrimitiveType) + " is a primitive type");
                return;
            }

            //htmlBuilder.AppendFormat("<a href=\"#\" class=\"cstype\">{0}</a> is an unknown type", type);
            htmlBuilder.AppendAscii("<a href=\"#\" class=\"cstype\">");
            htmlBuilder.AppendAscii(type);
            htmlBuilder.AppendAscii("</a> is an unknown type");
        }
        public void GenerateCallPage(ITextBuilder htmlBuilder, string call)
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
                htmlBuilder.AppendAscii("<div style=\"background:#333;color:#0f5;padding:5px;\"><h1>Success</h1></div>");
            }
            else
            {
                htmlBuilder.AppendAscii("<div style=\"background:#333;color:#f00;padding:5px;\"><h1>Exception</h1></div>");
            }

            htmlBuilder.AppendAscii("<br/>");

            //htmlBuilder.AppendFormat("<div><div><span class=\"SectionTitle\">Function Called</span><hr/></div> {0}&nbsp;<span class=\"bold\">{1}</span>(", TypeAsHtml(npcMethodInfo.methodInfo.ReturnType), methodName);
            htmlBuilder.AppendAscii("<div><div><span class=\"SectionTitle\">Function Called</span><hr/></div> ");
            htmlBuilder.AppendAscii(TypeAsHtml(npcMethodInfo.methodInfo.ReturnType));
            htmlBuilder.AppendAscii("&nbsp;<span class=\"bold\">");
            htmlBuilder.AppendAscii(methodName);
            htmlBuilder.AppendAscii("</span>(");
            if (parameterCount > 0)
            {
                htmlBuilder.AppendAscii("<table>");
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameterInfo = npcMethodInfo.parameters[i];
                    String parameterString = parameters[i];
                    //htmlBuilder.AppendFormat("<tr><td>&nbsp;{0}</td><td>&nbsp;{1}</td><td>&nbsp;=&nbsp;</td><td>{2}</td></tr>",
                    //    TypeAsHtml(parameterInfo.ParameterType), parameterInfo.Name, parameterString);
                    htmlBuilder.AppendAscii("<tr><td>&nbsp;");
                    htmlBuilder.AppendAscii(TypeAsHtml(parameterInfo.ParameterType));
                    htmlBuilder.AppendAscii("</td><td>&nbsp;");
                    htmlBuilder.AppendAscii(parameterInfo.Name);
                    htmlBuilder.AppendAscii("</td><td>&nbsp;=&nbsp;</td><td>");
                    htmlBuilder.AppendAscii(parameterString);
                    htmlBuilder.AppendAscii("</td></tr>");
                }
                htmlBuilder.AppendAscii("</table>");
            }
            htmlBuilder.AppendAscii(")</div>");

            htmlBuilder.AppendAscii("<br/>");

            if (returnObject.exception == null)
            {
                if (returnObject.type != typeof(void))
                {
                    //htmlBuilder.AppendFormat("<div><span class=\"SectionTitle\">Return Value</span>&nbsp;{0}<hr/></div>", TypeAsHtml(returnObject.type));
                    htmlBuilder.AppendAscii("<div><span class=\"SectionTitle\">Return Value</span>&nbsp;");
                    htmlBuilder.AppendAscii(TypeAsHtml(returnObject.type));
                    htmlBuilder.AppendAscii("<hr/></div>");
                    GenerateHtmlValue(htmlBuilder, returnObject.value);
                }
            }
            else
            {
                //htmlBuilder.AppendFormat("<div><span class=\"SectionTitle\">Exception</span>&nbsp;<span class=\"cstype\">{0}</span><hr/></div>", returnObject.type.FullName);
                htmlBuilder.AppendAscii("<div><span class=\"SectionTitle\">Exception</span>&nbsp;<span class=\"cstype\">");
                htmlBuilder.AppendAscii(returnObject.type.FullName);
                htmlBuilder.AppendAscii("</span><hr/></div>");
                GenerateExceptionHtml(htmlBuilder, returnObject.exception);
            }
        }
    }
}
