#undef  EMVMDMVisa
#undef  EMVCompassPlus
#undef  EMVCMPVisa1
#undef  EMVCMPVisa2
#undef  EMVCMPMasterCard

#define EMVUniversal
//#undef EMVUniversal

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//...
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Configuration;  // для ConfigurationManager, требуется подключить "System.Configuration.dll"
//...
using Compass.TpTp;
using TestEMVTransaction.emv;

using System.Security.Cryptography;
using System.Globalization;

namespace TestCompassPlus
{
  public partial class Form1 : Form
  {
    private bool doWork = true;
    private TcpClient tcpClient = null;
    private NetworkStream stream = null;
    //private LogonParams logon = new LogonParams("91.227.244.215", 4201, "сервер Compass Plus для разработчиков");
    private LogonParams logon = new LogonParams("192.168.207.53", 40005, "сервер СМП банка для разработчиков");

    public Form1()
    {
      InitializeComponent();
      Init();  // считываем данные конфигурационного файла

      this.Logger("[Inf] сервер: {0}:{1} [{2}]", this.logon.Host, this.logon.Port, this.logon.Desc);
    }

    private void Init()
    {
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var appSettingSection = (AppSettingsSection)config.GetSection("appSettings");

      string host = appSettingSection.Settings["Host"].Value;
      int port = int.Parse(appSettingSection.Settings["Port"].Value);
      string desc = appSettingSection.Settings["Description"].Value;
      this.logon = new LogonParams(host, port, desc);

      string ccPAN = appSettingSection.Settings["CC_PAN"].Value;
      string ccExpDate = appSettingSection.Settings["CC_ExpDate"].Value;
      string ccData = appSettingSection.Settings["CC_Data"].Value;
      string ccCVV2 = appSettingSection.Settings["CC_CVV2"].Value;

      this.ebCCNumber.Text = ccPAN;
      this.ebCCExpDate.Text = ccExpDate;
      this.edCCData.Text = ccData;
      this.edCCCVV2.Text = ccCVV2;

      this.tbTerminalID.Text = appSettingSection.Settings["MSG_TerminalID"].Value;
      this.tbEmployeeID.Text = appSettingSection.Settings["MSG_EmployeeID"].Value;
    }

    private void Logger(string msg)
    {
      lbLog.Items.Add(msg);
    }
    private void Logger(string format, params object[] args)
    {
      lbLog.Items.Add(string.Format(format, args));
    }

    //------------------------
    private void logData(String text, List<BerTlv> data)
    {
      foreach( BerTlv tlv in data ) {
        Tag tag = tlv.getTag();

        if( tag.isConstructed() ) {
          String s = tag.toIntValue().ToString("X") + "(C): ";  //"X4"

          try {
            List<BerTlv> list = BerTlv.createList(tlv.getValue());
            logText(text + s); //text + s + "\n"
            logData("  " + text, list);
          }
          catch( Exception ) {
            logText(text + s + EMVUtils.byteArrayToHexString(tlv.getValue()) + "\n");
          }
        }
        else {
          String s = tag.toIntValue().ToString("X4") + ", " + tlv.getValueAsHexString();
          logText(text + s);  //text + s + "\n"
        }
      }
    }
    private void logText(String text)
    {
      logText(text, false);
    }

    private void logText(String text, Boolean bold)
    {
      //UiHelper.appendColorText(mLogView, mLogScrollView, text, Color.BLACK, bold);
      Console.WriteLine(text);
    }
    //------------------------

    private void btCopyLog_Click(object sender, EventArgs e)
    {
      Clipboard.Clear();
      string s = string.Empty;
      for( int i = 0; i < lbLog.Items.Count; i++ ) {
        s += lbLog.Items[i].ToString() + "\n";
      }
      Clipboard.SetText(s);
    }

    private void chbCardReaderMode_CheckedChanged(object sender, EventArgs e)
    {
      if( this.chbCardReaderMode.Checked ) {
        edCCData.Enabled = true;
        edCCCVV2.Enabled = false;
      }
      else {
        edCCData.Enabled = false;
        edCCCVV2.Enabled = true;
      }
    }

    private void btRun_Click(object sender, EventArgs e)
    {
      //-[ Complass Plus ]----
      //SRV:91.227.244.161:20144  <- frame, active on 12.04.2013
      //SRV:91.227.224.215:4201   <- TPTP, active on 13.04.2013

      //string host = "91.227.224.215";  // 91.227.224.215:4201
      //int port = 4201;

      //TpTpHeader header = new TpTpHeader();
      //string msg = header.GetFormatedValue();
      //byte[] bb = System.Text.Encoding.ASCII.GetBytes(msg);
      //Console.WriteLine("header:[{0}]", msg);

      /*
      // отправляем сообщение HandShakeRequest
      Compass.TpTp.Header header = new Compass.TpTp.Header();
      header.MessageType = Compass.TpTp.MessageType.Admin;
      header.MessageSubType = Compass.TpTp.MessageSubType.OnlineTransaction;
      //header.TransactionCode = Compass.TpTp.TransactionCode.HandshakeRequest;
      header.TransactionCode = Compass.TpTp.TransactionCode.LogonRequest;
      header.CurrentDate = DateTime.Now.ToString("yyMMdd");
      header.CurrentTime = DateTime.Now.ToString("HHmmss");
      string msg = header.GetFormatedValue();

      // подсчитываем контрольную сумму
      for( int i = 0; i < msg.Length; ++i ) {
        LRC ^= (byte)msg[i];
      }
      LRC ^= ETX;
      Console.WriteLine("{0,-15}:lrc=[0x{1:X2}],rq=[{2}]", "request", LRC, msg);

      msg = string.Format("{0}{1}{2}{3}", (char)STX, msg, (char)ETX, (char)LRC);

      byte[] bq = System.Text.Encoding.ASCII.GetBytes(msg);
      //Console.WriteLine("{0,-15}:[{1}]", "header", msg);
      //Console.WriteLine("{0,-15}:[{1}]", "request", msg);
      */

      // LogonRequest
      //Compass.TpTp.LogonRequest logoffMsg = new Compass.TpTp.LogonRequest();
      //bq = logonMsg.Encode();

      // HandShakeRequest
      //Compass.TpTp.Message message = new Compass.TpTp.HandShakeRequest();
      //bq = message.Encode();

      //Compass.TpTp.CardVerificationRequest message = new Compass.TpTp.CardVerificationRequest();
      //(message.Fields[0] as Compass.TpTp.InvoiceNumberField).Number = DateTime.Now.ToString("yyMMddhhmmss");
      //(message.Fields[1] as Compass.TpTp.Track2Field).Type = Compass.TpTp.Track2Field.EnterType.Manual;
      //(message.Fields[1] as Compass.TpTp.Track2Field).PAN = "5890000000002008";  //"4890000000002001"; //"5890000000002008";
      //(message.Fields[1] as Compass.TpTp.Track2Field).ExpirationDate = "1511";
      //(message.Fields[1] as Compass.TpTp.Track2Field).Data = String.Empty;
      //////(message.Fields[1] as Compass.TpTp.Track2Field).Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      //////(message.Fields[1] as Compass.TpTp.Track2Field).PAN = "4890000000001003";
      //////(message.Fields[1] as Compass.TpTp.Track2Field).ExpirationDate = "1511";
      //////(message.Fields[1] as Compass.TpTp.Track2Field).Data = "10190880639";
      //((message.Fields[2] as Compass.TpTp.OptionalDataField).Fields[0] as Compass.TpTp.StringSubField).Value = "810";  //USD:840, EUR:978

      //------------------------------------------------------------------------

      //-[ Complass Plus ]----
      //SRV:195.54.3.217:19999
      //SRV:91.227.244.161:20144  <- active on 12.04.2013
      //SRV:10.7.2.89:25000
      //SRV:10.7.4.32:39997

      string host = "91.227.244.161";
      int port = 20144;

      this.tcpClient = new TcpClient(host, port);
      this.tcpClient.NoDelay = true;
      this.stream = this.tcpClient.GetStream();

      //this.tcpClient = new TcpClient();
      //this.tcpClient.Connect(host, port);
      //this.tcpClient.NoDelay = true;
      //this.stream = this.tcpClient.GetStream();

      //byte CR = 0x0D;
      //this.stream.WriteByte(CR);

      byte[] b1 = new byte[2];
      //b1[0] = 0;   // старшие байт длинны сообщения (short)
      //b1[1] = 10;  // младший байт длинны сообщения (short)

      //string str1 = "test=test;";
      //string str1 = "Product=MB\nMB/Ver=1\nMB/Operation=LogOn\nMB/SessionId=444\nMB/AuthPAN=444\nMB/AuthMBR\nMB/PAN=444\nMB/STAN=0\n";
      string str1 = "Product=MB\nMB/Ver=1\nMB/Operation=LogOn\nMB/STAN=0\n";
      //byte[] b2 = new byte[10];
      //System.Buffer.BlockCopy(str1.ToCharArray(), 0, b2, 0, b2.Length);
      //byte[] b1 = System.Text.Encoding.UTF8.GetBytes(myString);
      byte[] b2 = System.Text.Encoding.ASCII.GetBytes(str1);

      b1[0] = (byte)(b2.Length >> 8);  // старшие байт длинны сообщения (short)
      b1[1] = (byte)b2.Length;         // младший байт длинны сообщения (short)

      this.stream.Write(b1, 0, 2);
      this.stream.Write(b2, 0, b2.Length);
      this.stream.Flush();

      //int num = this.stream.ReadByte();
      //if( (num == 0) || (num == -1) ) {
      //  //goto Label_0043;
      //}

      while( this.doWork ) {
        if( !this.stream.DataAvailable ) {
          Thread.Sleep(5);

          this.doWork = false;
        }
        else {
          //while( true ) {
          int n1 = this.stream.ReadByte();
          int n2 = this.stream.ReadByte();
          n2 = (n1 << 8) | n2; // восстанавливаем длинну принятого сообщения (short) из 2-ух байт: старшего(1-ый байт) и младшего(2-ой байт)
          byte[] bb1 = new byte[n2];
          this.stream.Read(bb1, 0, n2);

          string str2 = System.Text.Encoding.ASCII.GetString(bb1);
          Console.WriteLine(str2);

          //if( (num == 0) || (num == -1) ) {
          //  goto Label_0043;
          //}
          //builder.Append((char)num);
          //}
        }
      }

      this.tcpClient.Close();
      this.tcpClient = null;
      this.stream = null;
    }

