using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JBuild
{
    class Program
    {
        static void Main(string[] args)
        {


        }

        //
        // Examples:


        // currentRootNode is a forward slash separated set of names (may or may not begin with forward slash, does not end with slash)
        // currentNode is either the same as the currentRootNode or it is appended with a slash and a forward slash seperated set of names (does not end with a slash)
        static void ProcessTreeSet(List<String> leafNodeSet, String currentRootNode, String currentNode, String setString, Int32 offset)
        {
            if (setString == null || offset >= setString.Length) return;


            //
            // Process each comma separated tree set
            //
            while(true)
            {
                //
                // Check first character of tree set
                //
                String atNode = currentRootNode;
                if (setString[offset] == '/')
                {
                    offset++;
                    atNode = currentRootNode;
                }
                else
                {
                    atNode = currentNode;
                }

                //
                // Get next token
                //
                
                



            }





        }

    }
}
