using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;


namespace TestEMVTransaction.emv
{
  public class Tag {
	  private static int MASK_CONSTRUCTED_DATA_OBJECT = 0x20;
	  private static int MASK_SUBSEQUENT_BYTES = 0x1F;
	  private static int MASK_ANOTHER_BYTE = 0x80;		
	
      private byte[] mBytes;
   
      /**
       * Create new tag from given byte array.
       * 
       * @param bytes tag data.
       */
      public Tag(byte[] bytes)
      {
          validate(bytes);
          this.mBytes = bytes;
      }
    
	  /**
	   * Create new tag from given int value.
	   * 
	   * @param tag tag value.
	   */
	  public Tag(int tag) : this(encodeTag(tag))
    {
		  //this(encodeTag(tag));
	  }

	  // Encode integer tag value to byte array
	  private static byte[] encodeTag(int tag)
    {
		  byte b0 = (byte)(tag >> 24);
		  byte b1 = (byte)(tag >> 16);
		  byte b2 = (byte)(tag >> 8);
		  byte b3 = (byte)(tag);
		
		  if (b0 != 0) return new byte[] { b0, b1, b2, b3 };
		  if (b1 != 0) return new byte[] { b1, b2, b3 };
		  if (b2 != 0) return new byte[] { b2, b3 };
		  if (b3 != 0) return new byte[] { b3 };
		
		  throw new ArgumentException("The argument 'tag' can not be null");
	  }
	
	  // Validate tag information
      private void validate(byte[] b) {
        if (b == null || b.Length == 0) {
          throw new ArgumentException("Tag must be constructed with a non-empty byte array");  // IllegalArgumentException
        }
        
        if( b.Length == 1 ) {
          if( (b[0] & (byte)MASK_SUBSEQUENT_BYTES) == (byte)MASK_SUBSEQUENT_BYTES ) {
            throw new ArgumentException("If first 5 bits are set tag must not be only one byte long");
          }
        }
        else {
          if( (b[b.Length - 1] & (byte)MASK_ANOTHER_BYTE) != (byte) 0x00 ) {
            throw new ArgumentException("For multibyte tag bit 8 of the final byte must be 0");
          }
          if( b.Length > 2 ) {
            for( int i = 1; i < b.Length - 1; i++ ) {
              if( (b[i] & (byte)MASK_ANOTHER_BYTE) != (byte)MASK_ANOTHER_BYTE ) {
                throw new ArgumentException("For multibyte tag bit 8 of the internal bytes must be 1");
              }
            }
          }
        }
      }
    
      /**
       * Returns the tag class.
       * 
       * @return tag class;
       */    
      public TagClass getTagClass() {
    	  // Get last 2 bits of tag first byte to determinate class type.
          byte classValue = (byte)(this.mBytes[0] >> 6 & 0x03); //byte classValue = (byte)(this.mBytes[0] >>> 6 & 0x03);
        
          switch(classValue){
              case (byte)0x00: return TagClass.UNIVERSAL;
              case (byte)0x01: return TagClass.APPLICATION;                
              case (byte)0x02: return TagClass.CONTEXT_SPECIFIC;                
              case (byte)0x03: return TagClass.PRIVATE;          
              // This is not possible at all...
              default: throw new SystemException("Tag has invalid class type: " + classValue.ToString("X2")); //RuntimeException  // Integer.toHexString(classValue)
          }                
      }
    
      /**
       * Returns the tag type.
       * 
       * @return tag type;
       */
      public TagType getTagType() {
    	  if (isConstructed()) {
    		  return TagType.CONSTRUCTED; 
    	  } else {
    		  return TagType.PRIMITIVE;
    	  }
      }       
    
      /**
       * Get tag data.
       * 
       * @return tag data.
       */
      public byte[] getBytes() {
          return mBytes;
      }

      /**
       * Get tag data as int value.
       * 
       * @return tag data.
       */
      public int toIntValue() {
    	  int value = 0;
    	
    	  //for (byte b: mBytes) {
        foreach( byte b in mBytes ) {
    		  value = (value << 8) + (b & 0xff); 
    	  }
    	
          return value;
      }
    
      /**
       * Get tag data as hex string.
       * 
       * @return tag data.
       */
      public String toHexValue() {
        //return Integer.toHexString(toIntValue()).toUpperCase();
        return toIntValue().ToString("X").ToUpper();  //ToString("X4")
      } 
    
      /**
       * Get whether tag contains constructed data object.
       * 
       * @return true if tag contains constructed data object.
       */
      public Boolean isConstructed() {
        return ((mBytes[0] & MASK_CONSTRUCTED_DATA_OBJECT) != 0);
      }
        
      /**
      * Get whether tag contains primitive data object.
      * 
      * @return true if tag contains primitive data object.
      */
      public Boolean isPrimitive() {
          return !isConstructed();
      }    
        
      /**
       * Create a new tag from byte buffer.
       * 
       * @param buffer the buffer contains tag data. 
       * @return the tag.
       */
      public static Tag create(MemoryStream buffer)  //ByteBuffer
      {
        byte b = (byte)buffer.ReadByte();
    	  int len = 1;
    	
    	  if( (b & MASK_SUBSEQUENT_BYTES) == MASK_SUBSEQUENT_BYTES ) {
          do {
            b = (byte)buffer.ReadByte();
            len++;
          } while ((b & MASK_ANOTHER_BYTE) == MASK_ANOTHER_BYTE);
        }
        
    	  byte[] bytes = new byte[len];
        buffer.Position = buffer.Position - len;
        buffer.Read(bytes, 0, bytes.Length);
    	
    	  return new Tag(bytes);
      }
    
      //@Override
      //public boolean equals(Object obj)
      public override Boolean Equals(Object obj)
      {
          //if (obj != null && (obj instanceof Tag)) {
          if( obj != null && (obj is Tag) ) {
              Tag other = (Tag) obj;
            
              if (mBytes.Length != other.mBytes.Length) return false;
            
              for (int i = 0; i < mBytes.Length; i++) {
            	  if (mBytes[i] != other.mBytes[i]) return false;
              }            
            
              return true;
          }
        
          return false;
      }
    
      //@Override
      //public int hashCode()
      public override int GetHashCode()
      {
        //Tag.class.getName().hashCode();
    	  int result = 1 + typeof(Tag).Name.GetHashCode();
		
		    //for (byte element : mBytes) {
        foreach( byte element in mBytes ) {
			    result = 31 * result + element;
		    }
			
		    return result;
      }
    
      //@Override
      public override String ToString()
      {
    	  return "Tag [" + toHexValue() + ", Type=" + getTagType() + ", Class=" + getTagClass() + "]";        
      }
  }

}
