using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.Tank
{
    public class TankLevel
    {
	    //
	    // Start positions are encoded as the following
	    //
	    // byte numberOfStartPositionSets
	    //   byte numberOfPositionsInThisSet
	    //   0-(n-1) UInt16 startX, UInt16 startY
	    //
	    // Example:
	    // (byte)2      // There are 2 start position sets
	    // (byte) 3     // Set 0 has 3 start positions
	    // (UInt16) 20  // Set 0, Position 0, x=20
	    // (UInt16) 20  // Set 0, Position 0, y=20
	    // (UInt16) 20  // Set 0, Position 1, x=20
	    // (UInt16) 120 // Set 0, Position 1, y=120
	    // (UInt16) 20  // Set 0, Position 2, x=20
	    // (UInt16) 220 // Set 0, Position 2, y=220
	    // (byte) 1     // Set 1 has 1 start position
	    // (UInt16) 500 // Set 1, Position 0, x=500
	    // (UInt16) 600 // Set 1, Position 0, y=600
	    //	
    	
	    public int width,height;
    	
	    //private final Position[][] startPositionSets;
    	
	    //
	    // Walls
	    //
	    //public final Wall[] walls;
    }
}
