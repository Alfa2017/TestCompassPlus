using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.Numerics;  // тпебуется сборка System.Numerics.dll
using System.IO;
using System.Security.Cryptography;

namespace TestEMVTransaction.emv
{
  public class EMVProcessor
  {
    #region RSA private key
    private byte[] modBytes = {
            (byte)0x77, (byte)0x6C, (byte)0xD1, (byte)0xEF, (byte)0x62, (byte)0xE9,
            (byte)0x8D, (byte)0x8F, (byte)0x19, (byte)0xB3, (byte)0x4F, (byte)0xDA,
            (byte)0xD2, (byte)0x41, (byte)0x1C, (byte)0x7A, (byte)0xC7, (byte)0xE8,
            (byte)0xD9, (byte)0x5D, (byte)0x10, (byte)0xD4, (byte)0xF4, (byte)0xA5,
            (byte)0x68, (byte)0x26, (byte)0xAA, (byte)0x2C, (byte)0xCE, (byte)0x8E,
            (byte)0xA3, (byte)0x3C, (byte)0x05, (byte)0xEA, (byte)0x1B, (byte)0x81,
            (byte)0x6A, (byte)0x39, (byte)0xDE, (byte)0x34, (byte)0x7B, (byte)0x23,
            (byte)0xC5, (byte)0xE6, (byte)0x25, (byte)0x50, (byte)0x73, (byte)0x55,
            (byte)0xD8, (byte)0x3F, (byte)0x3F, (byte)0x33, (byte)0x2E, (byte)0x5B,
            (byte)0x28, (byte)0xB9, (byte)0xFE, (byte)0x4C, (byte)0x40, (byte)0xAA,
            (byte)0xD2, (byte)0x40, (byte)0x1A, (byte)0xE4, (byte)0x37, (byte)0x85,
            (byte)0x33, (byte)0x87, (byte)0x32, (byte)0x46, (byte)0xAE, (byte)0xCE,
            (byte)0x54, (byte)0x03, (byte)0xD2, (byte)0xAD, (byte)0x8A, (byte)0xE0,
            (byte)0xAF, (byte)0x27, (byte)0xC6, (byte)0x03, (byte)0x7C, (byte)0xCF,
            (byte)0x78, (byte)0x96, (byte)0x17, (byte)0xF5, (byte)0x5A, (byte)0x2D,
            (byte)0x38, (byte)0x94, (byte)0x28, (byte)0x2B, (byte)0x6F, (byte)0xD9,
            (byte)0xEC, (byte)0xA0, (byte)0x7C, (byte)0x5F, (byte)0xDE, (byte)0x20,
            (byte)0xE8, (byte)0x2F, (byte)0xA6, (byte)0x51, (byte)0x07, (byte)0xCD,
            (byte)0xD5, (byte)0xB0, (byte)0xC8, (byte)0xB0, (byte)0x44, (byte)0xB0,
            (byte)0x4A, (byte)0xE2, (byte)0x5B, (byte)0xD7, (byte)0xC4, (byte)0x99,
            (byte)0x22, (byte)0x98, (byte)0xAC, (byte)0x95, (byte)0x75, (byte)0x99,
            (byte)0x5D, (byte)0xEB, (byte)0xBB, (byte)0x97, (byte)0x22, (byte)0x82,
            (byte)0xC0, (byte)0xF4, (byte)0x6A, (byte)0x4E, (byte)0x0E, (byte)0x74,
            (byte)0xE3, (byte)0xA8, (byte)0x11, (byte)0x17, (byte)0xBA, (byte)0x0F,
            (byte)0xD1, (byte)0x47, (byte)0x7E, (byte)0x38, (byte)0x96, (byte)0xA0,
            (byte)0xDA, (byte)0x5F, (byte)0x99, (byte)0x1B, (byte)0x6B, (byte)0x68,
            (byte)0x76, (byte)0x46, (byte)0x9C, (byte)0xED, (byte)0x6A, (byte)0x5F,
            (byte)0xE3, (byte)0x3A, (byte)0xA0, (byte)0x03, (byte)0x5D, (byte)0xBC,
            (byte)0x27, (byte)0x2B, (byte)0x45, (byte)0xC1, (byte)0x29, (byte)0xBA,
            (byte)0x6D, (byte)0x6B, (byte)0xF0, (byte)0xBF, (byte)0x8A, (byte)0x93,
            (byte)0xBB, (byte)0x9C, (byte)0x34, (byte)0xB6, (byte)0xB1, (byte)0xC9,
            (byte)0x33, (byte)0xC8, (byte)0x3B, (byte)0x53, (byte)0xE2, (byte)0xE7,
            (byte)0x40, (byte)0xF7, (byte)0x30, (byte)0x74, (byte)0x98, (byte)0xF1,
            (byte)0x7D, (byte)0xB5, (byte)0x60, (byte)0x7C, (byte)0x55, (byte)0x28,
            (byte)0x73, (byte)0x19, (byte)0x5C, (byte)0x74, (byte)0x22, (byte)0xB7,
            (byte)0xB8, (byte)0x65, (byte)0xFC, (byte)0xA1, (byte)0xBA, (byte)0x3A,
            (byte)0xC3, (byte)0x4D, (byte)0x70, (byte)0x75, (byte)0xFE, (byte)0x95,
            (byte)0x6A, (byte)0x96, (byte)0x0F, (byte)0xC2, (byte)0x75, (byte)0x86,
            (byte)0xB1, (byte)0x26, (byte)0x00, (byte)0x07, (byte)0x20, (byte)0x02,
            (byte)0x35, (byte)0x50, (byte)0x23, (byte)0xA0, (byte)0x94, (byte)0x47,
            (byte)0xC7, (byte)0x1D, (byte)0x4F, (byte)0x72, (byte)0x77, (byte)0xBE,
            (byte)0xAA, (byte)0x6B, (byte)0xAA, (byte)0xFB, (byte)0xDC, (byte)0x28,
            (byte)0xB6, (byte)0x48, (byte)0xE1, (byte)0xC7
        };
    // The private exponent
    private byte[] expBytes = {
            (byte)0x50, (byte)0x9B, (byte)0xF7, (byte)0x28, (byte)0x2A, (byte)0x0F,
            (byte)0x93, (byte)0x29, (byte)0x60, (byte)0x23, (byte)0x94, (byte)0x67,
            (byte)0x13, (byte)0x3C, (byte)0x37, (byte)0xC8, (byte)0xF8, (byte)0x5E,
            (byte)0xC7, (byte)0x38, (byte)0xF6, (byte)0x3F, (byte)0x87, (byte)0xD2,
            (byte)0x8D, (byte)0xF6, (byte)0x6B, (byte)0x2F, (byte)0x4B, (byte)0x4D,
            (byte)0x24, (byte)0x09, (byte)0x43, (byte)0xC4, (byte)0xBD, (byte)0x44,
            (byte)0x21, (byte)0x3B, (byte)0x66, (byte)0x2C, (byte)0xEE, (byte)0x61,
            (byte)0x3B, (byte)0x17, (byte)0x19, (byte)0x60, (byte)0xB0, (byte)0x38,
            (byte)0xE5, (byte)0x79, (byte)0xEB, (byte)0x62, (byte)0xD4, (byte)0x8B,
            (byte)0x5B, (byte)0x76, (byte)0x0F, (byte)0x9B, (byte)0xD0, (byte)0x9A,
            (byte)0x7C, (byte)0xC8, (byte)0x20, (byte)0x5E, (byte)0xA2, (byte)0xCB,
            (byte)0x19, (byte)0xF8, (byte)0xCB, (byte)0x8A, (byte)0xC2, (byte)0x3B,
            (byte)0x2A, (byte)0xA2, (byte)0x59, (byte)0xF6, (byte)0x21, (byte)0xA3,
            (byte)0x7F, (byte)0x16, (byte)0xCD, (byte)0xA5, (byte)0x54, (byte)0xFD,
            (byte)0x85, (byte)0x5B, (byte)0x6A, (byte)0x58, (byte)0x85, (byte)0xC1,
            (byte)0xB8, (byte)0x4A, (byte)0xE8, (byte)0xC2, (byte)0x49, (byte)0x01,
            (byte)0x43, (byte)0xA3, (byte)0x1F, (byte)0xD0, (byte)0x65, (byte)0xD2,
            (byte)0xB8, (byte)0x66, (byte)0x51, (byte)0x50, (byte)0xA8, (byte)0x7F,
            (byte)0xDB, (byte)0x19, (byte)0x34, (byte)0x9D, (byte)0x26, (byte)0x00,
            (byte)0x08, (byte)0xCB, (byte)0xB9, (byte)0x4A, (byte)0x6E, (byte)0xBD,
            (byte)0x1E, (byte)0x89, (byte)0x07, (byte)0x14, (byte)0xEB, (byte)0x07,
            (byte)0xD6, (byte)0x48, (byte)0x6D, (byte)0x11, (byte)0xA8, (byte)0x14,
            (byte)0xC3, (byte)0xB3, (byte)0x14, (byte)0x2A, (byte)0x63, (byte)0x63,
            (byte)0x51, (byte)0x0C, (byte)0xF5, (byte)0x93, (byte)0x1C, (byte)0x94,
            (byte)0x73, (byte)0xD3, (byte)0x7B, (byte)0x21, (byte)0xA3, (byte)0xE7,
            (byte)0x57, (byte)0x22, (byte)0xC1, (byte)0x60, (byte)0x62, (byte)0xB9,
            (byte)0xBA, (byte)0x2E, (byte)0x40, (byte)0xFC, (byte)0xF3, (byte)0x35,
            (byte)0x1F, (byte)0xEC, (byte)0x7C, (byte)0x10, (byte)0xE7, (byte)0x27,
            (byte)0x13, (byte)0x1C, (byte)0x32, (byte)0x4C, (byte)0xA1, (byte)0x58,
            (byte)0x85, (byte)0x5D, (byte)0xEB, (byte)0x0D, (byte)0xFC, (byte)0x91,
            (byte)0x5B, (byte)0xD4, (byte)0xFF, (byte)0x46, (byte)0xF4, (byte)0x8F,
            (byte)0x2A, (byte)0x98, (byte)0x05, (byte)0x1D, (byte)0xE6, (byte)0xAE,
            (byte)0xCC, (byte)0x24, (byte)0xFE, (byte)0xD7, (byte)0xCC, (byte)0x40,
            (byte)0xE7, (byte)0xF9, (byte)0x22, (byte)0xD0, (byte)0x02, (byte)0x29,
            (byte)0xA5, (byte)0x65, (byte)0x7A, (byte)0x54, (byte)0x9A, (byte)0xDB,
            (byte)0xCC, (byte)0x4C, (byte)0x83, (byte)0x59, (byte)0xD6, (byte)0xDB,
            (byte)0xE8, (byte)0x8C, (byte)0xD9, (byte)0xE1, (byte)0x75, (byte)0x57,
            (byte)0x43, (byte)0xAB, (byte)0xDC, (byte)0x66, (byte)0x80, (byte)0xA1,
            (byte)0x9D, (byte)0x9B, (byte)0x5B, (byte)0xBC, (byte)0xB1, (byte)0x0C,
            (byte)0x84, (byte)0x30, (byte)0xDF, (byte)0x91, (byte)0x48, (byte)0xA4,
            (byte)0xA3, (byte)0x5D, (byte)0xF9, (byte)0xEF, (byte)0x9F, (byte)0xFF,
            (byte)0x28, (byte)0xA5, (byte)0xA9, (byte)0x28, (byte)0x00, (byte)0xFE,
            (byte)0xDA, (byte)0xD0, (byte)0x4A, (byte)0xE1
        };
    #endregion

