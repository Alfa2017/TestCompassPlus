using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Compass.TpTp;

namespace Compass.TpTp.Emv
{
  public class EMVTransactionRequest : Message
  {
    public EMVTransactionRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.Purchase)
    //public EMVTransactionRequest() : base(MessageType.Finance, MessageSubType.ForsedTransaction, TransactionCode.Purchase)
    {
      //формируем необходимые суб поля в составном поле ProductSubFIDsField
      //ProductSubFIDsField proddata = new ProductSubFIDsField();
      //EMVRequestDataSubField emvdata = new EMVRequestDataSubField();
      ////EMVAdditionalRequestDataField optdata = new EMVAdditionalRequestDataField();
      
      ////emvdata.Add<>();
      //emvdata.SmartCardScheme = EMVRequestDataSubField.SmartCardSchemeType.EMV1996;

      //proddata.Add(SubFieldf6Type.fO, emvdata);
      //proddata.Add(SubFieldf6Type.fP, optdata);
      //this.Add<ProductSubFIDsField>(proddata);

      //формируем необходимые суб поля в составном поле ProductSubFIDsField
      //ProductSubFIDsField proddata = new ProductSubFIDsField();
      //this.Add<ProductSubFIDsField>(proddata);
    }

    //public void Init(string track, double amount) //SGN: непонятно как реализовывать этот метод при древовидной системе вложенных полей...