    private void btMessage_Click(object sender, EventArgs e)
    {
      /*
      //создаем пробное сообщение PurchaseRequest
      Compass.TpTp.PurchaseRequest purchaseRequest = new Compass.TpTp.PurchaseRequest();
      //purchaseRequest.Init("1000200030001234", 10.55);
      purchaseRequest.amount.Amount = 10;
      purchaseRequest.track.Type = Compass.TpTp.Track2Field.EnterType.Manual;
      purchaseRequest.track.PAN = "1000200030001234";
      purchaseRequest.track.ExpirationDate = "1304";
      purchaseRequest.track.Data = String.Empty;
      Console.WriteLine("{0,-15}:[{1}]", "purchase request", purchaseRequest.GetFormatedValue());

      //создаем пробное сообщение HandShakeRequest
      Compass.TpTp.HandShakeRequest handShakeRequest = new Compass.TpTp.HandShakeRequest();
      Console.WriteLine("{0,-15}:[{1}]", "handShake request", handShakeRequest.GetFormatedValue());

      //создаем пробное сообщение PaymentRequest
      Compass.TpTp.PaymentRequest paymentRequest = new Compass.TpTp.PaymentRequest();
      ////paymentRequest.amount.Amount = 11.55;
      ////paymentRequest.track.Type = Compass.TpTp.Track2Field.EnterType.Manual;
      ////paymentRequest.track.PAN = "1000200030001234";
      ////paymentRequest.track.ExpirationDate = "1304";
      ////paymentRequest.track.Data = String.Empty;
      (paymentRequest.Fields[0] as Compass.TpTp.Amount1Field).Amount = 11;
      (paymentRequest.Fields[1] as Compass.TpTp.Track2Field).Type = Compass.TpTp.Track2Field.EnterType.Manual;
      (paymentRequest.Fields[1] as Compass.TpTp.Track2Field).PAN = "1000200030001234";
      (paymentRequest.Fields[1] as Compass.TpTp.Track2Field).ExpirationDate = "1304";
      (paymentRequest.Fields[1] as Compass.TpTp.Track2Field).Data = String.Empty;
      //((paymentRequest.Fields[2] as Compass.TpTp.OptionalDataField).Fields[0] as Compass.TpTp.StringSubField).Value = "testValue";
      Console.WriteLine("{0,-15}:[{1}]", "payment request", paymentRequest.GetFormatedValue());
      */
    }

    public static byte[] ConvertHexStringToByteArray(string hexString)
    {
      if( hexString.Length % 2 != 0 ) {
        throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
      }

      byte[] HexAsBytes = new byte[hexString.Length / 2];
      for( int index = 0; index < HexAsBytes.Length; index++ ) {
        string byteValue = hexString.Substring(index * 2, 2);
        HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      }

      return HexAsBytes;
    }

    private void btDownloadParams_Click(object sender, EventArgs e)
    {
      #region sgn : jan 12, 2014
      // --------------------------------------
      //PMK - 0001020304050607 08090a0b0c0d0e0f
      //MMK - 0011223344556677 8899aabbccddeeff
      //EWK - 0011223344556677 7766554433221100

      //byte[] MainKeyPMK = ASCIIEncoding.ASCII.GetBytes("000102030405060708090a0b0c0d0e0f");  // err
      byte[] MainKeyPMK = ConvertHexStringToByteArray("000102030405060708090a0b0c0d0e0f");
      byte[] EncryptPINKey = ConvertHexStringToByteArray("798F5A41223A60029FABC1EDC873A8B6");  // <- значение "ключа" присланное с сервера
      byte[] decryptData = null;
      ICryptoTransform transform = null;

      if( EncryptPINKey.Length == 8 ) {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.Zeros;
        des.Key = MainKeyPMK;
        //decryptData = des.CreateDecryptor().TransformFinalBlock(EncryptPINKey, 0, EncryptPINKey.Length);
        transform = des.CreateDecryptor();
      }
      else if( EncryptPINKey.Length == 16 ) {
        TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        tdes.Mode = CipherMode.ECB;
        tdes.Padding = PaddingMode.Zeros;
        tdes.Key = MainKeyPMK;
        //decryptData = tdes.CreateDecryptor().TransformFinalBlock(EncryptPINKey, 0, EncryptPINKey.Length);
        transform = tdes.CreateDecryptor();
      }
      else {
        throw new Exception("неизвестная длина ключа...");
      }
      decryptData = transform.TransformFinalBlock(EncryptPINKey, 0, EncryptPINKey.Length);
      string result = Encoding.ASCII.GetString(decryptData);
      Console.WriteLine("[Inf] Result=[{0}]", result);
      //return;
      #endregion 
      //------------------------------------------------------------------

      // полная прогрузка терминала

      //Compass.TpTp.DownloadRequest message = new Compass.TpTp.DownloadRequest();
      //(message.Fields[0] as Compass.TpTp.DownloadKeyField).AccessCode = Compass.TpTp.DownloadKeyField.AccessCodeType.Begin;

      Compass.TpTp.DownloadRequest message = new Compass.TpTp.DownloadRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<DownloadKeyField>().AccessCode = DownloadKeyField.AccessCodeType.Begin;

      //Console.WriteLine("{0,-15}:rq=[{1}]", "request", message.GetFormatedValue());
      //string s4 = string.Format("[Inf] {0}({1}:{2}:{0:D})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:D}:x:x)", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();

        client.Send(message);
        client.Read(message);
        while( message.HeaderRs.ResponseCode == "881" ) {
          message.Get<DownloadKeyField>().AccessCode = DownloadKeyField.AccessCodeType.Continue;
          message.Get<DownloadKeyField>().ProcessingFlag = message.GetRs<DownloadKeyField>().ProcessingFlag;
          message.Get<DownloadKeyField>().Filler = message.GetRs<DownloadKeyField>().Filler;

          ////string pf = "H ";
          ////(message.Fields[0] as Compass.TpTp.DownloadKeyField).ProcessingFlag = pf;
          ////message.Get<DownloadKeyField>().ProcessingFlag = pf;
          ////message.Get<DownloadKeyField>().ProcessingFlag = message.GetRs<DownloadKeyField>().ProcessingFlag;

          //this.Logger("{0,-15}:rq=[{1}]", "[Inf] request", message.GetFormatedValue());
          s4 = string.Format("[Inf] {0}({1}:{2}:{0:D})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
          this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());
          client.Send(message);
          client.Read(message);
        }
        if( message.HeaderRs.ResponseCode == "880" ) {
          this.Logger("[Inf] полная прогрузка параметров успешно завершена");
        }
        else {
          this.Logger("[Err] ошибка при прогрузке параметров");
        }

        // все параметры загружены - декодируем ключи
        // ...
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btPartialDownload_Click(object sender, EventArgs e)
    {
      // частичная прогрузка терминала

      Compass.TpTp.DownloadRequest message = new Compass.TpTp.DownloadRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<DownloadKeyField>().AccessCode = DownloadKeyField.AccessCodeType.Partial;
      message.Get<DownloadKeyField>().ProcessingFlag = ebDID.Text;

      //Console.WriteLine("{0,-15}:rq=[{1}]", "request", message.GetFormatedValue());
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:D})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();

        client.Send(message);
        client.Read(message);

        if( message.HeaderRs.ResponseCode == "880" ) {
          this.Logger("[Inf] частичная прогрузка параметров успешно завершена");
        }
        else {
          this.Logger("[Err] ошибка при частичной прогрузке параметров");
        }
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btAdminRequests_Click(object sender, EventArgs e)
    {
      List<Compass.TpTp.Message> messages = new List<Compass.TpTp.Message>();
      if( chbLogOn.Checked ) {
        Compass.TpTp.Message m = new Compass.TpTp.LogonRequest();
        m.Header.TerminalID = tbTerminalID.Text;
        m.Header.EmployeeID = tbEmployeeID.Text;
        messages.Add(m);
      }
      if( chbLogOff.Checked ) {
        Compass.TpTp.Message m = new Compass.TpTp.LogoffRequest();
        m.Header.TerminalID = tbTerminalID.Text;
        m.Header.EmployeeID = tbEmployeeID.Text;
        messages.Add(m);
      }
      if( chbHandShake.Checked ) {
        Compass.TpTp.Message m = new Compass.TpTp.HandShakeRequest();
        m.Header.TerminalID = tbTerminalID.Text;
        m.Header.EmployeeID = tbEmployeeID.Text;
        messages.Add(m);
      }

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);

      foreach( var message in messages ) {
        //Console.WriteLine("{0,-15}:rq=[{1}]", "request", message.GetFormatedValue());
        string s4 = string.Format("[Inf] {0}({1}:{2}:{0:D})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
        this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

        //////Compass.TpTp.TpTpClient.LogHandler logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);

        try {
          client.Connect();

          client.Send(message);
          client.Read(message);

          if( message.HeaderRs.ResponseCode == "007" ) {
            this.Logger("[Inf] административная транзакция выполнена успешно");
          }
          else {
            this.Logger("[Err] ошибка при выполнении административной транзакции");
          }
        }
        catch( Exception ex ) {
          this.Logger("[Err] ex::[{0}]", ex.Message);
        }
        finally {
          client.Disconnect();
        }
      }
    }

