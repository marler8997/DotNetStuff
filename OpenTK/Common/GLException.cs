using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Marler.OpenTK.Common
{
    public class GLException : Exception
    {
        public static void ThrowOnError()
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new GLException(errorCode, String.Format("OpenGL Error({0}): {1}", (Int32)errorCode, errorCode));
            }
        }

        public readonly ErrorCode errorCode;
        public GLException(ErrorCode errorCode, String message)
		    : base(message)
        {
            this.errorCode = errorCode;
	    }
    }

}