    public override void Decode(byte[] data, Encoding encoding)
    {
      // декодируем header и body
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  // используется, например, при EMV-транзакциях или для передачи CVV2 кода
  public class ProductSubFIDsField : Field
  {
    public const byte GS = 0x1D; // Group Separator (GS)
    private Fields<SubFieldf6Type, SubField<SubFieldf6Type>> fields = new Fields<SubFieldf6Type, SubField<SubFieldf6Type>>();

    public ProductSubFIDsField() : base(Compass.TpTp.FieldType.f6)
    {
    }

    public U Get<U>(SubFieldf6Type type) where U : SubField<SubFieldf6Type>
    {
      SubField<SubFieldf6Type> f = null;
      if( this.fields.TryGetValue(type, out f) ) {
        return (f as U);
      }
      return null;

      // первый вариант: генерирует исключение при отсутсвии требуемого поля...
      //return (this.Fields[type] as U);
    }

    //public U Get<U>() where U : SubField<SubFieldf6Type>
    //{
    //  //U SubField<SubFieldf6Type>;
    //  SubField<SubFieldf6Type> f = null;
    //  if( this.fields.TryGetValue(f.SFID, out f) ) {
    //    return (f as U);
    //  }
    //  return null;
    //}

    protected override string GetValue()
    {
      StringBuilder sb = new StringBuilder();
      if( this.fields != null ) {
        foreach( var f in fields ) {
          string val = f.GetValue();
          if( !String.IsNullOrEmpty(val) ) { //if( !String.IsNullOrEmpty(f.GetValue()) ) {
            sb.Append(string.Format("{0:D1}{1:D1}{2}", (char)GS, (char)f.SFID, val));  //(char)
          }
        }
      }
      if( sb.Length == 0 ) {
        return String.Empty;
      }
      return sb.ToString();
    }

    internal override void Decode(int pos, byte[] data)
    {
      //throw new NotImplementedException();
      if( data[pos] == (byte)FieldType.f6 ) {
        SubField<SubFieldf6Type> sf = null;
        SubFieldf6Type fieldType = SubFieldf6Type.None;
        bool fieldFound = true;
        byte state = 0; // 0 - none, 1 - fieldstart, 2 - fielddecoding, 3 - endofdata, 4 - subfieldstart 5 - subfielddecoding
        //for( int i = 2; i < (956 + 1); ++i ) {
        for( int i = 1; i < data.Length; ++i ) {
          if( state == 0 ) {
            if( data[pos + i] == (byte)EscType.ETX ) {
              // встретили окончание сообщения - выходим из цикла
              state = 3;
              break;
            }
            else if( data[pos + i] == Message.FS ) {
              // встретили начало нового поля (FS <- разделитель полей)
              state = 1;
              break;
            }
            else if( data[pos + i] == ProductSubFIDsField.GS ) {
              // встретили разделитель начала субполя -> Group Separator (GS)
              state = 4;
            }
            else {
              //Console.WriteLine("[Dbg] что-то не так как надо... нужно разобраться... state={0}, data[pos + i]={1}", state, data[pos + i]);
            }
          }
          else if( state == 4 ) {
            string sfid = this.Encoding.GetString(data, pos + i, 1).TrimEnd();
            //if( sfid == "O" ) {
            //  fieldType = SubFieldf6Type.fO;
            //  if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
            //    sf = new EMVRequestDataSubField();
            //  }
            //}
            if( sfid == "Q" ) {
              fieldType = SubFieldf6Type.fQ;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new EMVResponseDataSubField();
              }
            }
            else if( sfid == "R" ) {
              fieldType = SubFieldf6Type.fR;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new EMVAdditionalResponseDataSubField();
              }
            }
            else if( sfid == "E" ) {
              fieldType = SubFieldf6Type.fE;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new POSEntryModeSubField();
              }
            }
            //else if() {
            //}
            else {
              //Console.WriteLine("[Dbg] что-то не так как надо... нужно разобраться... state={0}, did={1}", state, did);
              state = 0; //пока делаем так:
            }
          }
          else {
            state = 0;
          }

          if( sf != null ) {
            try {
              sf.Decode(pos + i, data);
              if( !fieldFound ) {
                this.Fields.Add(fieldType, sf); //this.Fields.Add(sf.Name, sf);
              }
            }
            catch( Exception ) {
              Console.WriteLine("[Err] не удалось декодировать субполе ({0})", fieldType);
            }
            i += sf.Size(); //увеличиваем позицию на размер поля
            sf = null; // сброс

            state = 0; // чтобы были готовы считывать следующее поле
          }
        }
      }
      else {
        Console.WriteLine("[Err] ProductSubFIDsField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      int len = 0;
      foreach( var f in fields ) {
        //string val = f.GetValue();
        if( !string.IsNullOrEmpty(f.GetValue()) ) {
          len += f.Size() + 2;
        }
      }
      return len;
    }

    public void Add(int id, SubFieldf6Type code, SubField<SubFieldf6Type> subField)
    {
      this.fields.Add(id, code, subField);
    }

    public void Add(SubFieldf6Type code, SubField<SubFieldf6Type> subField)
    {
      this.fields.Add(code, subField);
    }

    public U Add<U>(SubFieldf6Type code) where U : SubField<SubFieldf6Type> //, new()
    {
      U subField = Activator.CreateInstance(typeof(U), code) as U;
      //U subField = new U(code);
      this.fields.Add(code, subField);
      return subField;
    }

    public U Add<U>() where U : SubField<SubFieldf6Type>, new()
    {
      //U subField = Activator.CreateInstance(typeof(U), code) as U;
      U subField = new U();
      this.fields.Add(subField.SFID, subField);
      return subField;
    }

    public Fields<SubFieldf6Type, SubField<SubFieldf6Type>> Fields
    {
      get { return fields; }
    }
  }

  public class EMVRequestDataSubField : SubField<SubFieldf6Type>
  {
    public enum SmartCardSchemeType : sbyte
    {
      EMV1996 = (sbyte)'0',
      EMV2000 = (sbyte)'1',
    }

    //private Fields<SubFieldf6Type, SubField<SubFieldf6Type>> fields = new Fields<SubFieldf6Type, SubField<SubFieldf6Type>>();

