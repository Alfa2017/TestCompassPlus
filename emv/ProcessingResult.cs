using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestEMVTransaction.emv
{
  /**
   * Defines EMV processing result codes. 
   */
  public static class ProcessingResult
  {  
    /**
     * In [SEPA-FAST] this actual value is defined as NONE and used as initialization value. This
     * specification defines the value as OK indicator.
     */
    public const int OK = 0x0000;
    
    /**
     * Set if the transaction is cancelled by the attendant or cardholder.
     */
    public const int CANCELLED = 0x0001;
    
    /**
     * Set if the card returns a response code SW1 SW2 = ‘6A81’.
     */
    public const int CARD_BLOCKED = 0x0002;
    
    /**
     * Set if the chip card is removed during chip processing.
     */
    public const int CARD_MISSING = 0x0003;
    
    /**
     * Set if communication with the chip card does not work correctly (Only used in the commands
     * defined in section 7.1, not during transaction processing.)
     */
    public const int CHIP_ERROR = 0x0004;
    
    /**
     * Set if duplicate data or incorrect TLV coding, data format, length or value is determined.
     */
    public const int DATA_ERROR = 0x0005;
    
    /**
     * Set in Application Selection if the candidate list is empty.
     */
    public const int EMPTY_LIST = 0x0006;
    
    /**
     * Set if SW1 SW2 = ‘6085’ is received in response to the GPO command.
     */
    public const int GPO6985 = 0x0007;
    
    /**
     * Set if one (or more) mandatory card data object(s) have not been found.
     */
    public const int MISSING_DATA = 0x0008;
    
    /**
     * Set if waiting for card insertion and timeout or another break occurs. (Only used in the
     * commands defined in section 7.1, not during transaction processing.)
     */
    public const int NO_CARD_INSERTED = 0x0009;
    
    /**
     * Set if no Application Profile exists for the AID which was selected.
     */
    public const int NO_PROFILE = 0x000A;
    
    /**
     * Set during Cardholder Application Confirmation.
     */
    public const int NOT_ACCEPTED = 0x000B;
    
    /**
     * Set during PIN or data entry.
     */
    public const int TIMEOUT = 0x000C;
    
    /**
     * Set if a technical error occurs during entry of data.
     */
    public const int ABORTED = 0x000D;
    
    /**
     * Set during Pre-Processing for Non Chip if fallback transactions are not allowed.
     */
    public const int FALLBACK_PROHIBITED = 0x000E;
    
    /**
     * Set when missing or inconsistent terminal configuration data is detected.
     */
    public const int CONFIGURATION_ERROR = 0x000F;
    
    /**
     * Indicates that EMV.LIB has returned an error not handled by the reader firmware.
     */
    public const int EMV_LIB = 0x8001;
    
    /**
     * Indicates that a function was called that is not allowed in the current state.
     */
    public const int FLOW_CONTROL  = 0x8002;
    
    /**
     * Indicates that processing was aborted due to internal errors.
     */
    public const int INTERNAL_ERROR  = 0x8003;
    
    /**
     * Indicates that reselect must be performed.
     */
    public const int RESELECT  = 0x8004;
    
    /**
     * Security error.
     */
    public const int SECURITY  = 0x8005;
    
    /**
     * Indicates that incorrect data has been received.
     */
    public const int INPUT_DATA_ERROR  = 0x8006;
    
    /**
     * Returned by the Reader to indicate that the transaction was aborted due to memory problems.
     */
    public const int OUT_OF_MEMORY  = 0x8007;
  }
}