    private void btCardVerification_Click(object sender, EventArgs e)
    {
      int statusCode = 1; //421;
      string json = "{ \"Date\": \"01.01.0001 0:00:00\", \"invoiceNo\": \"1123\", \"Amount\": \"500\", \"RC\": \"999\", \"StatusCode\": \"" + statusCode.ToString("000") + "\", \"TransID\": \"1111\" }";
      string[] ss = json.Split(',');
      //int pp = json.IndexOf("\"RC\"");
      int pp = json.IndexOf("\"RC\": \"");
      if( pp > 0 ) {
        Console.WriteLine(json.Substring(pp + 7, 3));
      }
      pp = json.IndexOf("\"StatusCode\": \"");
      if( pp > 0 ) {
        Console.WriteLine(json.Substring(pp + 15, 3));
      }

      Compass.TpTp.CardVerificationRequest message = new Compass.TpTp.CardVerificationRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      if( !chbCardReaderMode.Checked )  //&& string.IsNullOrEmpty(edCCData.Text)
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      else
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;

      message.Get<Compass.TpTp.Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
      message.Get<Compass.TpTp.Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";
      message.Get<Compass.TpTp.Track2Field>().Data = string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text;
      //message.Get<Compass.TpTp.OptionalDataField>().Get<Compass.TpTp.StringSubField>(SubFieldType.C).Value = "840";  //USD:840, EUR:978

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        string sRez = "error!";
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" )
          sRez = "success";

        this.Logger("[Inf] {0}, invoiceNum=[{1}],tranID=[{2}]", sRez,
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btBalanceInquiry_Click(object sender, EventArgs e)
    {
      Compass.TpTp.BalanceInquiryRequest message = new Compass.TpTp.BalanceInquiryRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;

      message.Get<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      if( !chbCardReaderMode.Checked )  //&& string.IsNullOrEmpty(edCCData.Text)
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      else
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;

      message.Get<Compass.TpTp.Track2Field>().PAN = ebCCNumber.Text;
      message.Get<Compass.TpTp.Track2Field>().ExpDate = ebCCExpDate.Text;
      message.Get<Compass.TpTp.Track2Field>().Data = string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text;
      //message.Get<Compass.TpTp.OptionalDataField>().Get<Compass.TpTp.StringSubField>(Compass.TpTp.SubFieldType.C).Value = "978";  //USD:840, EUR:978, RUR:810/643

      // добавляем поле валюты платежа
      if( !string.IsNullOrWhiteSpace(tbCurrency.Text) ) {
        message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = tbCurrency.Text;   //USD:840, EUR:978, RUR:810/643
      }

      if( chbPOSConditionCodeEMVChip.Checked ) {
        message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      }

      //message.Get<Compass.TpTp.EchoDataField>().Value = "1234567890";

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        string sRez = "error!";
        if( message.HeaderRs.ResponseCode == "000" || message.HeaderRs.ResponseCode == "003" )
          sRez = "success";

        // отпределяем валюту
        String currency = "none";
        if( message.Get<OptionalDataField>() != null ) {
          if( message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C) != null ) {
            currency = message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value;
          }
        }

        this.Logger("[Inf] {0}({1}), invoiceNum=[{2}],tranID=[{3}],amount=[{4}],cur=[{5}]", sRez, message.HeaderRs.ResponseCode,
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number),
          (message.GetRs<Compass.TpTp.AvailableBalanceField>() == null ? "none" : message.GetRs<Compass.TpTp.AvailableBalanceField>().Value.ToString()),
          currency);
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btPurchase_Click(object sender, EventArgs e)
    {
      string transIDRs = string.Empty;

      //
      //string sss1 = null;
      //string sss2 = string.Format("s={0}", sss1 == null ? "(null)" : sss1);

      Compass.TpTp.PurchaseRequest message = new Compass.TpTp.PurchaseRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;

      //message.Header.TransmissionNumber = 1;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Add<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);

      #region для сертификации
      //различные виды счетов
      if( chbAccountDefault.Checked ) {
        message.Add<ApplicationAccountTypeField>().AccountType = ApplicationAccountTypeField.ApplicationAccountTypeEnum.Default;
      }
      else if( chbAccountCheking.Checked ) {
        message.Add<ApplicationAccountTypeField>().AccountType = ApplicationAccountTypeField.ApplicationAccountTypeEnum.Checking;
      }
      else if( chbAccountSavings.Checked ) {
        message.Add<ApplicationAccountTypeField>().AccountType = ApplicationAccountTypeField.ApplicationAccountTypeEnum.Savings;
      }
      else if( chbAccountCredint.Checked ) {
        message.Add<ApplicationAccountTypeField>().AccountType = ApplicationAccountTypeField.ApplicationAccountTypeEnum.Credit;
      }
      else if( chbAccountBonus.Checked ) {
        message.Add<ApplicationAccountTypeField>().AccountType = ApplicationAccountTypeField.ApplicationAccountTypeEnum.Bonus;
      }

      // формируем дополнительные поля для EMV чипов (когда данные по чиповой карте вводится вручную - режим "fallback")
      // замечания из Compass Plus:
      // По кейсу 5.1.01 не вижу полей FID e = 91, FID 6 SFID O,P – not present в запросе. Насколько я помню, вы писали, что чиповые транзакции не поддерживаете.
      // Но в случае Fallback должен быть послан чиповый POS Condition Code, а сама транзакция пройти по магнитке. В качестве признака Fallback в запросе используется наличие в запросе FID “e” со значением 91 (EMV Chip) и отсутствие в нем FID “6” SFID “O”, “P”.
      // Если в запросе явно указан режим ввода карты в FID 6 SFID E для Fallback его значение должно отличаться от чипового (т.е. не быть равным 05x, 07x, 95x).
      if( chbPOSConditionCodeEMVChip.Checked ) {
        message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      }
      #endregion

      // добавляем поле валюты платежа
      if( !string.IsNullOrWhiteSpace(tbCurrency.Text) ) {
        message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = tbCurrency.Text;   //USD:840, EUR:978, RUR:810/643
      }

      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      if( !chbCardReaderMode.Checked ) {
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";

        // формируем дополнительно поле с CVV2
        //формируем необходимые суб поля в составном поле OptionalDataField
        //OptionalDataField optdata = new OptionalDataField();
        //optdata.Add<StringSubField>(SubFieldType.V);  // <-- валюта транзакции
        if( string.IsNullOrEmpty(edCCCVV2.Text) ) {
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-01";  // <-– CVV2 отсутствует на карте (CVV2 not provided)
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-02";  // <-– CVV2 присутствует на карте, но не читается (CVV2 is on card but is illegible)
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-09";    // <-– владелец карты отказывается от ввода CVV2 (cardholder states that the card has no CVV2 imprint)          
        }
        else {
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = edCCCVV2.Text;
        }
      }
      else {
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
        message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      }

      //message.Get<Track2Field>().PAN = "4890000000001003";

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);

      try {
        client.SendTimeout = 10000;
        client.ReceiveTimeout = 5000;
        client.Connect();
        client.Send(message);
        client.Read(message);

        string sRez = "error!";
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" )
          sRez = "success";

        //transIDRs = message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number;
        this.Logger("[Inf] {0}({1}), tranID=[{2}], invoiceNum=[{3}], approvalCode=[{4}]", sRez, message.HeaderRs.ResponseCode,
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number),
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.ApprovalCodeField>() == null ? "none" : message.GetRs<Compass.TpTp.ApprovalCodeField>().Value));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }

      // если транзакция успешно выполнена, обновляем ebCCTransID
      if( !string.IsNullOrEmpty(transIDRs) ) {
        ebCCTransID.Text = transIDRs;
      }
    }

    private void btPurchasePIN_Click(object sender, EventArgs e)
    {
      Compass.TpTp.PurchaseRequest message = new Compass.TpTp.PurchaseRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmssffff");
      message.Add<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);
      message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      message.Get<Track2Field>().PAN = ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      message.Get<Track2Field>().Data = String.Empty;
      //message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "840";  //USD:840, EUR:978, RUR:810/643
      message.Add<PINField>().Value = "5555"; //"1234"
      message.Add<AuthenticationField>().Value = "12345678"; //?????

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.SendTimeout = 10000;
        client.ReceiveTimeout = 5000;
        client.Connect();
        client.Send(message);
        client.Read(message);

        string sRez = "error!";
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" )
          sRez = "success";

        this.Logger("[Inf] {0}, invoiceNum=[{1}],tranID=[{2}]", sRez,
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btEMVTransaction_Click(object sender, EventArgs e)
    {
      Console.WriteLine(String.Format("{0:000000000000}", "340"));

      // материалы по теме:
      // http://dev.inversepath.com/download/emv/emv_2011.pdf
      // cvr  -> \\demon\c$\SGN\_Development\MyBillJSON\_doc\emv\SDK нового ридера\EMV v4.3\EMV_v4.3_Book_3_Application_Specification_20120607062110791.pdf
      // CVM  -> set the CVM Results according to Book 4 section 6.3.4.5
      // 9F10 -> EMV_v4.3_Book_3_Application_Specification_20120607062110791.pdf
      // aip - [Application Interchange Profile] - http://www.ecuait.com/attachments/article/8/EMV%20v4.2%20Book%203%20Application%20Specification%20CR05.pdf
      // tvr - [Terminal Verification Results] - http://www.ecuait.com/attachments/article/8/EMV%20v4.2%20Book%203%20Application%20Specification%20CR05.pdf
      // tsi - [Transaction Status Information] - http://www.ecuait.com/attachments/article/8/EMV%20v4.2%20Book%203%20Application%20Specification%20CR05.pdf

      Compass.TpTp.Emv.EMVTransactionRequest message = new Compass.TpTp.Emv.EMVTransactionRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;


      #region EMV :: MDM Bank - VISA
      #if EMVMDMVisa
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "MDM Bank", "VISA");

      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      message.Add<Amount1Field>().Amount = 1; //Convert.ToInt64(ebCCAmount.Text);

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "4652055993348926";  // ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = "1207";          //ebCCExpDate.Text;
      message.Get<Track2Field>().Data = "2211212251"; //(string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);

      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = "80";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F1A_terminalCountryCode = "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate;  // "140208"; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = "5203CFA02BACE62C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = "5C00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = "0047";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = "40F22264";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = "8040000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag5F2A_transactionCurrencyCode = "810";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = "06010A03A000000F0400000000000000000001EC2C46FB";

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = "21";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = "1E0300";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = "008C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = "A0000000032010";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F2A_transactionCurrencyCode = "810";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = "87654321";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = "602040";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = "000067";
      #endif
      #endregion

      #region -[ EMV :: Compass Plus - VISA ]----------------------------------------------------------
      #if EMVCompassPlus
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "Compass Plus", "VISA");
      //////message.Header.CurrentDate = "140130";
      //////message.Header.CurrentTime = "024535";

      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      //message.Add<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);
      message.Add<Amount1Field>().Amount = 1;

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "4890000000002001"; // ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = "1511"; //ebCCExpDate.Text;
      message.Get<Track2Field>().Data = ""; // "20118353465"; //(string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);

      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = "80";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).Tag9F1A_TerminalCountryCode = "643"; // "826"; // "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate; // "140202"; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = "724077E56B20F013"; // "724077E56B20F013";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = "7C00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = "0041";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = "00CEC7D7";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = "8080000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).Tag5F2A_TransactionCurrencyCode = "810"; //826 <- ??    (из простой продажи :: //USD:840, EUR:978, RUR:810/643)
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = "06010A03A02000";

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = "21";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = "1E0300";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = "008C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = "A0000000031010";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).Tag5F2A_TransactionCurrencyCode = "810"; // "826"; // "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = "38373635"; //"3837363534333231";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = "A02000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = "000044";

      ////message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      ////message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0";

      //if( chbPOSConditionCodeEMVChip.Checked ) {
      //  message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      //  message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0";

      //  message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      //}

      ////message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      ////message.Get<Track2Field>().PAN = ebCCNumber.Text;
      ////message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      ////message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      ////message.Add<Amount1Field>().Amount = 1