    private byte[] sessionAESKey = new byte[16];

    public EMVProcessor()
    {
    }

    public void decodeSessionAESKey(String encryptedKey)
    {
      byte[] enc = EMVUtils.hexStringToByteArray(encryptedKey);

      int rsaOffset = 2 + 16 + 8;
      byte[] rsa = EMVUtils.byteArrayToSubArray(enc, rsaOffset, 256);
      Array.Reverse(rsa, 0, rsa.Length);
      BigInteger cipher = new BigInteger(rsa);

      //модуль
      byte[] m4 = new byte[modBytes.Length];
      Buffer.BlockCopy(modBytes, 0, m4, 0, m4.Length);
      Array.Reverse(m4, 0, m4.Length);
      BigInteger mod = new BigInteger(m4);

      //экспонента
      byte[] e4 = new byte[expBytes.Length];
      Buffer.BlockCopy(expBytes, 0, e4, 0, e4.Length);
      Array.Reverse(e4, 0, e4.Length);
      BigInteger exp = new BigInteger(e4);

      //декодируем
      BigInteger r4 = BigInteger.ModPow(cipher, exp, mod);

      //преобразуем
      byte[] dec4 = r4.ToByteArray();
      Array.Reverse(dec4, 0, dec4.Length);  // так как BigInteger возвращает массив bytes в перевернутом виде, то здесь мы его еще раз переворачиваем
      Buffer.BlockCopy(dec4, 207, sessionAESKey, 0, sessionAESKey.Length);
      //Console.WriteLine(EMVUtils.byteArrayToHexString(key4, 0, key4.Length));
    }