    private SmartCardSchemeType smartCardScheme;
    public String tag9F27_cryptographicInformationData;     //EMV Tag: 9F27
    private String tag9F1A_terminalCountryCode;             //EMV Tag: 9F1A - 3-цифровой код страны терминала согласно стандарту ISO 3166
    public String tag9A_emvDate;                            //EMV Tag: 9A   - Локальная дата (в формате ГГММДД).
    public String tag9F26_arqc;                             //EMV Tag: 9F26 - AuthorizationRequestCryptogram (ARQC)
    public String tag82_aip;                                //EMV Tag: 82   - Application Interchange Profile (AIP)
    public String tag9F36_atc;                              //EMV Tag: 9F36 - Application Transaction Counter (ATC)
    public String tag9F37_unpredictableNumber;              //EMV Tag: 9F37 - Unpredictable Number
    public String tag95_tvr;                                //EMV Tag: 95   - Terminal Verification Results (TVR)
    public String tag9C_cryptogramTransactionType;          //EMV Tag: 9C   - Cryptogram Transaction Type
    private String tag5F2A_transactionCurrencyCode;         //EMV Tag: 5F2A - Transaction Currency Code
    public String tag9F02_transactionAmount;                //EMV Tag: 9F02 - Transaction Amount
    public String tag9F10_issuerApplicationData;            //EMV Tag: 9F10 - Issuer Application Data

    public EMVRequestDataSubField() : base(SubFieldf6Type.fO)  //public EMVRequestDataSubField(SubFieldf6Type sfid) : base(sfid)
    {
      smartCardScheme = SmartCardSchemeType.EMV2000;
    }

    #region
    private String GetSmartCardSchemeValue()
    {
      if( smartCardScheme == SmartCardSchemeType.EMV1996 ) return "00";
      else if( smartCardScheme == SmartCardSchemeType.EMV2000 ) return "01";
      else throw new Exception("unknown smart card scheme");
    }
    #endregion

    public override string GetValue()
    {
      String val = String.Empty;
      if( smartCardScheme == SmartCardSchemeType.EMV1996 ) {
        val = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", GetSmartCardSchemeValue(), tag9F27_cryptographicInformationData, tag9F1A_terminalCountryCode, tag9A_emvDate, tag9F26_arqc, tag82_aip, tag9F36_atc, tag9F37_unpredictableNumber, tag95_tvr, tag9C_cryptogramTransactionType, tag9F10_issuerApplicationData);
      }
      else if( smartCardScheme == SmartCardSchemeType.EMV2000 ) {
        val = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", GetSmartCardSchemeValue(), tag9F27_cryptographicInformationData, tag9F1A_terminalCountryCode, tag9A_emvDate, tag9F26_arqc, tag82_aip, tag9F36_atc, tag9F37_unpredictableNumber, tag95_tvr, tag9C_cryptogramTransactionType, tag5F2A_transactionCurrencyCode, tag9F02_transactionAmount, tag9F10_issuerApplicationData);
      }
      else {
        throw new Exception("unknown scheme");
      }
      return val;
    }

    public override void Decode(int pos, byte[] data)
    {
      throw new NotImplementedException();
    }

    public override int Size()
    {
      int len = 0;
      if( smartCardScheme == SmartCardSchemeType.EMV1996 ) {
        len = 121;
      }
      else if( smartCardScheme == SmartCardSchemeType.EMV2000 ) {
        len = 136;
      }
      else {
        throw new Exception("unknown scheme");
      }
      return len;
    }

    public override string ToString()
    {
      //throw new NotFiniteNumberException();
      return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", GetSmartCardSchemeValue(), tag9F27_cryptographicInformationData, tag9F1A_terminalCountryCode, tag9A_emvDate, tag9F26_arqc, tag82_aip, tag9F36_atc, tag9F37_unpredictableNumber, tag95_tvr, tag9C_cryptogramTransactionType, tag5F2A_transactionCurrencyCode, tag9F02_transactionAmount, tag9F10_issuerApplicationData);
    }