#endif
      #endregion

      #region -[ EMV :: CMP Bank - VISA (1) ]----------------------------------------------------------
      #if EMVCMPVisa1
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "CMP Bank", "VISA");

      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      if( !string.IsNullOrWhiteSpace(ebCCAmount.Text) ) {
        message.Add<Amount1Field>().Amount = 1; //Convert.ToInt64(ebCCAmount.Text);
      }

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "4442380254463965"; // ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = "1508";         //ebCCExpDate.Text;
      message.Get<Track2Field>().Data = "20118184918";     //(string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);

      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = "80";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F1A_terminalCountryCode = "643"; // "826"; "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate; // "140204"; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = "1305CB6F55BD4811"; //"7997E923A9CA0A0B";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = "7D00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = "000D"; //000C
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = "24722F4B";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = "8080100000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag5F2A_transactionCurrencyCode = "810"; // (из простой продажи :: //USD:840, EUR:978, RUR:810/643)
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = "06010A03A0A800";

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = "21";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = "5E0300";  //5E0300
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = "008C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = "A0000000031010";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F2A_transactionCurrencyCode = "810"; // "826"; // "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = "87654321"; //"3837363534333231";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = "602040";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = "000051";  //00000049

      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0";

      //if( chbPOSConditionCodeEMVChip.Checked ) {
      //  message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      //}

      ////message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      ////message.Get<Track2Field>().PAN = ebCCNumber.Text;
      ////message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      ////message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      ////message.Add<Amount1Field>().Amount = 1;
      #endif
      #endregion

      #region -[ EMV :: CMP Bank - VISA (2) ]----------------------------------------------------------
      #if EMVCMPVisa2
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "CMP Bank", "VISA");

      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      if( !string.IsNullOrWhiteSpace(ebCCAmount.Text) ) {
        message.Add<Amount1Field>().Amount = 1; //Convert.ToInt64(ebCCAmount.Text);
      }

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "4442380254479102"; // ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = "1502";         //ebCCExpDate.Text;
      message.Get<Track2Field>().Data = "20118783481";     //(string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);

      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = "80";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F1A_terminalCountryCode = "643"; // "826"; "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate; // "140204"; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = "1B83721D9E438EAC";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = "7D00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = "000D";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = "7548B450";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = "8000100000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag5F2A_transactionCurrencyCode = "810"; // (из простой продажи :: //USD:840, EUR:978, RUR:810/643)
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = "06010A03A0A800";

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = "21";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = "5E0300";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = "008C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = "A0000000031010";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F2A_transactionCurrencyCode = "810"; // "826"; // "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = "87654321"; //"3837363534333231";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = "602040";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = "000082";  //00000049

      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0";

      //if( chbPOSConditionCodeEMVChip.Checked ) {
      //  message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      //}

      ////message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      ////message.Get<Track2Field>().PAN = ebCCNumber.Text;
      ////message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      ////message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      ////message.Add<Amount1Field>().Amount = 1;
      #endif
      #endregion

      #region -[ EMV :: CMP Bank - Master Card ]-------------------------------------------------------
      #if EMVCMPMasterCard
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "CMP Bank", "Master Card");

      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      message.Add<Amount1Field>().Amount = 1; //Convert.ToInt64(ebCCAmount.Text);

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "5188204000000019"; // ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = "1412";         //ebCCExpDate.Text;
      message.Get<Track2Field>().Data = "22011027412"; // "20118184918";     //(string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);

      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = "80";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F1A_terminalCountryCode = "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate; //"140207"; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = "72573C8D06D5E543"; // "54F7C8042847B66C";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = "3800";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = "001F";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = "775B8E0D"; //"0B1AAD46";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = "8000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag5F2A_transactionCurrencyCode = "810";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = "0210A00003220000000000000000000000FF";

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = "00";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = "21";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = "5E0300";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = "0002";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = "A0000000041010";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F2A_transactionCurrencyCode = "810";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = "87654321";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = "602040";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = "000061";

      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      //message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0"; // "2";

      //if( chbPOSConditionCodeEMVChip.Checked ) {
      //  message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      //}

      //message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      //message.Get<Track2Field>().PAN = ebCCNumber.Text;
      //message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      //message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      //message.Add<Amount1Field>().Amount = 1;
      #endif
      #endregion

      #region -[ EMV :: FULL ]--------------------------------------------------------------
      #if EMVUniversal
      this.Logger("[Inf] EMV транзакция -> банк:[{0}] тип карты:[{1}]", "CMP Bank", "VISA");

      //- EMV ----------------------------------------
      byte[] dec = null;
      EMVProcessor ep = new EMVProcessor();
      // декодируем сессионый AES ключ
      if( string.IsNullOrWhiteSpace(ebSessKey.Text) ) {
        //Compass Plus
        ep.decodeSessionAESKey("000044524435303133303030303338000000FFFFFF010000000121D771E460548528DC2594DD11911742B7F3448374F3D0F40F50E1541EE6CEF6A5E97B16257CB93DACFAD1DE842D89CBD1664B53E5F38C473FA062E9B13A812B933991179B715C784C9DD95CE261C81158566199C64D6277C2A3D2CD1F1041DD5DC89E0DD14F9B3BCD27D9F4E72DC0E164A72F5A7272D7686AA70FF8BDF66B77B415ADE2CC47F149859E2C734E45ED93E13B4347ACBEE29F777658AE3296A6AA036D6F0E61E6A56233814A248357DFA7CA46D610363990DEBED80FA5C15D8DA1EEFFE5CA3464427E0BAB7F428DC36031193D91B0037CE47C15B0D04E2D5120B71D7F4191729CE4D9B3C34E53DF4DCBC2AFAE5B203B1357BA2461BE57E03EF49D");
        //CMP
        //ep.decodeSessionAESKey("000044524435303133303030303338000000FFFFFF010000000121D771E460548528DC2594DD11911742B7F3448374F3D0F40F50E1541EE6CEF6A5E97B16257CB93DACFAD1DE842D89CBD1664B53E5F38C473FA062E9B13A812B933991179B715C784C9DD95CE261C81158566199C64D6277C2A3D2CD1F1041DD5DC89E0DD14F9B3BCD27D9F4E72DC0E164A72F5A7272D7686AA70FF8BDF66B77B415ADE2CC47F149859E2C734E45ED93E13B4347ACBEE29F777658AE3296A6AA036D6F0E61E6A56233814A248357DFA7CA46D610363990DEBED80FA5C15D8DA1EEFFE5CA3464427E0BAB7F428DC36031193D91B0037CE47C15B0D04E2D5120B71D7F4191729CE4D9B3C34E53DF4DCBC2AFAE5B203B1357BA2461BE57E03EF49D");
        //ep.decodeSessionAESKey("000044524435303133303030303338000000FFFFFF010000000121D771E460548528DC2594DD11911742B7F3448374F3D0F40F50E1541EE6CEF6A5E97B16257CB93DACFAD1DE842D89CBD1664B53E5F38C473FA062E9B13A812B933991179B715C784C9DD95CE261C81158566199C64D6277C2A3D2CD1F1041DD5DC89E0DD14F9B3BCD27D9F4E72DC0E164A72F5A7272D7686AA70FF8BDF66B77B415ADE2CC47F149859E2C734E45ED93E13B4347ACBEE29F777658AE3296A6AA036D6F0E61E6A56233814A248357DFA7CA46D610363990DEBED80FA5C15D8DA1EEFFE5CA3464427E0BAB7F428DC36031193D91B0037CE47C15B0D04E2D5120B71D7F4191729CE4D9B3C34E53DF4DCBC2AFAE5B203B1357BA2461BE57E03EF49D");
      }
      else {
        ep.decodeSessionAESKey(ebSessKey.Text);
      }

      // кодируем запрос AES ключем для отправки в ридер
      List<BerTlv> rq = new List<BerTlv>();
      rq.Add(EMVUtils.createTlv(PrivateTags.TAG_C4_INITIATE_PROCESSING_FLAGS, "40008000"));  //"40008000" //"50008000"
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_9C_TRANSACTION_TYPE, "00"));
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_9F1C_TERMINAL_ID, "00000001"));
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_9F41_TRANSACTION_SEQ_COUNTER, EMVUtils.encodeTransactionSequence(91)));
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_81_AMOUNT_AUTHORISED_BINARY, EMVUtils.encodeAmount("1,00")));
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_9A_TRANSACTION_DATE, EMVUtils.encodeTransactionDate(DateTime.Now)));
      rq.Add(EMVUtils.createTlv(EMVTags.TAG_9F21_TRANSACTION_TIME, EMVUtils.encodeTransactionTime(DateTime.Now)));

      byte[] enc = ep.createCommand(0x04, BerTlv.listToByteArray(rq));
      this.Logger("[Inf] EMV транзакция : запрос(enc) -> {0}", EMVUtils.byteArrayToHexString(enc));

      // декодируем данные из ридера
      if( string.IsNullOrWhiteSpace(ebEMVST.Text) ) {
        //Compass Plus
        //dec = ep.decryptAESData("E85AC66CE033988811DBB56C20D81BF580380A8A9B508F32A46524869C3F6E137F15C70172AB9701C52DBF6AFB31CF790556187557AD77A21456A730F15360511B24EBDB27F0F75AC95E96536DAF716A4CE34B038C8640601539F000916B3FD97428C6BCF3466CCBD80C7715318CF6A62A9F0CF41209886D54A45F545297E4FAAE6F27FDBF3C4D8BDB1CF6624CFAFA92D5BE484C2ECB0671650A5F3F588EAEABA4809BE1CE4A81DC8C427BEB830FD3070DB0A724165F938CD9C86A86635199B3");
          dec = ep.decryptAESData("FE3753DE05DB41EA4C26FC798EAD96C3A9A7C10B023808174E858D006D6E4AD314F3F6414A94552C3E46E9D4B0F63423C170F0C149B360151AC4BCCD6BBFFAF4AFC0DC6E9AD1C2A6248BCD7F00993B7B7FCA96ACDEB41D82DF48EF7B1B68CA1A37C9B2DA658AF231086A6658A9B82916FEF5DF10AA7A758EE906B869F32B66366F0B63CEF6D9373D1E6D18B01EFF864312657472FBA711098EEE5DFEF9D7F5625C8E6707381122579226F240F7F9AE825B4223C35D15216A4D1411B8CBAE7DDB");
        //CMP
        //dec = ep.decryptAESData("26B77379D342D64086F847569C469626CA18A0F731BDFEEA616AC5734A45D3D9685858895C119A95F4CFB3AFB95215976E7723A9B8365CFB1FCE3D5BCBEFAFBE4ED2418876BE88312F12E505C057E5CF14B923F0CF70B80EECEAC25B488D215506E33EE1B60967F5D1E45F5EAB6E992A88284EF78FD00B3A675CB6CED1F66D051F3AE67DC56E0F1C206D1B84442AF2FD2D1F67F27254230663115CFD28AC64EE1B2D35A8269CABBFF4EF0425C45B26B373AE73AB8AC1B9A009AA38A4D9354252");
        //dec = ep.decryptAESData("26B77379D342D64086F847569C4696268E262DD1CFC648ABD491517DD5C698597A5BAF2E80505A3F34132E21ED11376CDAE29DAE486295D81B64BD6228746BCCC151E94F872FC325635D6BE601EE30CF101606E33A9D2764A27D104AB76A62CA8D616D54A6801398297CAE8DE1919B939EA0C1EFB67857AF0C4A8732580B7C8642B5CB8B8354A93637DC8FE639A54D37D25FE8CDA8C115F3C42A0BA7C64F0DC686FD6D3AAFE8843095369AED65D264111826BFF5D6C3ECBF7E73D0E4515D660B");
      }
      else {
        dec = ep.decryptAESData(ebEMVST.Text);
      }

      List<BerTlv> list = BerTlv.createList(dec);
      Console.WriteLine(BerTlv.listTohexString(list));
      logData("  @Tag: ", list);

      Dictionary<Tag, byte[]> mapEMV = BerTlv.createMap(dec);
      String tag_0057 = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_57_TRACK2_EQUIVALENT_DATA)]);

      //String tag_9f1e = EMVUtils.decodeAscii(mapEMV[new Tag(EMVTags.TAG_9F1E_SERIAL_NUMBER)]);
      //String tag_9f1a = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F1A_TERMINAL_COUNTRY_CODE)]);
      //String tag_9f26 = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F26_APP_CRYPTOGRAM)]);
      //String tag_9f10 = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F10_ISSUER_APP_DAT)]);
      //String tag_0084 = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_84_DF_NAME)]);
     

      //amount
      String Stramount = "1,00";
      string[] amt = Stramount.Split("/"[0]);

      double amount = 0;
      for( int i = 0; i < amt.Length; i++ ) {
        string temp = amt[i];
        if( temp.Contains(".") ) { temp = temp.Replace(".", ","); }
        amount = amount + Double.Parse(temp);
      }
      long EMVamountCC = Convert.ToInt64(amount * 100); // преобразуем в минимальный шаг валюты (TODO: не работает для emv транзакций???)
      String EMVAmount = String.Format("{0:000000000000}", EMVamountCC);//"000000000100";
      //EMVamountCC = Convert.ToInt64(amount);  //TODO: корректируем для emv-транзакций?????????

      //PAN
      String PAN = String.Empty;
      String PANExpDate = String.Empty;
      String PANData = String.Empty;
      if( String.IsNullOrWhiteSpace(tag_0057) ) {
        throw new Exception("tag 57 notfould");
      }
      else {
        int pos = tag_0057.IndexOf('D', 16);
        if( pos < 0 ) throw new Exception("wrong PAN data");
        PAN = tag_0057.Substring(0, pos);
        PANExpDate = tag_0057.Substring(pos + 1, 4);
        PANData = tag_0057.Substring(pos + 5, tag_0057.Length - (pos + 5));
      }

      //- формируем запрос вбанк -----------------
      //FieldType.S
      message.Add<Compass.TpTp.InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");

      //FieldType.N
      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //FieldType.B
      message.Add<Amount1Field>().Amount = EMVamountCC;
      //if( !string.IsNullOrWhiteSpace(ebCCAmount.Text) ) {
        //message.Add<Amount1Field>().Amount = Convert.ToInt64(EMVAmount);
      //}

      //FieldType.a.C - добавляем поле валюты платежа
      if( !string.IsNullOrWhiteSpace(tbCurrency.Text) ) {
        message.Add<OptionalDataField>().Add<StringSubField>(SubFieldType.C).Value = tbCurrency.Text;   //USD:840, EUR:978, RUR:810/643
      }

      //FieldType.q
      message.Add<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = PAN;
      message.Get<Track2Field>().ExpDate = PANExpDate;
      message.Get<Track2Field>().Data = (string.IsNullOrEmpty(PANData) ? string.Empty : PANData);

      //FieldType.O (EMV)
      message.Add<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVRequestDataSubField>().SmartCardScheme = Compass.TpTp.Emv.EMVRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F27_cryptographicInformationData = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F27_CRYPT_INFO_DATA)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).Tag9F1A_TerminalCountryCode = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F1A_TERMINAL_COUNTRY_CODE)]); // "826"; "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9A_emvDate = message.Header.CurrentDate; //ГГММДД
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F26_arqc = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F26_APP_CRYPTOGRAM)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag82_aip = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_82_AIP)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F36_atc = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F36_ATC)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F37_unpredictableNumber = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F37_UNPREDICTABLE_NUMBER)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag95_tvr = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_95_TVR)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9C_cryptogramTransactionType = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9C_TRANSACTION_TYPE)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).Tag5F2A_TransactionCurrencyCode = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_5F2A_TRANSACTION_CURR_CODE)]); // (из простой продажи :: //USD:840, EUR:978, RUR:810/643)
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F02_transactionAmount = EMVAmount;  //EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F02_AMOUNT_AUTHORISED_NUM)])
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVRequestDataSubField>(SubFieldf6Type.fO).tag9F10_issuerApplicationData = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F10_ISSUER_APP_DAT)]);
      //FieldType.P (EMV)
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>().emvCardScheme = Compass.TpTp.Emv.EMVAdditionalRequestDataSubField.SmartCardSchemeType.EMV1996;
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag5F34_applicationPANSequenceNumber = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_5F34_PAN_SEQUENCE_NUMBER)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F35_emvTerminalType = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F35_TERMINAL_TYPE)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F34_cvmresults = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F34_CH_VERIF_METHOD_RESULT)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F09_applicationVersionNumber = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F09_APP_VERSION_NUMBER)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag84_applicationIdentifier = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_84_DF_NAME)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).Tag5F2A_TransactionCurrencyCode = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_5F2A_TRANSACTION_CURR_CODE)]); // "826"; // "643";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F02_transactionAmount = EMVAmount; // EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F02_AMOUNT_AUTHORISED_NUM)]); // "000000000100";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F03_transactionCashBackAmount = "000000000000"; //EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F03_AMOUNT_OTHER_NUM)]); // "000000000000";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F1E_terminalSerialNumber = EMVUtils.decodeAscii(mapEMV[new Tag(EMVTags.TAG_9F1E_SERIAL_NUMBER)]); //"87654321";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F33_terminalCapabilitiesBitMap = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F33_TERMINAL_CAPABILITIES)]);
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVAdditionalRequestDataSubField>(SubFieldf6Type.fP).tag9F41_transactionSequenceCounter = EMVUtils.decodeHex(mapEMV[new Tag(EMVTags.TAG_9F41_TRANSACTION_SEQ_COUNTER)]);

      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Add<Compass.TpTp.Emv.POSEntryModeSubField>().posCondition = "05";
      message.Get<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.POSEntryModeSubField>(SubFieldf6Type.fE).pinCondition = "0";

      if( chbPOSConditionCodeEMVChip.Checked ) {
        message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      }
      #endif
      #endregion

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x}:x:x)", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.SendTimeout = 10000;
        client.ReceiveTimeout = 5000;
        client.Connect();
        client.Send(message);
        client.Read(message);

        //---
        const String EMV_OL_SUCCESS_TC = "0000";  // Transaction is permit by host (Tag 8A is 3030)
      //const String EMV_OL_SUCCESS_AAC = "0001"; // Transaction is denied
      //const String EMV_OL_FAILED = "0002";      // Failed to connect to server.
        const String EMV_OL_INVALID = "0003";     // Invalid transaction result.

        string sRez = "error!";
        List<BerTlv> result = new List<BerTlv>();
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" ) {
          sRez = "success";

          result.Add(EMVUtils.createTlv(PrivateTags.TAG_C7_ONLINE_AUTHORIZATION_PROCESSING_RESULT, EMV_OL_SUCCESS_TC));
          //result.Add(EMVUtils.createTlv(EMVTags.TAG_8A_AUTH_RESP_CODE, "3030"));
        }
        else {
          //Console.WriteLine(hex.ToString());


          result.Add(EMVUtils.createTlv(PrivateTags.TAG_C7_ONLINE_AUTHORIZATION_PROCESSING_RESULT, EMV_OL_INVALID));
          //result.Add(EMVUtils.createTlv(EMVTags.TAG_8A_AUTH_RESP_CODE, hex.ToString()));
        }

        // ищем тег 8A
        String rc = message.GetRs<Compass.TpTp.Emv.ProductSubFIDsField>().Get<Compass.TpTp.Emv.EMVResponseDataSubField>(SubFieldf6Type.fQ).ResponseCode;
        // преобразуем в HEX
        StringBuilder hex = new StringBuilder(rc.Length * 2);
        foreach( byte b in rc ) {
          hex.AppendFormat("{0:x2}", b);
        }
        //добавляем
        result.Add(EMVUtils.createTlv(EMVTags.TAG_8A_AUTH_RESP_CODE, hex.ToString()));

        // ищем тег IAD
        //...

        //отправляем ответ из банка через мобильное приложение в ридер
        //byte[] encRs = ep.createCommand(0x30, BerTlv.listToByteArray(result));
        //this.Logger("[Inf] EMV транзакция : ответ -> {0}", EMVUtils.byteArrayToHexString(BerTlv.listToByteArray(result)));
        //this.Logger("[Inf] EMV транзакция : ответ(enc) -> {0}", EMVUtils.byteArrayToHexString(encRs));

        this.Logger("[Inf] {0}({1}), tranID=[{2}], invoiceNum=[{3}], approvalCode=[{4}]", sRez, message.HeaderRs.ResponseCode,
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number),
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.ApprovalCodeField>() == null ? "none" : message.GetRs<Compass.TpTp.ApprovalCodeField>().Value));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }

      // выполняем реверс транзакции (только для успешных)
      if( chbEMVReverse.Checked ) {
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" ) {
          String tranID = message.GetRs<Compass.TpTp.TransactionIDField>().Number;
          this.Logger("[Inf] реверс EMV транзакции [{0}]", tranID);

          if( String.IsNullOrWhiteSpace(tranID) ) {
            this.Logger("[Err] в ответе банка не обнаруно поле с номером транзакции (.FieldType.t)");
          }

          // команда на обычный реверс
          //-------------------------------
          //message.Add<TransactionIDField>().Number = tranID;  // <-- ebCCTransID.Text;
          //string s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
          //this.Logger("{0,-38}:rq=[{1}]", s5, message.GetFormatedValue());

          Compass.TpTp.PurchaseRequest messageRev = new Compass.TpTp.PurchaseRequest();
          messageRev.Header.TerminalID = tbTerminalID.Text;
          messageRev.Header.EmployeeID = tbEmployeeID.Text;
          messageRev.SetReversal(Compass.TpTp.MessageSubType.ReversalClient);
          //messageRev.SetReversal(Compass.TpTp.MessageSubType.ReversalOther);
          messageRev.Add<TransactionIDField>().Number = tranID;  // <-- ebCCTransID.Text;

          //messageRev.Del<Amount1Field>();

          // добавляем поле идентификатор терминала (клиента)
          if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
            messageRev.Add<CustomerIDField>().Value = tbCustomerID.Text;
          }

          string s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", messageRev.Header.TransactionCode, (char)messageRev.Header.MessageType, (char)messageRev.Header.MessageSubType);
          this.Logger("{0,-38}:rq=[{1}]", s5, messageRev.GetFormatedValue());

          // отправляемкоманду на сервер (в банк)
          try {
            client.Connect();
            client.Send(messageRev);
            client.Read(messageRev);

            string sRez = "error!";
            if( messageRev.HeaderRs.ResponseCode == "001" || messageRev.HeaderRs.ResponseCode == "003" )
              sRez = "success";

            this.Logger("[Inf] {0}({1}), tranID=[{2}], invoiceNum=[{3}]", sRez, messageRev.HeaderRs.ResponseCode,
              (messageRev.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : messageRev.GetRs<Compass.TpTp.TransactionIDField>().Number),
              (messageRev.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : messageRev.GetRs<Compass.TpTp.InvoiceNumberField>().Number));
          }
          catch( Exception ) {
          }
          finally {
            client.Disconnect();
          }
        }
        else {
          this.Logger("[Err] реверс EMV транзакции невозможен! RC=[{0}]", message.HeaderRs.ResponseCode);
        }
      }
    }

    /// <summary описание реверса транзакций>
    /// 1) реверс с типам 'C' (реверс после получения ответа от сервера)
    ///    a) если в ответе есть поле 't', то отправляем значение этого поля, а дату и время устанавливаем в значания, пришедшие в ответе на реверсируемую транзакцию
    ///    б) если в ответе нет поля 't', то устанавливаем дату и время в значания, пришедшие в ответе на реверсируемую транзакцию
    /// 2) реверс с типами 'R', 'M', 'U' (реверс после получения ответа от сервера)
    ///    a) если в ответе есть поле 't', то отправляем только его при этом дата и время могут быть любыми
    ///    б) если в ответе нет поля 't', то устанавливаем дату и время в значания, пришедшие в ответе на реверсируемую транзакцию
    /// 3) реверс с типами 'A', 'T' (реверс по timeout, без ответа от сервера)
    ///    так как ответ от сервера не получен, то устанавливаем дату и время в значения взятые из запроса реверсируемой транзакции
    ///    (так ответ не получен, то поля 't' быть не может)
    /// </summary>
    private void btPurchaseReversal_Click(object sender, EventArgs e)
    {
      string s5 = null;

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      client.ReceiveTimeout = 5000;

      Compass.TpTp.PurchaseRequest message = new Compass.TpTp.PurchaseRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      //message.Header.TransmissionNumber = ++client.TransmissionNumber;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Add<Amount1Field>().Amount = 1;
      message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      message.Get<Track2Field>().PAN = "5890000000002008";
      message.Get<Track2Field>().ExpDate = "1511";   // "1511";
      message.Get<Track2Field>().Data = String.Empty;
      message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "840";

      //test
      //message.Header.TransmissionNumber = 3;

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] Purchase", message.GetFormatedValue());
      s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s5, message.GetFormatedValue());

      short reversal = 0;
      try {
        try {
          client.Connect();
          client.Send(message);
          client.Read(message);

          //TEST
          client.Status |= TpTpStatus.Error;
          throw new Exception("the TEST: read timeout");

          //// реверс по timeout'ту
          //Compass.TpTp.Message message3 = new Compass.TpTp.PurchaseRequest();
          //message3.Header.TransmissionNumber = ++client.TransmissionNumber;
          //// ВАЖНО: при  реверсе по timeout, CurrentDate и CurrentTime устанавливаются в значения из запроса исходного сообщения!!!!
          //message3.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);  // OK!
          //message3.Get<InvoiceNumberField>().Number = message.Get<InvoiceNumberField>().Number;
          //message3.Header.CurrentDate = message.Header.CurrentDate;
          //message3.Header.CurrentTime = message.Header.CurrentTime;

          //Thread.Sleep(5000);

          //s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message3.Header.TransactionCode, (char)message3.Header.MessageType, (char)message3.Header.MessageSubType);
          //Console.WriteLine("{0,-38}:rq=[{1}]", s5, message3.GetFormatedValue());

          //client.Send(message3);
          //client.Read(message3);
          //client.Read(message3);

          //return;
        }
        catch( Exception ex ) {
          this.Logger("ERR(in request): {0}", ex.Message);

          reversal = 3;
          //client.Disconnect();

          //message.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);
          //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] Purchase Reversal", message.GetFormatedValue());
          //s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
          //Console.WriteLine("{0,-38}:rq=[{1}]", s5, message.GetFormatedValue());

          //-[ Apr 27, 2013 ]--------
          do {
            if( (client.Status & TpTpStatus.Error) == TpTpStatus.Error ) {
              //Compass.TpTp.Message reversMsg = new Compass.TpTp.PurchaseRequest();
              //reversMsg.Header.TransmissionNumber = ++message.HeaderRs.TransmissionNumber;
              //reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);

              Compass.TpTp.Message reversMsg = new Compass.TpTp.PurchaseRequest();

              TransactionIDField tx = message.GetRs<TransactionIDField>();
              if( tx == null ) {
                // произошла ошибка в процессе получения ответа -> выполняем реверс по timeout
                reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);
                reversMsg.Get<InvoiceNumberField>().Number = message.GetRs<InvoiceNumberField>().Number;
                reversMsg.Header.CurrentDate = message.Header.CurrentDate;
                reversMsg.Header.CurrentTime = message.Header.CurrentTime;
              }
              else {
                reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalOther);
                reversMsg.Add<TransactionIDField>().Number = tx.Number;
                reversMsg.Header.CurrentDate = message.HeaderRs.CurrentDate; // нужно для режима ReversalOther и НЕ нужно для ReversalClient
                reversMsg.Header.CurrentTime = message.HeaderRs.CurrentTime; // нужно для режима ReversalOther и НЕ нужно для ReversalClient
              }

              client.Disconnect();
              client.Connect();
              client.Send(reversMsg);
              client.Read(reversMsg);

              if( reversMsg.HeaderRs.ResponseCode != "001" ) {  // на отправленный запрос реверса получен статус ошибки 
                reversMsg.Header.TransmissionNumber = 0;
                client.Status |= TpTpStatus.Error; // <- добавить другой статус?
              }
            }
            else {
              // ошибки нет -> выходим
              break;
            }
          }
          while( reversal-- > 0 );
        }
        finally {
          client.Disconnect();
        }

        return;

        if( reversal == 0 ) {
          //message.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout); //ReversalClient //ReversalFraudAlert
          //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] Purchase Reversal", message.GetFormatedValue());
          //string s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
          //Console.WriteLine("{0,-38}:rq=[{1}]", s5, message.GetFormatedValue());
          reversal = 5;
        }

        //if( message.HeaderRs.ResponseCode != "001" ) {
        //  Console.WriteLine("[Err] ошибка при выполнении транзакции: {0}", message.HeaderRs.ResponseCode);
        //  return;
        //}

        ////реверс не по TIMEOUT!!!!!! дату и время берем из ответа
        //Compass.TpTp.Message message2 = new Compass.TpTp.PurchaseRequest();
        ////message2.Header.CurrentDate = message.HeaderRs.CurrentDate;
        ////message2.Header.CurrentTime = message.HeaderRs.CurrentTime;
        ////message2.Get<InvoiceNumberField>().Number = message.Get<InvoiceNumberField>().Number;
        ////message2.Get<Amount1Field>().Amount = 0; //message.Get<Amount1Field>().Amount;
        ////message2.Get<Track2Field>().Type = message.Get<Track2Field>().Type;
        ////message2.Get<Track2Field>().PAN = message.Get<Track2Field>().PAN;
        ////message2.Get<Track2Field>().ExpDate = message.Get<Track2Field>().ExpDate;
        ////message2.Get<Track2Field>().Data = message2.Get<Track2Field>().Data;
        ////message2.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value;
        //message2.Add<TransactionIDField>().Number = message.GetRs<TransactionIDField>().Number;
        //// реверсируем транзакцию
        //message2.SetReversal(Compass.TpTp.MessageSubType.ReversalClient);  //OK!
        ////message2.SetReversal(Compass.TpTp.MessageSubType.ReversalOther);  //OK!
        ////message2.SetReversal(Compass.TpTp.MessageSubType.ReversalFraudAlert);  //OK!
        ////message2.SetReversal(Compass.TpTp.MessageSubType.ReversalWrongMAC);  //OK!

        //// реверс по timeout - ВАЖНО: при  реверсе по timeout, CurrentDate и CurrentTime устанавливаются в значения из запроса исходного сообщения!!!!
        ////message2.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeoutA);  // OK!
        ////message2.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);  // ОК!
        ////message2.Get<InvoiceNumberField>().Number = message.Get<InvoiceNumberField>().Number;
        //message2.Header.CurrentDate = message.Header.CurrentDate;
        //message2.Header.CurrentTime = message.Header.CurrentTime;

        ////test
        ////message2.Header.TransmissionNumber = message.Header.TransmissionNumber;

        ////s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message2.Header.TransactionCode, (char)message2.Header.MessageType, (char)message2.Header.MessageSubType);
        ////Console.WriteLine("{0,-38}:rq=[{1}]", s5, message2.GetFormatedValue());

        // реверс по timeout'ту
        Compass.TpTp.Message message2 = new Compass.TpTp.PurchaseRequest();
        //message3.Header.TransmissionNumber = ++client.TransmissionNumber;
        // ВАЖНО: при  реверсе по timeout, CurrentDate и CurrentTime устанавливаются в значения из запроса исходного сообщения!!!!
        message2.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);  // OK!
        message2.Get<InvoiceNumberField>().Number = message.GetRs<InvoiceNumberField>().Number;
        message2.Header.CurrentDate = message.Header.CurrentDate;
        message2.Header.CurrentTime = message.Header.CurrentTime;

        //реверс
        while( reversal-- > 0 ) {
          try {
            Thread.Sleep(11000);

            s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message2.Header.TransactionCode, (char)message2.Header.MessageType, (char)message2.Header.MessageSubType);
            this.Logger("{0,-38}:rq=[{1}]", s5, message2.GetFormatedValue());

            //client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
            client.Connect();
            client.Send(message2);
            client.Read(message2);
            reversal = 0;
          }
          catch( Exception ex ) {
            this.Logger("ERR(in reversal): {0}", ex.Message);
            Thread.Sleep(3000);
          }
          finally {
            //client.Disconnect();
          }
        }
      }
      catch( Exception ex ) {
        this.Logger("ERR(*): {0}", ex.Message);
      }
      finally {
        //client.Disconnect();
      }
    }

    private void btReversal_Click(object sender, EventArgs e)
    {

      //Compass.TpTp.Message reversMsg = new Compass.TpTp.PurchaseRequest();
      //reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);
      //reversMsg.Get<InvoiceNumberField>().Number = "130516110114";
      //reversMsg.Header.CurrentDate = "130516";
      //reversMsg.Header.CurrentTime = "110113";

      //Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      //client.ReceiveTimeout = 5000;

      //try {
      //  client.Connect();
      //  client.Send(reversMsg);
      //  client.Read(reversMsg);
      //}
      //catch( Exception ) {
      //  Console.WriteLine("Err");
      //}
      //finally {
      //  client.Disconnect();
      //}

      //-------------------------------
      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      client.ReceiveTimeout = 5000;

      Compass.TpTp.PurchaseRequest message = new Compass.TpTp.PurchaseRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.SetReversal(Compass.TpTp.MessageSubType.ReversalClient);
      message.Add<TransactionIDField>().Number = ebCCTransID.Text;

      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] Purchase", message.GetFormatedValue());
      string s5 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s5, message.GetFormatedValue());

      short reversal = 3;
      try {
        try {
          client.Connect();
          client.Send(message);
          client.Read(message);

          this.Logger("[Inf] success, invoiceNum=[{0}],tranID=[{1}]",
            (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
            (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));

          //TEST
          //client.Status |= TpTpStatus.Error;
          //throw new Exception("the TEST: read timeout");
        }
        catch( Exception ) {
          do {
            if( (client.Status & TpTpStatus.Error) == TpTpStatus.Error ) {
              Compass.TpTp.Message reversMsg = new Compass.TpTp.PurchaseRequest();

              TransactionIDField tx = message.GetRs<TransactionIDField>();
              if( tx == null ) {
                // произошла ошибка в процессе получения ответа -> выполняем реверс по timeout
                reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalTimeout);
                reversMsg.Get<InvoiceNumberField>().Number = message.Get<InvoiceNumberField>().Number;
                reversMsg.Header.CurrentDate = message.Header.CurrentDate;
                reversMsg.Header.CurrentTime = message.Header.CurrentTime;
              }
              else {
                // 1) реверс -> MessageSubType.ReversalOther
                //reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalOther);
                //reversMsg.Add<TransactionIDField>().Number = tx.Number;
                //reversMsg.Header.CurrentDate = message.HeaderRs.CurrentDate; // нужно для режима ReversalOther и НЕ нужно для ReversalClient
                //reversMsg.Header.CurrentTime = message.HeaderRs.CurrentTime; // нужно для режима ReversalOther и НЕ нужно для ReversalClient
                // 2) реверс -> MessageSubType.ReversalClient
                reversMsg.SetReversal(Compass.TpTp.MessageSubType.ReversalClient);
                reversMsg.Add<TransactionIDField>().Number = tx.Number;
              }

              client.Disconnect();
              client.Connect();
              client.Send(reversMsg);
              client.Read(reversMsg);

              if( reversMsg.HeaderRs.ResponseCode != "001" ) {  // на отправленный запрос реверса получен статус ошибки 
                reversMsg.Header.TransmissionNumber = 0;
                client.Status |= TpTpStatus.Error; // <- добавить другой статус?
              }
            }
            else {
              // ошибки нет -> выходим
              break;
            }
          }
          while( reversal-- > 0 );
        }
      }
      finally {
        client.Disconnect();
      }
    }

    private void btMailOrTelephoneOrder_Click(object sender, EventArgs e)
    {
      //this.doWork = true;

      //Compass.TpTp.MailOrTelephoneOrderRequest message = new Compass.TpTp.MailOrTelephoneOrderRequest();
      //message.invoice.Number = DateTime.Now.ToString("yyMMddhhmmss");
      //message.amount.Amount = 1;
      //message.track.Type = Compass.TpTp.Track2Field.EnterType.Manual;
      //message.track.PAN = "5890000000002008";
      //message.track.ExpirationDate = "1511";
      //message.track.Data = String.Empty;
      //(message.optdata.Fields[Compass.TpTp.SubFieldType.C] as Compass.TpTp.StringSubField).Value = "840";
      //message.posConditionCode.Value = Compass.TpTp.POSConditionCodeField.POSConditionCodeType.MailOrTelephoneOrder;

      //Compass.TpTp.MerchandiseReturnRequest message = new Compass.TpTp.MerchandiseReturnRequest();
      //message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      //message.Get<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);
      //message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      //message.Get<Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
      //message.Get<Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";
      //message.Get<Track2Field>().Data = String.Empty;
      //message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "978";  //USD:840, EUR:978, RUR:810/643

      //Compass.TpTp.MailOrTelephoneOrderRequest message = new Compass.TpTp.MailOrTelephoneOrderRequest();
      //message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      //message.Get<Amount1Field>().Amount = 1;
      //message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      //message.Get<Track2Field>().PAN = "5890000000002008";
      //message.Get<Track2Field>().ExpDate = "1511";
      //message.Get<Track2Field>().Data = String.Empty;
      //message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "978";  //USD:840, EUR:978, RUR:810/643
      //message.Get<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.MailOrTelephoneOrder;

      Compass.TpTp.MailOrTelephoneOrderRequest message = new Compass.TpTp.MailOrTelephoneOrderRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Get<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);

      if( !chbCardReaderMode.Checked )  //&& string.IsNullOrEmpty(edCCData.Text) 
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      else
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;

      message.Get<Track2Field>().PAN = ebCCNumber.Text;
      message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
      message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "978";  //USD:840, EUR:978, RUR:810/643
      message.Get<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.MailOrTelephoneOrder;

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] MailOrTelephoneOrder", message.GetFormatedValue());
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());


      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btMerchandiseReturn_Click(object sender, EventArgs e)
    {
      Compass.TpTp.MerchandiseReturnRequest message = new Compass.TpTp.MerchandiseReturnRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;

      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Get<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);

      // добавляем поле валюты платежа
      message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "810";  //USD:840, EUR:978, RUR:810/643

      if( !chbCardReaderMode.Checked ) {  //&& string.IsNullOrEmpty(edCCData.Text) 
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";

        // формируем дополнительно поле с CVV2
        if( string.IsNullOrEmpty(edCCCVV2.Text) ) {
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-01";  // <-– CVV2 отсутствует на карте (CVV2 not provided)
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-02";  // <-– CVV2 присутствует на карте, но не читается (CVV2 is on card but is illegible)
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-09";    // <-– владелец карты отказывается от ввода CVV2 (cardholder states that the card has no CVV2 imprint)          
        }
        else {
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = edCCCVV2.Text;
        }
      }
      else {
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
        message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      }

      // добавляем поле валюты платежа
      if( !string.IsNullOrWhiteSpace(tbCurrency.Text) ) {
        //message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "810";  //USD:840, EUR:978, RUR:810/643
        message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = tbCurrency.Text;
      }

      // добавляем поле идентификатор терминала (клиента)
      if( !string.IsNullOrWhiteSpace(tbCustomerID.Text) ) {
        message.Add<CustomerIDField>().Value = tbCustomerID.Text;
      }

      #region старый код, формирующий track2
      //if( string.IsNullOrEmpty(edCCData.Text) ) message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
      //else message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;

      //message.Get<Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
      //message.Get<Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";
      //message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      #endregion

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] MerchandiseReturn", message.GetFormatedValue());
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        string sRez = "error!";
        if( message.HeaderRs.ResponseCode == "001" || message.HeaderRs.ResponseCode == "003" )
          sRez = "success";

        this.Logger("[Inf] {0}({1}), tranID=[{2}], invoiceNum=[{3}], approvalCode=[{4}]", sRez, message.HeaderRs.ResponseCode,
          (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "none" : message.GetRs<Compass.TpTp.TransactionIDField>().Number),
          (message.GetRs<Compass.TpTp.InvoiceNumberField>() == null ? "none" : message.GetRs<Compass.TpTp.InvoiceNumberField>().Number),
          (message.GetRs<Compass.TpTp.ApprovalCodeField>() == null ? "none" : message.GetRs<Compass.TpTp.ApprovalCodeField>().Value));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btCardVerificationMSC_Click(object sender, EventArgs e)
    {
      //this.doWork = true;

      //4890000000001003=151110190880639
      //Compass.TpTp.CardVerificationRequest message = new Compass.TpTp.CardVerificationRequest();
      //(message.Fields[0] as Compass.TpTp.InvoiceNumberField).Number = DateTime.Now.ToString("yyMMddhhmmss");
      //(message.Fields[1] as Compass.TpTp.Track2Field).Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      //(message.Fields[1] as Compass.TpTp.Track2Field).PAN = "4890000000001003";
      //(message.Fields[1] as Compass.TpTp.Track2Field).ExpirationDate = "1511";
      //(message.Fields[1] as Compass.TpTp.Track2Field).Data = "10190880639";
      //((message.Fields[2] as Compass.TpTp.OptionalDataField).Fields[0] as Compass.TpTp.StringSubField).Value = "810";  //USD:840, EUR:978

      Compass.TpTp.CardVerificationRequest message = new Compass.TpTp.CardVerificationRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      message.Get<Track2Field>().PAN = "4890000000001003";
      message.Get<Track2Field>().ExpDate = "1511";
      message.Get<Track2Field>().Data = "10190880639";
      message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = "810";  //USD:840, EUR:978

      if( chbPOSConditionCodeEMVChip.Checked ) {
        message.Add<POSConditionCodeField>().Value = POSConditionCodeField.POSConditionCodeType.EMVChip;
      }

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] CardVerification", message.GetFormatedValue());
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btReplenishment_Click(object sender, EventArgs e)
    {
      this.Logger("[Inf] Пополнение баланса...");

      Compass.TpTp.ReplenishmentRequest message = new Compass.TpTp.ReplenishmentRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<InvoiceNumberField>().Number = DateTime.Now.ToString("yyMMddhhmmss");
      message.Get<Amount1Field>().Amount = Convert.ToInt64(ebCCAmount.Text);
      //message.Get<Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
      //message.Get<Track2Field>().PAN = "4890000000001003";
      //message.Get<Track2Field>().ExpDate = "1511";
      //message.Get<Track2Field>().Data = "10190880639";
      //message.Get<OptionalDataField>().Get<Compass.TpTp.StringSubField>(Compass.TpTp.SubFieldType.C).Value = "643";  //USD:"840", EUR:"978", RUB:"643"

      if( !chbCardReaderMode.Checked ) {
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.Manual;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;  //"4890000000002001"; //"5890000000002008";
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text; //"1511";

        // формируем дополнительно поле с CVV2
        //формируем необходимые суб поля в составном поле OptionalDataField
        //OptionalDataField optdata = new OptionalDataField();
        //optdata.Add<StringSubField>(SubFieldType.V);  // <-- валюта транзакции
        if( string.IsNullOrEmpty(edCCCVV2.Text) ) {
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-01";  // <-– CVV2 отсутствует на карте (CVV2 not provided)
          //message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-02";  // <-– CVV2 присутствует на карте, но не читается (CVV2 is on card but is illegible)
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = "-09";    // <-– владелец карты отказывается от ввода CVV2 (cardholder states that the card has no CVV2 imprint)          
        }
        else {
          message.Get<OptionalDataField>().Add<StringSubField>(SubFieldType.V).Value = edCCCVV2.Text;
        }
      }
      else {
        message.Get<Compass.TpTp.Track2Field>().Type = Compass.TpTp.Track2Field.EnterType.CardReader;
        message.Get<Track2Field>().PAN = ebCCNumber.Text;
        message.Get<Track2Field>().ExpDate = ebCCExpDate.Text;
        message.Get<Track2Field>().Data = (string.IsNullOrEmpty(edCCData.Text) ? string.Empty : edCCData.Text);
      }

      // добавляем поле валюты платежа
      if( !string.IsNullOrWhiteSpace(tbCurrency.Text) ) {
        message.Get<OptionalDataField>().Get<StringSubField>(SubFieldType.C).Value = tbCurrency.Text;   //USD:840, EUR:978, RUR:810/643
      }

      //Console.WriteLine("{0,-38}:rq=[{1}]", "[Inf] Replenishment", message.GetFormatedValue());
      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Compass.TpTp.Message message = new Compass.TpTp.HandShakeRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Header.TransmissionNumber = 1;
      byte[] bq = message.Encode(Encoding.GetEncoding(866));

      //Client c = new Client(this.logon.Host, this.logon.Port);
      //c.Connect();
      //c.Write(bq);

      ////AsynchronousClient ac = new AsynchronousClient();
      //AsynchronousClient.StartClient(this.logon.Host, this.logon.Port);
      //AsynchronousClient.Send(AsynchronousClient.client, message.Encode(Encoding.GetEncoding(866)));
      //AsynchronousClient.Receive(AsynchronousClient.client);
      //AsynchronousClient.Connect(AsynchronousClient.client, this.logon.Host, this.logon.Port);
      //AsynchronousClient.Send(AsynchronousClient.client, message.Encode(Encoding.GetEncoding(866)));
      //AsynchronousClient.Receive(AsynchronousClient.client);

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btCloseBatch_Click(object sender, EventArgs e)
    {
      CloseBatchRequest message = new CloseBatchRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsBatchField>().ShiftNumber = 1;
      message.Get<TotalsBatchField>().BatchNumber = 1;
      message.Get<TotalsBatchField>().DebetTransactionCount = 0;
      message.Get<TotalsBatchField>().DebetTransactionSum = 0;
      message.Get<TotalsBatchField>().CreditTransactionCount = 0;
      message.Get<TotalsBatchField>().CreditTransactionSum = 0;
      message.Get<TotalsBatchField>().CorrectionTransactionCount = 0;
      message.Get<TotalsBatchField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btCloseShift_Click(object sender, EventArgs e)
    {
      CloseShiftRequest message = new CloseShiftRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsShiftField>().ShiftCount = 1;
      message.Get<TotalsShiftField>().BatchCount = 1;
      message.Get<TotalsShiftField>().DebetTransactionCount = 0;
      message.Get<TotalsShiftField>().DebetTransactionSum = 0;
      message.Get<TotalsShiftField>().CreditTransactionCount = 0;
      message.Get<TotalsShiftField>().CreditTransactionSum = 0;
      message.Get<TotalsShiftField>().CorrectionTransactionCount = 0;
      message.Get<TotalsShiftField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btCloseDay_Click(object sender, EventArgs e)
    {
      CloseDayRequest message = new CloseDayRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsDayField>().ShiftCount = 1;
      message.Get<TotalsDayField>().BatchCount = 1;
      message.Get<TotalsDayField>().DebetTransactionCount = 0;
      message.Get<TotalsDayField>().DebetTransactionSum = 0;
      message.Get<TotalsDayField>().CreditTransactionCount = 0;
      message.Get<TotalsDayField>().CreditTransactionSum = 0;
      message.Get<TotalsDayField>().CorrectionTransactionCount = 0;
      message.Get<TotalsDayField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btSubtotalsBatch_Click(object sender, EventArgs e)
    {
      SubtotalsBatchRequest message = new SubtotalsBatchRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsBatchField>().ShiftNumber = 1;
      message.Get<TotalsBatchField>().BatchNumber = 1;
      message.Get<TotalsBatchField>().DebetTransactionCount = 0;
      message.Get<TotalsBatchField>().DebetTransactionSum = 0;
      message.Get<TotalsBatchField>().CreditTransactionCount = 0;
      message.Get<TotalsBatchField>().CreditTransactionSum = 0;
      message.Get<TotalsBatchField>().CorrectionTransactionCount = 0;
      message.Get<TotalsBatchField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btSubtotalsShift_Click(object sender, EventArgs e)
    {
      SubtotalsShiftRequest message = new SubtotalsShiftRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsShiftField>().ShiftCount = 1;
      message.Get<TotalsShiftField>().BatchCount = 1;
      message.Get<TotalsShiftField>().DebetTransactionCount = 0;
      message.Get<TotalsShiftField>().DebetTransactionSum = 0;
      message.Get<TotalsShiftField>().CreditTransactionCount = 0;
      message.Get<TotalsShiftField>().CreditTransactionSum = 0;
      message.Get<TotalsShiftField>().CorrectionTransactionCount = 0;
      message.Get<TotalsShiftField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }

    private void btSubtotalsDay_Click(object sender, EventArgs e)
    {
      SubtotalsDayRequest message = new SubtotalsDayRequest();
      message.Header.TerminalID = tbTerminalID.Text;
      message.Header.EmployeeID = tbEmployeeID.Text;
      message.Get<TotalsDayField>().ShiftCount = 1;
      message.Get<TotalsDayField>().BatchCount = 1;
      message.Get<TotalsDayField>().DebetTransactionCount = 0;
      message.Get<TotalsDayField>().DebetTransactionSum = 0;
      message.Get<TotalsDayField>().CreditTransactionCount = 0;
      message.Get<TotalsDayField>().CreditTransactionSum = 0;
      message.Get<TotalsDayField>().CorrectionTransactionCount = 0;
      message.Get<TotalsDayField>().CorrectionTransactionSum = 0;

      string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.Header.TransactionCode, (char)message.Header.MessageType, (char)message.Header.MessageSubType);
      this.Logger("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());

      Compass.TpTp.TpTpClient client = new Compass.TpTp.TpTpClient(this.logon.Host, this.logon.Port);
      client.Logger = new Compass.TpTp.TpTpClient.LogHandler(Logger);
      try {
        client.Connect();
        client.Send(message);
        client.Read(message);

        this.Logger("[Inf] success, tranID={0}", (message.GetRs<Compass.TpTp.TransactionIDField>() == null ? "[none]" : message.GetRs<Compass.TpTp.TransactionIDField>().Number));
      }
      catch( Exception ex ) {
        this.Logger("ERR: {0}", ex.Message);
      }
      finally {
        client.Disconnect();
      }
    }
  }

  internal class LogonParams
  {
    internal string Host { get; private set; }
    internal int Port { get; private set; }
    internal string Desc { get; private set; }

    internal LogonParams(string host, int port, string desc)
    {
      this.Host = host;
      this.Port = port;
      this.Desc = desc;
    }
  }

  internal class CardTrack2
  {
    internal string PAN { get; private set; }
    internal string ExpDate { get; private set; }
    internal string Data { get; private set; }

    internal CardTrack2(string pan, string expDate, string data)
    {
      this.PAN = pan;
      this.ExpDate = expDate;
      this.Data = data;
    }
  }

  #region old
  public class TpTpMessage
  {
    public TpTpHeader header = new TpTpHeader();
    public HashSet<TpTpField> fields = new HashSet<TpTpField>();

    public TpTpMessage()
    {
    }

    public HashSet<TpTpField> AddField(TpTpField field)
    {
      fields.Add(field);
      return fields;
    }

    public string GetFormatedValue()
    {
      string val = header.GetFormatedValue();
      foreach( var f in fields ) {
        val += f.GetFormatedValue();
      }
      return val;
    }
  }

  public class TpTpField
  {
    public TpTpFieldType FID;
    public object value = new object();

    public TpTpField()
    {
    }

    public virtual string GetFormatedValue()
    {
      throw new Exception("method has been not implemented");
    }
  }

  public enum TpTpFieldType : sbyte
  {
    A = (sbyte)'A',  // CustomerBillingAddress
    B = (sbyte)'B',  // Amount1
    C = (sbyte)'C',  // Amount2
    D = (sbyte)'D',  // ApplicationAcountType
    E = (sbyte)'E',  // ApplicationAcountNumber
    b = (sbyte)'b',  // PIN/Customer
    c = (sbyte)'c',  // New PIN/Customer
    q = (sbyte)'q'   // Track2/Customer
  }

  public class TpTpHeader
  {
    public TpTpHeader()
    {
      //TransmissionNumber = "00"; // не используется
      //MessageType = "X"; // пустой тип, нет операций
      //TransactionCode = "99";
      //ProcessingFlag1 = "0";  // ответ и разрыв связи
      //ProcessingFlag2 = "0";  // прогрузка не требуется

      DeviceType = "9.";
      TransmissionNumber = 0;   // не используется
      TerminalID = "MYBILLR";
      EmployeeID = "";
      CurrentDate = "000000";
      CurrentTime = "      ";
      MessageType = TpTpMessageType.Finance;
      MessageSubType = TpTpMessageSubType.ReversalByTimeout;
      TransactionCode = TpTpTransactionCode.HandshakeRequest;
      ProcessingFlag1 = TpTpProcessingFlag1.ReplayAndDisconnect;
      ProcessingFlag2 = TpTpProcessingFlag2.NoLoad;
      ProcessingFlag3 = "0";   // "любое значение"
      ResponseCode = "999";    // используется только в ответе
    }

    public string GetFormatedValue()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DeviceType);
      sb.Append(TransmissionNumber.ToString("D2"));
      sb.Append(TerminalID.PadRight(16));
      sb.Append(EmployeeID.PadRight(6));
      sb.Append(CurrentDate.PadRight(6));
      sb.Append(CurrentTime.PadRight(6));
      sb.Append((char)MessageType);
      sb.Append((char)MessageSubType);
      sb.Append(((sbyte)TransactionCode).ToString("D2"));
      sb.Append((char)ProcessingFlag1);
      sb.Append((char)ProcessingFlag2);
      sb.Append(ProcessingFlag3);
      sb.Append(ResponseCode);
      return sb.ToString();
    }

    public string DeviceType { get; private set; }
    public byte TransmissionNumber { get; set; }
    public string TerminalID { get; set; }
    public string EmployeeID { get; set; }
    public string CurrentDate { get; set; }
    public string CurrentTime { get; set; }
    public TpTpMessageType MessageType { get; set; }
    public TpTpMessageSubType MessageSubType { get; set; }
    public TpTpTransactionCode TransactionCode { get; set; }
    public TpTpProcessingFlag1 ProcessingFlag1 { get; set; }
    public TpTpProcessingFlag2 ProcessingFlag2 { get; set; }
    public string ProcessingFlag3 { get; set; }
    public string ResponseCode { get; set; }
  }

  public enum TpTpMessageType : sbyte
  {
    Admin = (sbyte)'A',
    Finance = (sbyte)'F'
  }

  public enum TpTpMessageSubType : sbyte
  {
    ReversalByTimeout = (sbyte)'A',
    ReversalByTimeout2 = (sbyte)'T',
    ReversalWrongMAC = (sbyte)'R',
    ReversalFraudAlert = (sbyte)'M',
    ReversalClient = (sbyte)'U',
    ReversalOther = (sbyte)'C',
    OfflineTransaction = (sbyte)'O',
    ForsedTransaction = (sbyte)'F',
    OfflineTransaction2 = (sbyte)'S'
  }

  public enum TpTpTransactionCode : sbyte
  {
    Purchase = 0,
    MailOrTelephoneOrder = 3,
    MerchendiseReturn = 4,
    Payment = 41,
    LogonRequest = 50,
    LogoffRequest = 51,
    HandshakeRequest = 95
  }

  public enum TpTpProcessingFlag1 : sbyte
  {
    ReplayAndDisconnect = 0,
    ReplayAndWait = 1
  }

  public enum TpTpProcessingFlag2 : sbyte
  {
    NoLoad = 0,
    NeedLoadParameters = 1
  }
  #endregion
}