    public byte[] decryptAESData(String encryptedData)
    {
      byte[] encryptedBytes = EMVUtils.hexStringToByteArray(encryptedData);
      byte[] decrypted = null;
      byte[] IV = new byte[16];

      // расшифровываем
      using( Aes aesAlg = Aes.Create() ) {
        aesAlg.Key = this.sessionAESKey;
        aesAlg.IV = IV;
        aesAlg.Padding = PaddingMode.None;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for decryption.
        using( MemoryStream msDecrypt = new MemoryStream(encryptedBytes) ) {
          using( CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read) ) {

            using( BinaryReader srDecrypt = new BinaryReader(csDecrypt) ) {
              decrypted = new byte[msDecrypt.Length];
              srDecrypt.Read(decrypted, 0, (int)msDecrypt.Length);
              //Console.WriteLine("decrypted data: {0}", EMVUtils.byteArrayToHexString(decrypted, 0, decrypted.Length));
            }

            // работает!
            //var buffer = new byte[1024];
            //var read = csDecrypt.Read(buffer, 0, buffer.Length);
            //Console.WriteLine("8");
          }
        }

      }

      if( decrypted != null ) {
        //Message output = Message.parse(decrypted);

        Response result = Response.parse(decrypted);

        //Console.WriteLine("emv buffer: {0}", EMVUtils.byteArrayToHexString(result.data, 0, result.data.Length));
        //logData("  @Tag: ", BerTlv.createList(result.data));

        return result.data;
      }

