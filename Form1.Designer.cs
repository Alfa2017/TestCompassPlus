namespace TestCompassPlus
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if( disposing && (components != null) ) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btRun = new System.Windows.Forms.Button();
      this.btBalanceInquiry = new System.Windows.Forms.Button();
      this.btMessage = new System.Windows.Forms.Button();
      this.btDownloadParams = new System.Windows.Forms.Button();
      this.btPurchase = new System.Windows.Forms.Button();
      this.btMailOrTelephoneOrder = new System.Windows.Forms.Button();
      this.btMerchandiseReturn = new System.Windows.Forms.Button();
      this.btCardVerification = new System.Windows.Forms.Button();
      this.btCardVerificationMSC = new System.Windows.Forms.Button();
      this.btReplenishment = new System.Windows.Forms.Button();
      this.btPurchaseReversal = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.btReversal = new System.Windows.Forms.Button();
      this.ebCCNumber = new System.Windows.Forms.TextBox();
      this.ebCCExpDate = new System.Windows.Forms.TextBox();
      this.ebCCAmount = new System.Windows.Forms.TextBox();
      this.ebCCTransID = new System.Windows.Forms.TextBox();
      this.edCCData = new System.Windows.Forms.TextBox();
      this.edCCCVV2 = new System.Windows.Forms.TextBox();
      this.btPurchasePIN = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.btCloseBatch = new System.Windows.Forms.Button();
      this.btSubtotalsBatch = new System.Windows.Forms.Button();
      this.btSubtotalsShift = new System.Windows.Forms.Button();
      this.btSubtotalsDay = new System.Windows.Forms.Button();
      this.btCloseShift = new System.Windows.Forms.Button();
      this.btCloseDay = new System.Windows.Forms.Button();
      this.btPartialDownload = new System.Windows.Forms.Button();
      this.ebDID = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.btAdminRequests = new System.Windows.Forms.Button();
      this.chbLogOn = new System.Windows.Forms.CheckBox();
      this.chbLogOff = new System.Windows.Forms.CheckBox();
      this.chbHandShake = new System.Windows.Forms.CheckBox();
      this.label6 = new System.Windows.Forms.Label();
      this.chbAccountCheking = new System.Windows.Forms.CheckBox();
      this.chbAccountDefault = new System.Windows.Forms.CheckBox();
      this.chbAccountNone = new System.Windows.Forms.CheckBox();
      this.chbAccountSavings = new System.Windows.Forms.CheckBox();
      this.chbAccountCredint = new System.Windows.Forms.CheckBox();
      this.chbAccountBonus = new System.Windows.Forms.CheckBox();
      this.chbPOSConditionCodeEMVChip = new System.Windows.Forms.CheckBox();
      this.lbLog = new System.Windows.Forms.ListBox();
      this.btCopyLog = new System.Windows.Forms.Button();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.tbTerminalID = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.tbEmployeeID = new System.Windows.Forms.TextBox();
      this.label12 = new System.Windows.Forms.Label();
      this.chbCardReaderMode = new System.Windows.Forms.CheckBox();
      this.label13 = new System.Windows.Forms.Label();
      this.tbCurrency = new System.Windows.Forms.TextBox();
      this.label14 = new System.Windows.Forms.Label();
      this.tbCustomerID = new System.Windows.Forms.TextBox();
      this.btEMVTransaction = new System.Windows.Forms.Button();
      this.label15 = new System.Windows.Forms.Label();
      this.ebSessKey = new System.Windows.Forms.TextBox();
      this.label16 = new System.Windows.Forms.Label();
      this.ebEMVST = new System.Windows.Forms.TextBox();
      this.chbEMVReverse = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // btRun
      // 
      this.btRun.Enabled = false;
      this.btRun.Location = new System.Drawing.Point(470, 338);
      this.btRun.Name = "btRun";
      this.btRun.Size = new System.Drawing.Size(146, 23);
      this.btRun.TabIndex = 0;
      this.btRun.Text = "Run";
      this.btRun.UseVisualStyleBackColor = true;
      this.btRun.Click += new System.EventHandler(this.btRun_Click);
      // 
      // btBalanceInquiry
      // 
      this.btBalanceInquiry.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btBalanceInquiry.Location = new System.Drawing.Point(268, 139);
      this.btBalanceInquiry.Name = "btBalanceInquiry";
      this.btBalanceInquiry.Size = new System.Drawing.Size(171, 23);
      this.btBalanceInquiry.TabIndex = 1;
      this.btBalanceInquiry.Text = "BalanceInquiry";
      this.btBalanceInquiry.UseVisualStyleBackColor = true;
      this.btBalanceInquiry.Click += new System.EventHandler(this.btBalanceInquiry_Click);
      // 
      // btMessage
      // 
      this.btMessage.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btMessage.Location = new System.Drawing.Point(470, 311);
      this.btMessage.Name = "btMessage";
      this.btMessage.Size = new System.Drawing.Size(146, 23);
      this.btMessage.TabIndex = 2;
      this.btMessage.Text = "Message";
      this.btMessage.UseVisualStyleBackColor = true;
      this.btMessage.Click += new System.EventHandler(this.btMessage_Click);
      // 
      // btDownloadParams
      // 
      this.btDownloadParams.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btDownloadParams.Location = new System.Drawing.Point(323, 10);
      this.btDownloadParams.Name = "btDownloadParams";
      this.btDownloadParams.Size = new System.Drawing.Size(116, 23);
      this.btDownloadParams.TabIndex = 3;
      this.btDownloadParams.Text = "Download";
      this.btDownloadParams.UseVisualStyleBackColor = true;
      this.btDownloadParams.Click += new System.EventHandler(this.btDownloadParams_Click);
      // 
      // btPurchase
      // 
      this.btPurchase.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btPurchase.ForeColor = System.Drawing.Color.DarkRed;
      this.btPurchase.Location = new System.Drawing.Point(268, 175);
      this.btPurchase.Name = "btPurchase";
      this.btPurchase.Size = new System.Drawing.Size(171, 23);
      this.btPurchase.TabIndex = 4;
      this.btPurchase.Text = "Purchase";
      this.btPurchase.UseVisualStyleBackColor = true;
      this.btPurchase.Click += new System.EventHandler(this.btPurchase_Click);
      // 
      // btMailOrTelephoneOrder
      // 
      this.btMailOrTelephoneOrder.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btMailOrTelephoneOrder.ForeColor = System.Drawing.Color.DarkRed;
      this.btMailOrTelephoneOrder.Location = new System.Drawing.Point(268, 340);
      this.btMailOrTelephoneOrder.Name = "btMailOrTelephoneOrder";
      this.btMailOrTelephoneOrder.Size = new System.Drawing.Size(171, 23);
      this.btMailOrTelephoneOrder.TabIndex = 5;
      this.btMailOrTelephoneOrder.Text = " MailOrTelephoneOrder";
      this.btMailOrTelephoneOrder.UseVisualStyleBackColor = true;
      this.btMailOrTelephoneOrder.Click += new System.EventHandler(this.btMailOrTelephoneOrder_Click);
      // 
      // btMerchandiseReturn
      // 
      this.btMerchandiseReturn.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btMerchandiseReturn.ForeColor = System.Drawing.Color.DarkBlue;
      this.btMerchandiseReturn.Location = new System.Drawing.Point(445, 175);
      this.btMerchandiseReturn.Name = "btMerchandiseReturn";
      this.btMerchandiseReturn.Size = new System.Drawing.Size(171, 23);
      this.btMerchandiseReturn.TabIndex = 6;
      this.btMerchandiseReturn.Text = "MerchandiseReturn";
      this.btMerchandiseReturn.UseVisualStyleBackColor = true;
      this.btMerchandiseReturn.Click += new System.EventHandler(this.btMerchandiseReturn_Click);
      // 
      // btCardVerification
      // 
      this.btCardVerification.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btCardVerification.Location = new System.Drawing.Point(268, 110);
      this.btCardVerification.Name = "btCardVerification";
      this.btCardVerification.Size = new System.Drawing.Size(171, 23);
      this.btCardVerification.TabIndex = 7;
      this.btCardVerification.Text = "CardVerification";
      this.btCardVerification.UseVisualStyleBackColor = true;
      this.btCardVerification.Click += new System.EventHandler(this.btCardVerification_Click);
      // 
      // btCardVerificationMSC
      // 
      this.btCardVerificationMSC.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btCardVerificationMSC.Location = new System.Drawing.Point(470, 369);
      this.btCardVerificationMSC.Name = "btCardVerificationMSC";
      this.btCardVerificationMSC.Size = new System.Drawing.Size(146, 23);
      this.btCardVerificationMSC.TabIndex = 8;
      this.btCardVerificationMSC.Text = "CardVerification (MSC)";
      this.btCardVerificationMSC.UseVisualStyleBackColor = true;
      this.btCardVerificationMSC.Click += new System.EventHandler(this.btCardVerificationMSC_Click);
      // 
      // btReplenishment
      // 
      this.btReplenishment.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btReplenishment.ForeColor = System.Drawing.Color.DarkGreen;
      this.btReplenishment.Location = new System.Drawing.Point(445, 204);
      this.btReplenishment.Name = "btReplenishment";
      this.btReplenishment.Size = new System.Drawing.Size(171, 23);
      this.btReplenishment.TabIndex = 9;
      this.btReplenishment.Text = "Replenishment";
      this.btReplenishment.UseVisualStyleBackColor = true;
      this.btReplenishment.Click += new System.EventHandler(this.btReplenishment_Click);
      // 
      // btPurchaseReversal
      // 
      this.btPurchaseReversal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btPurchaseReversal.ForeColor = System.Drawing.Color.DarkRed;
      this.btPurchaseReversal.Location = new System.Drawing.Point(268, 204);
      this.btPurchaseReversal.Name = "btPurchaseReversal";
      this.btPurchaseReversal.Size = new System.Drawing.Size(171, 23);
      this.btPurchaseReversal.TabIndex = 10;
      this.btPurchaseReversal.Text = "Purchase (Reversal)";
      this.btPurchaseReversal.UseVisualStyleBackColor = true;
      this.btPurchaseReversal.Click += new System.EventHandler(this.btPurchaseReversal_Click);
      // 
      // button1
      // 
      this.button1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.button1.Location = new System.Drawing.Point(470, 398);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(146, 23);
      this.button1.TabIndex = 11;
      this.button1.Text = "Async";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // btReversal
      // 
      this.btReversal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btReversal.ForeColor = System.Drawing.Color.DarkRed;
      this.btReversal.Location = new System.Drawing.Point(268, 311);
      this.btReversal.Name = "btReversal";
      this.btReversal.Size = new System.Drawing.Size(171, 23);
      this.btReversal.TabIndex = 12;
      this.btReversal.Text = "Reversal";
      this.btReversal.UseVisualStyleBackColor = true;
      this.btReversal.Click += new System.EventHandler(this.btReversal_Click);
      // 
      // ebCCNumber
      // 
      this.ebCCNumber.Location = new System.Drawing.Point(70, 110);
      this.ebCCNumber.Name = "ebCCNumber";
      this.ebCCNumber.Size = new System.Drawing.Size(137, 21);
      this.ebCCNumber.TabIndex = 13;
      this.ebCCNumber.Text = "5890000000002008";
      // 
      // ebCCExpDate
      // 
      this.ebCCExpDate.Location = new System.Drawing.Point(147, 137);
      this.ebCCExpDate.Name = "ebCCExpDate";
      this.ebCCExpDate.Size = new System.Drawing.Size(60, 21);
      this.ebCCExpDate.TabIndex = 14;
      this.ebCCExpDate.Text = "1511";
      // 
      // ebCCAmount
      // 
      this.ebCCAmount.Location = new System.Drawing.Point(107, 217);
      this.ebCCAmount.Name = "ebCCAmount";
      this.ebCCAmount.Size = new System.Drawing.Size(100, 21);
      this.ebCCAmount.TabIndex = 15;
      this.ebCCAmount.Text = "100";
      // 
      // ebCCTransID
      // 
      this.ebCCTransID.Location = new System.Drawing.Point(90, 311);
      this.ebCCTransID.Name = "ebCCTransID";
      this.ebCCTransID.Size = new System.Drawing.Size(117, 21);
      this.ebCCTransID.TabIndex = 16;
      this.ebCCTransID.Text = "1489738";
      // 
      // edCCData
      // 
      this.edCCData.Location = new System.Drawing.Point(70, 164);
      this.edCCData.Name = "edCCData";
      this.edCCData.Size = new System.Drawing.Size(137, 21);
      this.edCCData.TabIndex = 17;
      this.edCCData.Text = "10190880639";
      // 
      // edCCCVV2
      // 
      this.edCCCVV2.Location = new System.Drawing.Point(107, 190);
      this.edCCCVV2.Name = "edCCCVV2";
      this.edCCCVV2.Size = new System.Drawing.Size(100, 21);
      this.edCCCVV2.TabIndex = 54;
      // 
      // btPurchasePIN
      // 
      this.btPurchasePIN.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btPurchasePIN.ForeColor = System.Drawing.Color.DarkRed;
      this.btPurchasePIN.Location = new System.Drawing.Point(268, 369);
      this.btPurchasePIN.Name = "btPurchasePIN";
      this.btPurchasePIN.Size = new System.Drawing.Size(171, 23);
      this.btPurchasePIN.TabIndex = 18;
      this.btPurchasePIN.Text = "Purchase (PIN)";
      this.btPurchasePIN.UseVisualStyleBackColor = true;
      this.btPurchasePIN.Click += new System.EventHandler(this.btPurchasePIN_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 314);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(45, 13);
      this.label1.TabIndex = 19;
      this.label1.Text = "TransID";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 220);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(44, 13);
      this.label2.TabIndex = 20;
      this.label2.Text = "Amount";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 115);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(30, 13);
      this.label3.TabIndex = 21;
      this.label3.Text = "Card";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 140);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(48, 13);
      this.label4.TabIndex = 22;
      this.label4.Text = "ExpDate";
      // 
      // btCloseBatch
      // 
      this.btCloseBatch.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btCloseBatch.Location = new System.Drawing.Point(15, 406);
      this.btCloseBatch.Name = "btCloseBatch";
      this.btCloseBatch.Size = new System.Drawing.Size(81, 23);
      this.btCloseBatch.TabIndex = 23;
      this.btCloseBatch.Text = "CloseBatch";
      this.btCloseBatch.UseVisualStyleBackColor = true;
      this.btCloseBatch.Click += new System.EventHandler(this.btCloseBatch_Click);
      // 
      // btSubtotalsBatch
      // 
      this.btSubtotalsBatch.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btSubtotalsBatch.Location = new System.Drawing.Point(107, 406);
      this.btSubtotalsBatch.Name = "btSubtotalsBatch";
      this.btSubtotalsBatch.Size = new System.Drawing.Size(100, 23);
      this.btSubtotalsBatch.TabIndex = 24;
      this.btSubtotalsBatch.Text = "SubtotalsBatch";
      this.btSubtotalsBatch.UseVisualStyleBackColor = true;
      this.btSubtotalsBatch.Click += new System.EventHandler(this.btSubtotalsBatch_Click);
      // 
      // btSubtotalsShift
      // 
      this.btSubtotalsShift.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btSubtotalsShift.Location = new System.Drawing.Point(107, 435);
      this.btSubtotalsShift.Name = "btSubtotalsShift";
      this.btSubtotalsShift.Size = new System.Drawing.Size(100, 23);
      this.btSubtotalsShift.TabIndex = 25;
      this.btSubtotalsShift.Text = "SubtotalsShift";
      this.btSubtotalsShift.UseVisualStyleBackColor = true;
      this.btSubtotalsShift.Click += new System.EventHandler(this.btSubtotalsShift_Click);
      // 
      // btSubtotalsDay
      // 
      this.btSubtotalsDay.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btSubtotalsDay.Location = new System.Drawing.Point(107, 464);
      this.btSubtotalsDay.Name = "btSubtotalsDay";
      this.btSubtotalsDay.Size = new System.Drawing.Size(100, 23);
      this.btSubtotalsDay.TabIndex = 26;
      this.btSubtotalsDay.Text = "SubtotalsDay";
      this.btSubtotalsDay.UseVisualStyleBackColor = true;
      this.btSubtotalsDay.Click += new System.EventHandler(this.btSubtotalsDay_Click);
      // 
      // btCloseShift
      // 
      this.btCloseShift.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btCloseShift.Location = new System.Drawing.Point(15, 435);
      this.btCloseShift.Name = "btCloseShift";
      this.btCloseShift.Size = new System.Drawing.Size(81, 23);
      this.btCloseShift.TabIndex = 27;
      this.btCloseShift.Text = "CloseShift";
      this.btCloseShift.UseVisualStyleBackColor = true;
      this.btCloseShift.Click += new System.EventHandler(this.btCloseShift_Click);
      // 
      // btCloseDay
      // 
      this.btCloseDay.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btCloseDay.Location = new System.Drawing.Point(15, 464);
      this.btCloseDay.Name = "btCloseDay";
      this.btCloseDay.Size = new System.Drawing.Size(81, 23);
      this.btCloseDay.TabIndex = 28;
      this.btCloseDay.Text = "CloseDay";
      this.btCloseDay.UseVisualStyleBackColor = true;
      this.btCloseDay.Click += new System.EventHandler(this.btCloseDay_Click);
      // 
      // btPartialDownload
      // 
      this.btPartialDownload.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btPartialDownload.Location = new System.Drawing.Point(323, 39);
      this.btPartialDownload.Name = "btPartialDownload";
      this.btPartialDownload.Size = new System.Drawing.Size(116, 23);
      this.btPartialDownload.TabIndex = 29;
      this.btPartialDownload.Text = "PartialDownload";
      this.btPartialDownload.UseVisualStyleBackColor = true;
      this.btPartialDownload.Click += new System.EventHandler(this.btPartialDownload_Click);
      // 
      // ebDID
      // 
      this.ebDID.Location = new System.Drawing.Point(268, 41);
      this.ebDID.Name = "ebDID";
      this.ebDID.Size = new System.Drawing.Size(49, 21);
      this.ebDID.TabIndex = 30;
      this.ebDID.Text = "H";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(232, 44);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(25, 13);
      this.label5.TabIndex = 31;
      this.label5.Text = "DID";
      // 
      // btAdminRequests
      // 
      this.btAdminRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btAdminRequests.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btAdminRequests.Location = new System.Drawing.Point(864, 97);
      this.btAdminRequests.Name = "btAdminRequests";
      this.btAdminRequests.Size = new System.Drawing.Size(116, 23);
      this.btAdminRequests.TabIndex = 32;
      this.btAdminRequests.Text = "AdminRequests";
      this.btAdminRequests.UseVisualStyleBackColor = true;
      this.btAdminRequests.Click += new System.EventHandler(this.btAdminRequests_Click);
      // 
      // chbLogOn
      // 
      this.chbLogOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbLogOn.AutoSize = true;
      this.chbLogOn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbLogOn.Location = new System.Drawing.Point(923, 21);
      this.chbLogOn.Name = "chbLogOn";
      this.chbLogOn.Size = new System.Drawing.Size(57, 17);
      this.chbLogOn.TabIndex = 33;
      this.chbLogOn.Text = "LogOn";
      this.chbLogOn.UseVisualStyleBackColor = true;
      // 
      // chbLogOff
      // 
      this.chbLogOff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbLogOff.AutoSize = true;
      this.chbLogOff.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbLogOff.Location = new System.Drawing.Point(921, 43);
      this.chbLogOff.Name = "chbLogOff";
      this.chbLogOff.Size = new System.Drawing.Size(59, 17);
      this.chbLogOff.TabIndex = 34;
      this.chbLogOff.Text = "LogOff";
      this.chbLogOff.UseVisualStyleBackColor = true;
      // 
      // chbHandShake
      // 
      this.chbHandShake.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbHandShake.AutoSize = true;
      this.chbHandShake.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbHandShake.Location = new System.Drawing.Point(900, 66);
      this.chbHandShake.Name = "chbHandShake";
      this.chbHandShake.Size = new System.Drawing.Size(80, 17);
      this.chbHandShake.TabIndex = 35;
      this.chbHandShake.Text = "HandShake";
      this.chbHandShake.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(12, 167);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(30, 13);
      this.label6.TabIndex = 36;
      this.label6.Text = "Data";
      // 
      // chbAccountCheking
      // 
      this.chbAccountCheking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountCheking.AutoSize = true;
      this.chbAccountCheking.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountCheking.Location = new System.Drawing.Point(911, 217);
      this.chbAccountCheking.Name = "chbAccountCheking";
      this.chbAccountCheking.Size = new System.Drawing.Size(69, 17);
      this.chbAccountCheking.TabIndex = 39;
      this.chbAccountCheking.Text = "Checking";
      this.chbAccountCheking.UseVisualStyleBackColor = true;
      // 
      // chbAccountDefault
      // 
      this.chbAccountDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountDefault.AutoSize = true;
      this.chbAccountDefault.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountDefault.Location = new System.Drawing.Point(919, 194);
      this.chbAccountDefault.Name = "chbAccountDefault";
      this.chbAccountDefault.Size = new System.Drawing.Size(61, 17);
      this.chbAccountDefault.TabIndex = 38;
      this.chbAccountDefault.Text = "Default";
      this.chbAccountDefault.UseVisualStyleBackColor = true;
      // 
      // chbAccountNone
      // 
      this.chbAccountNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountNone.AutoSize = true;
      this.chbAccountNone.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountNone.Location = new System.Drawing.Point(929, 172);
      this.chbAccountNone.Name = "chbAccountNone";
      this.chbAccountNone.Size = new System.Drawing.Size(51, 17);
      this.chbAccountNone.TabIndex = 37;
      this.chbAccountNone.Text = "None";
      this.chbAccountNone.UseVisualStyleBackColor = true;
      // 
      // chbAccountSavings
      // 
      this.chbAccountSavings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountSavings.AutoSize = true;
      this.chbAccountSavings.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountSavings.Location = new System.Drawing.Point(917, 237);
      this.chbAccountSavings.Name = "chbAccountSavings";
      this.chbAccountSavings.Size = new System.Drawing.Size(63, 17);
      this.chbAccountSavings.TabIndex = 40;
      this.chbAccountSavings.Text = "Savings";
      this.chbAccountSavings.UseVisualStyleBackColor = true;
      // 
      // chbAccountCredint
      // 
      this.chbAccountCredint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountCredint.AutoSize = true;
      this.chbAccountCredint.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountCredint.Location = new System.Drawing.Point(925, 260);
      this.chbAccountCredint.Name = "chbAccountCredint";
      this.chbAccountCredint.Size = new System.Drawing.Size(55, 17);
      this.chbAccountCredint.TabIndex = 41;
      this.chbAccountCredint.Text = "Credit";
      this.chbAccountCredint.UseVisualStyleBackColor = true;
      // 
      // chbAccountBonus
      // 
      this.chbAccountBonus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbAccountBonus.AutoSize = true;
      this.chbAccountBonus.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbAccountBonus.Location = new System.Drawing.Point(925, 283);
      this.chbAccountBonus.Name = "chbAccountBonus";
      this.chbAccountBonus.Size = new System.Drawing.Size(55, 17);
      this.chbAccountBonus.TabIndex = 42;
      this.chbAccountBonus.Text = "Bonus";
      this.chbAccountBonus.UseVisualStyleBackColor = true;
      // 
      // chbPOSConditionCodeEMVChip
      // 
      this.chbPOSConditionCodeEMVChip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbPOSConditionCodeEMVChip.AutoSize = true;
      this.chbPOSConditionCodeEMVChip.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbPOSConditionCodeEMVChip.Location = new System.Drawing.Point(910, 337);
      this.chbPOSConditionCodeEMVChip.Name = "chbPOSConditionCodeEMVChip";
      this.chbPOSConditionCodeEMVChip.Size = new System.Drawing.Size(70, 17);
      this.chbPOSConditionCodeEMVChip.TabIndex = 43;
      this.chbPOSConditionCodeEMVChip.Text = "EMV Chip";
      this.chbPOSConditionCodeEMVChip.UseVisualStyleBackColor = true;
      // 
      // lbLog
      // 
      this.lbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbLog.FormattingEnabled = true;
      this.lbLog.HorizontalScrollbar = true;
      this.lbLog.Location = new System.Drawing.Point(15, 594);
      this.lbLog.Name = "lbLog";
      this.lbLog.ScrollAlwaysVisible = true;
      this.lbLog.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lbLog.Size = new System.Drawing.Size(973, 277);
      this.lbLog.TabIndex = 44;
      // 
      // btCopyLog
      // 
      this.btCopyLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btCopyLog.Location = new System.Drawing.Point(913, 563);
      this.btCopyLog.Name = "btCopyLog";
      this.btCopyLog.Size = new System.Drawing.Size(75, 23);
      this.btCopyLog.TabIndex = 45;
      this.btCopyLog.Text = "Copy";
      this.btCopyLog.UseVisualStyleBackColor = true;
      this.btCopyLog.Click += new System.EventHandler(this.btCopyLog_Click);
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(795, 156);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(189, 13);
      this.label7.TabIndex = 46;
      this.label7.Text = "различные виды счетов (Purchase):";
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(835, 321);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(145, 13);
      this.label8.TabIndex = 47;
      this.label8.Text = "режим \"fallback\" (Purchase):";
      // 
      // tbTerminalID
      // 
      this.tbTerminalID.Location = new System.Drawing.Point(107, 43);
      this.tbTerminalID.Name = "tbTerminalID";
      this.tbTerminalID.Size = new System.Drawing.Size(100, 21);
      this.tbTerminalID.TabIndex = 48;
      this.tbTerminalID.Text = "MYBILLR";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(12, 22);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(47, 13);
      this.label9.TabIndex = 49;
      this.label9.Text = "HEADER";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(33, 47);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(58, 13);
      this.label10.TabIndex = 50;
      this.label10.Text = "TerminalID";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(33, 67);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(64, 13);
      this.label11.TabIndex = 51;
      this.label11.Text = "EmployeeID";
      // 
      // tbEmployeeID
      // 
      this.tbEmployeeID.Location = new System.Drawing.Point(107, 66);
      this.tbEmployeeID.Name = "tbEmployeeID";
      this.tbEmployeeID.Size = new System.Drawing.Size(100, 21);
      this.tbEmployeeID.TabIndex = 52;
      this.tbEmployeeID.Text = "u0450u";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(12, 193);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(32, 13);
      this.label12.TabIndex = 53;
      this.label12.Text = "CVV2";
      // 
      // chbCardReaderMode
      // 
      this.chbCardReaderMode.AutoSize = true;
      this.chbCardReaderMode.Checked = true;
      this.chbCardReaderMode.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chbCardReaderMode.Location = new System.Drawing.Point(213, 167);
      this.chbCardReaderMode.Name = "chbCardReaderMode";
      this.chbCardReaderMode.Size = new System.Drawing.Size(15, 14);
      this.chbCardReaderMode.TabIndex = 55;
      this.chbCardReaderMode.UseVisualStyleBackColor = true;
      this.chbCardReaderMode.CheckedChanged += new System.EventHandler(this.chbCardReaderMode_CheckedChanged);
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(12, 247);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(51, 13);
      this.label13.TabIndex = 57;
      this.label13.Text = "Currency";
      // 
      // tbCurrency
      // 
      this.tbCurrency.Location = new System.Drawing.Point(107, 244);
      this.tbCurrency.Name = "tbCurrency";
      this.tbCurrency.Size = new System.Drawing.Size(100, 21);
      this.tbCurrency.TabIndex = 56;
      this.tbCurrency.Text = "810";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(12, 274);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(64, 13);
      this.label14.TabIndex = 59;
      this.label14.Text = "CustomerID";
      // 
      // tbCustomerID
      // 
      this.tbCustomerID.Location = new System.Drawing.Point(107, 271);
      this.tbCustomerID.Name = "tbCustomerID";
      this.tbCustomerID.Size = new System.Drawing.Size(100, 21);
      this.tbCustomerID.TabIndex = 58;
      this.tbCustomerID.Text = "1";
      // 
      // btEMVTransaction
      // 
      this.btEMVTransaction.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.btEMVTransaction.ForeColor = System.Drawing.Color.DarkRed;
      this.btEMVTransaction.Location = new System.Drawing.Point(90, 563);
      this.btEMVTransaction.Name = "btEMVTransaction";
      this.btEMVTransaction.Size = new System.Drawing.Size(146, 23);
      this.btEMVTransaction.TabIndex = 60;
      this.btEMVTransaction.Text = "EMV Transaction";
      this.btEMVTransaction.UseVisualStyleBackColor = true;
      this.btEMVTransaction.Click += new System.EventHandler(this.btEMVTransaction_Click);
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(12, 512);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(46, 13);
      this.label15.TabIndex = 62;
      this.label15.Text = "sessKey";
      // 
      // ebSessKey
      // 
      this.ebSessKey.Location = new System.Drawing.Point(90, 509);
      this.ebSessKey.Name = "ebSessKey";
      this.ebSessKey.Size = new System.Drawing.Size(797, 21);
      this.ebSessKey.TabIndex = 61;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(12, 539);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(75, 13);
      this.label16.TabIndex = 64;
      this.label16.Text = "EMV ST (data)";
      // 
      // ebEMVST
      // 
      this.ebEMVST.Location = new System.Drawing.Point(90, 536);
      this.ebEMVST.Name = "ebEMVST";
      this.ebEMVST.Size = new System.Drawing.Size(797, 21);
      this.ebEMVST.TabIndex = 63;
      // 
      // chbEMVReverse
      // 
      this.chbEMVReverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chbEMVReverse.AutoSize = true;
      this.chbEMVReverse.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chbEMVReverse.Checked = true;
      this.chbEMVReverse.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chbEMVReverse.Location = new System.Drawing.Point(798, 567);
      this.chbEMVReverse.Name = "chbEMVReverse";
      this.chbEMVReverse.Size = new System.Drawing.Size(89, 17);
      this.chbEMVReverse.TabIndex = 65;
      this.chbEMVReverse.Text = "EMV Reverse";
      this.chbEMVReverse.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(992, 876);
      this.Controls.Add(this.chbEMVReverse);
      this.Controls.Add(this.label16);
      this.Controls.Add(this.ebEMVST);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.ebSessKey);
      this.Controls.Add(this.btEMVTransaction);
      this.Controls.Add(this.label14);
      this.Controls.Add(this.tbCustomerID);
      this.Controls.Add(this.label13);
      this.Controls.Add(this.tbCurrency);
      this.Controls.Add(this.chbCardReaderMode);
      this.Controls.Add(this.edCCCVV2);
      this.Controls.Add(this.label12);
      this.Controls.Add(this.tbEmployeeID);
      this.Controls.Add(this.label11);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.tbTerminalID);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.btCopyLog);
      this.Controls.Add(this.lbLog);
      this.Controls.Add(this.chbPOSConditionCodeEMVChip);
      this.Controls.Add(this.chbAccountBonus);
      this.Controls.Add(this.chbAccountCredint);
      this.Controls.Add(this.chbAccountSavings);
      this.Controls.Add(this.chbAccountCheking);
      this.Controls.Add(this.chbAccountDefault);
      this.Controls.Add(this.chbAccountNone);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.chbHandShake);
      this.Controls.Add(this.chbLogOff);
      this.Controls.Add(this.chbLogOn);
      this.Controls.Add(this.btAdminRequests);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.ebDID);
      this.Controls.Add(this.btPartialDownload);
      this.Controls.Add(this.btCloseDay);
      this.Controls.Add(this.btCloseShift);
      this.Controls.Add(this.btSubtotalsDay);
      this.Controls.Add(this.btSubtotalsShift);
      this.Controls.Add(this.btSubtotalsBatch);
      this.Controls.Add(this.btCloseBatch);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.btPurchasePIN);
      this.Controls.Add(this.edCCData);
      this.Controls.Add(this.ebCCTransID);
      this.Controls.Add(this.ebCCAmount);
      this.Controls.Add(this.ebCCExpDate);
      this.Controls.Add(this.ebCCNumber);
      this.Controls.Add(this.btReversal);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.btPurchaseReversal);
      this.Controls.Add(this.btReplenishment);
      this.Controls.Add(this.btCardVerificationMSC);
      this.Controls.Add(this.btCardVerification);
      this.Controls.Add(this.btMerchandiseReturn);
      this.Controls.Add(this.btMailOrTelephoneOrder);
      this.Controls.Add(this.btPurchase);
      this.Controls.Add(this.btDownloadParams);
      this.Controls.Add(this.btMessage);
      this.Controls.Add(this.btBalanceInquiry);
      this.Controls.Add(this.btRun);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.MinimumSize = new System.Drawing.Size(600, 550);
      this.Name = "Form1";
      this.Text = "TpTp Client Tool";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btRun;
    private System.Windows.Forms.Button btBalanceInquiry;
    private System.Windows.Forms.Button btMessage;
    private System.Windows.Forms.Button btDownloadParams;
    private System.Windows.Forms.Button btPurchase;
    private System.Windows.Forms.Button btMailOrTelephoneOrder;
    private System.Windows.Forms.Button btMerchandiseReturn;
    private System.Windows.Forms.Button btCardVerification;
    private System.Windows.Forms.Button btCardVerificationMSC;
    private System.Windows.Forms.Button btReplenishment;
    private System.Windows.Forms.Button btPurchaseReversal;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button btReversal;
    private System.Windows.Forms.TextBox ebCCNumber;
    private System.Windows.Forms.TextBox ebCCExpDate;
    private System.Windows.Forms.TextBox ebCCAmount;
    private System.Windows.Forms.TextBox ebCCTransID;
    private System.Windows.Forms.TextBox edCCData;
    private System.Windows.Forms.Button btPurchasePIN;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button btCloseBatch;
    private System.Windows.Forms.Button btSubtotalsBatch;
    private System.Windows.Forms.Button btSubtotalsShift;
    private System.Windows.Forms.Button btSubtotalsDay;
    private System.Windows.Forms.Button btCloseShift;
    private System.Windows.Forms.Button btCloseDay;
    private System.Windows.Forms.Button btPartialDownload;
    private System.Windows.Forms.TextBox ebDID;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button btAdminRequests;
    private System.Windows.Forms.CheckBox chbLogOn;
    private System.Windows.Forms.CheckBox chbLogOff;
    private System.Windows.Forms.CheckBox chbHandShake;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.CheckBox chbAccountCheking;
    private System.Windows.Forms.CheckBox chbAccountDefault;
    private System.Windows.Forms.CheckBox chbAccountNone;
    private System.Windows.Forms.CheckBox chbAccountSavings;
    private System.Windows.Forms.CheckBox chbAccountCredint;
    private System.Windows.Forms.CheckBox chbAccountBonus;
    private System.Windows.Forms.CheckBox chbPOSConditionCodeEMVChip;
    private System.Windows.Forms.ListBox lbLog;
    private System.Windows.Forms.Button btCopyLog;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox tbTerminalID;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox tbEmployeeID;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox edCCCVV2;
    private System.Windows.Forms.CheckBox chbCardReaderMode;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.TextBox tbCurrency;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.TextBox tbCustomerID;
    private System.Windows.Forms.Button btEMVTransaction;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.TextBox ebSessKey;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.TextBox ebEMVST;
    private System.Windows.Forms.CheckBox chbEMVReverse;
  }
}

