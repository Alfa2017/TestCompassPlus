using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEMVTransaction.emv
{
  internal class Message
  {
    public Boolean encrypted;
    //public final boolean rfu;
    public byte[] data;

    public Message(Boolean encrypted, Boolean rfu, byte[] data)
    {
      this.encrypted = encrypted;
      //this.rfu = rfu;
      this.data = data;
    }

    public byte[] toArray()
    {
      byte[] buffer = new byte[data.Length + 2];
      buffer[0] = (byte)((encrypted ? 0x80 : 0) | (data.Length >> 8));
      buffer[1] = (byte)(data.Length & 0xFF);
      Buffer.BlockCopy(data, 0, buffer, 2, data.Length);
      return buffer;
    }

    public static Message parse(byte[] data)
    {
      if( data.Length < 2 || (((data[0] & 0x3F) << 8) + (data[1] & 0xff)) != (data.Length - 2) ) {
        throw new ArgumentException("Invalid data content length");
      }

      Boolean encrypted = (data[0] & 0x80) != 0;
      byte[] buffer = new byte[data.Length - 2];
      Buffer.BlockCopy(data, 2, buffer, 0, buffer.Length);
      return new Message(encrypted, false, buffer);
    }
  }

  internal class Response
  {
    /**
      * The command identifier
      */
    public int cid;
    /**
      * Reserved for future use.
      */
    public int reserved;
    /**
      * Response data.
      */
    public byte[] data;

    private Response(int cid, int reserved, byte[] data)
    {
      this.cid = cid;
      this.reserved = reserved;
      this.data = data;
    }

    public static Response parse(byte[] input)
    {
      int cid = EMVUtils.byteArrayToInt(input, 0, 1);
      int reserved = EMVUtils.byteArrayToInt(input, 1, 1);
      int length = EMVUtils.byteArrayToInt(input, 2, 2);
      byte[] data = EMVUtils.byteArrayToSubArray(input, 4, length);
      return new Response(cid, reserved, data);
    }

    public override String ToString()
    {
      return "Response [cid=" + cid + ", data(" + data.Length + ")=" + emv.EMVUtils.byteArrayToHexString(data) + "]";
    }
  }

  /**
    * A class that represents final transaction response.
    */
  internal class TransactionResponse
  {
      public const int DECLINED = 0x00;
      public const int AUTHORIZED = 0x01;
      public const int ABORTED = 0x02;
      public const int NOT_ACCEPTED = 0x03;
      public const int AUTHORIZED_SIGNATURE = 0x81;
        
      /**
        * The complete list of transaction tags. 
        */
      public Dictionary<Tag, byte[]> tags;        
        
      // Constructs a new instance of this class
      public TransactionResponse(Dictionary<Tag, byte[]> tags)
      {
        this.tags = tags;            
      }
       
      public byte[] getValue(Tag tag)
      {
        if( tags.ContainsKey(tag) ) {
          return tags[tag];
        }            
        return null;
      }
        
      public byte[] getValue(int tag)
      {
        return getValue(new Tag(tag));                       
      }
                
      public int getProcessingResult()
      {
        byte[] value = getValue((int)PrivateTags.TAG_C1_PROCESSING_RESULT);  //TODO: избавиться от привидения!!!!
            
        if (value != null) {
          return EMVUtils.byteArrayToInt(value, 0, value.Length);
        }
            
        return -1;            
      }

      public int getTransactonResult()
      {
        byte[] value = getValue((int)PrivateTags.TAG_CB_TRANSACTION_RESULT);  //TODO: избавиться от привидения!!!!
            
        if( value != null ) {
          return EMVUtils.byteArrayToInt(value, 0, value.Length);
        }
            
        return -1;                  
      }
        
      public String getTransactonResultDescription()
      {
        int value = getTransactonResult();
            
        switch( value ) {
          case DECLINED: return "Declined";
          case AUTHORIZED: return "Authorized, no signature required";
          case ABORTED: return "Aborted";
          case NOT_ACCEPTED: return "Not Accepted";
          case AUTHORIZED_SIGNATURE: return "Authorized, signature required";
          default: return "Unknown transaction result " + value;
        }
      }               
            
      public Boolean isSignatureRequired()
      {
        int result = getTransactonResult();
            
        if( result == AUTHORIZED_SIGNATURE ) {
          return true;
        }
            
        return false;            
      }        
        
      public override String ToString()
      {            
          return "TransactionResponse [processingResult=" + getProcessingResult() + ", transactionResult=" + getTransactonResultDescription();            
      }
  }
}