    #region properties
    public SmartCardSchemeType SmartCardScheme
    {
      get { return this.smartCardScheme; }
      set { this.smartCardScheme = value; }
    }
    public String CryptographicInformationData
    {
      get { return this.tag9F27_cryptographicInformationData; }
      set { this.tag9F27_cryptographicInformationData = value; }
    }
    public String Tag9F1A_TerminalCountryCode
    {
      get { return this.tag9F1A_terminalCountryCode; }
      set { 
        if( String.IsNullOrWhiteSpace(value) ) {
          this.tag9F1A_terminalCountryCode = "000";
        }
        else if( value.Length > 3 ) {
          this.tag9F1A_terminalCountryCode = value.Substring(value.Length - 3);
        }
        else if( value.Length < 3 ) {
          //this.tag9F1A_terminalCountryCode = String.Format("{0:3}", value);
          throw new Exception("wrong lenght");
        }
      }
    }
    public String EMVDate
    {
      get { return this.tag9A_emvDate; }
      set { this.tag9A_emvDate = value; }
    }
    public String ARQC
    {
      get { return this.tag9F26_arqc; }
      set { this.tag9F26_arqc = value; }
    }

    public String AIP
    {
      get { return this.tag82_aip; }
      set { this.tag82_aip = value; }
    }
    public String ATC
    {
      get { return this.tag9F36_atc; }
      set { this.tag9F36_atc = value; }
    }
    public String UnpredictableNumber
    {
      get { return this.tag9F37_unpredictableNumber; }
      set { this.tag9F37_unpredictableNumber = value; }
    }

    public String TVR
    {
      get { return this.tag95_tvr; }
      set { this.tag95_tvr = value; }
    }
    public String CryptogramTransactionType
    {
      get { return this.tag9C_cryptogramTransactionType; }
      set { this.tag9C_cryptogramTransactionType = value; }
    }

    public String Tag5F2A_TransactionCurrencyCode
    {
      get { return this.tag5F2A_transactionCurrencyCode; }
      set {
        if( String.IsNullOrWhiteSpace(value) ) {
          this.tag5F2A_transactionCurrencyCode = "000"; 
        }
        else if( value.Length > 3 ) {
          this.tag5F2A_transactionCurrencyCode = value.Substring(value.Length - 3);
        }
        else if( value.Length < 3 ) {
          //this.tag5F2A_transactionCurrencyCode = String.Format("{0:3}", value);
          throw new Exception("wrong lenght");
        }
      }
    }
    public String TransactionAmount
    {
      get { return this.tag9F02_transactionAmount; }
      set { this.tag9F02_transactionAmount = value; }
    }
    public String IssuerApplicationData
    {
      get { return this.tag9F10_issuerApplicationData; }
      set { this.tag9F10_issuerApplicationData = value; }
    }
    #endregion properties
  }

  public class EMVAdditionalRequestDataSubField : SubField<SubFieldf6Type>
  {
    public enum SmartCardSchemeType : sbyte
    {
      EMV1996 = (sbyte)'0',
      EMV2000 = (sbyte)'1',
    }

    public SmartCardSchemeType emvCardScheme;
    public String tag5F34_applicationPANSequenceNumber;   //EMV Tag: 5F34 - Application PAN Sequence Number
    public String tag9F35_emvTerminalType;                //EMV Tag: 9F35 - EMV Terminal Type

    private String tag5F2A_transactionCurrencyCode;       //EMV Tag: 5F2A - Transaction Currency Code
    public String tag9F02_transactionAmount;              //EMV Tag: 9F02 - Transaction Amount
    public String tag9F03_transactionCashBackAmount;      //EMV Tag: 9F03 - Transaction cash back amount
    public String tag9F1E_terminalSerialNumber;           //EMV Tag: 9F1E - Terminal serial number
    public String tag9F33_terminalCapabilitiesBitMap;     //EMV Tag: 9F33 - Terminal capabilities bit map

