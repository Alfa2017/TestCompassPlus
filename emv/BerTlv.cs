using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TestEMVTransaction.emv
{
  /**
   * Class for representing BER-TLV objects.
   */
  public class BerTlv {
      private Tag mTag;
      private byte[] mValue;

      /**
       * Constructs a new BER-TLV object from given tag and value.
       * 
       * @param tag the tag.
       * @param value the value.
       */
      public BerTlv(Tag tag, byte[] value) {
        if( tag == null ) {
          throw new ArgumentException("The argument 'tag' can not be null");  //IllegalArgumentException
        }

        if( value == null ) {
          throw new ArgumentException("The argument 'value' can not be null");
        }

        this.mTag = tag;
        this.mValue = value;
      }

      /**
       * Constructs a new object from given tag and value.
       * 
       * @param tag the tag.
       * @param value the value.
       */
      public BerTlv(int tag, byte[] value) : this(new Tag(tag), value)
      {
        //this(new Tag(tag), value);
      }

      /**
       * Gets a tag instance of the BER-TLV object.
       *
       * @return A tag integer.
       */
      public Tag getTag() {
          return mTag;
      }

      /**
       * Gets the encoded length of the BER-TLV object.
       *
       * @return the encoded length.
       */
      public byte[] getLengthBytes() {
          return encodeLength(mValue.Length);
      }

      /**
       * Gets a value of the BER-TLV object.
       *
       * @return the value.
       */
      public byte[] getValue() {
          return mValue;
      }

      /**
       * Gets a value of the BER-TLV object as HEX string.
       *
       * @return the value as HEX string.
       */
      public String getValueAsHexString() {
          char[] hex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
          char[] buf = new char[mValue.Length * 2];

          for (int i = 0, j = 0; i < mValue.Length; i++) {
              buf[j++] = hex[((mValue[i] >> 4) & 0xf)];
              buf[j++] = hex[((mValue[i]) & 0xf)];
          }
        
          return new String(buf);
      }

      /**
       * Encode the BER-TLV object and store the result in new byte array.
       * 
       * @return the encoded object.
       */
      public byte[] toByteArray() {
          byte[] tmpTag = mTag.getBytes();
          byte[] tmpLen = getLengthBytes();
          byte[] tmpVal = mValue;
          byte[] buffer = new byte[tmpTag.Length + tmpLen.Length + tmpVal.Length];
          Buffer.BlockCopy(tmpTag, 0, buffer, 0, tmpTag.Length);
          Buffer.BlockCopy(tmpLen, 0, buffer, tmpTag.Length, tmpLen.Length);
          Buffer.BlockCopy(tmpVal, 0, buffer, tmpTag.Length + tmpLen.Length, tmpVal.Length);
          return buffer;
      }

      /**
       * Encode the LENGTH and store the result in new byte array.
       * 
       * @return the encoded LENGTH.
       */
      public static byte[] encodeLength(int length) {
        byte[] data = null;

        if( length == 0 ) {
          data = new byte[] { (byte) 0x00 };
        }
        else if( length <= 127 ) {
          data = new byte[] { (byte) length };
        }
        else {
          int numberOfBytes = 0;

          do {
            numberOfBytes++;
          } while ((length & (0x7FFFFF << (8 * numberOfBytes))) > 0);

          data = new byte[numberOfBytes + 1];
          data[0] = (byte) (0x80 + numberOfBytes);
          for( int i = 0; i < numberOfBytes; i++ ) {
            data[numberOfBytes - i] = (byte) ((length >> (i * 8)) & 0xff);
          }
        }

        return data;
      }

      /**
       * Decode LENGTH from given byte buffer.
       * 
       * @return the length.
       */    
      public static int decodeLength(MemoryStream data) {  //ByteBuffer
        int length = (int)data.ReadByte() & 0xff;  //.get()

        if ((length & 0x80) != 0) {
          int numberOfBytes = length & 0x7F;

          length = 0;
          while (numberOfBytes > 0) {
            length = (length << 8) + ((int) data.ReadByte() & 0xff);  //.get()
            numberOfBytes--;
          }
        }

        return length;
      }

      /**
       * Constructs a new BER-TLV object from given tag and value.
       * 
       * @param tag the tag.
       * @param value the value.
       * 
       * @return a new BER-TLV object.
       */
      public static BerTlv create(Tag tag, byte[] value)
      {
        return new BerTlv(tag, value);
      }

      /**
       * Constructs a new BER-TLV object from given tag and list of BER-TLV objects as value.
       * 
       * @param tag the tag.
       * @param values the list of BER-TLV objects.
       * 
       * @return a new BER-TLV object.
       */
      public static BerTlv create(Tag tag, List<BerTlv> values)
      {
        byte[][] container = new byte[values.Count][];
        int totalDataLen = 0;

        for (int i = 0; i < container.Length; i++) {
          container[i] = values[i].toByteArray(); //.get(i)
          totalDataLen += container[i].Length;
        }

        byte[] buffer = new byte[totalDataLen];

        for (int i = 0, off = 0; i < container.Length; i++) {
          Buffer.BlockCopy(container[i], 0, buffer, off, container[i].Length);
          off += container[i].Length;
        }

        return new BerTlv(tag, buffer);
      }

      /**
       * Constructs a new BER-TLV object from given byte buffer.
       * 
       * @param buffer the byte buffer.
       * 
       * @return a new BER-TLV object.
       */
      public static BerTlv create(MemoryStream buffer)  //ByteBuffer
      {
          Tag tag = Tag.create(buffer);
          int len = decodeLength(buffer);
          byte[] val = new byte[len];
          buffer.Read(val, 0, val.Length); //buffer.get(val);  //get(val)
          return new BerTlv(tag, val);
      }

      /**
       * Constructs a new BER-TLV object from given byte array.
       * 
       * @param src the byte array.
       * @param off the offset into array.
       * @param len the length.
       * 
       * @return a new BER-TLV object. 
       */
      public static BerTlv create(byte[] src, int off, int len)
      {
        //ByteBuffer buffer = ByteBuffer.wrap(src, off, len);
        MemoryStream buffer = new MemoryStream(src, off, len);
        return create(buffer);
      }

      /**
       * Constructs a new BER-TLV object from given byte array.
       *     
       * @param src the byte array.
       *      
       * @return a new BER-TLV object. 
       */
      public static BerTlv create(byte[] src)
      {
        MemoryStream buffer = new MemoryStream(src, 0, src.Length);
        return create(buffer);
      }

      /**
       * Constructs a list of BER-TLV objects from given byte buffer.
       *     
       * @param buffer the byte buffer.
       *      
       * @return a list of BER-TLV objects.
       */
      public static List<BerTlv> createList(MemoryStream buffer)  //ByteBuffer
      {
        List<BerTlv> tlvList = new List<BerTlv>();   //ArrayList<BerTlv>()

        while( buffer.Position < buffer.Length ) {
          BerTlv tlv = BerTlv.create(buffer);
          tlvList.Add(tlv);
        }

        return tlvList;
      }

      /**
       * Constructs a list of BER-TLV objects from given byte array.
       *     
       * @param array the byte array.
       *      
       * @return a list of BER-TLV objects. 
       */
      public static List<BerTlv> createList(byte[] array) {
        //return createList(ByteBuffer.wrap(array));
        return createList(new MemoryStream(array));
      }

      /**
       * Constructs a map of tag and value from given byte buffer.
       *     
       * @param buffer the byte buffer.
       *      
       * @return a map object. 
       */
      public static Dictionary<Tag, byte[]> createMap(MemoryStream buffer)  //ByteBuffer
      {
        Dictionary<Tag, byte[]> tlvMap = new Dictionary<Tag, byte[]>();  //HashMap<Tag, byte[]>();

        while( buffer.Position < buffer.Length ) {
          BerTlv tlv = BerTlv.create(buffer);
          tlvMap.Add(tlv.getTag(), tlv.getValue()); //tlvMap.put(tlv.getTag(), tlv.getValue());
        }

        return tlvMap;
      }

      /**
       * Constructs a map of tag and value from given byte array.
       *     
       * @param array the byte array.
       *      
       * @return a map object. 
       */
      public static Dictionary<Tag, byte[]> createMap(byte[] array)
      {
        return createMap(new MemoryStream(array));
      }

      /**
       * Encode the list of BER-TLV objects to byte array.
       *     
       * @param input a list of BER-TLV objects.
       *      
       * @return the data. 
       */
      public static byte[] listToByteArray(List<BerTlv> input) {
        List<byte[]> dataList = new List<byte[]>();  //ArrayList<byte[]>();
        int totalLen = 0;

        foreach( BerTlv tlv in input ) {
          byte[] tmp = tlv.toByteArray();
          dataList.Add(tmp);
          totalLen += tmp.Length;
        }

        byte[] buffer = new byte[totalLen];
        totalLen = 0;
        foreach( byte[] data in dataList ) {
          Buffer.BlockCopy(data, 0, buffer, totalLen, data.Length);
          totalLen += data.Length;
        }

        return buffer;
      }

      /**
       * Encode the map of tag and values to byte array.
       *     
       * @param input the map.
       *      
       * @return the data. 
       */
      public static byte[] mapToByteArray(Dictionary<Tag, byte[]> input)
      {
        List<byte[]> dataList = new List<byte[]>();  //ArrayList<byte[]>();
        int totalLen = 0;

        foreach( var kp in input ) {
          byte[] tmpTag = kp.Key.getBytes();
          byte[] tmpVal = kp.Value;
          byte[] tmpLen = BerTlv.encodeLength(tmpVal.Length);

          dataList.Add(tmpTag);
          dataList.Add(tmpLen);
          dataList.Add(tmpVal);

          totalLen += tmpTag.Length + tmpLen.Length + tmpVal.Length;
        }

        // original
        //foreach( Tag tag in input.Keys ) {
        //  byte[] tmpTag = tag.getBytes();
        //  byte[] tmpVal = input[tag];
        //  byte[] tmpLen = BerTlv.encodeLength(tmpVal.Length);

        //  dataList.Add(tmpTag);
        //  dataList.Add(tmpLen);
        //  dataList.Add(tmpVal);

        //  totalLen += tmpTag.Length + tmpLen.Length + tmpVal.Length;
        //}

        byte[] buffer = new byte[totalLen];
        totalLen = 0;
        foreach( byte[] data in dataList ) {
          Buffer.BlockCopy(data, 0, buffer, totalLen, data.Length);
          totalLen += data.Length;
        }

        return buffer;
      }

      //sgn : feb 14, 2014
      public static byte[] mapToByteArray(Dictionary<Tag, BerTlv> input)
      {
        List<byte[]> dataList = new List<byte[]>();  //ArrayList<byte[]>();
        int totalLen = 0;

        foreach( var kp in input ) {
          //byte[] tmpTag = kp.Key.getBytes();
          //byte[] tmpVal = kp.Value.toByteArray();
          //byte[] tmpLen = BerTlv.encodeLength(tmpVal.Length);

          //dataList.Add(tmpTag);
          //dataList.Add(tmpLen);
          //dataList.Add(tmpVal);

          //totalLen += tmpTag.Length + tmpLen.Length + tmpVal.Length;

          byte[] tmpVal = kp.Value.toByteArray();
          dataList.Add(tmpVal);
          totalLen += tmpVal.Length;
        }

        // original
        //foreach( Tag tag in input.Keys ) {
        //  byte[] tmpTag = tag.getBytes();
        //  byte[] tmpVal = input[tag];
        //  byte[] tmpLen = BerTlv.encodeLength(tmpVal.Length);

        //  dataList.Add(tmpTag);
        //  dataList.Add(tmpLen);
        //  dataList.Add(tmpVal);

        //  totalLen += tmpTag.Length + tmpLen.Length + tmpVal.Length;
        //}

        byte[] buffer = new byte[totalLen];
        totalLen = 0;
        foreach( byte[] data in dataList ) {
          Buffer.BlockCopy(data, 0, buffer, totalLen, data.Length);
          totalLen += data.Length;
        }

        return buffer;
      }

      // sgn : feb 14, 2014
      public static String listTohexString(List<BerTlv> data)
      {
        String s = String.Empty;

        foreach( BerTlv tlv in data ) {
          Tag tag = tlv.getTag();
          s += tag.toHexValue();
          s += tlv.getValue().Length.ToString("X2");  //TODO: почему именно "X2"?
          if( tag.isConstructed() ) {
            try {
              List<BerTlv> list = BerTlv.createList(tlv.getValue());
              s += listTohexString(list);
            }
            catch( Exception ex ) {
              //logText(text + s + EMVUtils.byteArrayToHexString(tlv.getValue()) + "\n");
              throw new Exception(String.Format("hex=[{0}] err=[{1}]", s, ex.Message) );
            }
          }
          else {
            //s += EMVUtils.byteArrayToHexString(tag.getValue());
            s += tlv.getValueAsHexString();
          }
        }

        return s;
      }

      /**
       * Find and constructs a BER-TLV objects from byte buffer.
       *     
       * @param buffer the byte buffer.
       * @param tag the searching tag object.
       *      
       * @return a new BER-TLV object. 
       */
      public static BerTlv find(MemoryStream buffer, Tag tag)  //ByteBuffer
      {
        while( buffer.Position < buffer.Length ) {  //.hasRemaining()
          BerTlv tlv = BerTlv.create(buffer);

          //if( tlv.getTag().equals(tag) ) {
          if( tlv.getTag().Equals(tag) ) {
            return tlv;
          }
        }

        return null;
      }

      /**
       * Find and constructs a BER-TLV objects from byte buffer.
       *     
       * @param buffer the byte buffer.
       * @param tag the searching tag value.
       *      
       * @return a new BER-TLV object. 
       */
      public static BerTlv find(MemoryStream buffer, int tag)  //ByteBuffer
      {
          return find(buffer, new Tag(tag));
      }

      /**
       * Find and constructs a BER-TLV objects from byte array.
       *     
       * @param array the byte array.
       * @param tag the searching tag object.
       *      
       * @return a new BER-TLV object. 
       */
      public static BerTlv find(byte[] array, Tag tag) {
        //return find(ByteBuffer.wrap(array), tag);
        return find(new MemoryStream(array), tag);
      }

      /**
       * Find and constructs a BER-TLV objects from byte array.
       *     
       * @param array the byte array.
       * @param tag the searching tag value.
       *      
       * @return a new BER-TLV object. 
       */
      public static BerTlv find(byte[] array, int tag) {
        //return find(ByteBuffer.wrap(array), tag);
        return find(new MemoryStream(array), tag);
      }

      //@Override
      //public boolean equals(Object obj) {
      public override Boolean Equals(Object obj)
      {
        if( obj != null && (obj is Tag) ) {
          BerTlv other = (BerTlv) obj;

          //if( !mTag.equals(other.getTag()) )
          if( !mTag.Equals(other.getTag()) )
            return false;

          if( mValue.Length != other.mValue.Length )
            return false;

          for( int i = 0; i < mValue.Length; i++ ) {
            if( mValue[i] != other.mValue[i] )
              return false;
          }

          return true;
        }

        return false;
      }

      //@Override
      //public int hashCode() {
      public override int GetHashCode()
      {
        //int result = 1 + BerTlv.class.getName().hashCode();
        int result = 1 + typeof(Tag).Name.GetHashCode();

        foreach( byte element in mTag.getBytes() ) {
          result = 31 * result + element;
        }

        foreach( byte element in mValue ) {
          result = 31 * result + element;
        }

        return result;
      }

      //@Override
      //public String toString()
      public override String ToString()
      {
        return "BerTlv [Tag=" + mTag.toHexValue() + ", Length=" + mValue.Length + ", Value=" + getValueAsHexString() + "]";
      }
  }
}
