using System.Collections;
using UnityEngine;

public class QuaternionCompress
{
    public class Compress
    {
        public short x;
        public short y;
        public short z;
    }

    static public void EncodeQuaternion( Quaternion quaternion, in Compress outValue )
    {
        if( quaternion.w < 0f )
        {
            quaternion.x *= -1f;
            quaternion.y *= -1f;
            quaternion.z *= -1f;
        }

        outValue.x = ( short )Convert( quaternion.x );
        outValue.y = ( short )Convert( quaternion.y );
        outValue.z = ( short )Convert( quaternion.z );

        ushort Convert( float fValue )
        {
            return ( ushort )( ( ( fValue + 1.0f ) * ushort.MaxValue ) / 2 );
        }
    }

    static public Quaternion DecodeQuaternion( in Compress outValue )
    {
        int   halfMaxValue = ushort.MaxValue / 2;
        float halfMaxFValue = ushort.MaxValue * 0.5f;

        float x = Deconvert( outValue.x );
        float y = Deconvert( outValue.y );
        float z = Deconvert( outValue.z );
        float w = Mathf.Sqrt(1f - Mathf.Clamp( x * x + y * y + z * z, -1f,1f));

        return new Quaternion( x, y, z, w );

        float Deconvert( short uValue )
        {
            int iValue = (ushort)uValue - halfMaxValue;

            return iValue / halfMaxFValue;
        }
    }
}
