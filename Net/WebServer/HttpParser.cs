using System;

namespace More.Net
{
    /*
    // Low Level Http Handler
    public interface IHttpRequestHandler
    {
        int OnMessageBegin(HttpParser parser);



        // TODO: have some way of setting the value handler
        int OnHeaderField(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        // This callback should be set by OnHeaderField
        //int OnHeaderValue(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        int OnHeadersComplete(HttpParser parser);
    }

    // Low Level Http Handler
    public interface IHttpHandler
    {
        int OnMessageBegin(HttpParser parser);

        // optional?
        // OnCustomStatusCode?

        // OnCustomFunction?
        //
        
        // RequestOnly
        // The HTTP function will be accessible at this point
        int OnUrl(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        // ResponseOnly
        // The Status Code will be accessible at this point
        int OnStatus(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        // TODO: have some way of setting the value handler
        int OnHeaderField(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        // This callback should be set by OnHeaderField
        //int OnHeaderValue(HttpParser parser, Byte[] buffer, UInt32 offset, UInt32 length, Boolean complete);

        int OnHeadersComplete(HttpParser parser);
    }



    public enum HttpParserType : byte
    {
        Request  = 0,
        Response = 1,
        Both     = 2,
    }
    public class HttpParser
    {
        static readonly State[] InitialStates = new State[] {
            State.StartReq,      // HttpParserType.Request  == 0
            State.StartRes,      // HttpParserType.Response == 1
            State.StartReqOrRes, // HttpParserType.Both     == 2
        };

        enum State
        {
            Zero = 0,
            Dead = 1, // Important that this is > 0

            StartReqOrRes,
            ResOrRespH,
            StartRes,
            ResH,
            ResHT,
            ResHTT,
            ResHTTP,
            ResFirstHttpMajor,
            ResHttpMajor,
            ResFirstHttpMinor,
            ResHttpMinor,
            ResFirstStatusCode,
            ResStatusCode,
            ResStatusStart,
            ResStatus,
            ResLineAlmostDone,

            StartReq,

            ReqMethod,
            ReqSpacesBeforeUrl,
            ReqSchema,
            ReqSchemaSlash,
            ReqSchemaSlashSlash,
            ReqServerStart,
            ReqServer,
            ReqServerWithAt,
            ReqPath,
            ReqQueryStringStart,
            ReqQueryString,
            ReqFragmentStart,
            ReqFragment,
            ReqHttpStart,
            ReqHttpH,
            ReqHttpHT,
            ReqHttpHTT,
            ReqHttpHTTP,
            ReqFirstHttpMajor,
            ReqHttpMajor,
            ReqFirstHttpMinor,
            ReqHttpMinor,
            ReqLineAlmostDone,

            HeaderFieldStart,
            HeaderField,
            HeaderValueDiscardWS,
            HeaderValueDiscardWSAlmostDone,
            HeaderValueDiscardLws,
            HeaderValueStart,
            HeaderValue,
            HeaderValueLws,

            HeaderAlmostDone,

            ChunkSizeStart,
            ChunkSize,
            ChunkParameters,
            ChunkSizeAlmostDone,

            HeadersAlmostDone,
            HeadersDone,

            // Important: HeadersDone must be the last 'header' state.
            // All states beyond it must be 'body' states.  Used for overflow checking.

            ChunkData,
            ChunkDataAlmostDone,
            ChunkDataDone,

            BodyIdentity,
            BodyIdentifyEof,

            MessageDone
        }



        readonly IHttpHandler handler;
        public readonly HttpParserType type;
        State state;

        public HttpParser(IHttpHandler handler, HttpParserType type)
        {
            this.handler = handler;
            this.type = type;
            this.state = InitialStates[(byte)type];
            // error...
        }

        public void Execute(Byte[] data, UInt32 offset, UInt32 length)
        {
            State cachedState = this.state;

            //if (error != OK)
            //{
            //    return 0;
            //}

            if (length == 0)
            {
                throw new NotImplementedException();
            }





        }

        // Interface: Create a interface for callbacks when parsing the headers
    }
     */
}