      return null;
    }

    public byte[] encryptAESData(byte[] Data)
    {
      //byte[] data = EMVUtils.hexStringToByteArray(Data);
      byte[] encrypted = null;
      byte[] IV = new byte[16];

      // шифруем
      using( Aes aesAlg = Aes.Create() ) {
        aesAlg.Key = this.sessionAESKey;
        aesAlg.IV = IV;
        aesAlg.Padding = PaddingMode.PKCS7;  //??????????????????????????????????????????????? ( final Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding"); )

        // Create a decrytor to perform the stream transform.
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for encryption.
        using( MemoryStream msEncrypt = new MemoryStream() ) {
          using( CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write) ) {
            //using( StreamWriter swEncrypt = new StreamWriter(csEncrypt) ) {
            //  //Write all data to the stream.
            //  swEncrypt.Write(data);
            //}
            using( BinaryWriter bwEncrypt = new BinaryWriter(csEncrypt) ) {
              bwEncrypt.Write(Data);
            }
            encrypted = msEncrypt.ToArray();
          }
        }
      }

      if( encrypted != null ) {
        return encrypted;
      }
      return null;
    }

    public byte[] createCommand(int cid, byte[] data)
    {
      return createCommand(cid, data, 0, data.Length);
    }
    public byte[] createCommand(int cid, byte[] data, int offset, int length)
    {
      //private synchronized Response transmit(boolean check, int cid, byte[] data, int offset, int length) throws AudioReaderException, IOException {
      //    ByteArrayOutputStream o = new ByteArrayOutputStream();
      //    o.write(cid);
      //    o.write(0);
      //    o.write(length >> 8);
      //    o.write(length);
      //    o.write(data, offset, length);

      //    byte[] input = o.toByteArray(); 
      //}

      byte[] input = null;
      using( MemoryStream o = new MemoryStream() ) {
        //using( BinaryWriter bw = new BinaryWriter(o) ) {
        //  bw.Write(cid);
        //  bw.Write(0);
        //  bw.Write(length >> 8);
        //  bw.Write(length);
        //  bw.Write(data, offset, length);
        //  bw.Flush();
        //}
        o.WriteByte((byte)cid);
        o.WriteByte(0);
        o.WriteByte((byte)(length >> 8));
        o.WriteByte((byte)length);
        o.Write(data, offset, length);
        input = o.ToArray();
      }

      if( input != null ) {
        byte[] buffer = new byte[input.Length + 2];
        int crc = EMVUtils.crcccit(input, 0, input.Length);
        Buffer.BlockCopy(input, 0, buffer, 0, input.Length);
        buffer[input.Length + 0] = (byte)(crc >> 8);
        buffer[input.Length + 1] = (byte)(crc);

        return encryptAESData(buffer);
      }

      return null;
    }
  }
}