    public String tag9F34_cvmresults;                     //EMV Tag: 9F34 - CVM results
    public String tag9F09_applicationVersionNumber;       //Application Version Number - Номер версии приложения, назначенный платежной системой.
    public String tag84_applicationIdentifier;            //Application Identifier - Идентификатор приложения по стандарту ISO/IEC 7816-5
  //public String tag84_aid;                              //Application Identifier - Идентификатор приложения по стандарту ISO/IEC 7816-5

    public String tag9F41_transactionSequenceCounter;     //EMV Tag: 9F41 - Transaction Sequence Counter

    public EMVAdditionalRequestDataSubField() : base(SubFieldf6Type.fP)
    {
      emvCardScheme = SmartCardSchemeType.EMV2000;
    }

    ////TODO: разобраться: нафига еще раз передавать тип субполя!?
    //public EMVAdditionalRequestDataField(SubFieldf6Type sfid) : base(sfid)
    //{
    //  smartCardScheme = SmartCardSchemeType.EMV2000;
    //}

    #region
    private String GetSmartCardSchemeValue()
    {
      if( emvCardScheme == SmartCardSchemeType.EMV1996 ) return "00";
      else return "01";
    }
    #endregion

    public override string GetValue()
    {
      String val = String.Empty;
      if( emvCardScheme == SmartCardSchemeType.EMV1996 ) {
        val = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", GetSmartCardSchemeValue(), tag5F34_applicationPANSequenceNumber, tag9F35_emvTerminalType, tag5F2A_transactionCurrencyCode, tag9F02_transactionAmount, tag9F03_transactionCashBackAmount, tag9F1E_terminalSerialNumber, tag9F33_terminalCapabilitiesBitMap, tag9F34_cvmresults, tag9F41_transactionSequenceCounter);
      }
      else if( emvCardScheme == SmartCardSchemeType.EMV2000 ) {
        val = string.Format("{0}{1}{2}{3}{4}{5}", GetSmartCardSchemeValue(), tag5F34_applicationPANSequenceNumber, tag9F35_emvTerminalType, tag9F34_cvmresults, tag9F09_applicationVersionNumber, tag84_applicationIdentifier);
      }
      else {
        throw new Exception("unknown scheme");
      }
      return val;
    }

