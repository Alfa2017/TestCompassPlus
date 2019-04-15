using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEMVTransaction.emv
{
  class EMVUtils
  {
    //public static BerTlv createTlv(EMVTags tag, String value)
    //{
    //  return new BerTlv((int)tag, hexStringToByteArray(value));
    //}

    //public static BerTlv createTlv(EMVTags tag, byte[] value)
    //{
    //  return new BerTlv((int)tag, value);
    //}
    public static BerTlv createTlv(int tag, String value)
    {
      return new BerTlv(tag, hexStringToByteArray(value));
    }

    public static BerTlv createTlv(int tag, byte[] value)
    {
      return new BerTlv(tag, value);
    }

	  public static byte[] hexStringToByteArray(char[] src) {
		  return hexStringToByteArray(src, 0, src.Length);
	  }

    public static byte[] hexStringToByteArray(String s)
    {
      char[] src = s.ToCharArray();
      return hexStringToByteArray(src);
    }

    public static byte[] hexStringToByteArray(String s, char delimiter)
    {
      char[] src = s.ToCharArray();
      int srcLen = 0;

      foreach( char c in src ) {
        if( c != delimiter ) src[srcLen++] = c;
      }

      return hexStringToByteArray(src, 0, srcLen);
    }

	  public static byte[] hexStringToByteArray(char[] src, int off, int len) {
		  if ((len & 1) != 0) throw new ArgumentException("The argument 'len' can not be odd value");
		
		  byte[] buffer = new byte[len / 2];
			
		  for( int i = 0; i < len; i++ ) {
			  int nib = src[off + i];
			
			  if ('0' <= nib && nib <= '9') {
				  nib = nib - '0';
			  } else if ('A' <= nib && nib <= 'F') {
				  nib = nib - 'A' + 10;
			  } else if ('a' <= nib && nib <= 'f') {
				  nib = nib - 'a' + 10;
			  } else {
				  throw new ArgumentException("The argument 'src' can contains only HEX characters");
			  }
			
		    if ((i & 1) != 0) {
          buffer[i / 2] += (byte)nib; 
		    } else {
		    	buffer[i / 2] = (byte)(nib << 4);
		    }		    
		  }
		
		  return buffer;		
	  }

    //private static byte[] hexStringToByteArray(String src, int off, int len)
    //{
    //  if( (len & 1) != 0 ) throw new ArgumentException("The argument 'len' can not be odd value");  //IllegalArgumentException("The argument 'len' can not be odd value");

    //  byte[] buffer = new byte[len / 2];

    //  for( int i = 0; i < len; i++ ) {
    //    int nib = src[off + i];

    //    if( '0' <= nib && nib <= '9' ) {
    //      nib = nib - '0';
    //    }
    //    else if( 'A' <= nib && nib <= 'F' ) {
    //      nib = nib - 'A' + 10;
    //    }
    //    else if( 'a' <= nib && nib <= 'f' ) {
    //      nib = nib - 'a' + 10;
    //    }
    //    else {
    //      throw new Exception("The argument 'src' can contains only HEX characters"); //IllegalArgumentException("The argument 'src' can contains only HEX characters");
    //    }

    //    if( (i & 1) != 0 ) {
    //      buffer[i / 2] += (byte)nib; //nib
    //    }
    //    else {
    //      buffer[i / 2] = (byte)(nib << 4);
    //    }
    //  }

    //  return buffer;
    //}

    //// Helper method for extracting sub array from array
    //public static byte[] byteArrayToSubArray(byte[] buf, int offset, int length)
    //{
    //  byte[] result = new byte[length];
    //  Buffer.BlockCopy(buf, offset, result, 0, length);
    //  return result;
    //}
    
    // Helper method for extracting sub array from array
    //public static byte[] byteArrayToSubArray(byte[] buf, int offset, int length)
    //{
    //    byte[] result = new byte[length];
    //    Buffer.BlockCopy(buf, offset, result, 0, length);
    //    return result;
    //}
      
    // Helper method to convert byte array to hex string
    //private static String byteArrayToHexString(byte[] data, int offset, int length) {
    //    char[] hex = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
    //    char[] buf = new char[length * 2];
    //    int offs = 0;

    //    for (int i = 0; i < length; i++) {
    //        buf[offs++] = hex[(data[offset + i] >> 4) & 0xf];
    //        buf[offs++] = hex[(data[offset + i] >> 0) & 0xf];            
    //    }

    //    return new String(buf, 0, offs);
    //}

    public static String byteArrayToHexString(byte[] data)
    {
        return byteArrayToHexString(data, 0, data.Length);
    }

    public static String byteArrayToHexString(byte[] scr, int off, int len)
    {
      //private static char[] HEX = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      char[] HEX = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      //char[] buf = new char[len * 3];
      char[] buf = new char[len * 2];

      for( int i = 0, j = 0; i < len; i++ ) {
        buf[j++] = HEX[((scr[off + i] >> 4) & 0xf)];
        buf[j++] = HEX[((scr[off + i]) & 0xf)];
        //buf[j++] = ' ';
      }

      return new String(buf);
    }

    // Helper method for extracting sub array from array
    public static byte[] byteArrayToSubArray(byte[] buf, int offset, int length)
    {
      byte[] result = new byte[length];
      //System.arraycopy(buf, offset, result, 0, length);
      Buffer.BlockCopy(buf, offset, result, 0, length);
      return result;
    }

    public static int byteArrayToInt(byte[] buf, int offset, int length)
    {
      int result = 0;

      while( (length--) > 0 ) {
        result <<= 8; // Shift value with 8 bits.
        result += buf[offset++] & 0xff;
      }

      return result;
    }

    //------------------------------------------------------------------------
    private static int crcccit(int crc, int b)
    {
      crc = crc >> 8 & 0xFF | crc << 8 & 0xFFFF;
      crc ^= b & 0xFF;
      crc ^= (crc & 0xFF) >> 4;
      crc ^= crc << 8 << 4 & 0xFFFF;
      crc ^= (crc & 0xFF) << 4 << 1;
      return crc & 0xFFFF;
    }

    public static int crcccit(byte[] buf, int offset, int length)
    {
      int crc = 0;

      for( int i = 0; i < length; i++ ) {
        crc = crcccit(crc, buf[(offset + i)] & 0xFF);
      }

      return crc;
    }

    //------------------------------------------------------------------------
    private static byte encodeToBCD(int value)
    {
      if( value > 99 || value < 0 ) {
        throw new ArgumentException("Value must be between 0 and 99");
      }
      return (byte)(((value / 10) << 4) | (value % 10));
    }
    public static byte[] encodeTransactionDate(DateTime c)
    {
      byte[] result = new byte[3];
      result[0] = encodeToBCD(c.Year - 2000);
      result[1] = encodeToBCD(c.Month);
      result[2] = encodeToBCD(c.Day);
      return result;
    }

    public static byte[] encodeTransactionTime(DateTime c)
    {
      byte[] result = new byte[3];
      result[0] = encodeToBCD(c.Hour);
      result[1] = encodeToBCD(c.Minute);
      result[2] = encodeToBCD(c.Second);
      return result;
    }
    public static byte[] encodeTransactionSequence(int value)
    {
      byte[] result = new byte[3];
      result[0] = encodeToBCD(value / 10000);
      result[1] = encodeToBCD((value / 100) % 100);
      result[2] = encodeToBCD(value % 100);
      return result;
    }

    public static byte[] encodeAmount(String amount)
    {
      int v = (int)(Double.Parse(amount) * 100 + 0.5);
      return new byte[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)(v) };
    }

    public static String decodeNib(byte[] value) { 
      StringBuilder sb = new StringBuilder(value.Length);
        
      foreach( byte b in value ) {
        int v = (int)b & 0xff;
        int f = v >> 4;
        int s = v & 0xf;
            
        if( f > 9 ) {
            if( f != 0xf ) throw new SystemException("Invalid value: " + s.ToString("X")); //Integer.toHexString(f)
        }
        else {
            sb.Append((char)(48 + f));                              
        }
            
        if (s > 9) {
          if( s != 0xf ) throw new SystemException("Invalid value: " + s.ToString("X"));  //Integer.toHexString(s)
        }
        else {
          sb.Append((char)(48 + s));                              
        }           
      }
        
      return sb.ToString();
    }

    public static String decodeAscii(byte[] value) {
      StringBuilder sb = new StringBuilder();
        
      foreach( byte b in value ) {
        if( b != 0 ) {
          sb.Append((char)b);
        }
      }
        
      return sb.ToString();
    }

    public static int decodeInt(byte[] value) {
      int n = 0;
        
      foreach( byte b in value ) {
        n = (n << 8) + (b & 0xff);
      }
        
      return n;
    }

    public static String decodeDate(byte[] value)
    {
      String s = decodeNib(value);
      return "20" + s.Substring(0, 2) + "-" + s.Substring(2, 4) + "-" + s.Substring(4, 6);
    }

    public static String decodeTime(byte[] value)
    {
      String s = decodeNib(value);
      return s.Substring(0, 2) + ":" + s.Substring(2, 4) + ":" + s.Substring(4, 6);
    }

    public static String decodeAmount(byte[] value)
    {
      long amount = 0;
        
      foreach( byte b in value ) {
        amount = amount << 8;
        amount += (int)b & 0xff;            
      }
        
      //return "" + String.valueOf(amount / 100) + "." + String.valueOf(100 + (amount % 100)).substring(1);
      return "" + Convert.ToString(amount / 100) + "." + Convert.ToString(100 + (amount % 100)).Substring(1);
    }

    public static String decodeHex(byte[] value)
    {
      char[] hex = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      char[] buf = new char[value.Length * 2];

      for( int i = 0; i < value.Length; i++ ) {
        buf[i * 2 + 0] = hex[(value[i] & 0xff) >> 4];
        buf[i * 2 + 1] = hex[(value[i] & 0x0f) >> 0];
      }

      return new String(buf);
    }
  }
}