    public override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)this.SFID ) {
        throw new NotImplementedException();
      }
      else {
        Console.WriteLine("[Err] EMVAdditionalRequestDataField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      //throw new NotFiniteNumberException();

      int len = 0;
      if( this.emvCardScheme == SmartCardSchemeType.EMV1996 ) {
        len += 2;
        len += tag5F34_applicationPANSequenceNumber.Length;
        len += tag9F35_emvTerminalType.Length;
        len += tag5F2A_transactionCurrencyCode.Length;
        len += tag9F02_transactionAmount.Length;
        len += tag9F03_transactionCashBackAmount.Length;
        len += tag9F1E_terminalSerialNumber.Length;
        len += tag9F33_terminalCapabilitiesBitMap.Length;
        len += tag9F34_cvmresults.Length;
        len += tag9F41_transactionSequenceCounter.Length;
      }
      else if( emvCardScheme == SmartCardSchemeType.EMV2000 ) {
        len += 2;
        len += tag5F34_applicationPANSequenceNumber.Length;
        len += tag9F35_emvTerminalType.Length;
        len += tag9F34_cvmresults.Length;
        len += tag9F09_applicationVersionNumber.Length;
        len += tag84_applicationIdentifier.Length;          
      }
      else {
        throw new Exception("unknown card schema");
      }

      //int len = 0;
      //if( smartCardScheme == SmartCardSchemeType.EMV1996 ) {
      //  len = 121;
      //}
      //else if( smartCardScheme == SmartCardSchemeType.EMV2000 ) {
      //  len = 136;
      //}
      //else {
      //  throw new Exception("unknown scheme");
      //}
      
      return len;
    }

    public override string ToString()
    {
      return string.Format("{0}{1}{2}{3}{4}{5}", GetSmartCardSchemeValue(), tag5F34_applicationPANSequenceNumber, tag9F35_emvTerminalType, tag9F34_cvmresults, tag9F09_applicationVersionNumber, tag84_applicationIdentifier);
    }

    #region properties
    public String Tag5F2A_TransactionCurrencyCode
    {
      get { return this.tag5F2A_transactionCurrencyCode; }
      set
      {
        if( String.IsNullOrWhiteSpace(value) ) {
          this.tag5F2A_transactionCurrencyCode = "000";
        }
        else if( value.Length > 3 ) {
          this.tag5F2A_transactionCurrencyCode = value.Substring(value.Length - 3);
        }
        else if( value.Length < 3 ) {
          //this.tag5F2A_transactionCurrencyCode = String.Format("{0:3}", value);
          throw new Exception("wrong lenght");
        }
      }
    }
    #endregion
  }

  public class POSEntryModeSubField : SubField<SubFieldf6Type>
  {
    public String posCondition;
    public String pinCondition;

    public POSEntryModeSubField() : base(SubFieldf6Type.fE)
    {
    }

    public override string GetValue()
    {
      return string.Format("{0}{1}", posCondition, pinCondition);
    }

    public override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)this.SFID ) {
        posCondition = this.Encoding.GetString(data, pos + 1, 2);
        pinCondition = this.Encoding.GetString(data, pos + 1 + 2, 1);
      }
      else {
        Console.WriteLine("[Err] POSEntryModeSubField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      int len = 0;
      len = posCondition.Length + pinCondition.Length;
      return len + 0; // <- длина значения поля + длина кода поля + длинна 1-ог разделителя
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}", posCondition, pinCondition);
    }
  }

  public class EMVResponseDataSubField : SubField<SubFieldf6Type>
  {
    private String emvCardScheme;                      //EMV Card Scheme - Значение – «00» либо «01».
    private String responseCode;                       //EMV Tag: 8a   - Response Code
    private String issuerAuthenticationData;  //IAD    //EMV Tag: 91   - Issuer Authentication Data - Может отсутствовать

    public EMVResponseDataSubField() : base(SubFieldf6Type.fQ)
    {
    }

    public override string GetValue()
    {
      //throw new NotImplementedException();
      return ToString();
    }

    public override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)this.SFID ) {
        emvCardScheme = this.Encoding.GetString(data, pos + 1, 2);
        responseCode = this.Encoding.GetString(data, pos + 1 + 2, 2);

        // последнее поле переменной длинны
        int i = 0;
        do {
          if( data[pos + 1 + 4 + i] == ProductSubFIDsField.GS ) break;  // обнаружили разделитель следующего подполя
          if( data[pos + 1 + 4 + i] == Message.FS ) break;              // обнаружили разделитель следующего поля
          if( data[pos + 1 + 4 + i] == (byte)EscType.ETX ) break;       // обнаружили конец сообщения
          ++i;
        }
        while( i < 600 );  //TODO: почему установлено ограничение 600??????????????????
        if( i > 0 ) issuerAuthenticationData = this.Encoding.GetString(data, pos + 1 + 4, i);
        else issuerAuthenticationData = string.Empty;


        //string ss = this.Encoding.GetString(data, pos + 1, 2);
        //cardScheme = ss;
        //issuerAuthenticationData = this.Encoding.GetString(data, pos + 1 + 4, 11); //???????????????????????????????????????????????????????????????????????? ERROR!!!!!!!!!! NOT 11 !!!!!!!!!!!!!
        //ss = this.Encoding.GetString(data, pos + 1 + 6, 3); //Encoding.GetEncoding(866).GetString(data, pos + 1 + 6, 3);
        //seqNumber = Convert.ToUInt16(ss);
        //resetFlag = (SequenceNumberResetFlag)data[pos + 1 + 9];
      }
      else {
        Console.WriteLine("[Err] EMVResponseDataSubField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      int len = 0;
      len = emvCardScheme.Length + responseCode.Length + issuerAuthenticationData.Length;
      return len + 0; // <- длина значения поля + длина кода поля + длинна 1-ог разделителя
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}:{2}", emvCardScheme, responseCode, issuerAuthenticationData);
    }

    #region properties

    public String EmvCardScheme
    {
      get { return this.emvCardScheme; }
    }
    public String ResponseCode
    {
      get { return this.responseCode; }
    }
    public String IssuerAuthenticationData
    {
      get { return this.issuerAuthenticationData; }
    }


    #endregion
  }

  public class EMVAdditionalResponseDataSubField : SubField<SubFieldf6Type>
  {
    private String emvCardScheme;                    //EMV Card Scheme - Значение – «00» либо «01».
    private String issuerScriptData;                 //EMV Tag72. Данное подполе – необязательное, переменной длины. Возможна передача нескольких тегов 72, в этом случае теги разделяются символом ‘;’.
    private String additionalIssuerScriptData;       //EMV Tag71. Данное подполе – необязательное, переменной длины. Возможна передача нескольких тегов 71, в этом случае теги разделяются символом ‘;’.

    public EMVAdditionalResponseDataSubField() : base(SubFieldf6Type.fR)
    {
    }

    public override string GetValue()
    {
      //throw new NotImplementedException();
      return ToString();
    }

    public override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)this.SFID ) {
        emvCardScheme = this.Encoding.GetString(data, pos + 1, 2);
        //issuerScriptData = this.Encoding.GetString(data, pos + 1 + 2, 256);

        // поле переменной длинны
        int i = 0;
        do {
          if( data[pos + 1 + 2 + i] == (byte)',' ) break;               // обнаружили внутренний разделитель [ ‘,’ (0x2C) ]
          if( data[pos + 1 + 2 + i] == ProductSubFIDsField.GS ) break;  // обнаружили разделитель следующего подполя
          if( data[pos + 1 + 2 + i] == Message.FS ) break;              // обнаружили разделитель следующего поля
          if( data[pos + 1 + 2 + i] == (byte)EscType.ETX ) break;       // обнаружили конец сообщения
          ++i;
        }
        while( i < 256 );  //TODO: почему установлено ограничение 256??????????????????
        if( i > 0 ) issuerScriptData = this.Encoding.GetString(data, pos + 1 + 2, i);
        else issuerScriptData = string.Empty;

        if( data[pos + 1 + 2 + i] == 0x2C ) {  //Разделитель – ‘,’ (0x2C).
          // поле переменной длинны
          int j = i + 1;  // учитываем разделитель ‘,’ (0x2C)
          i = 0;
          do {
            if( data[pos + 1 + 2 + j + i] == ProductSubFIDsField.GS ) break;  // обнаружили разделитель следующего подполя
            if( data[pos + 1 + 2 + j + i] == Message.FS ) break;              // обнаружили разделитель следующего поля
            if( data[pos + 1 + 2 + j + i] == (byte)EscType.ETX ) break;       // обнаружили конец сообщения
            ++i;
          }
          while( i < 256 );  //TODO: почему установлено ограничение 256??????????????????
          if( i > 0 ) additionalIssuerScriptData = this.Encoding.GetString(data, pos + 1 + 2 + j, i);
          else additionalIssuerScriptData = string.Empty;
        }


        //string ss = this.Encoding.GetString(data, pos + 1, 2);
        //cardScheme = ss;
        //issuerAuthenticationData = this.Encoding.GetString(data, pos + 1 + 4, 11); //???????????????????????????????????????????????????????????????????????? ERROR!!!!!!!!!! NOT 11 !!!!!!!!!!!!!
        //ss = this.Encoding.GetString(data, pos + 1 + 6, 3); //Encoding.GetEncoding(866).GetString(data, pos + 1 + 6, 3);
        //seqNumber = Convert.ToUInt16(ss);
        //resetFlag = (SequenceNumberResetFlag)data[pos + 1 + 9];
      }
      else {
        Console.WriteLine("[Err] EMVResponseDataSubField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      int len = 0;
      if( !String.IsNullOrWhiteSpace(emvCardScheme) ) {
        len += emvCardScheme.Length;
      }
      if( !String.IsNullOrWhiteSpace(issuerScriptData) ) {
        len += issuerScriptData.Length;
      }
      if( !String.IsNullOrWhiteSpace(additionalIssuerScriptData) ) {
        len += 1; // так как присутствует данные по поля additionalIssuerScriptData, то увеличиваем длинну на размер разделителя (‘,’ (0x2C))
        len += additionalIssuerScriptData.Length;
      }
      return len + 0; // <- длина значения поля + длина кода поля + длинна 1-ог разделителя
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}:{2}", emvCardScheme, issuerScriptData, additionalIssuerScriptData);
    }
  }

  public class EMVAdditionalRequestDataForAdviceReversalSubField : SubField<SubFieldf6Type>
  {
    public enum SmartCardSchemeType : sbyte
    {
      EMV1996 = (sbyte)'0',
      EMV2000 = (sbyte)'1',
    }

    private SmartCardSchemeType emvCardScheme;
    private String tag9F5B_issuerScriptResults;            //EMV Tag 9F5B - Issuer Script Results


    public EMVAdditionalRequestDataForAdviceReversalSubField() : base(SubFieldf6Type.fS)
    {
      emvCardScheme = SmartCardSchemeType.EMV2000;
    }

    #region
    private String GetSmartCardSchemeValue()
    {
      if( emvCardScheme == SmartCardSchemeType.EMV1996 ) return "00";
      else return "01";
    }
    #endregion

    public override string GetValue()
    {
      return string.Format("{0}{1}", GetSmartCardSchemeValue(), tag9F5B_issuerScriptResults);
    }

    public override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)this.SFID ) {
        String ss = this.Encoding.GetString(data, pos + 1, 2);
        emvCardScheme = (ss == "00" ? SmartCardSchemeType.EMV1996 : SmartCardSchemeType.EMV2000);
        tag9F5B_issuerScriptResults = this.Encoding.GetString(data, pos + 1 + 2, 10);
      }
      else {
        Console.WriteLine("[Err] POSEntryModeSubField -> попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      int len = 2;
      len = tag9F5B_issuerScriptResults.Length;
      return len + 0; // <- длина значения поля + длина кода поля + длинна 1-ог разделителя
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}", GetSmartCardSchemeValue(), tag9F5B_issuerScriptResults);
    }

    #region properties
    public SmartCardSchemeType EmvCardScheme
    {
      get { return this.emvCardScheme; }
      set { this.emvCardScheme = value; }
    }

    public String Tag9F5B_IssuerScriptResults
    {
      get { return this.tag9F5B_issuerScriptResults; }
      set { this.tag9F5B_issuerScriptResults = value; }
    }
    #endregion
  }

  public abstract class SubField<K> //where K : sbyte
  {
    private K subFieldType;
    public SubField(K sfid)
    {
      this.SFID = sfid;
      this.Encoding = Encoding.GetEncoding(866);
    }

    //public virtual string GetValue()
    //{
    //  throw new Exception("the method SubField::GetValue() should be overrided");
    //}

    public abstract string GetValue();
    public virtual void Decode(int pos, byte[] data)
    {
      //TODO: так пока не работает....
      // нужнодоразобраться почемуи сделать проверку по SFID именно здесь, а не в производных классах 
      //if( data[pos] != (byte)this.SFID ) {
      //  Console.WriteLine("[Err] попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      //  throw new Exception("unknown SFID: []:[]");
      //}
    }
    public abstract int Size();

    //public SubFieldType SFID { get; private set; }
    public K SFID
    {
      get { return this.subFieldType; }
      private set { this.subFieldType = value; }
    }
    public Encoding Encoding { get; set; }
  }
}
